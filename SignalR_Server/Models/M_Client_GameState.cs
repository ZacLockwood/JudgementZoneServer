using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;

namespace SignalR_Server.Models
{
    public class M_Client_GameState
    {
        [BsonId]
        public string GameKey { get; set; }

        // CLIENT STATE IDENTIFIER
        [BsonElement("ClientViewCode")]
        public int ClientViewCode { get; set; }

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
        [BsonElement("CanSubmitAnswer")]
        public bool CanSubmitAnswer { get; set; }

        [BsonElement("FocusedPlayerId")]
        public string FocusedPlayerId { get; set; }

        [BsonElement("CurrentQuestionId")]
        public int CurrentQuestionId { get; set; }

        [BsonElement("QuestionStats")]
        public M_Client_QuestionStats QuestionStats { get; set; }

        [BsonElement("PlayerGameStatsList")]
        public IList<M_Client_PlayerGameStats> PlayerGameStatsList { get; set; }

        #region Constructor

        public M_Client_GameState()
        {
            GameKey = TemporaryKeyGenerator();
        }

        public M_Client_GameState(M_Player firstPlayer)
        {
            GameKey = TemporaryKeyGenerator();

            PlayerList.Add(firstPlayer);
            FocusedPlayerId = firstPlayer.PlayerId;
        }

        #endregion

        #region Helper Method

        private string TemporaryKeyGenerator()
        {
            var gameKey = "";
            var randomNumGen = new Random();
            for (int i = 0; i < 5; i++)
            {
                gameKey += randomNumGen.Next(0, 10).ToString();
            }
            return gameKey;
        }

        #endregion
    }
}