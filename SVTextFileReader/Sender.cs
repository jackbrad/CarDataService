using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace SVTextFileReader
{
    class Sender
    {
        private EventHubClient _eventHubClient;

        public async void OnNextAsync(string value, CancellationToken token)
        {
            //todo: handle cancellation token
            if (_eventHubClient == null)
            {
                _eventHubClient = EventHubClient.CreateFromConnectionString("EventHubConnectionString", "EventHubName");
                var properties = new Dictionary<string, string>() { { "EventHubPath", _eventHubClient.Path } };
                
            }

            try
            {
                var formattedData = _converter.ConvertToBytesSimple(value);
                await _eventHubClient.SendAsync(new EventData(formattedData));

            }
            catch (Exception ex)
            {
                throw new ApplicationException("EventHub-Send-Error",ex);
                
            }
        }
    }
}
