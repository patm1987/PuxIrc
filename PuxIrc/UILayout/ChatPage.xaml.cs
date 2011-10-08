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
		//! \brief	string that identifies a message the user wrote
		const string kLocalMessageIdentifier = "\t--> ";

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

		/*!
		 * \brief	handle receiving a message from the server
		 * \note	at some point it would be nice to pretty this up XD
		 * */
		void receiveMessage(string message)
		{
			// make sure that the message goes in on the UI thread!
			Dispatcher.InvokeAsync(
				Windows.UI.Core.CoreDispatcherPriority.Normal,
				(x,y) => {
					Chat.Text += message + "\n";
					ChatScrollBox.ScrollToVerticalOffset(ChatScrollBox.ExtentHeight - ChatScrollBox.ViewportHeight);
				},
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

		//! \brief	user presses "Send"
		private void Send_Click(object sender, RoutedEventArgs e)
		{
			sendCurrentMessage();
		}

		//! \brief	handle a key up event, for pressing "Enter" and having something happen
		private void MessageBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyEventArgs e)
		{
			if (e.Key == Windows.System.VirtualKey.Enter)
			{
				sendCurrentMessage();
			}
		}

		//! \brief	sends the message currently in the message box
		private void sendCurrentMessage()
		{
			m_app.sendMessage(MessageBox.Text);

			// let the user see what he wrote
			receiveMessage(kLocalMessageIdentifier + MessageBox.Text);
			MessageBox.Text = "";
		}

		App m_app;
    }
}
