using DatagramsNet.Logging.Reading.Attributes;
using DatagramsNet.Logging.Reading.Models;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DatagramsNet.Logging.Reading.Arguments
{
    internal sealed class CommandArgument : IArgument<string>
    {
        public static CommandArgument AllCommandsArgument => ArgumentFactory.HelpTextArgument;
        public static IArgumentFactory<CommandArgument> Factory => _factory ??= new();
        private static ArgumentFactory? _factory;

        public string Name => "@Command";
        public string Value { get; }

        public CommandArgument(string value)
        {
            Value = value;
        }

        private sealed class ArgumentFactory : ArgumentFactory<CommandArgument>
        {
            public override string Name => "command";

            public static CommandArgument HelpTextArgument { get; }
            private static readonly CommandAttribute[] commandAttributes;

            static ArgumentFactory()
            {
                commandAttributes =
                    Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .SelectMany(t => t.GetCustomAttributes<CommandAttribute>())
                    .ToArray();

                string commandsHelpText = string.Join(Environment.NewLine, commandAttributes.Select(attribute => attribute.HelpText));
                HelpTextArgument = new(commandsHelpText);
            }

            public override bool TryCreate(string arg, [NotNullWhen(true)] out CommandArgument? argument)
            {
                if (arg is not null)
                {
                    CommandAttribute? commandAttribute = commandAttributes.FirstOrDefault(attribute => attribute.Command == arg);
                    if (commandAttribute is not null)
                    {
                        argument = new CommandArgument(commandAttribute.HelpText);
                        return true;
                    }
                    else
                    {
                        argument = null;
                        return false;
                    }
                }
                else
                {
                    argument = HelpTextArgument;
                    return true;
                }
            }
        }
    }
}
