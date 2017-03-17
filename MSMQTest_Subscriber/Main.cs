using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MSMQ_Subscriber
{
    public partial class Main : Form
    {
        private Subscriber subscriber;

        public Main()
        {
            InitializeComponent();
        }

        private void cmdGo_Click(object sender, EventArgs e)
        {
            this.subscriber = new Subscriber();
            this.subscriber.RunTest();
        }
    }
}
