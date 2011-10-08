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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace PuxIrc.IRC
{
	/*!
	 * \brief	The "Server" class handles communicating with the server (for the client)
	 * \note	this is the real meat and potatoes. Mmmm.
	 * 
	 * This class handles the IRC protocol for a client wishing to connect and interact with an IRC
	 * server. The protocol reference can be found here: http://www.irchelp.org/irchelp/rfc/rfc.html
	 * */
	public class Server
	{
		private const String kDefaultAddress = "irc.rizon.net";
		private const String kDefaultPort = "6667";
		private const String kMessageTerminator = "\r\n";

		//! \brief	message signature to conform to if you wish to receive notifications from the server
		public delegate void Receive(string message);

		//! \brief	enumeration for reporting the status of the conneciton wth a server
		public enum Status
		{
			Disconnected,
			Connecting,
			Connected
		}

		public Server()
		{
			m_user = new User();
			m_socket = new StreamSocket();
		}

		/*!
		 * \brief	begin an asynchronous connection with the server
		 * \note	server, port, and nick must be set before calling this
		 * */
		public void connect()
		{
			m_status = Status.Connecting;
			m_socket = new StreamSocket();
			try
			{
				StreamSocketConnectOperation connect = m_socket.ConnectAsync(new HostName(m_address), m_port, SocketProtectionLevel.PlainSocket);
				connect.Completed = connectComplete;
				connect.StartAsTask();
			}
			catch (System.IO.FileNotFoundException exception)
			{
				Debug.WriteLine(exception);
			}
		}

		//! \brief	asynchronously send a message to the server
		async private void sendMessage(string message)
		{
			m_outputStream.WriteString(message + kMessageTerminator);
			await m_outputStream.StoreAsync().StartAsTask();
		}

		//! \brief	finished asynchronously connecting to the server
		private void connectComplete(IAsyncAction result)
		{
			switch (result.Status)
			{
				case AsyncStatus.Completed:
					m_status = Status.Connected;

					m_inputStream = new DataReader(m_socket.InputStream);
					m_inputStream.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

					m_outputStream = new DataWriter(m_socket.OutputStream);
					m_outputStream.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

					sendLogin();

					// spin off a task to monitor responses from the server
					m_readTask = Task.Factory.StartNew(() => readFromStream());
					break;

				case AsyncStatus.Canceled:
				case AsyncStatus.Error:
					m_status = Status.Disconnected;
					break;
			}
		}

		//! \brief	login with the credentials previously provided to the server
		private void sendLogin()
		{
			sendNick(m_user.Nick);
			sendUser(m_user.Nick);
		}

		//! \brief	asynchronously read from the stream listening to the server
		async private void readFromStream()
		{
			string result = "";
			while (status == Status.Connected)
			{
				// TODO: find a better method to do this.
				// Continuously pull a single character from the server and see if we have a message yet
				await m_inputStream.LoadAsync(1);
				if (m_inputStream.UnconsumedBufferLength > 0)
				{
					result += m_inputStream.ReadString(m_inputStream.UnconsumedBufferLength);
					result = processServerResponse(result);
				}
			}
		}

		/*!
		 * \brief	parse a response from the server
		 * \param	message a single line to parse
		 * */
		string processServerResponse(string message)
		{
			int index = -1;
			do
			{
				index = message.IndexOf(kMessageTerminator);
				if (index != -1)
				{
					string prefix = message.Substring(0, index);
					string postfix = message.Substring(index + kMessageTerminator.Length);

					// NOTE: it may not be desired to block handled calls from the receive callback
					if (!handleResponse(prefix))
					{
						receiveCallback(prefix);
					}
					message = postfix;
				}
			} while (index != -1);
			return message;
		}

		/*!
		 * \brief	handle the server response to take care of any autonomous background tasks
		 * \return	true if the response was handled in some way, false if there was nothing done (not a failure)
		 * */
		bool handleResponse(string message)
		{
			if (message.StartsWith("PING"))
			{
				string daemons = message.Remove(0, 4);
				daemons.TrimStart(new char[] { ' ' });
				sendPong(daemons);
				return true;
			}
			return false;
		}

		//
		// MESSAGES
		//

		/*!
		 * \brief	sends the user's \a nick to the server
		 * \param	nick the nickname the player wishes to use
		 * */
		public void sendNick(string nick)
		{
			sendMessage("NICK " + nick);
		}

		/*!
		 * \brief	sets the player's \a user name to the server
		 * \param	user the user name to set
		 * */
		public void sendUser(string user)
		{
			sendMessage("USER " + user + " 8 * :PuxIrc");
		}

		//! \brief	informs the server that you are quitting
		public void sendQuit()
		{
			sendMessage("QUIT");
		}

		/*!
		 * \brief	permit the user to quit with some flourish
		 * \param	reason a reason for leaving
		 * */
		public void sendQuit(string reason)
		{
			sendMessage("QUIT :" + reason);
		}

		/*!
		 * \brief	join a specified \a channel
		 * \param	channel the channel to join
		 * */
		public void sendJoin(string channel)
		{
			sendMessage("JOIN " + channel);
		}

		/*!
		 * \brief	part from a channel
		 * \param	channel the channel to part from
		 * */
		public void sendPart(string channel)
		{
			sendMessage("PART " + channel);
		}

		//! \brief	a simple resonse to a PING
		public void sendPong(string daemons)
		{
			sendMessage("PONG " + daemons);
		}

		/*!
		 * \brief	attempt to communicate with some \a target entity with some \a message
		 * \param	target a valid target as described at
		 *			http://www.irchelp.org/irchelp/rfc/chapter4.html#c4_4_1.
		 *			Usually a channel or a nick.
		 * */
		public void sendPrivateMessage(string target, string message)
		{
			sendMessage("PRIVMSG " + target + " :" + message);
		}

		public String address
		{
			get { return m_address; }
			set { m_address = value; }
		}

		public String port
		{
			get { return m_port; }
			set { m_port = value; }
		}

		public User user
		{
			get { return m_user; }
			set { m_user = value; }
		}

		public Receive receiveCallback
		{
			get { return m_recvCallback; }
			set { m_recvCallback = value; }
		}

		public Status status
		{
			get { return m_status; }
		}

		private Status m_status = Status.Disconnected;

		private DataReader m_inputStream = null;
		private DataWriter m_outputStream = null;
		private String m_address = kDefaultAddress;
		private String m_port = kDefaultPort;
		private User m_user;

		private Task m_readTask = null;
		private Receive m_recvCallback = null;

		StreamSocket m_socket;
	}
}
