using System;
using System.Collections.Generic;

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
        public User user;
        public string fullCommand;
        bool isParsed = false;
        public object content;

        public Message(User user, string command)
        {
            this.user = user;
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
            var argsTab = ArgsPart.Split(' ');
            foreach(var keyValueArg in argsTab)
            {
                var tab = keyValueArg.Split(":");
                var key = tab[0];
                var value = tab[1];

                if (!ArgType.KeyIsValid(key))
                    throw new InvalidCommandFormatException("The argument's key '" + key+"' is not recognized (is it added to the list of possible arguments?)");

                commands.Add(new CommandArg(key, value));

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
        public bool mustBeParsed = false;

        public string GetArgument(string argKey)
        {
            if(args == null)
                args = GetArguments();

            var arg = args.Find(x => x.key == argKey);
            
            if (arg == null)
                throw new NullReferenceException("Couldn't find the value in the entered command for the following key : " + argKey );
            
            return arg.value ;
        }
    }
}