using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;

namespace SignalR_Server.Models
{
    public class M_GameState
    {
        [BsonId]
        public string GameId { get; set; }

		[BsonElement("GameType")]
		public int GameType { get; set; }

        [BsonElement("GamePlayers")]
        public IList<M_Player> GamePlayers { get; set; }

		[BsonElement("GameQuestions")]
		public IList<M_QuestionCard> GameQuestions { get; set; }

        [BsonElement("GameRound")]
        public int GameRound { get; set; }

        [BsonElement("FocusedPlayerId")]
        public string FocusedPlayerId { get; set; }
      
		[BsonElement("FocusedQuestionId")]
		public int FocusedQuestionId { get; set; }

        [BsonElement("GameAnswerStats")]
        public IList<M_AnswerStats> GameAnswerStats { get; set; }

        [BsonElement("StartRequests")]
        public int StartRequests { get; set; }

        [BsonElement("QuestionCount")]
        public int QuestionCount { get; set; }

        #region Constructors

        public M_GameState()
        {
        }

        public M_GameState(M_Player firstPlayer)
        {
            GameId = TemporaryIdGenerator();
            GameType = 1;
            GameRound = 1;
            StartRequests = 0;
            QuestionCount = 0;

            GamePlayers = new List<M_Player>();
            GameQuestions = new List<M_QuestionCard>();
            GameAnswerStats = new List<M_AnswerStats>();

            GamePlayers.Add(firstPlayer);
            FocusedPlayerId = firstPlayer.PlayerId;
        }

        private string TemporaryIdGenerator()
        {
            var gameId = "";
            var randomNumGen = new Random();
            for (int i = 0; i < 5; i++)
            {
                gameId += randomNumGen.Next(0, 10).ToString();
            }
            return gameId;
        }

        #endregion

        #region Helper Methods

        //THIS IS TERRIBLE, CHANGE LATER
        public M_AnswerStats GetAnswerStats()
        {
            int i = 0;
            while (GameAnswerStats[i].GameRound != GameRound || !GameAnswerStats[i].FocusedPlayerAnswer.PlayerId.Equals(FocusedPlayerId))
            {
                i++;
            }

            return GameAnswerStats[i];
        }

        //THIS IS TERRIBLE, CHANGE LATER
        public void UpdateAnswerStats(M_AnswerStats newAnswerStats)
        {
            int i = 0;
            while (GameAnswerStats[i].GameRound != GameRound || !GameAnswerStats[i].FocusedPlayerAnswer.PlayerId.Equals(FocusedPlayerId))
            {
                i++;
            }

            GameAnswerStats[i] = newAnswerStats;
        }
        
        //THIS IS TERRIBLE, CHANGE LATER
        public M_QuestionCard GetFocusedQuestion()
        {
            int i = 0;
            while (!GameQuestions[i].QuestionId.Equals(FocusedQuestionId))
            {
                i++;
            }

            return GameQuestions[i];
        }

        public void GenerateNextQuestion()
        {
            FocusedQuestionId = GameQuestions[QuestionCount].QuestionId;
            QuestionCount++;

            M_PlayerAnswer emptyFocusedPlayerAnswer = new M_PlayerAnswer(FocusedPlayerId);

            M_AnswerStats newAnswerStats = new M_AnswerStats(FocusedQuestionId, GameRound, emptyFocusedPlayerAnswer, GameId);

            GameAnswerStats.Add(newAnswerStats);
        }

        public void ChooseNextFocusedPlayer()
        {
            int i = 0;
            while (!GamePlayers[i].PlayerId.Equals(FocusedPlayerId))
            {
                i++;
            }

            FocusedPlayerId = GamePlayers[i + 1].PlayerId;

        }

        #endregion
    }
}