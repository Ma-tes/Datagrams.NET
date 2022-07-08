
namespace DatagramsNet.Prefixes
{
    public sealed class WarningPrefix : StandardPrefix
    {
        public override string Name => nameof(WarningPrefix);

        public override ConsoleColor Color => ConsoleColor.Yellow;
    }
}
