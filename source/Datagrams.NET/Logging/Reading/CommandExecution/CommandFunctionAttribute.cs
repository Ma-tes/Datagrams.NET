using DatagramsNet.Logging.Reading.Models;

namespace DatagramsNet.Logging.Reading.CommandExecution
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
