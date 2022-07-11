using DatagramsNet.Logging.Reading.Attributes;
using DatagramsNet.Logging.Reading.Models;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DatagramsNet.Logging.Reading.Arguments
{
    internal sealed class CommandArgument : IArgument<string>
    {
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

            private static readonly CommandAttribute[] commandAttributes;
            private static readonly CommandArgument helpTextArgument;

            static ArgumentFactory()
            {
                commandAttributes =
                    Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .SelectMany(t => t.GetCustomAttributes<CommandAttribute>())
                    .ToArray();

                string commandsHelpText = string.Join(Environment.NewLine, commandAttributes.Select(attribute => attribute.HelpText));
                helpTextArgument = new(commandsHelpText);
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
                    argument = helpTextArgument;
                    return true;
                }
            }
        }
    }
}
