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
using TransLib;
using Windows.ApplicationModel.DataTransfer;
using System.Threading.Tasks;

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
            foreach (KeyValuePair<string, string> lang in TransLib.Variables.LanguagePairs) {
                InputLanguageScroller.Items.Add(lang.Key);
                OutputLanguageScroller.Items.Add(lang.Key);
            }
        }

        private void Copy_Clicked(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine(outputBox.Text);
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(outputBox.Text);
            Clipboard.SetContent(dataPackage);
        }

        private void Switch_Clicked(object sender, RoutedEventArgs e) {
            string temp;
            temp = InputLanguageScroller.Text;
            InputLanguageScroller.Text = OutputLanguageScroller.Text;
            OutputLanguageScroller.Text = temp;

            if (!string.IsNullOrEmpty(InputLanguageScroller.Text)) {
                InputLanguageScroller.Text = FindClosestMatch(InputLanguageScroller.Text, TransLib.Variables.LanguagePairs.Keys);
            }
            if (!string.IsNullOrEmpty(OutputLanguageScroller.Text)) {
                OutputLanguageScroller.Text = FindClosestMatch(OutputLanguageScroller.Text, TransLib.Variables.LanguagePairs.Keys);
            }
        }

        private async void Translate_Clicked(object sender, RoutedEventArgs e) {
            if ((bool)AutoLanguage.IsChecked)
            {
                await HandleAutoTranslation(sender);
            }
            else
            {
                await HandleManualTranslation(sender);
            }
        }

        private async Task HandleManualTranslation(object sender)
        {
            if (!string.IsNullOrEmpty(InputLanguageScroller.Text))
            {
                InputLanguageScroller.Text = FindClosestMatch(InputLanguageScroller.Text, TransLib.Variables.LanguagePairs.Keys);
            }
            if (!string.IsNullOrEmpty(OutputLanguageScroller.Text))
            {
                OutputLanguageScroller.Text = FindClosestMatch(OutputLanguageScroller.Text, TransLib.Variables.LanguagePairs.Keys);
            }

            if (!string.IsNullOrEmpty(inputBox.Text) && !string.IsNullOrEmpty(InputLanguageScroller.Text) && !string.IsNullOrEmpty(OutputLanguageScroller.Text))
            {
                (sender as Button).IsEnabled = false;
                TranslationObject obj = new TranslationObject() { input = inputBox.Text, inputLanguageorCode = InputLanguageScroller.Text.ToString(), outputLanguageorCode = OutputLanguageScroller.Text.ToString() };
                try
                {
                    Translation translation = await Methods.TranslateAsync(obj);
                    outputBox.Text = translation.output;
                    if (!translation.savedToHistory) { ShowError(null, "Unfortunately, your translation was too long to add to history. Otherwise, it went fine."); }
                }
                catch (Exception ex) { ShowError(ex); }
                (sender as Button).IsEnabled = true;
            }
            else
            {
                ShowError(null, "One or more required parameters are empty. Check if you selected both an input and output language and you entered a query in the input box.");
            }
        }

        private async Task HandleAutoTranslation(object sender)
        {
            if (string.IsNullOrEmpty(inputBox.Text) || string.IsNullOrEmpty(OutputLanguageScroller.Text))
            {
                ShowError(null, "One or more required parameters are empty. Check if you selected both an input and output language and you entered a query in the input box.");
                return;
            }
            OutputLanguageScroller.Text = FindClosestMatch(OutputLanguageScroller.Text, TransLib.Variables.LanguagePairs.Keys);
            InputLanguageScroller.Text = string.Empty;

            try
            {
                (sender as Button).IsEnabled = false;
                TranslationObject obj = new TranslationObject() { input = inputBox.Text, inputLanguageorCode = "auto", outputLanguageorCode = OutputLanguageScroller.Text.ToString() };
                try
                {
                    Translation translation = await Methods.TranslateAsync(obj);
                    outputBox.Text = translation.output;
                    if (!translation.savedToHistory) { ShowError(null, "Unfortunately, your translation was too long to add to history. Otherwise, it went fine."); }
                }
                catch (Exception ex) { ShowError(ex); }
                (sender as Button).IsEnabled = true;
            }
            catch (Exception ex)
            {
                ShowError(ex);
                return;
            }
        }

        internal async void ShowError(Exception ex = null, string alterror = null) {
            if (ex != null) {
                await (new ContentDialog() { Title = "An error occured!", Content = ex.Message, PrimaryButtonText = "OK" }).ShowAsync();
            }
            else if (alterror != null) {
                await (new ContentDialog() { Title = "An error occured!", Content = alterror, PrimaryButtonText = "OK" }).ShowAsync();
            }
        }
        private string FindClosestMatch(string input, IEnumerable<string> options) {
            if (string.IsNullOrEmpty(input)) {
                return string.Empty;
            }
            return options.OrderBy(option => LevenshteinDistance(input, option)).FirstOrDefault();
        }

        private int LevenshteinDistance(string a, string b) {
            if (string.IsNullOrEmpty(a))
                return string.IsNullOrEmpty(b) ? 0 : b.Length;
            if (string.IsNullOrEmpty(b))
                return a.Length;

            int[,] costs = new int[a.Length + 1, b.Length + 1];

            for (int i = 0; i <= a.Length; i++)
                costs[i, 0] = i;
            for (int j = 0; j <= b.Length; j++)
                costs[0, j] = j;

            for (int i = 1; i <= a.Length; i++) {
                for (int j = 1; j <= b.Length; j++) {
                    int cost = (b[j - 1] == a[i - 1]) ? 0 : 1;
                    costs[i, j] = Math.Min(Math.Min(costs[i - 1, j] + 1, costs[i, j - 1] + 1), costs[i - 1, j - 1] + cost);
                }
            }

            return costs[a.Length, b.Length];
        }
        private void InputLanguageScroller_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput) {
                var suggestions = TransLib.Variables.LanguagePairs.Keys
                    .Where(p => p.StartsWith(sender.Text, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                sender.ItemsSource = suggestions;
            }
        }

        private void OutputLanguageScroller_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput) {
                var suggestions = TransLib.Variables.LanguagePairs.Keys
                    .Where(p => p.StartsWith(sender.Text, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                sender.ItemsSource = suggestions;
            }
        }

        private void inputBox_TextChanged(object sender, TextChangedEventArgs e) {
            outputBox.Text = string.Empty;
        }

        private void AutoLanguage_Click(object sender, RoutedEventArgs e)
        {
            InputLanguageScroller.Text = string.Empty;
        }
    }
}
