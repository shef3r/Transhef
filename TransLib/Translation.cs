using System;

namespace TransLib
{
    [Serializable]
    public class Translation
    {
        public string inputLanguage { get; set; }
        public string input { get; set; }
        public string outputLanguage { get; set; }
        public string output { get; set; }
        public bool savedToHistory { get; set; }
    }
}