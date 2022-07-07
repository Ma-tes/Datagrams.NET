using DatagramsNet.Logging.Reading.Attributes;
using DatagramsNet.Logging.Reading.CommandExecuting;
using DatagramsNet.Logging.Reading.Indexes;
using DatagramsNet.Logging.Reading.Interfaces;
using DatagramsNet.Prefixes;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DatagramsNet.Logging.Reading
{
    public static class InteropHelper 
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

        private MethodInfo commandFunctionHolderMethodInfo => typeof(CommandFunctionHolder).GetMethod(nameof(CommandFunctionHolder.GetFunction));

        public void StartReading() 
        {
            while (true) 
            {
                var keyState = InteropHelper.GetKeyAsync(ConsoleKey.Enter);
                string commandString = Console.ReadLine();
                string commandStringName = commandString.Split(' ')[0];
                if (keyState.Result) 
                {
                    var command = GetCommand(commandStringName);
                    if (command is not null) 
                    {
                        Span<object> indexes = new Span<object>(GetIndexes(commandString, command.Indexes).ToArray());
                        var attributeIndex = indexes[0];
                        var currentIndexes = indexes.Slice(1, indexes.Length - 1).ToArray();
                        if (command is ICommandAction newCommand) 
                        {
                            MethodInfo genericsCommandFuntion = commandFunctionHolderMethodInfo.MakeGenericMethod(command.GetType());
                            var newCommandAction = genericsCommandFuntion.Invoke(this, new object[] {newCommand});
                            command = (ICommand)(ICommandAction)(newCommandAction);
                        }

                        string commandMessage;
                        if(attributeIndex is ArgumentIndex argumentIndex)
                            commandMessage = command.ExecuteCommand(argumentIndex.Value, currentIndexes).Result;
                        else
                            commandMessage = command.ExecuteCommand(new Argument[0], indexes.ToArray()).Result;

                        if(commandMessage != String.Empty && commandMessage is not null)
                            ServerLogger.Log<NormalPrefix>(commandMessage, TimeFormat.HALF);
                    }
                }
            }
        }

        private IEnumerable<object> GetIndexes(string command, object[] baseIndexes) 
        {
            for (int i = 0; i < baseIndexes.Length; i++)
            {
                var currentIndex = (dynamic)baseIndexes[i];
                yield return currentIndex.GetIndex(command, separator, i);
            }
        }

        private ICommand GetCommand(string command) 
        {
            Type commandAttribute = typeof(CommandAttribute);
            var commands = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes().Where(a => a.GetCustomAttributes(commandAttribute, true).Length > 0)).ToArray();

            for (int i = 0; i < commands.Length; i++)
            {
                var attribute = (CommandAttribute)commands[i].GetCustomAttribute(commandAttribute);
                if (attribute.Command == command) 
                {
                    return GetTypeInstace<ICommand>(commands[i]);
                }
            }

            ServerLogger.Log<WarningPrefix>("Command was not found", TimeFormat.HALF);
            return null;
        }

        private T GetTypeInstace<T>(Type objectType) 
        {
            var currentObjectType = Type.GetType(objectType.FullName);
            var currentCommand = Activator.CreateInstance(currentObjectType);
            return (T)currentCommand;
        }
    }
}
