using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SignalR_Server.Models
{
    public class M_Client_QuestionStats
    {
        // KEY
        [BsonElement("GameKey")]
        public string GameKey { get; set; }

        [BsonElement("PlayerId")]
        public string PlayerId { get; set; }

        // CONTEXT
        [BsonElement("CurrentQuestionId")]
        public int QuestionId { get; set; }

        [BsonElement("IsPlayerCorrect")]
        public bool IsPlayerCorrect { get; set; }

        [BsonElement("CorrectAnswerId")]
        public int CorrectAnswerId { get; set; }

        [BsonElement("RedGuesses")] //red is 1
        public int RedGuesses { get; set; }

        [BsonElement("YellowGuesses")] //yellow is 2
        public int YellowGuesses { get; set; }

        [BsonElement("GreenGuesses")] //green is 3
        public int GreenGuesses { get; set; }

        [BsonElement("BlueGuesses")] //blue is 4
        public int BlueGuesses { get; set; }

        public M_Client_QuestionStats()
        {
        }
    }
}