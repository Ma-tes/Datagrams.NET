
namespace DatagramsNet
{
    public readonly struct MemberInformation
    {
        public object MemberValue { get; }

        public Type MemberType { get; }

        public MemberInformation(object @object, Type memberType) 
        {
            MemberValue = @object;
            MemberType = memberType;
        }
    }
}
