﻿using System.Reflection;
using DatagramsNet.Interfaces;
using DatagramsNet.Attributes;

namespace DatagramsNet
{
    public static class DatagramHelper
    {
        private static MethodInfo read = typeof(BinaryHelper).GetMethod(nameof(BinaryHelper.Read));

        public static async Task SendDatagramAsync(Func<byte[], Task> sendAction, List<byte[]> data) 
        {
            var newSubData = Task.Run(() => new DatagramIdentificator(data).SerializeDatagram());
            await sendAction(newSubData.Result.ToArray());
        }

        public static List<byte[]> WriteDatagram<T>(T datagram) where T : IDatagram 
        {
            var datagramList = new List<byte[]>();
            var customProperties = datagram.GetType().GetFields();
            for (int i = 0; i < customProperties.Length; i++) 
            {
                var value = customProperties[i].GetValue(datagram);

                if (value is not null) 
                {
                    using var _binaryHelper = new BinaryHelper(value);
                    _binaryHelper.Write();
                    datagramList.Add(_binaryHelper.MemoryHolder);
                }
            }
            return datagramList;
        }

        public static object ReadDatagram(List<byte[]> datagram)
        {
            int datagramId;
            Type datagramType;
            using (var idReader = new BinaryHelper(datagram[0])) 
            {
                datagramId = idReader.Read<int>();
                datagramType = GetBaseDatagramType(datagramId, typeof(PacketAttribute));
            }
            return SetDatagramData(datagramType, datagram);;
        }

        private static object SetDatagramData(Type datagramType, List<byte[]> data) 
        {
            var datagram = (Activator.CreateInstance(datagramType));
            FieldInfo[] fields = datagram.GetType().GetFields();
            for (int i = 1; i < data.Count; i++)
            {
                using (var reader = new BinaryHelper(data[i])) 
                {
                    Type fieldType = fields[i].FieldType;
                    var fieldValue = read.MakeGenericMethod(fieldType).Invoke(reader, Array.Empty<object>());
                    fields[i].SetValue(datagram, fieldValue);
                }
            }
            return datagram;
        }

        public static Type GetBaseDatagramType(int id, Type classAttributeType) 
        {
            var assemblies = Assembly.GetExecutingAssembly().GetTypes().Where(a => a.GetCustomAttributes(classAttributeType, true).Length > 0).ToArray();
            for (int i = 0; i < assemblies.Length; i++)
            {
                FieldInfo[] properties = assemblies[i].GetFields();
                for (int j = 0; j < properties.Length; j++)
                {
                    var fieldValue = properties[j].GetValue(Activator.CreateInstance(assemblies[i]));
                    if (fieldValue.Equals(id))
                        return assemblies[i];
                }
            }
            return null;
        }
    }
}