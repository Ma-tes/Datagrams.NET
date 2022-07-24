using System.Reflection;

namespace DatagramsNet.Datagram
{
    public static class DatagramHelper
    {
        private static MethodInfo read = typeof(BinaryHelper).GetMethod(nameof(BinaryHelper.Read))!;

        public static async Task SendDatagramAsync(Func<byte[], Task> sendAction, ReadOnlyMemory<byte[]> data)
        {
            var newSubData = new DatagramIdentificator(data).SerializeDatagram();
            await sendAction(newSubData);
        }

        public static Memory<byte[]> WriteDatagram<T>(T datagram)
        {
            var datagramList = new List<byte[]>();
            var customProperties = datagram.GetType().GetProperties();
            for (int i = 0; i < customProperties.Length; i++)
            {
                var value = customProperties[i].GetValue(datagram);

                if (value is not null)
                {
                    byte[] bytes = BinaryHelper.Write(value);
                    datagramList.Add(bytes);
                }
            }
            return datagramList.ToArray();
        }

        public static object SetObjectData(Type datagramType, Memory<byte[]> data)
        {
            var datagram = Activator.CreateInstance(datagramType);
            PropertyInfo[] fields = datagram.GetType().GetProperties();
            for (int i = 1; i < data.Length; i++)
            {
                Type fieldType = fields[i].PropertyType;
                var fieldValue = read.MakeGenericMethod(fieldType).Invoke(null, new object[] { data.Span[i] });
                fields[i].SetValue(datagram, fieldValue);
            }
            return datagram;
        }

        public static Type GetBaseDatagramType(int id, Type classAttributeType)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes().Where(a => a.GetCustomAttributes(classAttributeType, true).Length > 0)).ToArray();
            for (int i = 0; i < assemblies.Length; i++)
            {
                PropertyInfo[] properties = assemblies[i].GetProperties();
                object fieldValue = properties[0].GetValue(Activator.CreateInstance(assemblies[i]));
                if (fieldValue.Equals(id))
                    return assemblies[i];
            }
            return null;
        }
    }
}
