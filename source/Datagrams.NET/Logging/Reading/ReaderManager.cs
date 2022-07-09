using DatagramsNet.Logging.Reading.Attributes;
using DatagramsNet.Logging.Reading.CommandExecution;
using DatagramsNet.Logging.Reading.Arguments;
using DatagramsNet.Logging.Reading.Models;
using DatagramsNet.Prefixes;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DatagramsNet.Logging.Reading
{
    internal static class InteropHelper
    {
        [DllImport("User32.dll")]
        private static extern ushort GetAsyncKeyState(byte keyNumber);

        public static async Task<bool> GetKeyAsync(ConsoleKey key)
        {
            var stateNumber = await Task.Run(() => GetAsyncKeyState((byte)key));
            return stateNumber != 0;
        }
    }

    public sealed class ReaderManager
    {
        private const char baseCharacter = '>';

        private const char separator = ' ';

        private static readonly MethodInfo commandFunctionHolderMethodInfo = typeof(CommandFunctionHolder).GetMethod(nameof(CommandFunctionHolder.GetFunction))!;

        public void StartReading()
        {
            while (true)
            {
                var keyState = InteropHelper.GetKeyAsync(ConsoleKey.Enter);
                string commandString = Console.ReadLine() ?? string.Empty;
                string commandStringName = commandString.Split(' ')[0];
                if (keyState.Result)
                {
                    var command = GetCommand(commandStringName);
                    if (command is not null)
                    {
                        Span<object> indexes = GetIndexes(commandString, command.Arguments).ToArray();
                        var attributeIndex = indexes[0];
                        var currentIndexes = indexes[1..].ToArray();
                        if (command is ICommandAction newCommand)
                        {
                            MethodInfo genericsCommandFunction = commandFunctionHolderMethodInfo.MakeGenericMethod(command.GetType());
                            var newCommandAction = genericsCommandFunction.Invoke(this, new object[] { newCommand })!;
                            command = (ICommand)(ICommandAction)(newCommandAction);
                        }

                        string? commandMessage;
                        if (attributeIndex is OptionsArgument argumentIndex)
                            commandMessage = command.ExecuteCommandAsync(argumentIndex.Value, currentIndexes).Result;
                        else
                            commandMessage = command.ExecuteCommandAsync(Array.Empty<Option>(), indexes.ToArray()).Result;

                        if (commandMessage != string.Empty && commandMessage is not null)
                            _ = ServerLogger.LogAsync<NormalPrefix>(commandMessage, TimeFormat.Half);
                    }
                }
            }
        }

        private static IEnumerable<object> GetIndexes(string command, object[] baseIndexes)
        {
            for (int i = 0; i < baseIndexes.Length; i++)
            {
                var currentIndex = (dynamic)baseIndexes[i];
                yield return currentIndex.GetArgument(command, separator, i);
            }
        }

        private static ICommand? GetCommand(string command)
        {
            Type commandAttribute = typeof(CommandAttribute);
            var commands = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes().Where(a => a.GetCustomAttributes(commandAttribute, true).Length > 0)).ToArray();

            for (int i = 0; i < commands.Length; i++)
            {
                var attribute = (CommandAttribute)commands[i].GetCustomAttribute(commandAttribute)!;
                if (attribute.Command == command)
                {
                    return GetTypeInstace<ICommand>(commands[i]);
                }
            }

            _ = ServerLogger.LogAsync<WarningPrefix>("Command was not found", TimeFormat.Half);
            return null;
        }

        private static T? GetTypeInstace<T>(Type objectType)
        {
            return (T?)Activator.CreateInstance(objectType);
        }
    }
}
