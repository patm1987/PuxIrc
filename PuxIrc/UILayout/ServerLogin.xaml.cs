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
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace PuxIrc
{
	/*!
	 * \brief	UI for connecting to a server
	 * */
	public sealed partial class ServerLogin
	{
		/*!
		 * \param	app a pointer back to the app
		 * */
		public ServerLogin(App app)
		{
			m_app = app;
			InitializeComponent();
		}

		// NOTE: this is mostly auto-generated code
		// View state management for switching among Full, Fill, Snapped, and Portrait states

		private DisplayPropertiesEventHandler _displayHandler;
		private TypedEventHandler<ApplicationLayout, ApplicationLayoutChangedEventArgs> _layoutHandler;

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			if (_displayHandler == null)
			{
				_displayHandler = Page_OrientationChanged;
				_layoutHandler = Page_LayoutChanged;
			}
			DisplayProperties.OrientationChanged += _displayHandler;
			ApplicationLayout.GetForCurrentView().LayoutChanged += _layoutHandler;
			SetCurrentOrientation(this);
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			DisplayProperties.OrientationChanged -= _displayHandler;
			ApplicationLayout.GetForCurrentView().LayoutChanged -= _layoutHandler;
		}

		private void Page_LayoutChanged(object sender, ApplicationLayoutChangedEventArgs e)
		{
			SetCurrentOrientation(this);
		}

		private void Page_OrientationChanged(object sender)
		{
			SetCurrentOrientation(this);
		}

		private void SetCurrentOrientation(Control viewStateAwareControl)
		{
			VisualStateManager.GoToState(viewStateAwareControl, this.GetViewState(), false);
		}

		private String GetViewState()
		{
			var orientation = DisplayProperties.CurrentOrientation;
			if (orientation == DisplayOrientations.Portrait ||
				orientation == DisplayOrientations.PortraitFlipped) return "Portrait";
			var layout = ApplicationLayout.Value;
			if (layout == ApplicationLayoutState.Filled) return "Fill";
			if (layout == ApplicationLayoutState.Snapped) return "Snapped";
			return "Full";
		}

		//! \brief	handle the user pressing the "Connect" button, tell the app to login
		private void ConnectButton_Click(object sender, RoutedEventArgs e)
		{
			m_app.login(Server.Text, Port.Text, Nick.Text);
		}

		App m_app;
	}
}
