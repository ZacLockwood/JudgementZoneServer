using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using SignalR_Server.Models;
using SignalR_Server.Connectors;

namespace SignalR_Server
{
    public class Game_Controller
    {
        #region Global values

        GameStateDB_Connector gameDbConnector = new GameStateDB_Connector();

        #endregion

        #region Constructors

        // Default constructor    
        public Game_Controller()
        {
        }

        #endregion

        #region Game preparation methods

        // Creates a new game with a first player
        public M_ClientGameState CreateNewGameState(M_Player myPlayer)
        {
            var collection = gameDbConnector.GetCollection();
            M_GameState newGameState = new M_GameState(myPlayer);

            try
            {
                collection.InsertOne(newGameState);

                //NEW SYSTEM
                //This needs to return the clientGameState
                return BuildClientGameState(newGameState, 1);
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't create the game.", e);
            }
        }

        // Adds a single player to the given game
        public M_ClientGameState AddPlayerToGame(M_Player myPlayer, string gameKey)
        {
            try
            {
                M_GameState curGameState = GetGame(gameKey);
                curGameState.PlayerList.Add(myPlayer);
                
                UpdateGameState(curGameState);

                //NEW SYSTEM
                return BuildClientGameState(curGameState, 1);
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't add the player to the game.", e);
            }
        }

        // Gets all players in the given game
        public IList<M_Player> GetPlayerList(string gameKey)
        {
            try
            {
                M_GameState curGameState = GetGame(gameKey);
                return curGameState.PlayerList;
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't get the player list.", e);
            }
        }

        // Adds the calling player saying they're ready to begin
        public M_ClientGameState PlayerIsReadyToStart(string gameKey)
        {            
            M_GameState curGameState = GetGame(gameKey);
            curGameState.NumStartRequests++;

            if (curGameState.NumStartRequests == curGameState.PlayerList.Count)
            {
                return PrepGameForStart(curGameState);
            }
            else
            {
                UpdateGameState(curGameState);
                return BuildClientGameState(curGameState, 1);
            }
        }

        // Adds questions to the game state and updates the DB
        private M_ClientGameState PrepGameForStart(M_GameState curGameState)
        {
            QuestionDB_Connector dbConnector = new QuestionDB_Connector();
            IList<M_QuestionCard> list = null;

            try
            {
                list = dbConnector.PullInQuestions(curGameState.PlayerList.Count, 3);
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't get the questions from the database.", e);
            }

            if (list == null)
            {
                throw new Exception("The question list is null.");
            }
            else
            {
                curGameState.QuestionList = list;
                curGameState.GenerateNextQuestion();
                curGameState.CurrentQuestionNum = 1;
                curGameState.MaxQuestionNum = curGameState.PlayerList.Count;
                UpdateGameState(curGameState);

                return BuildClientGameState(curGameState, 2);
            }
        }

        #endregion

        #region Gameplay methods

        // Adds the calling client's answer to the game stats
        public M_ClientGameState AddAnswer(string playerId, int playerAnswer, string gameKey)
        {
            M_GameState curGameState = GetGame(gameKey);
            M_QuestionStats curAnswerStats = GetCurrentQuestionStats(curGameState);
            var index = curGameState.QuestionStatsList.IndexOf(curAnswerStats);

            if (curAnswerStats.FocusedPlayerAnswer.PlayerAnswer == 0)
            {
                curAnswerStats.FocusedPlayerAnswer = new M_PlayerAnswer(playerId, playerAnswer, gameKey);
            }
            else
            {
                curAnswerStats.OtherPlayerAnswers.Add(new M_PlayerAnswer(playerId, playerAnswer, gameKey));
            }

            curGameState.QuestionStatsList[index] = curAnswerStats;
            UpdateGameState(curGameState);

            return BuildClientGameState(curGameState, 2);
        }        

