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
    partial class ChatPage
    {
        public ChatPage(App app)
        {
			m_app = app;
            InitializeComponent();
			m_app.setMessageReceiveHandler(receiveMessage);
        }

		private void Send_Click(object sender, RoutedEventArgs e)
		{
			// send message
			m_app.sendMessage(MessageBox.Text);
			MessageBox.Text = "";
		}

		void receiveMessage(string message)
		{
			Dispatcher.InvokeAsync(
				Windows.UI.Core.CoreDispatcherPriority.Normal,
				(x,y) => { Chat.Text += message + "\n"; },
				this,
				null);
		}

		private void textFieldFocus(object sender, RoutedEventArgs e)
		{
			if (sender is TextBox)
			{
				TextBox textBox = (TextBox)sender;
				textBox.SelectAll();
			}
		}

		private void Join_Click(object sender, RoutedEventArgs e)
		{
			m_app.join(Channel.Text);
		}

		App m_app;
    }
}
