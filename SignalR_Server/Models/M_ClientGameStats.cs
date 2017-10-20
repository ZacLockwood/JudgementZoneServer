using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Collections.Generic;

namespace SignalR_Server.Models
{
    public class M_ClientGameStats
    {
        // KEY
        [BsonElement("GameKey")]
        public string GameKey { get; set; }

        // LIST OF GAME STATS OBJECTS FOR EACH PLAYER
        [BsonElement("PlayerGameStatsList")]
        public IList<M_ClientPlayerGameStats> ClientPlayerGameStats { get; set; }

        public M_ClientGameStats()
        {
        }
    }
}