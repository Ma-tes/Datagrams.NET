
namespace DatagramsNet.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property , AllowMultiple = false, Inherited = false)]
    public sealed class FieldAttribute : Attribute
    {
        public int FieldIndex { get; }

        public FieldAttribute(int index) => FieldIndex = index;
    }
}
