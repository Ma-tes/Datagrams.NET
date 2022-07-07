using DatagramsNet.Logging.Reading.Interfaces;
using System.Reflection;

namespace DatagramsNet.Logging.Reading.CommandExecuting
{
    internal struct ActionCommand
    {
        public Type CommandType { get; set; }

        public Action CommandAction { get; set; }
    }

    internal static class CommandFunctionHolder
    {
        private static List<ActionCommand> actionCommands = new();

        public static ICommandAction GetFunction<T>(T command) where T : ICommandAction
        {
            var firstCommand = actionCommands.FirstOrDefault(n => command.GetType() == n.CommandType);
            if (firstCommand.CommandType is not null)
            {
                command.CommandAction = firstCommand.CommandAction;
                return command;
            }
            var cacheCommand = CacheCommandFunction(command);
            actionCommands.Add(new ActionCommand() { CommandType = command.GetType(), CommandAction = cacheCommand.CommandAction });
            return cacheCommand;
        }

        private static ICommandAction CacheCommandFunction<T>(T command) where T : ICommandAction
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Action methodAction = default;
            for (int i = 0; i < assemblies.Length; i++)
            {
                //For now command will support only one action or it will throw exception
                int assembliesCount = assemblies[i].DefinedTypes.Count();
                for (int j = 0; j < assembliesCount; j++)
                {
                    var assemblyType = assemblies[i].DefinedTypes.ToArray()[j];
                    var commandActions = GetCommandActions(assemblyType, command).ToArray();
                    if (commandActions.Length != 0)
                        methodAction = commandActions[0];
                }
                //else
                //throw new Exception("Multiple actions for one command are not supported");
            }
            command.CommandAction = methodAction;
            return command;
        }

        private static IEnumerable<Action> GetCommandActions<T>(Type assembly, T command) where T : ICommandAction
        {
            var assemblyMethods = assembly.GetMethods().Where(n => n.GetCustomAttributes(typeof(CommandFunctionAttribute<T>), true).Length > 0).ToArray();
            for (int i = 0; i < assemblyMethods.Length; i++)
            {
                var commandAttribute = (CommandFunctionAttribute<T>[])assemblyMethods[i].GetCustomAttributes(typeof(CommandFunctionAttribute<T>), true);
                for (int j = 0; j < commandAttribute.Length; j++)
                {
                    if (commandAttribute[j].Command.GetType() == command.GetType())
                    {
                        var delegateAction = (Action)Delegate.CreateDelegate(typeof(Action), assemblyMethods[i]);
                        yield return delegateAction;
                    }
                }
            }
        }
    }
}
