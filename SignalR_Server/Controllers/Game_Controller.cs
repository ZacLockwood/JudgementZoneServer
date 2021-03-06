﻿using System;
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
                throw new Exception("Couldn't add the player to the game.");
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
            catch (MongoConnectionException ex)
            {
                string msg = ex.Message;
                return null;
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
                list = dbConnector.PullInQuestions();
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

            M_AnswerStats curAnswerStats = curGameState.GetAnswerStats();

            if (curAnswerStats.FocusedPlayerAnswer.PlayerAnswer == 0)
            {
                curAnswerStats.FocusedPlayerAnswer = newAnswer;
            }
            else
            {
                curAnswerStats.OtherPlayerAnswers.Add(newAnswer);
            }

            curGameState.UpdateAnswerStats(curAnswerStats);
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

            curGameState.ChooseNextFocusedPlayer();
            curGameState.GenerateNextQuestion();

            UpdateGameState(curGameState);
        }

        #endregion

        #region Get game value methods

        // Returns the answer stats for the current hand
        public M_AnswerStats GetHandAnswerStats(string gameKey)
        {            
            return GetGame(gameKey).GetAnswerStats();
        }
        
        // Returns the game stats
        public IList<M_AnswerStats> GetGameStats(string gameKey)
        {
            M_GameState curGameState = GetGame(gameKey);
            return curGameState.GameAnswerStats;
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
            if (GetGame(gameKey).GetAnswerStats().OtherPlayerAnswers.Count == 0)
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

            if (curGameState.GetAnswerStats().OtherPlayerAnswers.Count == curGameState.GamePlayers.Count - 1)
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
            catch (MongoConnectionException ex)
            {
                string msg = ex.Message;
                return null;
            }
        }

        #endregion
    }
}