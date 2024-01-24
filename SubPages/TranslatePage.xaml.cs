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

//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=234238

namespace Transhef.SubPages
{
    /// <summary>
    /// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
    /// </summary>
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
            // make copying work :p
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
                TranslationObject obj = new TranslationObject() { input = inputBox.Text, inputLanguageorCode = InputLanguageScroller.SelectedItem.ToString(), outputLanguageorCode = OutputLanguageScroller.SelectedItem.ToString() };
                try
                {
                    Translation translation = Methods.Translate(obj); 
                    outputBox.Text = translation.output;
                }
                catch (Exception ex) { ShowError(ex); }
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
