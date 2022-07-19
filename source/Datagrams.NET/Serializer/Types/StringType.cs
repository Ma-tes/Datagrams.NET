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
            byte[] bytes = Encoding.ASCII.GetBytes(((string)@object.Value));
            //CurrentTable.Add(new SerializeTable(@object.Value, bytes));
            return bytes;
        }
    }
}
