using System.Net;

namespace DatagramsNet.Interfaces
{
    public interface IReciever
    {
        public SocketReciever SocketReciever { get; set; }

        public virtual Task OnRecieveAsync(object datagram, IPAddress ipAddress)
        {
            return Task.CompletedTask;
        }
    }
}
