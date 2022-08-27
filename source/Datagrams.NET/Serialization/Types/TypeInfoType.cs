using DatagramsNet.Serialization.Attributes;

namespace DatagramsNet.Serialization.Types
{
    [TypeSerializer(typeof(TypeInfoType))]
    internal sealed class TypeInfoType : ManagedTypeSerializer
    {
        public override byte[] Serialize<TParent>(SizedObject @object)
        {
            if (@object.Value is Type newType)
            {
                var typeName = newType.AssemblyQualifiedName;
                return BinaryHelper.Write(typeName!);
            }
            throw new Exception($"This object is not type of {nameof(Type)}");
        }

        public override T Deserialize<T>(byte[] bytes)
        {
            var finalName = BinaryHelper.Read<string>(bytes);
            return (T)(object)Type.GetType(finalName)!;
        }
    }
}
