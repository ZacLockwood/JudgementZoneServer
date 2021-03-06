﻿using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using SignalR_Server.Models;
using SignalR_Server.Connectors;

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
            var gameState = new M_GameState();

            try
            {
                gameState = gameController.CreateNewGameState(myPlayer);
            }
            catch (Exception e)
            {
                Clients.Caller.DisplayError("Oops! Something went wrong :( Try that again!", e);
            }

            string gameKey = gameState.GameId;
            Groups.Add(Context.ConnectionId, gameKey);
            UpdatePlayerListScreen(gameKey);
        }

        //Client requests to join game
        public void RequestJoinGame(M_Player myPlayer, string gameKey)
        {

            if (gameController.GetGame(gameKey).GameQuestions.Count == 0)
            {
                try
                {
                    gameController.AddPlayerToGame(myPlayer, gameKey);
                }
                catch (Exception e)
                {
                    Clients.Caller.DisplayError("Oops! Something went wrong :( Try that again!", e);
                }

                Groups.Add(Context.ConnectionId, gameKey);
                UpdatePlayerListScreen(gameKey);
            }
            else
            {
                Clients.Caller.DisplayError("Sorry! We can't add you to a game that is already started. :(");
            }
        }

        //Client requests game start
        public void RequestStartGame(M_Player myPlayer, string gameKey)
        {
            try
            {
                bool beginGame = gameController.PlayerIsReadyToStart(myPlayer, gameKey);

                if (beginGame)
                {
                    M_GameState curGameState = gameController.GetGame(gameKey);
                    Clients.Group(gameKey).DisplayQuestion(curGameState.FocusedPlayerId, curGameState.GetFocusedQuestion());
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
            try
            {
                gameController.AddAnswer(myAnswer, gameKey);
            }
            catch (Exception e)
            {
                Clients.Caller.DisplayError("Oops! Something went wrong :( Try that again!", e);
            }

            if (gameController.IsFirstAnswer(gameKey))
            {
                Clients.Others.EnableAnswerSubmission();
            }
            else if (gameController.IsLastAnswer(gameKey))
            {
                Clients.All.DisplayQuestionStats(gameController.GetHandAnswerStats(gameKey));
            }
        }

        //Client[FP] requests continue
        public void RequestContinueToNextQuestion(string gameKey)
        {
            if (gameController.IsRoundOver(gameKey))
            {
                if (gameController.IsGameOver(gameKey))
                {
                    Clients.Group(gameKey).DisplayGameStats(gameController.GetGameStats(gameKey));
                    Groups.Remove(Context.ConnectionId, gameKey);
                }
                else
                {
                    gameController.BeginNewRound(gameKey);

                    M_GameState curGameState = gameController.GetGame(gameKey);
                    Clients.Group(gameKey).DisplayQuestion(curGameState.FocusedPlayerId, curGameState.GetFocusedQuestion());
                }
            }
            else
            {
                gameController.BeginNewQuestion(gameKey);

                M_GameState curGameState = gameController.GetGame(gameKey);
                Clients.Group(gameKey).DisplayQuestion(curGameState.FocusedPlayerId, curGameState.GetFocusedQuestion());
            }
        }

        #endregion

        #region Helper Methods

        private void UpdatePlayerListScreen(string gameKey)
        {
            IList<M_Player> playerList = gameController.GetPlayerList(gameKey);

            Clients.Caller.DisplayGameKey(gameKey);
            Clients.Group(gameKey).DisplayPlayerList(playerList);
        }

        #endregion
    }
}