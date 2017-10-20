using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;

namespace SignalR_Server.Models
{
    public class M_GameState
    {
        #region Server only values

        [BsonId]
        public string GameKey { get; set; }

		[BsonElement("GameType")]
		public int GameType { get; set; }

        [BsonElement("QuestionList")]
		public IList<M_QuestionCard> QuestionList { get; set; }

        [BsonElement("NumStartRequests")]
        public int NumStartRequests { get; set; }

        [BsonElement("QuestionCounter")]
        public int QuestionCounter { get; set; }

        [BsonElement("QuestionStatsList")]
        public IList<M_QuestionStats> QuestionStatsList { get; set; }

        #endregion

        #region Client values

        // STATE-INDEPENDENT DATA
        [BsonElement("PlayerList")]
        public IList<M_Player> PlayerList { get; set; }

        // GAME CYCLE METRICS
        [BsonElement("CurrentQuestionNum")]
        public int CurrentQuestionNum { get; set; }

        [BsonElement("MaxQuestionNum")]
        public int MaxQuestionNum { get; set; }

        [BsonElement("CurrentRoundNum")]
        public int CurrentRoundNum { get; set; }

        [BsonElement("MaxRoundNum")]
        public int MaxRoundNum { get; set; }

        [BsonElement("IsNewRound")]
        public bool IsNewRound { get; set; }

        // GAME STATE CONTEXT
        [BsonElement("FocusedPlayerId")]
        public string FocusedPlayerId { get; set; }

        [BsonElement("FocusedQuestionId")]
        public int FocusedQuestionId { get; set; }

        [BsonElement("FocusedQuestionStats")]
        public M_ClientQuestionStats FocusedClientQuestionStats { get; set; }

        [BsonElement("ClientGameStats")]
        public M_ClientGameStats ClientGameStats { get; set; }

        #endregion

        #region Constructors

        public M_GameState()
        {
        }

        public M_GameState(M_Player firstPlayer)
        {
            GameKey = TemporaryIdGenerator();
            GameType = 1;
            CurrentRoundNum = 1;
            MaxRoundNum = 3;
            NumStartRequests = 0;
            QuestionCounter = 0;
            IsNewRound = false;

            PlayerList = new List<M_Player>();
            QuestionList = new List<M_QuestionCard>();
            QuestionStatsList = new List<M_QuestionStats>();

            PlayerList.Add(firstPlayer);
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
                
        public void GenerateNextQuestion()
        {
            FocusedQuestionId = QuestionList[QuestionCounter].QuestionId;
            QuestionCounter++;

            M_PlayerAnswer emptyFocusedPlayerAnswer = new M_PlayerAnswer(FocusedPlayerId);

            M_QuestionStats newAnswerStats = new M_QuestionStats(FocusedQuestionId, CurrentRoundNum, emptyFocusedPlayerAnswer, GameKey);

            QuestionStatsList.Add(newAnswerStats);
        }

        #endregion
    }
}