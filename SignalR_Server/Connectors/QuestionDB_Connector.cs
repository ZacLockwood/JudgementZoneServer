using System;
using System.Data.SqlClient;
using SignalR_Server.Models;
using System.Collections.Generic;

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
                    string queryString = "SELECT QuestionID, QuestionDeck, GameRound, QuestionText, RedAnswer, YellowAnswer, GreenAnswer, BlueAnswer FROM QuestionCard";
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

        #endregion
    }
}