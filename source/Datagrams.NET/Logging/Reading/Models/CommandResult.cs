using System.Diagnostics.CodeAnalysis;

namespace DatagramsNet.Logging.Reading.Models
{
    public readonly struct CommandResult
    {
        public bool Success { get; }
        [MemberNotNullWhen(true, nameof(Success))]
        public string? Message { get; }

        private CommandResult(bool success, string? message)
        {
            Success = success;
            Message = message;
        }

        public static CommandResult Ok(string message)
        {
            return new CommandResult(true, message);
        }

        public static CommandResult Fail(string? message)
        {
            return new CommandResult(false, message);
        }
    }
}
