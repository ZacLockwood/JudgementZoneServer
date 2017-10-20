using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SignalR_Server.Models
{
    public class M_ClientPlayerGameStats
    {
        // KEY
        [BsonElement("PlayerId")]
        public string PlayerId { get; set; }

        // LINK TO PARENT
        [BsonElement("GameKey")]
        public string GameKey { get; set; }

        // CONTEXT
        [BsonElement("PercentRedSelections")]
        public float PercentRedSelections { get; set; }

        [BsonElement("PercentYellowSelections")]
        public float PercentYellowSelections { get; set; }

        [BsonElement("PercentGreenSelections")]
        public float PercentGreenSelections { get; set; }

        [BsonElement("PercentBlueSelections")]
        public float PercentBlueSelections { get; set; }

        [BsonElement("NumRedGuesses")]
        public float PercentRedGuesses { get; set; }

        [BsonElement("NumYellowGuesses")]
        public float PercentYellowGuesses { get; set; }

        [BsonElement("NumGreenGuesses")]
        public float PercentGreenGuesses { get; set; }

        [BsonElement("NumBlueGuesses")]
        public float PercentBlueGuesses { get; set; }

        public M_ClientPlayerGameStats()
        {
        }
    }
}