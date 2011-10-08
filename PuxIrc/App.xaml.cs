using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace PuxIrc
{
    partial class App
    {
        public App()
		{
			m_server = new IRC.Server();
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Window.Current.Content = new ServerLogin(this);
            Window.Current.Activate();
		}

		public void login(string server, string port, string nick)
		{
			m_server.address = server;
			m_server.port = port;
			m_server.user.Nick = nick;
			m_server.connect();

			Window.Current.Content = new ChatPage(this);
			Window.Current.Activate();
		}

		public void join(string channel)
		{
			m_channel = channel;
			m_server.sendJoin(channel);
		}

		public void sendMessage(string message)
		{
			m_server.sendPrivateMessage(m_channel, message);
		}

		public void setMessageReceiveHandler(IRC.Server.Receive receive)
		{
			m_server.receiveCallback = receive;
		}

		IRC.Server m_server;
		string m_channel;
    }
}
