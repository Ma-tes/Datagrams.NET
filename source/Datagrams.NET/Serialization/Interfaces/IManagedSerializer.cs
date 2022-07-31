namespace DatagramsNet.Serialization.Interfaces
{
    internal interface IManagedSerializer
    {
        public byte[] Serialize<TParent>(SizedObject @object);

        public T Deserialize<T>(byte[] bytes);
    }
}
