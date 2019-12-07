using System;

namespace ChatCommunication
{
    [Serializable]
    public class CommandArg
    {
        public string key;
        public string value;

        public CommandArg(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }

    public static class ArgType
    {
        public const string USERNAME = "u";
        public const string TEXT = "t";
        public const  string NAME = "n";
        public const string MESSAGE = "m";
        public const string PASSWORD = "p";
        public const string RESULT = "r";
        public const string PATH = "p";
        private static string[] keyArr = { USERNAME, TEXT, NAME , MESSAGE, PASSWORD, RESULT};

        internal static bool KeyIsValid(string key)
        {
            foreach (var k in keyArr)
                if (k.Equals(key))
                    return true;

            return false;
        }
    }
}