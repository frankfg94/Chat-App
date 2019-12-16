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
        public const string FILENAME_WITH_FORMAT = "fn";
        public const string DESCRIPTION = "d";
        public const string NEW_NAME ="nn";
        public const string MSG_ID = "id";
        private static string[] keyArr = { USERNAME, TEXT, NAME , MESSAGE, PASSWORD, RESULT, FILENAME_WITH_FORMAT, DESCRIPTION, NEW_NAME, MSG_ID};

        internal static bool KeyIsValid(string key)
        {
            foreach (var k in keyArr)
                if (k.Equals(key))
                    return true;

            return false;
        }
    }
}