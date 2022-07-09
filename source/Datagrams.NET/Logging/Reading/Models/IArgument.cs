namespace DatagramsNet.Logging.Reading.Models
{
    public interface IArgument<T, TOutput> where TOutput : new()
    {
        public string Name { get; }

        public T Value { get; set; }

        public TOutput GetArgument(string command, char separator, int index);
    }
}
