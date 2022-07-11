using DatagramsNet.Logging.Reading.Models;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace DatagramsNet.Logging.Reading.Arguments
{
    internal sealed class OptionsArgument : IArgument<Option[]>
    {
        public string Name => "@argument";
        public Option[] Value { get; }

        public static IArgumentFactory<OptionsArgument> Factory(ImmutableArray<Option> options) => new ArgumentFactory(options);

        public OptionsArgument(Option[] value)
        {
            Value = value;
        }

        private sealed class ArgumentFactory : ArgumentFactory<OptionsArgument>
        {
            public override string Name => "option";

            private readonly ImmutableArray<Option> validOptions;

            public ArgumentFactory(ImmutableArray<Option> validOptions)
            {
                this.validOptions = validOptions;
            }

            public override bool TryCreate(string arg, [NotNullWhen(true)] out OptionsArgument? argument)
            {
                Span<Option> options = stackalloc Option[arg.Length];
                for (int i = 0; i < arg.Length; i++)
                {
                    if (!validOptions.Any(option => option.Character == arg[i]))
                    {
                        argument = null;
                        return false;
                    }

                    options[i] = new Option(arg[i]);
                }
                argument = new OptionsArgument(options.ToArray());
                return true;
            }
        }
    }
}
