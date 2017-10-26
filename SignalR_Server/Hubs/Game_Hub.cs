using System;
using Microsoft.AspNet.SignalR;
using SignalR_Server.Models;
using SignalR_Server.Connectors;
using System.Linq;

namespace SignalR_Server.Hubs
{
    public class Game_Hub : Hub
    {
        #region Global values

        Game_Controller gameController = new Game_Controller();

        #endregion

        #region Client Requests

        //Client requests new game
        public async void RequestNewGame(M_Player myPlayer)
        {
            CheckAuthorization();

            //Pull out the connectionId for future reference
            myPlayer.SignalRConnectionId = Context.ConnectionId;
           
            //Create the new game
            var clientGameState = gameController.CreateNewGameState(myPlayer);
            
            //Add the player to a group with name of the GameKey
            await Groups.Add(Context.ConnectionId, clientGameState.GameKey);

            //Update the group
            UpdateGroup(clientGameState);
        }

        //Client requests to join game
        public async void RequestJoinGame(M_Player myPlayer, string gameKey)
        {
            CheckAuthorization();

            if (gameController.GetGame(gameKey).QuestionList.Count == 0)
            {
                try
                {
                    //Pull out the connectionId for future reference
                    myPlayer.SignalRConnectionId = Context.ConnectionId;
                    
                    //Add the player to the game state
                    var clientGameState = gameController.AddPlayerToGame(myPlayer, gameKey);

                    //Add the player to a group with name of the GameKey
                    await Groups.Add(Context.ConnectionId, clientGameState.GameKey);

                    //Update the group
                    UpdateGroup(clientGameState);
                }
                catch (Exception e)
                {
                    Clients.Caller.DisplayError("Oops! Something went wrong :( Try that again!", e);
                }
            }
            else
            {
                Clients.Caller.DisplayError("Sorry! We can't add you to a game that is already started. :(");
            }
        }

        //Client requests game start
        public void RequestStartGame(string gameKey)
        {
            CheckAuthorization();

            try
            {
                //Set the current player ready to start
                var clientGameState = gameController.PlayerIsReadyToStart(gameKey);

                //If the the stateId is 2, broadcast the update.
                //If not, then do nothing.
                if (clientGameState.ClientGameStateId == 2)
                {
                    UpdateGroupForNewQuestion(clientGameState);
                }
            }
            catch (Exception e)
            {
                Clients.Caller.DisplayError("Oops! Something went wrong :( Try that again!", e);
            }
        }

        //Client sends answer
        public void SubmitAnswer(string playerId, int myAnswer, string gameKey)
        {
            CheckAuthorization();

            var clientGameState = gameController.AddAnswer(playerId, myAnswer, gameKey);
            
            if (playerId.Equals(clientGameState.FocusedPlayerId))
            {
                clientGameState.CanSubmitAnswer = true;
                Clients.Others.ServerUpdate(clientGameState);
            }
            else if (gameController.IsLastAnswer(gameKey))
            {
                try
                {
                    var clientGameStateList = gameController.CalculateFocusedClientQuestionStats(gameKey);

                    //Iterate through all the players and send each one its game state
                    foreach (M_Player player in clientGameState.PlayerList)
                    {
                        var result = from cGameState in clientGameStateList
                                     where player.PlayerId.Equals(cGameState.ClientFocusedQuestionStats.PlayerId)
                                     select cGameState;

                        Clients.Client(player.SignalRConnectionId).ServerUpdate(result.First());
                    }
                }
                catch (Exception e)
                {
                    Clients.Caller.DisplayError("Didn't calculate the question stats.", e);
                }
            }
        }

        //Client [Focused player] requests continue
        public void RequestContinueToNextQuestion(string gameKey)
        {
            CheckAuthorization();

            if (gameController.IsRoundOver(gameKey))
            {
                if (gameController.IsGameOver(gameKey))
                {
                    Clients.Group(gameKey).DisplayGameStats(gameController.GetGameStats(gameKey));
                    Groups.Remove(Context.ConnectionId, gameKey);
                }
                else
                {
                    //Ask the controller to begin a new round
                    var clientGameState = gameController.BeginNewRound(gameKey);

                    UpdateGroupForNewQuestion(clientGameState);
                }
            }
            else
            {
                var clientGameState = gameController.BeginNewQuestion(gameKey);

                UpdateGroupForNewQuestion(clientGameState);
            }
        }

        //Client requests update to question db
        public void RequestQuestionDbUpdate(DateTimeOffset clientLastUpdate)
        {
            CheckAuthorization();

            Clients.Caller.PushQuestionCards(gameController.GetModifiedQuestionListFromDb(clientLastUpdate));
        }

        #endregion

        #region Helper Methods

        //Checks to make sure the client is authorized to access the server methods
        private void CheckAuthorization()
        {
            var token = Context.Headers.Get("authtoken");

            if (token.Equals(ConnectionConstants.SIGNALR_GAME_HUB_TOKEN))
            {
                return;
            }
            else
            {
                Clients.Caller.DisplayError("You're not authorized to use this method.");
            }
        }
        
        //Updates all clients in the group with the new client game state
        private void UpdateGroup(M_ClientGameState clientGameState)
        {
            Clients.Group(clientGameState.GameKey).ServerUpdate(clientGameState);
        }

        //Updates the
        private void UpdateGroupForNewQuestion(M_ClientGameState clientGameState)
        {
            //Pull out the connection id for the focused player
            var result = from player in clientGameState.PlayerList
                         where player.PlayerId == clientGameState.FocusedPlayerId
                         select player.SignalRConnectionId;

            var focusedPlayerConnectionId = result.First();

            //Send out client updates
            Clients.Group(clientGameState.GameKey, focusedPlayerConnectionId).ServerUpdate(clientGameState);

            //Allow the focused player to submit answer
            clientGameState.CanSubmitAnswer = true;
            Clients.Client(focusedPlayerConnectionId).ServerUpdate(clientGameState);
        }

        #endregion
    }
}