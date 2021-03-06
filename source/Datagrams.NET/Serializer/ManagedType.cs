using DatagramsNet.Serializer.Interfaces;

namespace DatagramsNet.Serializer
{
    internal readonly struct SerializeTable 
    {
        public object Object { get; }

        public byte[] Bytes { get; }

        public SerializeTable(object @object, byte[] bytes) 
        {
            Object = @object;
            Bytes = bytes;
        }
    }

    internal static class ManagedTypeFactory 
    {
        public static byte[] Serialize<TParent>(IManaged managedType, ObjectTableSize @object) => managedType.Serialize<TParent>(@object);

        public static T Deserialize<T>(IManaged managedType, byte[] bytes) => managedType.Deserialize<T>(bytes);
    }

    internal abstract class ManagedType : IManaged
    {
        private static Dictionary<ManagedObjectKey, List<SerializeTable>> cacheType = new();

        protected static List<SerializeTable> CurrentTable { get; set; }

        public virtual byte[] Serialize<TParent>(ObjectTableSize @object) 
        {
            var objectKey = new ManagedObjectKey(typeof(TParent), nameof(@object.Value));
            if (cacheType.ContainsKey(objectKey))
            {
                var tables = cacheType.GetValueOrDefault(objectKey);
                if (tables is null)
                    return null;
                byte[] bytes = tables!.First(n => n.Object.Equals(@object)).Bytes;
                return bytes;
            }
            cacheType.Add(objectKey, new());
            var newTable = cacheType.GetValueOrDefault(objectKey);
            CurrentTable = newTable;

            return null;
        }

        //Implement ability to check if result is not already cached
        public virtual T Deserialize<T>(byte[] bytes) { return default; }
    }
}
