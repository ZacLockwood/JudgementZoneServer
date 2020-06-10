using System;
using System.Data.SqlClient;
using SignalR_Server.Models;
using System.Collections.Generic;
using System.Linq;

namespace SignalR_Server.Connectors
{
    public class QuestionDB_Connector
    {
        #region Global values

        string connectionString = ConnectionConstants.QUESTIONDB_CONNECTIONSTRING;
        SqlConnection connection = null;

        #endregion

        #region Constructors

        public QuestionDB_Connector()
        {

        }

        #endregion

        #region Question query methods

        public IList<M_QuestionCard> PullInQuestions()
        {
            IList<M_QuestionCard> questionList = new List<M_QuestionCard>();

            try
            {
                using (connection = new SqlConnection(connectionString))
                {
                    string queryString = "SELECT QuestionID, QuestionDeck, CurrentRoundNum, QuestionText, RedAnswer, YellowAnswer, GreenAnswer, BlueAnswer FROM QuestionCard";
                    SqlDataReader reader;
                    SqlCommand command = new SqlCommand(queryString, connection);

                    connection.Open();
                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        M_QuestionCard qCard = new M_QuestionCard();

                        qCard.QuestionId = reader.GetInt32(0);
                        qCard.QuestionDeck = reader.GetInt32(1);
                        qCard.GameRound = reader.GetInt32(2);
                        qCard.QuestionText = reader.GetString(3);
                        qCard.RedAnswer = reader.GetString(4);
                        qCard.YellowAnswer = reader.GetString(5);
                        qCard.GreenAnswer = reader.GetString(6);
                        qCard.BlueAnswer = reader.GetString(7);

                        questionList.Add(qCard);
                    }
                    command.Connection.Close();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return questionList;
        }

        public IList<M_QuestionCard> PullInQuestions(int numPlayers, int numRounds)
        {
            IList<M_QuestionCard> questionList = new List<M_QuestionCard>();

            for (int i = 1; i <= numRounds; i++)
            {
                IList<M_QuestionCard> roundList = GetQuestionsForRound(i);

                Random rand = new Random();
                int[] randArray = Enumerable.Range(0, roundList.Count).OrderBy(x => rand.Next()).Take(numPlayers).ToArray();

                for (int j = 0; j < numPlayers; j++)
                {
                    questionList.Add(roundList[randArray[j]]);
                }
            }

            return questionList;
        }

        public IList<M_QuestionCard> GetQuestionsForRound(int round)
        {
            IList<M_QuestionCard> questionList = new List<M_QuestionCard>();

            try
            {
                using (connection = new SqlConnection(connectionString))
                {
                    string queryString = "SELECT QuestionID, QuestionDeck, GameRound, QuestionText, RedAnswer, YellowAnswer, GreenAnswer, BlueAnswer " +
                    "FROM QuestionCard " +
                        "WHERE GameRound = " + round;

                    SqlDataReader reader;
                    SqlCommand command = new SqlCommand(queryString, connection);

                    connection.Open();
                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        M_QuestionCard qCard = new M_QuestionCard();

                        qCard.QuestionId = reader.GetInt32(0);
                        qCard.QuestionDeck = reader.GetInt32(1);
                        qCard.GameRound = reader.GetInt32(2);
                        qCard.QuestionText = reader.GetString(3);
                        qCard.RedAnswer = reader.GetString(4);
                        qCard.YellowAnswer = reader.GetString(5);
                        qCard.GreenAnswer = reader.GetString(6);
                        qCard.BlueAnswer = reader.GetString(7);

                        questionList.Add(qCard);
                    }
                    command.Connection.Close();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return questionList;
        }

        public List<M_QuestionCard> GetModifiedQuestionList(DateTimeOffset clientLastUpdate)
        {
            List<M_QuestionCard> modifiedQuestionList = new List<M_QuestionCard>();

            try
            {
                using (connection = new SqlConnection(connectionString))
                {
                    string queryString = "SELECT QuestionID, QuestionDeck, GameRound, QuestionText, " +
                        "RedAnswer, YellowAnswer, GreenAnswer, BlueAnswer, DateCreated, DateModified " +
                        "FROM QuestionCard " +
                        "WHERE DateModified > '" + clientLastUpdate.ToString("yyyy-MM-dd HH:mm:ss") + "';";
                    SqlDataReader reader;
                    SqlCommand command = new SqlCommand(queryString, connection);

                    connection.Open();
                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        M_QuestionCard qCard = new M_QuestionCard();
                        qCard.QuestionId = reader.GetInt32(0);
                        qCard.QuestionDeck = reader.GetInt32(1);
                        qCard.GameRound = reader.GetInt32(2);
                        qCard.QuestionText = reader.GetString(3);
                        qCard.RedAnswer = reader.GetString(4);
                        qCard.YellowAnswer = reader.GetString(5);
                        qCard.GreenAnswer = reader.GetString(6);
                        qCard.BlueAnswer = reader.GetString(7);
                        qCard.DateCreated = reader.GetDateTimeOffset(8);
                        qCard.DateModified = reader.GetDateTimeOffset(9);

                        modifiedQuestionList.Add(qCard);
                    }
                    command.Connection.Close();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return modifiedQuestionList;
        }

        #endregion
    }
}