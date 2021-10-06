using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarDataService.Formatters;
using Microsoft.ServiceBus.Messaging;

namespace CarDataService.Subscribers
{
    public class EventHubSubscriber : SubscriberBase, IAsyncObserver<string>
    {
        private DateTime _lastTelemetrySent;
        private EventHubClient _eventHubClient;
        private bool _sessionStarted = false;

        public event EventHandler SessionStarted;

        public string EventHubName { get; set; }

        public string ConnectionString { get; set; }

        public EventHubSubscriber(IRowConverter converter) : base(converter) { }

        public void OnCompleted()
        {
            Log.Info("EventHubSubscriber completed");
        }

        public void OnError(Exception error)
        {
            Log.Error(error);
        }

        public void OnNext(string value)
        {
            Task.Run(() => { OnNextAsync(value); });
        }

        public void OnNextAsync(string value)
        {
            OnNextAsync(value, CancellationToken.None);
        }

        public async void OnNextAsync(string value, CancellationToken token)
        {
            //todo: handle cancellation token
            if (_eventHubClient == null)
            {
                try
                {
                    _eventHubClient = EventHubClient.CreateFromConnectionString(ConnectionString, EventHubName);
                    var properties = new Dictionary<string, string>() { { "EventHubPath", _eventHubClient.Path } };
                    Log.Event("EHClientCreated-Success", properties);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    return;
                }
            }

            try
            {
                var formattedData = _converter.ConvertToBytesSimple(value);
                await _eventHubClient.SendAsync(new EventData(formattedData));

                if (!_sessionStarted && SessionStarted != null)
                {
                    _sessionStarted = true;
                    SessionStarted(this, new NewSessionEventArgs(value));
                }

                if (_lastTelemetrySent.AddMinutes(1) < DateTime.UtcNow)
                {
                    SendSampleMessageToTelemetry(value);
                }
            }
            catch (Exception ex)
            {
                Log.Event("EventHub-Send-Error", new Dictionary<string, string>() { { "Data", value } });
                Log.Error(ex);
            }
        }

        private void SendSampleMessageToTelemetry(string message)
        {
            Log.Event("Sample Stream Data", new Dictionary<string, string>() { { "RawData", message }, { "LastSent", _lastTelemetrySent.ToString("O") } });
            _lastTelemetrySent = DateTime.UtcNow;
        }
    }
}