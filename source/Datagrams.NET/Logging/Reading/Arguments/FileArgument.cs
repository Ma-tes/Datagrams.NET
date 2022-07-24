using DatagramsNet.Logging.Reading.Models;
using System.Diagnostics.CodeAnalysis;

namespace DatagramsNet.Logging.Reading.Arguments
{
    internal sealed class FileArgument : IArgument<string>
    {
        public string Name => "@file";
        public string Value { get; }

        public static IArgumentFactory<FileArgument> Factory => _factory ??= new();
        private static ArgumentFactory? _factory;

        public FileArgument(string value)
        {
            Value = value;
        }

        private sealed class ArgumentFactory : ArgumentFactory<FileArgument>
        {
            public override string Name => "file";

            public override bool TryCreate(string arg, [NotNullWhen(true)] out FileArgument? argument)
            {
                if (File.Exists(arg))
                {
                    argument = new FileArgument(arg);
                    return true;
                }

                argument = null;
                return false;
            }
        }
    }
}
