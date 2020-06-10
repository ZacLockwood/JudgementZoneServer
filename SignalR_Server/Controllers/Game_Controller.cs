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
        public M_Client_GameState CreateNewGameState(M_Player myPlayer)
        {
            var collection = gameDbConnector.GetCollection();
            M_GameState newGameState = new M_GameState(myPlayer);

            try
            {
                // Insert game state into db and return client game state
                collection.InsertOne(newGameState);
                return BuildClientGameState(newGameState, 1);
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't create the game.", e);
            }
        }

        // Adds a single player to the given game
        public M_Client_GameState AddPlayerToGame(M_Player myPlayer, string gameKey)
        {
            try
            {
                M_GameState curGameState = GetGame(gameKey);
                curGameState.PlayerList.Add(myPlayer);

                // Update db and return client game state
                UpdateGameState(curGameState);
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
        public M_Client_GameState PlayerIsReadyToStart(M_Player myPlayer, string gameKey)
        {
            M_GameState curGameState = GetGame(gameKey);

            // Label the player as ready to start
            myPlayer.IsReadyToStart = true;

            //curGameState.NumStartRequests++;

            // Find and replace the same player in the player list
            var result = from player in curGameState.PlayerList
                         where player.PlayerId == myPlayer.PlayerId
                         select player;
            var index = curGameState.PlayerList.IndexOf(result.First());
            curGameState.PlayerList[index] = myPlayer;

            int numReady = 0;
            foreach (M_Player player in curGameState.PlayerList)
            {
                if (player.IsReadyToStart)
                {
                    numReady++;
                }
            }

            // If all players are ready to start, prep the game for start
            if (numReady == curGameState.PlayerList.Count)//curGameState.NumStartRequests == curGameState.PlayerList.Count)
            {
                curGameState.IsNewRound = true;
                return PrepGameForStart(curGameState);
            }
            else
            {
                // Update db and return client game state
                UpdateGameState(curGameState);
                return BuildClientGameState(curGameState, 1);
            }
        }

        // Adds questions to the game state and updates the DB
        private M_Client_GameState PrepGameForStart(M_GameState curGameState)
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

                // Update db and return client game state
                UpdateGameState(curGameState);
                return BuildClientGameState(curGameState, 2);
            }
        }

        #endregion

        #region Gameplay methods

        // Adds the calling client's answer to the game stats
        public M_Client_GameState AddAnswer(string playerId, int playerAnswer, string gameKey)
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
        public List<M_Client_GameState> CalculateFocusedClientQuestionStats(string gameKey)
        {
            //Get the game and the current answer stats
            var curGameState = GetGame(gameKey);
            var curAnswerStats = GetCurrentQuestionStats(curGameState);

            var clientGameStateList = new List<M_Client_GameState>();
            //var clientGameState = BuildClientGameState(curGameState, 3);

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
                var clientQuestionStats = new M_Client_QuestionStats();

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
                clientQuestionStats.RedGuesses = numRed;
                clientQuestionStats.YellowGuesses = numYellow;
                clientQuestionStats.GreenGuesses = numGreen;
                clientQuestionStats.BlueGuesses = numBlue;

                //Assign the questions stats to a new game state and add the game state to the list
                var clientGameState = BuildClientGameState(curGameState, 3);
                clientGameState.QuestionStats = clientQuestionStats;
                clientGameStateList.Add(clientGameState);
            }

            //Make one more client game state for the focused player
            var focusedPlayerQuestionStats = new M_Client_QuestionStats();

            //Assign IDs
            focusedPlayerQuestionStats.GameKey = gameKey;
            focusedPlayerQuestionStats.QuestionId = curGameState.FocusedQuestionId;
            focusedPlayerQuestionStats.PlayerId = curGameState.FocusedPlayerId;

            //This value doesn't matter since the focused player didn't guess
            focusedPlayerQuestionStats.IsPlayerCorrect = true;

            //Assign other values
            focusedPlayerQuestionStats.CorrectAnswerId = curAnswerStats.FocusedPlayerAnswer.PlayerAnswer;
            focusedPlayerQuestionStats.RedGuesses = numRed;
            focusedPlayerQuestionStats.YellowGuesses = numYellow;
            focusedPlayerQuestionStats.GreenGuesses = numGreen;
            focusedPlayerQuestionStats.BlueGuesses = numBlue;

            //Assign the questions stats to the game state and add the game state to the list
            var focusedPlayerGameState = BuildClientGameState(curGameState, 3);
            focusedPlayerGameState.QuestionStats = focusedPlayerQuestionStats;
            clientGameStateList.Add(focusedPlayerGameState);

            return clientGameStateList;
        }

        // Begin a new round of questions
        public M_Client_GameState BeginNewRound(string gameKey)
        {
            M_GameState curGameState = GetGame(gameKey);

            // Update all the values for a new round
            curGameState.FocusedPlayerId = curGameState.PlayerList.First().PlayerId;
            curGameState.CurrentRoundNum++;
            curGameState.CurrentQuestionNum = 1;
            curGameState.QuestionCounter++;
            curGameState.GenerateNextQuestion();
            curGameState.IsNewRound = true;

            // Update db and return client game state
            UpdateGameState(curGameState);
            return BuildClientGameState(curGameState, 2);
        }

        // Begin a new question with a new focused player
        public M_Client_GameState BeginNewQuestion(string gameKey)
        {
            M_GameState curGameState = GetGame(gameKey);

            // Finds the focused player in the PlayerList list
            var result = from player in curGameState.PlayerList
                         where player.PlayerId == curGameState.FocusedPlayerId
                         select player;

            // Finds the index of the focused player in the PlayerList list
            var index = curGameState.PlayerList.IndexOf(result.First());

            // Updates the focused player id to the new focused player
            curGameState.FocusedPlayerId = curGameState.PlayerList[index + 1].PlayerId;

            // Update all the values for the new question
            curGameState.QuestionCounter++;
            curGameState.CurrentQuestionNum++;
            curGameState.GenerateNextQuestion();


            // Update db and return client game state
            UpdateGameState(curGameState);
            return BuildClientGameState(curGameState, 2);
        }

        // End the game by getting the game stats and returning it to the players
        public M_Client_GameState EndGame(string gameKey)
        {
            var curGameState = GetGame(gameKey);

            // Calculate and store the game stats for each player in a list
            curGameState.PlayerGameStatsList = GetGameStats(curGameState);

            // Update db and return client game state
            UpdateGameState(curGameState);
            return (BuildClientGameState(curGameState, 4));
        }

        #endregion

        #region Get game value methods

        // Returns the stats for the finished question
        public M_QuestionStats GetQuestionStats(string gameKey)
        {
            return GetCurrentQuestionStats(GetGame(gameKey));
        }

        // Calculates and returns the game stats for each player in a list
        public List<M_Client_PlayerGameStats> GetGameStats(M_GameState curGameState)
        {
            // Initialize the list that will hold all the player game stats
            var playerGameStatsList = new List<M_Client_PlayerGameStats>();

            // Iterate through all the players in the game state to find the
            // game stats for each player
            foreach (M_Player player in curGameState.PlayerList)
            {
                // Pull out the list of stats for the current player
                var result = from qStats in curGameState.QuestionStatsList
                             where qStats.FocusedPlayerAnswer.PlayerId == player.PlayerId
                             select qStats;

                // Convert the result to an array
                var resultArray = result.ToArray();

                // Initialize all counter variables
                int playerSelectionsRed = 0;
                int playerSelectionsYellow = 0;
                int playerSelectionsGreen = 0;
                int playerSelectionsBlue = 0;

                int otherSelectionsRed = 0;
                int otherSelectionsYellow = 0;
                int otherSelectionsGreen = 0;
                int otherSelectionsBlue = 0;

                // Iterate across all the question stats in the result array
                // to tally up all the selections
                foreach (M_QuestionStats qStats in resultArray)
                {
                    //Tally up the selections by the focused player
                    switch (qStats.FocusedPlayerAnswer.PlayerAnswer)
                    {
                        case 1:
                            playerSelectionsRed++;
                            break;
                        case 2:
                            playerSelectionsYellow++;
                            break;
                        case 3:
                            playerSelectionsGreen++;
                            break;
                        case 4:
                            playerSelectionsBlue++;
                            break;
                    }

                    //Tally up the guesses by the other players
                    foreach (M_PlayerAnswer answer in qStats.OtherPlayerAnswers)
                    {
                        switch (answer.PlayerAnswer)
                        {
                            case 1:
                                otherSelectionsRed++;
                                break;
                            case 2:
                                otherSelectionsYellow++;
                                break;
                            case 3:
                                otherSelectionsGreen++;
                                break;
                            case 4:
                                otherSelectionsBlue++;
                                break;
                        }
                    }
                }

                //Create new player game stats and add it to the list
                var newPlayerGameStats = new M_Client_PlayerGameStats(player.PlayerId, curGameState.GameKey,
                    playerSelectionsRed, playerSelectionsYellow, playerSelectionsGreen, playerSelectionsBlue,
                    otherSelectionsRed, otherSelectionsYellow, otherSelectionsGreen, otherSelectionsBlue);

                playerGameStatsList.Add(newPlayerGameStats);
            }

            return playerGameStatsList;
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
        public M_Client_GameState GetClientGameState(string gameKey, int clientGameStateId)
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
        private M_Client_GameState BuildClientGameState(M_GameState curGameState, int clientGameStateId)
        {
            M_Client_GameState clientGameState = new M_Client_GameState();

            clientGameState.GameKey = curGameState.GameKey;
            // CLIENT STATE IDENTIFIER
            clientGameState.ClientViewCode = clientGameStateId;
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
            clientGameState.CurrentQuestionId = curGameState.FocusedQuestionId;
            clientGameState.QuestionStats = curGameState.QuestionStats;
            clientGameState.PlayerGameStatsList = curGameState.PlayerGameStatsList;

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