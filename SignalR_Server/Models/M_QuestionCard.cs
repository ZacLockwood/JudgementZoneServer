using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace SignalR_Server.Models
{
    public class M_QuestionCard
    {
		[BsonElement("CurrentQuestionId")]
		public int QuestionId { get; set; }

		[BsonElement("QuestionDeck")]
		public int QuestionDeck { get; set; }

		[BsonElement("CurrentRoundNum")]
		public int GameRound { get; set; }

		[BsonElement("QuestionText")]
		public string QuestionText { get; set; }

		[BsonElement("RedAnswer")]
		public string RedAnswer { get; set; }

		[BsonElement("YellowAnswer")]
		public string YellowAnswer { get; set; }

		[BsonElement("GreenAnswer")]
		public string GreenAnswer { get; set; }

		[BsonElement("BlueAnswer")]
		public string BlueAnswer { get; set; }

        [BsonElement("DateCreated")]
        public DateTimeOffset DateCreated { get; set; }        

        [BsonElement("DateUpdated")]
        public DateTimeOffset DateModified { get; set; }

        public M_QuestionCard()
        {
        }
    }
}
