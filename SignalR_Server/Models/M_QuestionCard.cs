using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SignalR_Server.Models
{
    public class M_QuestionCard
    {
		[BsonElement("QuestionId")]
		public int QuestionId { get; set; }

		[BsonElement("QuestionDeck")]
		public int QuestionDeck { get; set; }

		[BsonElement("GameRound")]
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

		public M_QuestionCard()
        {
        }
    }
}
