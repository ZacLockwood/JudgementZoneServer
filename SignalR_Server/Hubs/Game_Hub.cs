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
        public void RequestNewGame(M_Player myPlayer)
        {
            CheckAuthorization();

            //NEW SYSTEM
            //Pull out the connectionId for future reference
            myPlayer.SignalRConnectionId = Context.ConnectionId;           
           
            //Create the new game
            var clientGameState = gameController.CreateNewGameState(myPlayer);  
            
            //Add the player to a group with name of the GameKey
            Groups.Add(myPlayer.SignalRConnectionId, clientGameState.GameKey);

            //Update the client
            UpdateGroup(clientGameState);
        }

        //Client requests to join game
        public void RequestJoinGame(M_Player myPlayer, string gameKey)
        {
            CheckAuthorization();

            if (gameController.GetGame(gameKey).QuestionList.Count == 0)
            {
                try
                {
                    //NEW SYSTEM
                    var clientGameState = gameController.AddPlayerToGame(myPlayer, gameKey);
                    Groups.Add(Context.ConnectionId, gameKey);

                    //NEW SYSTEM
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
        public void RequestStartGame(M_Player myPlayer, string gameKey)
        {
            CheckAuthorization();

            try
            {
                //Set the current player ready to start
                var clientGameState = gameController.PlayerIsReadyToStart(myPlayer, gameKey);

                //If the the stateId is 2, broadcast the update.
                //If not, then do nothing.
                if (clientGameState.ClientGameStateId == 2)
                {
                    //NEW SYSTEM
                    UpdateGroup(clientGameState);
                }
            }
            catch (Exception e)
            {
                Clients.Caller.DisplayError("Oops! Something went wrong :( Try that again!", e);
            }
        }

        //Client sends answer
        public void SubmitAnswer(M_PlayerAnswer myAnswer, string gameKey)
        {
            CheckAuthorization();

            var clientGameState = gameController.AddAnswer(myAnswer, gameKey);
            
            if (myAnswer.PlayerId.Equals(clientGameState.FocusedPlayerId))
            {
                clientGameState.CanSubmitAnswer = true;
                Clients.Others.ServerUpdate(clientGameState);
            }
            else if (gameController.IsLastAnswer(gameKey))
            {
                //NEW SYSTEM
                var clientGameStateList = gameController.CalculateFocusedClientQuestionStats(gameKey);

                //Iterate through all the ClientGameStates and send out each one to its respective client
                foreach (M_ClientGameState cGameState in clientGameStateList)
                {
                    var result = from player in cGameState.PlayerList
                         where player.PlayerId.Equals(cGameState.ClientFocusedQuestionStats.PlayerId)
                         select player;
                    
                    Clients.User(result.First().SignalRConnectionId).ServerUpdate(cGameState);
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
                    //NEW SYSTEM
                    var clientGameState = gameController.BeginNewRound(gameKey);

                    var PlayerAndQuestion = gameController.GetFocusedPlayerIdAndQuestion(gameKey);
                    Clients.Group(gameKey).DisplayQuestion(PlayerAndQuestion.Item1, PlayerAndQuestion.Item2);

                    //NEW SYSTEM
                    UpdateGroup(clientGameState);
                }
            }
            else
            {
                //NEW SYSTEM
                var clientGameState = gameController.BeginNewQuestion(gameKey);

                var PlayerAndQuestion = gameController.GetFocusedPlayerIdAndQuestion(gameKey);
                Clients.Group(gameKey).DisplayQuestion(PlayerAndQuestion.Item1, PlayerAndQuestion.Item2);

                //NEW SYSTEM
                UpdateGroup(clientGameState);
            }
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
                throw new Exception("Client not authorized for this method.");
            }
        }
        
        //Updates all clients in the group with the new client game state
        private void UpdateGroup(M_ClientGameState clientGameState)
        {
            Clients.Group(clientGameState.GameKey).ServerUpdate(clientGameState);
        }

        #endregion
    }
}