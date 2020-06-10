using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SignalR_Server.Models
{
    public class M_Client_PlayerGameStats
    {
        // KEY
        [BsonElement("PlayerId")]
        public string PlayerId { get; set; }

        // LINK TO PARENT
        [BsonElement("GameKey")]
        public string GameKey { get; set; }

        // CONTEXT
        [BsonElement("PlayerSelectionsRed")]
        public int PlayerSelectionsRed { get; set; }

        [BsonElement("PlayerSelectionsYellow")]
        public int PlayerSelectionsYellow { get; set; }

        [BsonElement("PlayerSelectionsGreen")]
        public int PlayerSelectionsGreen { get; set; }

        [BsonElement("PlayerSelectionsBlue")]
        public int PlayerSelectionsBlue { get; set; }

        [BsonElement("OtherSelectionsRed")]
        public int OtherSelectionsRed { get; set; }

        [BsonElement("OtherSelectionsYellow")]
        public int OtherSelectionsYellow { get; set; }

        [BsonElement("OtherSelectionsGreen")]
        public int OtherSelectionsGreen { get; set; }

        [BsonElement("OtherSelectionsBlue")]
        public int OtherSelectionsBlue { get; set; }

        public M_Client_PlayerGameStats()
        {
        }

        public M_Client_PlayerGameStats(string pId, string gKey,
            int pSRed, int pSYellow, int pSGreen, int pSBlue,
            int oSRed, int oSYellow, int oSGreen, int oSBlue)
        {
            PlayerId = pId;
            GameKey = gKey;

            PlayerSelectionsRed = pSRed;
            PlayerSelectionsYellow = pSYellow;
            PlayerSelectionsGreen = pSGreen;
            PlayerSelectionsBlue = pSBlue;

            OtherSelectionsRed = oSRed;
            OtherSelectionsYellow = oSYellow;
            OtherSelectionsGreen = oSGreen;
            OtherSelectionsBlue = oSBlue;
        }
    }
}