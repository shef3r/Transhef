using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Transhef.SubPages;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml.Hosting;

namespace Transhef
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Window.Current.SetTitleBar(TTB);
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            MainNavView.SelectedItem = Home;
        }

        private void Home_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Focus(FocusState.Programmatic); // remove ugly focus rectangle on startup (moving it directly to the language selector), while keeping all elements focusable
        }

        private void MainNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem == Home) { MainFrame.Navigate(typeof(TranslatePage)); }
            else if (args.SelectedItem == History) { MainFrame.Navigate(typeof(HistoryPage)); }
        }
    }
}
