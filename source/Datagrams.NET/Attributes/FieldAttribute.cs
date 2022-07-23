
namespace DatagramsNet.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property , AllowMultiple = false, Inherited = false)]
    public sealed class FieldAttribute : Attribute
    {
        public int FieldIndex { get; set; }

        public FieldAttribute(int index) => FieldIndex = index;
    }
}
