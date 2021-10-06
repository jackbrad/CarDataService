using System.Configuration;
using System.Net.Mime;
using System.Text;
using Microsoft.ServiceBus.Messaging;

namespace DataMassager
{
    public class Queue
    {
        private readonly EventHubClient _eh;
        public Queue(bool dev = true, string ehName = "sv-gateway")
        {
            var xon = dev ? "ehtarget-dev" : "ehtarget";
            _eh = EventHubClient.CreateFromConnectionString(ConfigurationManager.AppSettings[xon], ehName);
        }

        public void WriteToHub(string data)
        {
            WriteToHub(Encoding.UTF8.GetBytes(data));
        }

        public async void WriteToHub(byte[] data)
        {
            await _eh.SendAsync(new EventData(data));
        }
    }
}