using System;
using System.Windows.Forms;

namespace RemoteServiceRestart
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void RestartServiceButton_Click(object sender, EventArgs e)
        {
            ServiceController sc = new ServiceController(ServerNameTextbox.Text, UserNameTextbox.Text, PasswordTextBox.Text, ServiceNameTextbox.Text);

            try
            {
                sc.Connect();
                UpdateStatusText("Connected to server.");

                string state = sc.GetServiceStatus();
                UpdateStatusText("State before call to stop: " + state);

                sc.StopService();
                UpdateStatusText("Call to stop service complete.");

                state = sc.GetServiceStatus();
                UpdateStatusText("State: " + state);

                sc.WaitForServiceToStop();
                UpdateStatusText("Service successfully stopped.");

                sc.StartService();
                UpdateStatusText("Call to start service complete.");

                state = sc.GetServiceStatus();
                UpdateStatusText("State: " + state);

                sc.WaitForServiceToBeRunning();

                UpdateStatusText("");
            }
            catch (Exception ex)
            {
                UpdateStatusText(ex.Message);
            }
        }

        private void UpdateStatusText(string text)
        {
            StatusTextBox.Text = text + "..." + Environment.NewLine + StatusTextBox.Text;
        }
    }
}