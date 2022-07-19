namespace DatagramsNet.Serializer.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class SerializeTypeAttribute : Attribute
    {
        public Type SerializerType { get; }

        public SerializeTypeAttribute(Type serializerType) 
        {
            SerializerType = serializerType;
        }
    }
}
