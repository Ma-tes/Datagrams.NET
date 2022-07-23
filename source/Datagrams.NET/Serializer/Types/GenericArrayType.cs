using DatagramsNet.Serializer.Attributes;

namespace DatagramsNet.Serializer.Types
{
    [SerializeType(typeof(Array))]
    internal sealed class GenericArrayType : ManagedType
    {
        public override byte[] Serialize<TParent>(ObjectTableSize @object)
        {
            var baseSerialize = base.Serialize<TParent>(@object);
            if (baseSerialize is not null)
                return baseSerialize;
            //TODO: Implement serialization of Array
            return null;
        }
    }
}
