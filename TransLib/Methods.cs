using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace TransLib
{
    public static class Methods
    {
        public static Translation Translate(TranslationObject obj)
        {
            string iLangCode = null;
            string oLangCode = null;
            if (obj != null)
            {
                if (Variables.LanguagePairs.ContainsValue(obj.inputLanguageorCode))
                { iLangCode = obj.inputLanguageorCode; }
                else if (Variables.LanguagePairs.ContainsKey(obj.inputLanguageorCode))
                { iLangCode = Variables.LanguagePairs[obj.inputLanguageorCode]; }
                else { throw new ArgumentOutOfRangeException(); }

                if (Variables.LanguagePairs.ContainsValue(obj.outputLanguageorCode))
                { oLangCode = obj.outputLanguageorCode; }
                else if (Variables.LanguagePairs.ContainsKey(obj.outputLanguageorCode))
                { oLangCode = Variables.LanguagePairs[obj.outputLanguageorCode]; }
                else { throw new ArgumentOutOfRangeException(); }

                Translation translation = new Translation() { input = obj.input, inputLanguage = Variables.LanguagePairs.FirstOrDefault(x => x.Value == iLangCode).Key, output = "TranslationOutput", outputLanguage = Variables.LanguagePairs.FirstOrDefault(x => x.Value == oLangCode).Key };
                AddToHistory(translation);
                // Enter actual translation code.
                return translation;

            }
            else { throw new ArgumentOutOfRangeException(); }
        }
        internal static void AddToHistory(Translation translation)
        {
            IPropertySet localSettings = ApplicationData.Current.LocalSettings.Values;

            if (localSettings != null && localSettings.ContainsKey("history"))
            {
                string historyJson = localSettings["history"] as string;

                if (!string.IsNullOrEmpty(historyJson))
                {
                    List<Translation> historyList = JsonSerializer.Deserialize<List<Translation>>(historyJson);
                    historyList.Add(translation);
                    localSettings["history"] = JsonSerializer.Serialize(historyList);
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                localSettings["history"] = JsonSerializer.Serialize(new List<Translation> { translation });
            }
        }
        public static List<Translation> RetrieveHistory()
        {
            IPropertySet localSettings = ApplicationData.Current.LocalSettings.Values;
            if (localSettings != null && localSettings.ContainsKey("history"))
            {
                string historyJson = localSettings["history"] as string;
                if (!string.IsNullOrEmpty(historyJson))
                {
                    return JsonSerializer.Deserialize<List<Translation>>(historyJson);
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            return new List<Translation>();
        }
    }
}
