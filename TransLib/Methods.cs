using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace TransLib {
    public static class Methods {
        public static async Task<Translation> TranslateAsync(TranslationObject obj) {
            if (obj == null)
                throw new ArgumentOutOfRangeException();

            Translation translation = new Translation();

            if (obj.inputLanguageorCode == "auto")
            {
                string oLangCode = GetLanguageCode(obj.outputLanguageorCode);

                string[] lines = obj.input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                StringBuilder translatedText = new StringBuilder();

                var client = new HttpClient();

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={oLangCode}&dt=t&q={Uri.EscapeDataString(line)}";
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    string jsonString = await response.Content.ReadAsStringAsync();
                    string translatedLine = ExtractAutoTranslation(jsonString);
                    translatedText.AppendLine(translatedLine);
                }

                translation = new Translation
                {
                    input = obj.input,
                    inputLanguage = string.Empty,
                    output = translatedText.ToString().Trim(),
                    outputLanguage = Variables.LanguagePairs.FirstOrDefault(x => x.Value == oLangCode).Key,
                    savedToHistory = true
                };
            }
            else
            {
                string iLangCode = GetLanguageCode(obj.inputLanguageorCode);
                string oLangCode = GetLanguageCode(obj.outputLanguageorCode);

                string[] lines = obj.input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                StringBuilder translatedText = new StringBuilder();

                var client = new HttpClient();

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={iLangCode}&tl={oLangCode}&dt=t&q={Uri.EscapeDataString(line)}";
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    string jsonString = await response.Content.ReadAsStringAsync();
                    string translatedLine = ExtractTranslation(jsonString);
                    translatedText.AppendLine(translatedLine);
                }

                translation = new Translation
                {
                    input = obj.input,
                    inputLanguage = Variables.LanguagePairs.FirstOrDefault(x => x.Value == iLangCode).Key,
                    output = translatedText.ToString().Trim(),
                    outputLanguage = Variables.LanguagePairs.FirstOrDefault(x => x.Value == oLangCode).Key,
                    savedToHistory = true
                };
            }

            try { AddToHistory(translation); }
            catch { translation.savedToHistory = false; }

            return translation;
        }

        private static string ExtractAutoTranslation(string jsonString)
        {
            JsonDocument jsonDoc = JsonDocument.Parse(jsonString);
            JsonElement root = jsonDoc.RootElement;
            StringBuilder translationBuilder = new StringBuilder();

            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                JsonElement mainTranslations = root[0];
                if (mainTranslations.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement translationUnit in mainTranslations.EnumerateArray())
                    {
                        if (translationUnit.ValueKind == JsonValueKind.Array &&
                            translationUnit.GetArrayLength() > 0)
                        {
                            JsonElement translationPart = translationUnit[0];
                            if (translationPart.ValueKind == JsonValueKind.String)
                            {
                                string translatedLine = translationPart.GetString().Trim();
                                translationBuilder.AppendLine(translatedLine);
                            }
                        }
                    }
                }
            }

            return translationBuilder.ToString().Trim();
        }

        internal static string ExtractTranslation(string jsonString) {
            JsonDocument jsonDoc = JsonDocument.Parse(jsonString);
            JsonElement root = jsonDoc.RootElement;
            StringBuilder translationBuilder = new StringBuilder();

            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0) {
                JsonElement mainTranslations = root[0];
                if (mainTranslations.ValueKind == JsonValueKind.Array) {
                    foreach (JsonElement translationUnit in mainTranslations.EnumerateArray()) {
                        if (translationUnit.ValueKind == JsonValueKind.Array &&
                            translationUnit.GetArrayLength() > 0) {
                            JsonElement translationPart = translationUnit[0];
                            if (translationPart.ValueKind == JsonValueKind.String) {
                                string translatedLine = translationPart.GetString().Trim();
                                translationBuilder.AppendLine(translatedLine);
                            }
                        }
                    }
                }
            }

            return translationBuilder.ToString().Trim();
        }

        internal static void AddToHistory(Translation translation) {
            IPropertySet localSettings = ApplicationData.Current.LocalSettings.Values;

            if (localSettings != null) {
                List<Translation> historyList = new List<Translation>();

                if (localSettings.ContainsKey("history")) {
                    string historyJson = localSettings["history"] as string;
                    if (!string.IsNullOrEmpty(historyJson)) {
                        historyList = JsonSerializer.Deserialize<List<Translation>>(historyJson);
                    }
                }

                historyList.Add(translation);
                localSettings["history"] = JsonSerializer.Serialize(historyList);
            }
        }

        public static List<Translation> RetrieveHistory() {
            IPropertySet localSettings = ApplicationData.Current.LocalSettings.Values;
            if (localSettings != null && localSettings.ContainsKey("history")) {
                string historyJson = localSettings["history"] as string;
                if (!string.IsNullOrEmpty(historyJson)) {
                    return JsonSerializer.Deserialize<List<Translation>>(historyJson);
                }
            }
            return new List<Translation>();
        }

        private static string GetLanguageCode(string languageOrCode) {
            if (Variables.LanguagePairs.ContainsValue(languageOrCode))
                return languageOrCode;
            else if (Variables.LanguagePairs.ContainsKey(languageOrCode))
                return Variables.LanguagePairs[languageOrCode];
            else
                throw new ArgumentOutOfRangeException();
        }
    }
}