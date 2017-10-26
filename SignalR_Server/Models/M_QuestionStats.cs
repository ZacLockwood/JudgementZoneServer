using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;

namespace SignalR_Server.Models
{
    public class M_QuestionStats
    {
		[BsonElement("CurrentQuestionId")]
        public int QuestionId { get; set; }

		[BsonElement("CurrentRoundNum")]
		public int GameRound { get; set; }

        [BsonElement("FocusedPlayerAnswer")]
        public M_PlayerAnswer FocusedPlayerAnswer{ get; set; }

        [BsonElement("OtherPlayerAnswers")]
        public IList<M_PlayerAnswer> OtherPlayerAnswers { get; set; }

		[BsonElement("GameKey")]
		public string GameId { get; set; }

        public M_QuestionStats()
        {
        }

        public M_QuestionStats(int qId, int gRound, M_PlayerAnswer fpAnswer, string gId)
        {
            QuestionId = qId;
            GameRound = gRound;
            FocusedPlayerAnswer = fpAnswer;
            OtherPlayerAnswers = new List<M_PlayerAnswer>();
            GameId = gId;
        }
    }
}
