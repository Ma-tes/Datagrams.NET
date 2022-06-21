﻿
namespace DatagramsNet.Datagrams.NET.Logger.Reader.Interfaces
{
    public interface IIndex<T, TOutPut> where TOutPut : new() 
    {
        public string Name { get; }

        public T Value { get; set; }

        public TOutPut GetIndex(string command, char separator, int index);
    }
}
