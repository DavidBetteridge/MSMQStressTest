using MSMQ_Subscriber.Properties;
using System;
using System.Messaging;
using System.Windows.Forms;

namespace MSMQ_Subscriber
{


    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Make sure that our message queue exists
            if (!MessageQueue.Exists(Settings.Default.QueueName))
                throw new System.Exception("Could not find a message queue called " + Settings.Default.QueueName);

            Application.Run(new Main());
            
        }


    }
}
