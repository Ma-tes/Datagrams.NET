namespace DatagramsNet.Logging.Reading.Models
{
    public readonly struct Option
    {
        public char Character { get; }
        public Option(char character) => Character = character;
    }

    public interface ICommand
    {
        public Option[]? Options { get; }
        public object[] Arguments { get; }

        public Task<string?> ExecuteCommandAsync(Option[] options, object[] arguments);
    }
}
