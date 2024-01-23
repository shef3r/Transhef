using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransLib
{
    public class Methods
    {
        public string Translate(TranslationObject obj)
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

                // Enter actual translation code.
                return "Translated string: " + obj.input + " from " + iLangCode + " to " + oLangCode;

            }
            else { throw new ArgumentOutOfRangeException(); }
        }
    }
}
