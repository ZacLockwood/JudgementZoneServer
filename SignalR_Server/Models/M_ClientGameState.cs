using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;

namespace SignalR_Server.Models
{
    public class M_ClientGameState
    {
        [BsonId]
        public string GameKey { get; set; }

        // CLIENT STATE IDENTIFIER
        [BsonElement("ClientGameStateId")]
        public int ClientGameStateId { get; set; }

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

        [BsonElement("FocusedQuestionId")]
        public int FocusedQuestionId { get; set; }

        [BsonElement("ClientFocusedQuestionStats")]
        public M_ClientQuestionStats ClientFocusedQuestionStats { get; set; }

        [BsonElement("ClientGameStats")]
        public M_ClientGameStats ClientGameStats { get; set; }

        #region Constructor

        public M_ClientGameState()
        {
            GameKey = TemporaryKeyGenerator();
        }

        public M_ClientGameState(M_Player firstPlayer)
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