        // THE LOGIC IN THIS METHOD NEEDS REWORKING
        // Calculates the current ClientFocusedQuestionStats and returns the ClientGameState
        public List<M_ClientGameState> CalculateFocusedClientQuestionStats(string gameKey)
        {
            //Get the game and the current answer stats
            var curGameState = GetGame(gameKey);
            var curAnswerStats = GetCurrentQuestionStats(curGameState);

            var clientGameStateList = new List<M_ClientGameState>();
            var clientGameState = BuildClientGameState(curGameState, 3);

            int numRed = 0;
            int numYellow = 0;
            int numGreen = 0;
            int numBlue = 0;

            //Calculate the number of answers for each color
            foreach (M_PlayerAnswer pAnswer in curAnswerStats.OtherPlayerAnswers)
            {
                switch (pAnswer.PlayerAnswer)
                {
                    case 1: numRed++;
                        break;
                    case 2: numYellow++;
                        break;
                    case 3: numGreen++;
                        break;
                    case 4: numBlue++;
                        break;
                }
            }

            //Iterate through all the player answers and build the list of clientGameStates
            //with their respective focusedPlayerQuestionStats
            foreach (M_PlayerAnswer pAnswer in curAnswerStats.OtherPlayerAnswers)
            {
                var clientQuestionStats = new M_ClientQuestionStats();

                //Assign IDs
                clientQuestionStats.GameKey = gameKey;
                clientQuestionStats.QuestionId = curGameState.FocusedQuestionId;
                clientQuestionStats.PlayerId = pAnswer.PlayerId;
                
                //Check to see if the player guessed correctly
                if (pAnswer.PlayerAnswer == curAnswerStats.FocusedPlayerAnswer.PlayerAnswer)
                {
                    clientQuestionStats.IsPlayerCorrect = true;
                }
                else
                {
                    clientQuestionStats.IsPlayerCorrect = false;
                }

                //Assign other values
                clientQuestionStats.CorrectAnswerId = curAnswerStats.FocusedPlayerAnswer.PlayerAnswer;
                clientQuestionStats.NumRedGuesses = numRed;
                clientQuestionStats.NumYellowGuesses = numYellow;
                clientQuestionStats.NumGreenGuesses = numGreen;
                clientQuestionStats.NumBlueGuesses = numBlue;

                //Assign the questions stats to the game state and add the game state to the list
                var copyGameState = clientGameState;
                copyGameState.ClientFocusedQuestionStats = clientQuestionStats;
                clientGameStateList.Add(copyGameState);
            }

            //Make one more client game state for the focused player
            var focusedPlayerQuestionStats = new M_ClientQuestionStats();

            //Assign IDs
            focusedPlayerQuestionStats.GameKey = gameKey;
            focusedPlayerQuestionStats.QuestionId = curGameState.FocusedQuestionId;
            focusedPlayerQuestionStats.PlayerId = curGameState.FocusedPlayerId;

            //This value doesn't matter since the focused player didn't guess
            focusedPlayerQuestionStats.IsPlayerCorrect = true;

            //Assign other values
            focusedPlayerQuestionStats.CorrectAnswerId = curAnswerStats.FocusedPlayerAnswer.PlayerAnswer;
            focusedPlayerQuestionStats.NumRedGuesses = numRed;
            focusedPlayerQuestionStats.NumYellowGuesses = numYellow;
            focusedPlayerQuestionStats.NumGreenGuesses = numGreen;
            focusedPlayerQuestionStats.NumBlueGuesses = numBlue;

            //Assign the questions stats to the game state and add the game state to the list
            var focusedPlayerCopyGameState = clientGameState;
            focusedPlayerCopyGameState.ClientFocusedQuestionStats = focusedPlayerQuestionStats;
            clientGameStateList.Add(focusedPlayerCopyGameState);

            return clientGameStateList;
        }

        // Begin a new round of questions
        public M_ClientGameState BeginNewRound(string gameKey)
        {
            M_GameState curGameState = GetGame(gameKey);

            curGameState.CurrentRoundNum++;
            curGameState.CurrentQuestionNum = 1;
            curGameState.FocusedPlayerId = curGameState.PlayerList.First().PlayerId;
            curGameState.GenerateNextQuestion();
            curGameState.IsNewRound = true;

            UpdateGameState(curGameState);

            return BuildClientGameState(curGameState, 2);
        }

        // Begin a new question with a new focused player
        public M_ClientGameState BeginNewQuestion(string gameKey)
        {
            M_GameState curGameState = GetGame(gameKey);

            // Finds the focused player in the PlayerList list
            var result = from player in curGameState.PlayerList
                         where player.PlayerId == curGameState.FocusedPlayerId
                         select player;

            // Finds the index of the focused player in the PlayerList list
            var focusedPlayer = result.First();
            var index = curGameState.PlayerList.IndexOf(focusedPlayer);

            // Updates the focused player id to the new focused player
            curGameState.FocusedPlayerId = curGameState.PlayerList[index + 1].PlayerId;
            curGameState.GenerateNextQuestion();

            UpdateGameState(curGameState);

            return BuildClientGameState(curGameState, 2);
        }

        #endregion

        #region Get game value methods

        // Returns the stats for the finished question
        public M_QuestionStats GetQuestionStats(string gameKey)
        {
            return GetCurrentQuestionStats(GetGame(gameKey));
        }
        
