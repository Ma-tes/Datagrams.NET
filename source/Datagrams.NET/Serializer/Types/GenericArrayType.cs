using DatagramsNet.Serializer.Attributes;

namespace DatagramsNet.Serializer.Types
{
    [SerializeType(typeof(Array))]
    internal sealed class GenericArrayType : ManagedType
    {
        public override byte[] Serialize<TParent>(ObjectTableSize @object)
        {
            //var baseSerialize = base.Serialize<TParent>(@object);
            //if (baseSerialize is not null)
            //return baseSerialize;

            var bytes = new List<byte>();
            bytes.Add((byte)@object.Size);
            var objectArray = (Array)@object.Value!;

            for (int i = 0; i < objectArray.Length; i++)
            {
                object currentValue = objectArray.GetValue(i)!;
                byte[] currentBytes = BinaryHelper.Write(currentValue);
                bytes.AddRange(currentBytes);
            }
            return bytes.ToArray();
        }
    }
}
