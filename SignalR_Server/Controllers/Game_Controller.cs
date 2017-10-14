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
        public M_GameState CreateNewGameState(M_Player myPlayer)
        {
            var collection = gameDbConnector.GetCollection();
            M_GameState newGameState = new M_GameState(myPlayer);

            try
            {
                collection.InsertOne(newGameState);
                return newGameState;
            }
            catch (MongoCommandException ex)
            {
                string msg = ex.Message;
                return null;
            }
        }

        // Adds a single player to the given game
        public void AddPlayerToGame(M_Player myPlayer, string gameKey)
        {
            try
            {
                M_GameState curGameState = GetGame(gameKey);
                curGameState.GamePlayers.Add(myPlayer);
                UpdateGameState(curGameState);
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
                return curGameState.GamePlayers;
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't get the player list.", e);
            }
        }

        // Adds the calling player saying they're ready to begin
        public bool PlayerIsReadyToStart(M_Player myPlayer, string gameKey)
        {            
            M_GameState curGameState = GetGame(gameKey);
            curGameState.StartRequests++;

            if (curGameState.StartRequests == curGameState.GamePlayers.Count)
            {
                PrepGameForStart(curGameState);
                return true;
            }
            else
            {
                UpdateGameState(curGameState);
                return false;
            }
        }

        // Adds questions to the game state and updates the DB
        private void PrepGameForStart(M_GameState curGameState)
        {
            QuestionDB_Connector dbConnector = new QuestionDB_Connector();
            IList<M_QuestionCard> list = null;

            try
            {
                list = dbConnector.PullInQuestions(curGameState.GamePlayers.Count, 3);
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
                curGameState.GameQuestions = list;              
                curGameState.GenerateNextQuestion();
                UpdateGameState(curGameState);
            }
        }

        #endregion

        #region Gameplay methods

        // Adds the calling player's answer to the game stats
        public void AddAnswer(M_PlayerAnswer newAnswer, string gameKey)
        {
            M_GameState curGameState = GetGame(gameKey);
            M_AnswerStats curAnswerStats = GetAnswerStats(curGameState);
            var index = curGameState.GameAnswerStats.IndexOf(curAnswerStats);

            if (curAnswerStats.FocusedPlayerAnswer.PlayerAnswer == 0)
            {
                curAnswerStats.FocusedPlayerAnswer = newAnswer;
            }
            else
            {
                curAnswerStats.OtherPlayerAnswers.Add(newAnswer);
            }

            curGameState.GameAnswerStats[index] = curAnswerStats;
            UpdateGameState(curGameState);
        }        

        // Begin a new round of questions
        public void BeginNewRound(string gameKey)
        {
            M_GameState curGameState = GetGame(gameKey);

            curGameState.GameRound++;
            curGameState.FocusedPlayerId = curGameState.GamePlayers.First().PlayerId;
            curGameState.GenerateNextQuestion();

            UpdateGameState(curGameState);
        }

        // Begin a new question with a new focused player
        public void BeginNewQuestion(string gameKey)
        {
            M_GameState curGameState = GetGame(gameKey);

            // Finds the focused player in the GamePlayers list
            var result = from player in curGameState.GamePlayers
                         where player.PlayerId == curGameState.FocusedPlayerId
                         select player;

            // Finds the index of the focused player in the GamePlayers list
            var focusedPlayer = result.First();
            var index = curGameState.GamePlayers.IndexOf(focusedPlayer);

            // Updates the focused player id to the new focused player
            curGameState.FocusedPlayerId = curGameState.GamePlayers[index + 1].PlayerId;
            curGameState.GenerateNextQuestion();

            UpdateGameState(curGameState);
        }

        #endregion

        #region Get game value methods

        // Returns the stats for the finished question
        public M_AnswerStats GetQuestionStats(string gameKey)
        {
            return GetAnswerStats(GetGame(gameKey));
        }
        
        // Returns the game stats
        public IList<M_AnswerStats> GetGameStats(string gameKey)
        {
            return GetGame(gameKey).GameAnswerStats;
        }

        public Tuple<string, M_QuestionCard> GetFocusedPlayerIdAndQuestion(string gameKey)
        {
            M_GameState curGameState = GetGame(gameKey);

            var result = from qCard in curGameState.GameQuestions
                         where qCard.QuestionId.Equals(curGameState.FocusedQuestionId)
                         select qCard;

            M_QuestionCard focusedQuestion = result.First();

            return new Tuple<string, M_QuestionCard>(curGameState.FocusedPlayerId, focusedQuestion);
            
        }
        
        #endregion

        #region Check game state methods

        // Checks to see if round is over
        public bool IsRoundOver(string gameKey)
        {
            //Leave this call because it saves an extra search of the DB
            M_GameState curGameState = GetGame(gameKey);

            if (curGameState.FocusedPlayerId.Equals(curGameState.GamePlayers.Last().PlayerId))
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
            if (GetGame(gameKey).GameRound == 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Checks if the answer just given is the first answer of the hand
        public bool IsFirstAnswer(string gameKey)
        {
            if (GetAnswerStats(GetGame(gameKey)).OtherPlayerAnswers.Count == 0)
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

            if (GetAnswerStats(curGameState).OtherPlayerAnswers.Count == curGameState.GamePlayers.Count - 1)
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
        private M_AnswerStats GetAnswerStats(M_GameState curGameState)
        {
            var result = from aStats in curGameState.GameAnswerStats
                         where aStats.GameRound == curGameState.GameRound 
                         && aStats.FocusedPlayerAnswer.PlayerId.Equals(curGameState.FocusedPlayerId)
                         select aStats;

            return result.First();
        }

        // Updates the given game state on the DB
        private void UpdateGameState(M_GameState curGameState)
        {
            var collection = gameDbConnector.GetCollection();
            var filter = Builders<M_GameState>.Filter.Eq("_id", curGameState.GameId);
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

        #endregion
    }
}