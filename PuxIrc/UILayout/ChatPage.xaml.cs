using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace PuxIrc
{
	/*!
	 * \brief	The main view of the app, the chat box and channel view
	 * */
    partial class ChatPage
    {
		/*!
		 * \brief	initializes this chat page, with a reference back to the app that spawned it
		 * \param	app the app that created this page
		 * */
        public ChatPage(App app)
        {
			m_app = app;
            InitializeComponent();
			m_app.setMessageReceiveHandler(receiveMessage);
        }

		//! \brief	user presses "Send"
		private void Send_Click(object sender, RoutedEventArgs e)
		{
			// send message
			m_app.sendMessage(MessageBox.Text);
			MessageBox.Text = "";
		}

		/*!
		 * \brief	handle receiving a message from the server
		 * \note	at some point it would be nice to pretty this up XD
		 * */
		void receiveMessage(string message)
		{
			Dispatcher.InvokeAsync(
				Windows.UI.Core.CoreDispatcherPriority.Normal,
				(x,y) => { Chat.Text += message + "\n"; },
				this,
				null);
		}

		//! \brief	Click event handler that causes a field to be cleared
		private void textFieldFocus(object sender, RoutedEventArgs e)
		{
			if (sender is TextBox)
			{
				TextBox textBox = (TextBox)sender;
				textBox.SelectAll();
			}
		}

		//! \brief	user presses the "Join" button to join a channel
		private void Join_Click(object sender, RoutedEventArgs e)
		{
			m_app.join(Channel.Text);
		}

		App m_app;
    }
}
