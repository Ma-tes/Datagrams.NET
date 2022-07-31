using DatagramsNet.Serialization.Interfaces;

namespace DatagramsNet.Serialization
{
    internal static class ManagedTypeFactory
    {
        public static byte[] Serialize<TParent>(IManagedSerializer managedType, SizedObject @object) => managedType.Serialize<TParent>(@object);

        public static T Deserialize<T>(IManagedSerializer managedType, byte[] bytes) => managedType.Deserialize<T>(bytes);
    }

    internal abstract class ManagedTypeSerializer : IManagedSerializer
    {
        private static readonly Dictionary<ManagedObjectKey, List<SerializeTable>> cacheType = new();

        protected static List<SerializeTable>? CurrentTable { get; set; }

        public virtual byte[] Serialize<TParent>(SizedObject @object)
        {
            var objectKey = new ManagedObjectKey(typeof(TParent), nameof(@object.Value));
            if (cacheType.ContainsKey(objectKey))
            {
                var tables = cacheType.GetValueOrDefault(objectKey);
                if (tables is null)
                    return null!;
                byte[] bytes = tables!.First(n => n.Object.Equals(@object)).Bytes;
                return bytes;
            }
            cacheType.Add(objectKey, new());
            var newTable = cacheType.GetValueOrDefault(objectKey);
            CurrentTable = newTable;

            return null!;
        }

        public virtual T Deserialize<T>(byte[] bytes) { return default!; }
    }
}
