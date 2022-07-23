using System.Text;
using DatagramsNet.Serializer.Attributes;

namespace DatagramsNet.Serializer.Types
{
    [SerializeType(typeof(String))]
    internal sealed class StringType : ManagedType
    {
        public override byte[] Serialize<TParent>(ObjectTableSize @object)
        {
            //var baseSerialize = base.Serialize<TParent>(@object);
            //if (baseSerialize is not null)
            //return baseSerialize;

            //This is really temporary solutions to prevent dynamic length of any string
            var bytesHolder = new List<byte>();
            byte[] bytes = Encoding.ASCII.GetBytes(((string)@object.Value));
            bytesHolder.Add((byte)bytes.Length);
            bytesHolder.AddRange(bytes);
            //CurrentTable.Add(new SerializeTable(@object.Value, bytes));
            return bytesHolder.ToArray();
        }
    }
}
