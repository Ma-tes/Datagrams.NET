using DatagramsNet.Datagram;
using DatagramsNet.Serialization.Attributes;
using DatagramsNet.Serialization.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DatagramsNet.Serialization
{
    internal readonly struct ManagedTypeHolder
    {
        public Type Type { get; }
        public TypeSerializerAttribute Attribute { get; }

        public ManagedTypeHolder(Type type, TypeSerializerAttribute attribute)
        {
            Type = type;
            Attribute = attribute;
        }
    }

    internal static class Serializer
    {
        private static readonly MethodInfo serialization = typeof(ManagedTypeFactory).GetMethod(nameof(ManagedTypeFactory.Serialize))!;
        private static ManagedTypeHolder[]? managedTypes;

        public static byte[] SerializeObject(object @object, int size)
        {
            if (Serializer.TryGetManagedType(@object.GetType(), out IManaged? managedType))
            {
                var table = new ObjectTableSize(@object, size);
                var bytes = (byte[])serialization.MakeGenericMethod(@object.GetType().BaseType!).Invoke(null, new object[] { managedType, table })!;
                return bytes;
            }

            if (@object.GetType().IsClass)
            {
                var classBytes = GetClassBytes(@object);
                if (classBytes is null)
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
                spanBytes = spanBytes[lastSize..];
                byte[] subBytes = spanBytes.ToArray();
                int size = BinaryHelper.GetSizeOf(membersInformation[i].MemberValue, membersInformation[i].MemberType, ref subBytes);

                var newBytes = subBytes[0..size];
                datagramTables[i] = new SubDatagramTable(newBytes, size);

                lastSize = size;
                spanBytes = subBytes;
            }
            return datagramTables;
        }

        public static bool TryGetManagedType(Type objectType, [NotNullWhen(true)] out IManaged? managedType)
        {
            managedTypes ??= GetManagedTypes().ToArray();

            for (int i = 0; i < managedTypes.Length; i++)
            {
                Type serializerType = managedTypes[i].Attribute.SerializerType;
                if (serializerType == objectType || serializerType == objectType.BaseType)
                {
                    managedType = Activator.CreateInstance(managedTypes[i].Type) as IManaged;
                    return managedType is not null;
                }
            }
            managedType = null;
            return false;
        }

        private static IEnumerable<ManagedTypeHolder> GetManagedTypes()
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(t => t.GetTypes().Where(a => a.GetCustomAttributes<TypeSerializerAttribute>(true).Any()))
                .Select(t => new ManagedTypeHolder(t, t.GetCustomAttribute<TypeSerializerAttribute>()!));
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
