using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TransLib;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Transhef.SubPages
{
    public sealed partial class TranslatePage : Page
    {
        public TranslatePage()
        {
            this.InitializeComponent();
            foreach (KeyValuePair<string, string> lang in TransLib.Variables.LanguagePairs)
            {
                InputLanguageScroller.Items.Add(lang.Key);
                OutputLanguageScroller.Items.Add(lang.Key);
            }
        }

        private void Copy_Clicked(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(outputBox.Text);
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(outputBox.Text);
            Clipboard.SetContent(dataPackage);
        }

        private void Switch_Clicked(object sender, RoutedEventArgs e)
        {
            object temp;
            temp = InputLanguageScroller.SelectedItem;
            InputLanguageScroller.SelectedItem = OutputLanguageScroller.SelectedItem;
            OutputLanguageScroller.SelectedItem = temp;
        }

        private async void Translate_Clicked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(inputBox.Text) && InputLanguageScroller.SelectedItem != null && OutputLanguageScroller.SelectedItem != null)
            {
                (sender as AppBarButton).IsEnabled = false;
                TranslationObject obj = new TranslationObject() { input = inputBox.Text, inputLanguageorCode = InputLanguageScroller.SelectedItem.ToString(), outputLanguageorCode = OutputLanguageScroller.SelectedItem.ToString() };
                try
                {
                    Translation translation = await Methods.TranslateAsync(obj); 
                    outputBox.Text = translation.output;
                    if (!translation.savedToHistory) { ShowError(null, "Unfortunately, your translation was too long to add to history. Otherwise, it went fine."); }
                }
                catch (Exception ex) { ShowError(ex); }
                (sender as AppBarButton).IsEnabled = true;
            }
            else
            {
                ShowError(null, "One or more required parameters are empty. Check if you selected both an input and output language and you entered a query in the input box.");
            }

        }

        internal async void ShowError(Exception ex = null, string alterror = null)
        {
            if (ex != null)
            {
                await (new ContentDialog() { Title = "An error occured!", Content = ex.Message, PrimaryButtonText = "OK" }).ShowAsync();
            }
            else if (alterror != null)
            {
                await (new ContentDialog() { Title = "An error occured!", Content = alterror, PrimaryButtonText = "OK" }).ShowAsync();
            }
        }

        }
}
