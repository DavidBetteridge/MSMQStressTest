using MSMQTest_Publisher.Properties;
using System;
using System.Data.SqlClient;
using System.Messaging;
using System.Transactions;

namespace MSMQTest_Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Make sure that our message queue exists
                if (!MessageQueue.Exists(Settings.Default.QueueName))
                    throw new System.Exception("Could not find a message queue called " + Settings.Default.QueueName);

                Console.WriteLine("Press A key to begin");
                Console.ReadKey(true);

                // Create our data
                for (int i = 0; i < 10000; i++)
                {
                    Console.WriteLine("Creating transaction " + i);
                    CreateTransaction();
                }

                Console.WriteLine("Complete");

            }

            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(ex.ToString());
                Console.ForegroundColor = ConsoleColor.White    ;
            }

            Console.WriteLine("Press A key to exist");
            Console.ReadKey(true);
        }

        private static void CreateTransaction()
        {
            // Start a transaction
            using (var ts = new TransactionScope())
            {
                using (var cn = new SqlConnection(Settings.Default.CS))
                {
                    // Connect to the database
                    cn.Open();

                    // Call a sp which does an insert
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = cn;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "dbo.usp_Insert";

                        var newRecordID = 0;
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (!dr.Read()) throw new Exception("Call to usp_Insert returned no data!");

                            // Read back the ID of the inserted record
                            newRecordID = (int)dr["ID"];
                        }

                        // Write the ID to a message queue
                        SendTransactionalMessage(Settings.Default.QueueName, newRecordID.ToString(), newRecordID.ToString());
                    }
                }

                //  Commit transaction
                ts.Complete();
            }
        }

        private static void SendTransactionalMessage(string queueName, string messageBody, string messageLabel)
        {
            var messageQueue = new MessageQueue(queueName);
            var message = new Message(messageBody);
            message.Label = messageLabel;
            message.UseDeadLetterQueue = true;
            messageQueue.Send(message, MessageQueueTransactionType.Automatic);
        }
    }
}
