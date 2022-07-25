using DatagramsNet.Datagram;
using DatagramsNet.Serializer.Attributes;
using DatagramsNet.Serializer.Interfaces;
using System.Reflection;

namespace DatagramsNet.Serializer
{
    internal readonly struct ManagedTypeHolder
    {
        public Type Type { get; }

        public SerializeTypeAttribute Attribute { get; }

        public ManagedTypeHolder(Type type, SerializeTypeAttribute attribute) 
        {
            Type = type;
            Attribute = attribute;
        }
    }

    internal static class Serialization
    {
        private static MethodInfo serialization = typeof(ManagedTypeFactory).GetMethod(nameof(ManagedTypeFactory.Serialize))!;

        private static ManagedTypeHolder[]? managedTypes;

        public static byte[] SerializeObject(object @object, int size)
        {
            var managedType = TryGetManagedType(@object.GetType());
            if (managedType is not null) 
            {
                var table = new ObjectTableSize(@object, size);
                var bytes = (byte[])serialization.MakeGenericMethod(@object.GetType().BaseType!).Invoke(null, new object[] { managedType, table })!;
                return bytes;
            }

            if (@object.GetType().IsClass) 
            {
                var classBytes = GetClassBytes(@object);
                if(classBytes is null)
                    throw new ArgumentNullException(nameof(@object));
                return classBytes.ToArray();
            }
            return null!;
        }

        public static object DeserializeBytes(Type datagramType, byte[] datagramData) 
        {
            var subData = GetSubDatagrams(datagramType, datagramData);
            var bytes = new List<byte[]>();
            for (int i = 0; i < subData.Length; i++)
            {
                bytes.Add(subData[i].Bytes);
            }

            return DatagramHelper.SetObjectData(datagramType, bytes.ToArray().AsMemory());
        }

        public static SubDatagramTable[] GetSubDatagrams(Type datagramType, byte[] datagramData)
        {
            Span<byte> spanBytes = datagramData;
            var datagramInstance = Activator.CreateInstance(datagramType);

            var membersInformation = BinaryHelper.GetMembersInformation(datagramInstance!).ToArray();
            var datagramTables = new SubDatagramTable[membersInformation.Length];

            int lastSize = 0;
            for (int i = 0; i < membersInformation.Length; i++)
            {
                spanBytes = spanBytes.Slice(lastSize);
                byte[] subBytes = spanBytes.ToArray();
                int size = BinaryHelper.GetSizeOf(membersInformation[i].MemberValue, ref subBytes);

                var newBytes = subBytes[0..size];
                datagramTables[i] = new SubDatagramTable(newBytes, size);

                lastSize = size;
                spanBytes = subBytes;
            }
            return datagramTables;
        }

        public static IManaged TryGetManagedType(Type objectType) 
        {
            if (managedTypes is null)
                managedTypes = GetManagedTypes().ToArray();

            for (int i = 0; i < managedTypes.Length; i++)
            {
                if (managedTypes[i].Attribute.SerializerType == objectType || managedTypes[i].Attribute.SerializerType == objectType.BaseType) 
                {
                    var newManagedType = (IManaged)(Activator.CreateInstance((managedTypes[i].Type)))!;
                    return newManagedType;
                }
            }
            return null!;
        }

        private static IEnumerable<ManagedTypeHolder> GetManagedTypes() 
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes().Where(a => a.GetCustomAttributes(typeof(SerializeTypeAttribute), true).Length > 0)).ToArray();
            for (int i = 0; i < types.Length; i++)
            {
                var attribute = (SerializeTypeAttribute)types[i].GetCustomAttribute(typeof(SerializeTypeAttribute))!;
                yield return new ManagedTypeHolder(types[i], attribute);
            }
        }

        private static byte[] GetClassBytes(object @object) 
        {
            var members = BinaryHelper.GetMembersInformation(@object);
            var bytes = new List<byte>();
            for (int i = 0; i < members.Length; i++)
            {
                bytes.AddRange(BinaryHelper.Write(members[i].MemberValue!));
            }
            return bytes.ToArray();
        }
    }
}