        // Returns the game stats
        public IList<M_QuestionStats> GetGameStats(string gameKey)
        {
            return GetGame(gameKey).QuestionStatsList;
        }

        // Returns the current focused player ID and question card
        public Tuple<string, M_QuestionCard> GetFocusedPlayerIdAndQuestion(string gameKey)
        {
            M_GameState curGameState = GetGame(gameKey);

            var result = from qCard in curGameState.QuestionList
                         where qCard.QuestionId.Equals(curGameState.FocusedQuestionId)
                         select qCard;

            M_QuestionCard focusedQuestion = result.First();

            return new Tuple<string, M_QuestionCard>(curGameState.FocusedPlayerId, focusedQuestion);
            
        }

        // Returns the client's game state object
        public M_ClientGameState GetClientGameState(string gameKey, int clientGameStateId)
        {
            return BuildClientGameState(GetGame(gameKey), clientGameStateId);
        }
        
        #endregion

        #region Check game state methods

        // Checks to see if round is over
        public bool IsRoundOver(string gameKey)
        {
            //Leave this call because it saves an extra search of the DB
            M_GameState curGameState = GetGame(gameKey);

            if (curGameState.CurrentQuestionNum == curGameState.MaxQuestionNum)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Checks to see if the game is over
        public bool IsGameOver(string gameKey)
        {
            if (GetGame(gameKey).CurrentRoundNum == 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Checks if the answer just given is the last answer of the hand
        public bool IsLastAnswer(string gameKey)
        {
            //Leave this call because it saves an extra search of the DB
            M_GameState curGameState = GetGame(gameKey);

            if (GetCurrentQuestionStats(curGameState).OtherPlayerAnswers.Count == curGameState.PlayerList.Count - 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Helper methods

        // Returns the answer stats for the current hand
        private M_QuestionStats GetCurrentQuestionStats(M_GameState curGameState)
        {
            var result = from aStats in curGameState.QuestionStatsList
                         where aStats.GameRound == curGameState.CurrentRoundNum 
                         && aStats.FocusedPlayerAnswer.PlayerId.Equals(curGameState.FocusedPlayerId)
                         select aStats;

            return result.First();
        }

        // Updates the given game state on the DB
        private void UpdateGameState(M_GameState curGameState)
        {
            var collection = gameDbConnector.GetCollection();
            var filter = Builders<M_GameState>.Filter.Eq("_id", curGameState.GameKey);
            collection.ReplaceOne(filter, curGameState);
        }

        // Returns a game for a given game id
        public M_GameState GetGame(string gameKey)
        {
            var collection = gameDbConnector.GetCollection();

            try
            {
                var filter = Builders<M_GameState>.Filter.Eq("_id", gameKey);
                return collection.Find(filter).FirstOrDefault();
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't retrieve the game from the database.", e);
            }
        }

        // Builds the client's game state from the server's game state
        private M_ClientGameState BuildClientGameState(M_GameState curGameState, int clientGameStateId)
        {
            M_ClientGameState clientGameState = new M_ClientGameState();

            clientGameState.GameKey = curGameState.GameKey;
            // CLIENT STATE IDENTIFIER
            clientGameState.ClientGameStateId = clientGameStateId;
            // STATE-INDEPENDENT DATA
            clientGameState.PlayerList = curGameState.PlayerList;
            // GAME CYCLE METRICS
            clientGameState.CurrentRoundNum = curGameState.CurrentRoundNum;
            clientGameState.CurrentQuestionNum = curGameState.CurrentQuestionNum;
            clientGameState.MaxQuestionNum = curGameState.MaxQuestionNum;
            clientGameState.CurrentRoundNum = curGameState.CurrentRoundNum;
            clientGameState.MaxRoundNum = curGameState.MaxRoundNum;
            clientGameState.IsNewRound = curGameState.IsNewRound;
            // GAME STATE CONTEXT
            clientGameState.FocusedPlayerId = curGameState.FocusedPlayerId;
            clientGameState.FocusedQuestionId = curGameState.FocusedQuestionId;
            clientGameState.ClientFocusedQuestionStats = curGameState.FocusedClientQuestionStats;
            clientGameState.ClientGameStats = curGameState.ClientGameStats;

            return clientGameState;
        }
        
        // Gets the updated question cards from the question db for the client
        public List<M_QuestionCard> GetModifiedQuestionListFromDb(DateTimeOffset clientLastUpdate)
        {
            var dbConnector = new QuestionDB_Connector();

            return dbConnector.GetModifiedQuestionList(clientLastUpdate);
        }

        #endregion
    }
}