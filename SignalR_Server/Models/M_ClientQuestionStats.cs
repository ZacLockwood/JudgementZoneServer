using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SignalR_Server.Models
{
    public class M_ClientQuestionStats
    {
        // KEY
        [BsonElement("GameKey")]
        public string GameKey { get; set; }

        [BsonElement("PlayerId")]
        public string PlayerId { get; set; }

        // CONTEXT
        [BsonElement("QuestionId")]
        public int QuestionId { get; set; }

        [BsonElement("IsPlayerCorrect")]
        public bool IsPlayerCorrect { get; set; }

        [BsonElement("CorrectAnswerId")]
        public int CorrectAnswerId { get; set; }

        [BsonElement("NumRedGuesses")] //red is 0
        public int NumRedGuesses { get; set; }

        [BsonElement("NumYellowGuesses")] //yellow is 1
        public int NumYellowGuesses { get; set; }

        [BsonElement("NumGreenGuesses")] //green is 2
        public int NumGreenGuesses { get; set; }

        [BsonElement("NumBlueGuesses")] //blue is 3
        public int NumBlueGuesses { get; set; }

        public M_ClientQuestionStats()
        {
        }
    }
}