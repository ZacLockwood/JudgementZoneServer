using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SignalR_Server.Models
{
    public class M_Player
    {
		[BsonElement("PlayerId")]
        public string PlayerId { get; set; } = Guid.NewGuid().ToString();

		[BsonElement("PlayerName")]
		public string PlayerName { get; set; }

        [BsonElement("SignalRConnectionId")]
        public string SignalRConnectionId { get; set; }

        [BsonElement("IsReadyToStart")]
        public bool IsReadyToStart { get; set; }

        public M_Player()
        {
        }
    }
}
