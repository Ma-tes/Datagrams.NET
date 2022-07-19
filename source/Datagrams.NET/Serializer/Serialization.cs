using DatagramsNet.Serializer.Attributes;
using DatagramsNet.Serializer.Interfaces;
using DatagramsNet.Serializer.Types;
using System.Reflection;

namespace DatagramsNet.Serializer
{
    internal static class Serialization
    {
        private static MethodInfo serialization = typeof(ManagedTypeFactory).GetMethod(nameof(ManagedTypeFactory.Serialize));

        public static byte[] SerializeObject(object @object, int size)
        {
            var managedType = TryGetManagedType(@object.GetType());
            if (managedType is not null) 
            {
                var table = new ObjectTableSize(@object, size);
                var bytes = (byte[])serialization.MakeGenericMethod(@object.GetType().BaseType).Invoke(null, new object[] { managedType, table });
                return bytes;
            }

            if (@object.GetType().IsClass) 
            {
                var classBytes = GetClassBytes(@object);
                if(classBytes is null)
                    throw new ArgumentNullException(nameof(@object));
                return classBytes.ToArray();
            }
            return null;
        }

        private static IManaged TryGetManagedType(Type objectType) 
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes().Where(a => a.GetCustomAttributes(typeof(SerializeTypeAttribute), true).Length > 0)).ToArray();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var attribute = (SerializeTypeAttribute)assemblies[i].GetCustomAttribute(typeof(SerializeTypeAttribute))!;
                if (attribute.SerializerType == objectType) 
                {
                    var newManagedType = (IManaged)(Activator.CreateInstance((assemblies[i])))!;
                    return newManagedType;
                }
            }
            return null;
        }

        private static byte[] GetClassBytes(object @object) 
        {
            var members = BinaryHelper.GetMembersInformation(@object);
            var bytes = new List<byte>();
            for (int i = 0; i < members.Length; i++)
            {
                bytes.AddRange(BinaryHelper.Write(members[i].MemberValue));
            }
            return bytes.ToArray();
        }
    }
}
