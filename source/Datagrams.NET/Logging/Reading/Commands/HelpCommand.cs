﻿using DatagramsNet.Logging.Reading.Attributes;
using DatagramsNet.Logging.Reading.Indexes;
using DatagramsNet.Logging.Reading.Interfaces;
using DatagramsNet.Prefixes;

namespace DatagramsNet.Logging.Reading.Commands
{
    [Command("Help", "Help: [Command]")]
    public sealed class HelpCommand : ICommand, ICommandAction
    {
        public Argument[] Arguments => null;

        public Action CommandAction { get; set; }

        public object[] Indexes => new object[]
        {
            new CommandIndex(),
        };

        public async Task<string> ExecuteCommand(Argument[] args, object[] indexes)
        {
            if (indexes[0] is CommandIndex commandIndex)
            {
                return $"{commandIndex.Name}: {commandIndex.Value}";
            }
            await ServerLogger.Log<ErrorPrefix>("This command was found", TimeFormat.HALF);
            return null;
        }
    }
}
