using MSMQ_Subscriber.Properties;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Messaging;
using System.Threading.Tasks;
using System.Transactions;

namespace MSMQ_Subscriber
{
    public class Subscriber
    {
        private MessageQueue messageQueue;

        public void RunTest()
        {
            // Open the message queue ready for reading.
            messageQueue = new MessageQueue(Settings.Default.QueueName);
            messageQueue.PeekCompleted += inputQueue_PeekCompleted;
            messageQueue.BeginPeek(TimeSpan.FromDays(1));


        }

        private void inputQueue_PeekCompleted(object sender, PeekCompletedEventArgs e)
        {
            try
            {


                using (var ts = new TransactionScope())
                {
                    var inputMsg = messageQueue.Receive(MessageQueueTransactionType.Automatic);

                    var id = 0;
                    if (!int.TryParse(inputMsg.Label, out id)) throw new System.Exception("Could not read the message label");

                    using (var cn = new SqlConnection(Settings.Default.CS))
                    {
                        // Connect to the database
                        cn.Open();

                        // Read the GUID for this entry from the database
                        Guid guid;
                        using (var cmd = new SqlCommand())
                        {
                            cmd.Connection = cn;
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.CommandText = "dbo.usp_Get";
                            cmd.Parameters.Add("ID", SqlDbType.Int).Value = id;

                            using (var dr = cmd.ExecuteReader())
                            {
                                if (!dr.Read()) throw new Exception("Call to usp_Get returned no data for ID " + id);

                                // Read back the GUID of the record
                                guid = (Guid)dr["GUID"];
                            }
                        }

                        if (guid == Guid.Empty) throw new Exception("No GUID was read");

                        // Update the record based on it's GUID
                        using (var cmd = new SqlCommand())
                        {
                            cmd.Connection = cn;
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.CommandText = "dbo.usp_Update";
                            cmd.Parameters.Add("GUID", SqlDbType.UniqueIdentifier).Value = guid;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    ts.Complete();
                }
                messageQueue.BeginPeek(TimeSpan.FromDays(1));
            }

            catch (Exception ex)
            {
                File.AppendAllText("error.txt", ex.ToString() + System.Environment.NewLine + System.Environment.NewLine);
                messageQueue.BeginPeek(TimeSpan.FromDays(1));
            }
        }

    }
}
