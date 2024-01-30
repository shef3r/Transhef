using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Collections;
using Windows.Storage;
using static System.Net.Mime.MediaTypeNames;

namespace TransLib
{
    public static class Methods
    {
        public static async Task<Translation> TranslateAsync(TranslationObject obj)
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

                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={iLangCode}&tl={oLangCode}&dt=t&q={obj.input}");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string jsonString = await response.Content.ReadAsStringAsync();

                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(jsonString);
                Clipboard.SetContent(dataPackage);

                string translated = ExtractTranslation(jsonString);
                Translation translation = new Translation() { input = obj.input, inputLanguage = Variables.LanguagePairs.FirstOrDefault(x => x.Value == iLangCode).Key, output = translated, outputLanguage = Variables.LanguagePairs.FirstOrDefault(x => x.Value == oLangCode).Key, savedToHistory = true };
                try { AddToHistory(translation); } catch { translation.savedToHistory = false; };
                return translation;

            }
            else { throw new ArgumentOutOfRangeException(); }
        }
        internal static string ExtractTranslation(string jsonString)
        {
            JsonDocument jsonDoc = JsonDocument.Parse(jsonString);

            string translation = "";

            if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
            {
                JsonElement.ArrayEnumerator translationArray = jsonDoc.RootElement.EnumerateArray();

                while (translationArray.MoveNext())
                {
                    if (translationArray.Current.ValueKind == JsonValueKind.Array)
                    {
                        JsonElement.ArrayEnumerator sentenceArray = translationArray.Current.EnumerateArray();

                        while (sentenceArray.MoveNext())
                        {
                            if (sentenceArray.Current.ValueKind == JsonValueKind.Array && sentenceArray.Current.GetArrayLength() > 0 && sentenceArray.Current[0].ValueKind == JsonValueKind.String)
                            {
                                translation += sentenceArray.Current[0].GetString() + " ";
                            }
                        }
                    }
                }
            }

            translation = translation.Trim().Replace("  ", " ");

            return translation;
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
                try
                {
                    localSettings["history"] = JsonSerializer.Serialize(new List<Translation> { translation });
                }
                catch {
                    throw new Exception("The translation was too long to add to your history. Sorry.");
                }
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
