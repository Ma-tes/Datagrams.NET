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
        private static readonly List<IPrefix> prefixes = new();

        private static readonly ConcurrentQueue<Message> queueMessages = new();

        public static async Task LogAsync<TSource>(string message, TimeFormat timeFormat = TimeFormat.Half, int delay = 0) where TSource : IPrefix, new()
        {
            var prefix = GetPrefix<TSource>();
            var dateTime = await GetDateText(timeFormat);
            lock (queueMessages)
            {
                queueMessages.Enqueue(new Message() { SingleMessage = $"<{dateTime}> {message}", Prefix = prefix });
            }
            await Task.Delay(delay);
        }

        public static void StartConsoleWriter()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    if (!queueMessages.IsEmpty && queueMessages.TryDequeue(out Message message))
                    {
                        if (message.Prefix is not null)
                            await message.Prefix.WritePrefixAsync();
                        await Console.Out.WriteLineAsync(message.SingleMessage);
                    }
                }
            });
        }

        private static Task<string> GetDateText(TimeFormat timeFormat)
        {
            if (timeFormat == TimeFormat.None)
                return Task.FromResult(string.Empty);
            return timeFormat == TimeFormat.Half ? Task.FromResult(DateTime.Now.ToShortTimeString()) : Task.FromResult(DateTime.Now.ToLongTimeString());
        }

        public static TSource GetPrefix<TSource>() where TSource : IPrefix, new()
        {
            for (int i = 0; i < prefixes.Count; i++)
            {
                if (typeof(TSource) == prefixes[i].GetType())
                    return (TSource)prefixes[i];
            }
            prefixes.Add(new TSource());
            return (TSource)prefixes[^1];
        }
    }
}
