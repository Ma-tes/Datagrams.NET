namespace DatagramsNet.Logging.Reading.Models
{
    public interface IFactory
    {
        public string Name { get; }
        public object? Create(string arg);
    }
}
