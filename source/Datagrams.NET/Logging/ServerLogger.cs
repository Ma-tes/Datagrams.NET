using DatagramsNet.Interfaces;
using System.Collections.Concurrent;

namespace DatagramsNet.Logging
{
    [Flags]
    public enum TimeFormat
    {
        Full = 0,
        Half = 1,
        None = 2
    }

    public static class ServerLogger
    {
        private static readonly ConcurrentDictionary<Type, IPrefix> prefixes = new();
        private static readonly Queue<Message> messageQueue = new();
        private static readonly SemaphoreSlim messageQueueSemaphore = new(initialCount: 0);

        private const int WriterIdle = 0;
        private const int WriterRunning = 1;
        private static int writerState = WriterIdle;

        public static void Log<TPrefix>(string message, TimeFormat timeFormat = TimeFormat.Half) where TPrefix : IPrefix, new()
        {
            TPrefix prefix = GetPrefixInstance<TPrefix>();
            string dateTime = GetDateText(timeFormat);
            messageQueue.Enqueue(new Message() { Content = $"<{dateTime}> {message}", Prefix = prefix });
            messageQueueSemaphore.Release();
        }

        public static async Task LogAsync<TPrefix>(string message, TimeFormat timeFormat = TimeFormat.Half, int delay = 0) where TPrefix : IPrefix, new()
        {
            TPrefix prefix = GetPrefixInstance<TPrefix>();
            string dateTime = GetDateText(timeFormat);
            await Task.Delay(delay);
            messageQueue.Enqueue(new Message() { Content = $"<{dateTime}> {message}", Prefix = prefix });
            messageQueueSemaphore.Release();
        }

        public static void StartConsoleWriter()
        {
            if (Interlocked.Exchange(ref writerState, WriterRunning) == WriterRunning)
            {
                return; // Already running
            }

            Task.Run(async () =>
            {
                while (true)
                {
                    await messageQueueSemaphore.WaitAsync();
                    if (messageQueue.TryDequeue(out Message message))
                    {
                        if (message.Prefix is not null)
                            await message.Prefix.WritePrefixAsync();
                        await Console.Out.WriteLineAsync(message.Content);
                    }
                }
            });
        }

        private static string GetDateText(TimeFormat timeFormat)
        {
            if (timeFormat == TimeFormat.None)
                return string.Empty;
            return timeFormat == TimeFormat.Half ? DateTime.Now.ToShortTimeString() : DateTime.Now.ToLongTimeString();
        }

        private static TSource GetPrefixInstance<TSource>() where TSource : IPrefix, new()
        {
            return (TSource)prefixes.GetOrAdd(typeof(TSource), _ => new TSource());
        }
    }
}
