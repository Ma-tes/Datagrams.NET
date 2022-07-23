
namespace DatagramsNet
{
    public readonly struct NullValue { }

    public sealed class MemberInformation
    {
        public object? MemberValue { get; }

        public Type MemberType { get; }

        public MemberInformation(object? @object, Type memberType) 
        {
            if (@object is null) 
            {
                MemberValue = new NullValue();
            }
            else
                MemberValue = @object;
            MemberType = memberType;
        }
    }
}
