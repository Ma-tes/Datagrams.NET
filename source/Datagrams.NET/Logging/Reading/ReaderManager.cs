using DatagramsNet.Logging.Reading.Arguments;
using DatagramsNet.Logging.Reading.Attributes;
using DatagramsNet.Logging.Reading.CommandExecution;
using DatagramsNet.Logging.Reading.Models;
using DatagramsNet.Prefixes;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DatagramsNet.Logging.Reading
{
    public static class ReaderManager
    {
        private const char PrefixCharacter = '>';
        private const char SeparatorCharacter = ' ';

        private static readonly MethodInfo commandFunctionHolderMethodInfo = typeof(CommandFunctionHolder).GetMethod(nameof(CommandFunctionHolder.GetFunction))!;
        private static readonly Dictionary<string, Command> commands =
            AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes().Where(type => type.GetCustomAttributes<CommandAttribute>(true).Any()))
            .ToDictionary(type => type.GetCustomAttribute<CommandAttribute>()!.Command, type => (Command)Activator.CreateInstance(type)!);

        private static int reading = 0;

        public static void StartReading()
        {
            // Prevent this method from being ran multiple times
            if (Interlocked.Exchange(ref reading, 1) == 1)
                return;

            Task.Run(async () =>
            {
                while (true)
                {
                    await ReadAsync();
                }
            }).Wait();
        }

        private static async Task ReadAsync()
        {
            Console.Write($"{PrefixCharacter} ");
            // Read console input
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return;

            // Get input tokens
            List<string> tokens = Tokenize(input);

            // Try to find a matching command
            if (!commands.TryGetValue(tokens[0], out Command? command))
            {
                ServerLogger.Log<WarningPrefix>("Command was not found.", TimeFormat.Half);
                return;
            }

            // Parse arguments if any
            object[]? arguments;
            if (tokens.Count == 1)
            {
                arguments = Array.Empty<object>();
            }
            else
            {
                arguments = GetArguments(command, CollectionsMarshal.AsSpan(tokens)[1..]);
                if (arguments is null)
                    return;
            }

            // Invoke the command
            CommandResult result;
            if (arguments.FirstOrDefault() is OptionsArgument options)
                result = await command.ExecuteAsync(options.Value, arguments[1..]);
            else
                result = await command.ExecuteAsync(Array.Empty<Option>(), arguments);

            // Display result
            if (result.Message is not null)
            {
                if (result.Success)
                {
                    ServerLogger.Log<NormalPrefix>(result.Message, TimeFormat.Half);
                }
                else
                {
                    ServerLogger.Log<ErrorPrefix>(result.Message, TimeFormat.Half);
                }
            }
        }

        private static List<string> Tokenize(string text)
        {
            var tokens = new List<string>();

            int start = 0; // Start of next token, inclusive
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsWhiteSpace(text[i]))
                {
                    if (i == start) // Ignore empty entries
                    {
                        start++;
                        continue;
                    }

                    tokens.Add(text[start..i]);
                    start = i + 1;
                }
                else if (text[i] == '"')
                {
                    start = i + 1;
                    for (i = start; i < text.Length && text[i] != '"'; i++) ; // Increment until next '"' or end of string
                    tokens.Add(text[start..i]);
                    start = i + 1;
                }
            }

            // Add the rest
            if (start < text.Length)
                tokens.Add(text[start..]);

            return tokens;
        }

        private static object[]? GetArguments(Command command, Span<string> args)
        {
            var arguments = new object[args.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                IFactory factory = command.Arguments[i];
                object? argument = factory.Create(args[i]);
                if (argument is not null)
                {
                    arguments[i] = argument!;
                }
                else
                {
                    ServerLogger.Log<ErrorPrefix>($"Received an invalid {factory.Name} argument '{args[i]}'.", TimeFormat.Half);
                    return null;
                }
            }
            return arguments;
        }
    }
}
