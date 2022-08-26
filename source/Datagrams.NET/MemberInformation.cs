namespace DatagramsNet
{
    public readonly struct NullValue { }

    public sealed class MemberInformation
    {
        public object? MemberValue { get; }
        public Type MemberType { get; set; }

        public MemberInformation(object? @object, Type memberType)
        {
            MemberValue = @object ?? new NullValue();
            MemberType = memberType;
        }
    }
}
