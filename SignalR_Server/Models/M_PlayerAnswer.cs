using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SignalR_Server.Models
{
    public class M_PlayerAnswer
    {
		[BsonElement("PlayerId")]
        public string PlayerId { get; set; }

		[BsonElement("PlayerAnswer")]
		public int PlayerAnswer { get; set; }

		[BsonElement("GameKey")]
		public string GameId { get; set; }

        public M_PlayerAnswer()
        {
        }

        public M_PlayerAnswer(string pId)
        {
            PlayerId = pId;
        }
    }
}
