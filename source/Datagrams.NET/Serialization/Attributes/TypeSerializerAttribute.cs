namespace DatagramsNet.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class TypeSerializerAttribute : Attribute
    {
        public Type SerializerType { get; }

        public TypeSerializerAttribute(Type serializerType)
        {
            SerializerType = serializerType;
        }
    }
}
