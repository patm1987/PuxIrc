/*
 * Copyright (c) 2011 Patrick Martin
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * */

using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

/*!
 * \brief	Main entry point for the IRC GUI frontend
 * \note	this is very rough
 * */
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

		/*!
		 * \brief	login to a server
		 * \param	server the address of the server
		 * \param	port the port to connect to on the server
		 * \param	nick the nick (and username) to use
		 * */
		public void login(string server, string port, string nick)
		{
			m_server.address = server;
			m_server.port = port;
			m_server.user.Nick = nick;
			m_server.connect();

			Window.Current.Content = new ChatPage(this);
			Window.Current.Activate();
		}

		/*!
		 * \brief	joins a channel
		 * \param	channel the channel to join
		 * */
		public void join(string channel)
		{
			m_channel = channel;
			m_server.sendJoin(channel);
		}

		/*!
		 * \brief	sends a message to the last channel joined
		 * \param	message the message to send
		 * */
		public void sendMessage(string message)
		{
			m_server.sendPrivateMessage(m_channel, message);
		}

		/*!
		 * \brief	set some callback that will receive messages from the server
		 * */
		public void setMessageReceiveHandler(IRC.Server.Receive receive)
		{
			m_server.receiveCallback = receive;
		}

		IRC.Server m_server;
		string m_channel;
    }
}
