using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Wpf;

namespace MyWpfBrowser
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			// Open an initial home tab on startup
			AddNewTab("https://www.google.com");
		}

		/// <summary>
		/// Gets the WebView2 instance residing inside the currently selected Tab.
		/// </summary>
		private WebView2 GetCurrentWebView()
		{
			if (tabControl.SelectedItem is TabItem currentTab)
			{
				return currentTab.Content as WebView2;
			}
			return null;
		}

		/// <summary>
		/// Creates a new tab containing a WebView2 browser control.
		/// </summary>
		private void AddNewTab(string url)
		{
			// 1. Create a new WebView2 instance
			WebView2 webView = new WebView2();

			// 2. Build custom Header container (Title text + Close button)
			StackPanel headerStack = new StackPanel { Orientation = Orientation.Horizontal };

			TextBlock titleBlock = new TextBlock
			{
				Text = "New Tab",
				MaxWidth = 120,
				TextTrimming = TextTrimming.CharacterEllipsis,
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(0, 0, 8, 0)
			};

			Button closeButton = new Button
			{
				Content = "✕",
				Width = 18,
				Height = 18,
				FontSize = 10,
				Padding = new Thickness(0),
				Background = System.Windows.Media.Brushes.Transparent,
				BorderThickness = new Thickness(0),
				Cursor = Cursors.Hand
			};

			headerStack.Children.Add(titleBlock);
			headerStack.Children.Add(closeButton);

			// 3. Create the TabItem
			TabItem tab = new TabItem
			{
				Header = headerStack,
				Content = webView
			};

			// Event handler to close this specific tab
			closeButton.Click += (s, e) =>
			{
				// Clean up WebView2 resources before removing
				webView.Dispose();
				tabControl.Items.Remove(tab);

				// Close window if no tabs remain
				if (tabControl.Items.Count == 0)
				{
					this.Close();
				}
			};

			// 4. Attach WebView2 events
			webView.CoreWebView2InitializationCompleted += (s, e) =>
			{
				if (e.IsSuccess)
				{
					// Update tab header title as the web page title changes
					webView.CoreWebView2.DocumentTitleChanged += (sender, args) =>
					{
						titleBlock.Text = string.IsNullOrWhiteSpace(webView.CoreWebView2.DocumentTitle)
							? "Untitled"
							: webView.CoreWebView2.DocumentTitle;
					};

					// Update URL bar when navigation completes
					webView.SourceChanged += (sender, args) =>
					{
						if (tabControl.SelectedItem == tab)
						{
							txtAddress.Text = webView.Source.ToString();
						}
					};
				}
			};

			// Add tab and select it
			tabControl.Items.Add(tab);
			tabControl.SelectedItem = tab;

			// Initialize and navigate
			webView.Source = new Uri(url);
		}

		private void NavigateCurrentTab()
		{
			WebView2 currentWebView = GetCurrentWebView();
			if (currentWebView != null && currentWebView.CoreWebView2 != null)
			{
				string url = txtAddress.Text.Trim();

				if (string.IsNullOrEmpty(url)) return;

				// Prepend https:// if missing
				if (!url.StartsWith("http://") && !url.StartsWith("https://"))
				{
					url = "https://" + url;
				}

				currentWebView.CoreWebView2.Navigate(url);
			}
		}

		// --- Event Handlers ---

		private void BtnGo_Click(object sender, RoutedEventArgs e)
		{
			NavigateCurrentTab();
		}

		private void TxtAddress_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				NavigateCurrentTab();
			}
		}

		private void BtnNewTab_Click(object sender, RoutedEventArgs e)
		{
			AddNewTab("https://www.google.com");
		}

		private void BtnBack_Click(object sender, RoutedEventArgs e)
		{
			WebView2 current = GetCurrentWebView();
			if (current != null && current.CanGoBack)
			{
				current.GoBack();
			}
		}

		private void BtnForward_Click(object sender, RoutedEventArgs e)
		{
			WebView2 current = GetCurrentWebView();
			if (current != null && current.CanGoForward)
			{
				current.GoForward();
			}
		}

		private void BtnRefresh_Click(object sender, RoutedEventArgs e)
		{
			WebView2 current = GetCurrentWebView();
			if (current != null)
			{
				current.Reload();
			}
		}

		private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Sync address bar text when switching between tabs
			WebView2 current = GetCurrentWebView();
			if (current != null && current.Source != null)
			{
				txtAddress.Text = current.Source.ToString();
			}
		}
	}
}