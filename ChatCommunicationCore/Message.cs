using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace ChatCommunication
{
    /*
     * Format d'un message :   COMMANDE | ARGUMENTSs
     */
    [Serializable]
    public class Message
    {
        private string commandPart;
        private string argsPart;
        public User author { get; set; }
        public string fullCommand;
        bool isParsed = false;

        /// <summary>
        /// The content can be used to transport any kind of information such as images, videos which are
        /// often in bytes array
        /// </summary>
        public object content;

        public Message(User user, string command)
        {
            this.author = user;
            this.fullCommand = command;
        }

        public void Parse()
        {
            fullCommand = fullCommand.Trim();
            if (!HasValidFormat(fullCommand, out string reason))
            {
                throw new InvalidCommandFormatException(fullCommand, reason);
            }
            else
            {
                var areas = fullCommand.Split("|");
                if (areas.Length != 2)
                    throw new InvalidCommandFormatException(fullCommand, "The separator '|' must be detected once only it is detected : " + areas.Length + " times");

                commandPart = areas[0].Trim();
                argsPart = areas[1].Trim();

                if (string.IsNullOrEmpty(commandPart) || string.IsNullOrEmpty(argsPart))
                {
                    throw new InvalidCommandFormatException(fullCommand, "One of the part of the commands is null or empty");
                }
                isParsed = true;
            }
        }

        internal List<CommandArg> GetArguments()
        {
            var commands = new List<CommandArg>();
            if (ArgsPart == null)
                Parse();
            var splittedTab = ArgsPart.Split(' ');
            string[] argsTab = null;
    
            // Descriptions or chat messages can be more than 1 word long
            if(ArgsPart.Contains("m:") || ArgsPart.Contains("d:"))
            {
                int startMessagePos = -1;
                int endMessagesPos = -1;
                List<string> messageValue = new List<string>();
                // To accept long words
                for (int i = 0; i < splittedTab.Length; i++)
                {
                    // We only accept spaced commands for a message type command
                    if (splittedTab[i].StartsWith("m:") || splittedTab[i].StartsWith("d:"))
                    {
                        startMessagePos = i;
                        messageValue.Add(splittedTab[i]);
                    }
                    else if (startMessagePos != -1 && splittedTab[i].Contains(':'))
                    {
                        endMessagesPos = i - 1;
                        break;
                    }
                    else if(startMessagePos != -1)
                    {
                        messageValue.Add(splittedTab[i]);
                    }
                }
                if(endMessagesPos == -1)
                    endMessagesPos = splittedTab.Length - 1;

                if (startMessagePos == -1)
                  argsTab = splittedTab;
                else
                {
                    List<string> cleanedTab = new List<string>();

                    for (int i = 0; i < splittedTab.Length; i++)
                    {
                        if(i<startMessagePos || i>endMessagesPos)
                        cleanedTab.Add(splittedTab[i]);
                    }
                    cleanedTab.Insert(startMessagePos,string.Join(" ",messageValue.ToArray()));
                    argsTab = cleanedTab.ToArray();
                }
            }
            else
            {
                argsTab = splittedTab;
            }
            if(string.IsNullOrEmpty(ArgsPart) || !argsPart.Contains(":"))
            {
                return commands;
            }
            foreach(var keyValueArg in argsTab)
            {
                if(!keyValueArg.Equals(""))
                {
                    var tab = keyValueArg.Split(":");
                    var key = tab[0];
                    var value = tab[1];

                    if (!ArgType.KeyIsValid(key))
                        throw new InvalidCommandFormatException("The argument's key '" + key+"' is not recognized (is it added to the list of possible arguments?)");

                    commands.Add(new CommandArg(key, value));
                }


            }
            return commands;
        }

        private bool HasValidFormat(string text, out string reason)
        {
            reason = "";
            // A remplacer par du regex pour que ce soit propre
            if(!text.Contains("|"))
            {
                reason = "Separator '|' is missing in the command";
                return false;
            }
            return true;
        }

        public string CommandPart
        {
            get
            {
                return commandPart;
            }
        }

        public string ArgsPart
        {
            get
            {
                return argsPart;
            }
        }

        List<CommandArg> args;

        /// <summary>
        /// Indicates if the message has to be parsed by the client or the server after being received or just displayed as an information message
        /// </summary>
        public bool mustBeParsed = false;

        public string GetArgument(string argKey)
        {
            if(args == null)
                args = GetArguments();

            var arg = args.Find(x => x.key == argKey);
            
            if (arg == null)
                throw new InvalidCommandFormatException("Couldn't find the value in the entered command for the following key : " + argKey );
            
            return arg.value ;
        }

        /// <summary>
        /// Does not throw exceptions if the argument is not found, is used for optional arguments (example: description of a topic)
        /// Returns null if no argument is found
        /// </summary>
        public string GetOptionalArgument(string argKey)
        {
            if (args == null)
                args = GetArguments();

            var arg = args.Find(x => x.key == argKey);
                if (arg == null)
                    return null;

            return arg.value;
        }
    }
}