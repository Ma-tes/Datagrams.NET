using DatagramsNet.Datagrams.NET.Logger;
using DatagramsNet.Interfaces;
using System.Collections.Concurrent;

namespace DatagramsNet.NET.Logger
{
    [Flags]
    public enum TimeFormat
    {
        FULL = 0,
        HALF = 1,
        NONE = 2
    }

    public static class ServerLogger
    {
        private static IList<IPrefix> prefixes = new List<IPrefix>();

        private static ConcurrentQueue<Message> queueMessages = new();

        public static async Task Log<TSource>(string message, TimeFormat timeFormat = TimeFormat.HALF, int delay = 0) where TSource : IPrefix, new()
        {
            var prefix = await GetPrefix<TSource>();
            var dateTime = await GetDateText(timeFormat);
            lock (queueMessages) 
            {
                queueMessages.Enqueue(new Message() { SingleMessage = $"<{dateTime}> {message}", Prefix = prefix });
            }
            await Task.Delay(delay);
        }

        public static void StartConsoleWriter() 
        {
            Task.Run(async() => { while (true) 
            {
                if (queueMessages.Count != 0) 
                {

                    Message newMessage = new Message();
                    queueMessages.TryDequeue(out newMessage);
                    if(newMessage.Prefix is not null)
                        await newMessage.Prefix.WritePrefixAsync();
                    await Console.Out.WriteLineAsync(newMessage.SingleMessage);
                }
            }});
        }

        private static Task<string> GetDateText(TimeFormat timeFormat) 
        {
            if (timeFormat == TimeFormat.NONE)
                return Task.FromResult(String.Empty);
            return timeFormat == TimeFormat.HALF ? Task.FromResult(DateTime.Now.ToShortTimeString()) : Task.FromResult(DateTime.Now.ToLongTimeString());
        }

        public static async Task<TSource> GetPrefix<TSource>() where TSource : IPrefix, new()
        {
            for (int i = 0; i < prefixes.Count; i++)
            {
                if (typeof(TSource) == prefixes[i].GetType())
                    return (TSource)prefixes[i];
            }
            prefixes.Add(new TSource());
            return (TSource)prefixes[prefixes.Count - 1];
        }
    }
}
