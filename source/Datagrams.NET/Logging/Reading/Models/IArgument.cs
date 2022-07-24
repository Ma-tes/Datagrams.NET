namespace DatagramsNet.Logging.Reading.Models
{
    public interface IArgument<T>
    {
        public string Name { get; }
        public T Value { get; }
    }
}
