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
	public class Server
	{
		private const String kDefaultAddress = "irc.rizon.net";
		private const String kDefaultPort = "6667";
		private const String kMessageTerminator = "\r\n";

		public delegate void Receive(string message);

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

		async private void sendMessage(string message)
		{
			m_outputStream.WriteString(message + kMessageTerminator);
			await m_outputStream.StoreAsync().StartAsTask();
		}

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
					m_readTask = Task.Factory.StartNew(() => readFromStream());
					break;

				case AsyncStatus.Canceled:
				case AsyncStatus.Error:
					m_status = Status.Disconnected;
					break;
			}
		}

		private void sendLogin()
		{
			sendNick(m_user.Nick);
			sendUser(m_user.Nick);
		}

		async private void readFromStream()
		{
			string result = "";
			while (status == Status.Connected)
			{
				await m_inputStream.LoadAsync(1);
				if (m_inputStream.UnconsumedBufferLength > 0)
				{
					result += m_inputStream.ReadString(m_inputStream.UnconsumedBufferLength);
					result = processServerResponse(result);
				}
			}
		}

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
					if (!handleResponse(prefix))
					{
						receiveCallback(prefix);
					}
					message = postfix;
				}
			} while (index != -1);
			return message;
		}

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

		// messages
		public void sendNick(string nick)
		{
			sendMessage("NICK " + nick);
		}

		public void sendUser(string user)
		{
			sendMessage("USER " + user + " 8 * :PuxIrc");
		}

		public void sendQuit()
		{
			sendMessage("QUIT");
		}

		public void sendQuit(string reason)
		{
			sendMessage("QUIT :" + reason);
		}

		public void sendJoin(string channel)
		{
			sendMessage("JOIN " + channel);
		}

		public void sendPart(string channel)
		{
			sendMessage("PART " + channel);
		}

		public void sendPong(string daemons)
		{
			sendMessage("PONG " + daemons);
		}

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
