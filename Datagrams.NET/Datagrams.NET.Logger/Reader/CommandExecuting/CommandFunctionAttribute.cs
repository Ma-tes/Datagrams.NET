using DatagramsNet.Datagrams.NET.Logger.Reader.Interfaces;

namespace DatagramsNet.Datagrams.NET.Logger.Reader.CommandExecuting
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandFunctionAttribute<T> : Attribute where T : ICommandAction
    {
        public ICommandAction Command { get; private set; }

        public CommandFunctionAttribute() 
        {
            Command = Activator.CreateInstance<T>();
        }
    }
}
