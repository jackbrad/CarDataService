using System.ServiceProcess;
using CarDataService.Formatters;
using Microsoft.ApplicationInsights;

namespace CarDataService
{
    public abstract class SubscriberBase : TelemetryBase
    {
        protected IRowConverter _converter;

        protected SubscriberBase()
        {

        }

        protected SubscriberBase(IRowConverter converter)
        {
            _converter = converter;
        }
    }

    public abstract class TelemetryBase
    {
        public TelemetryClient Tc { get; }
        public Log Log { get; private set; }

        internal TelemetryBase()
        {
            if (Tc == null)
            {
                Tc = new TelemetryClient();
            }
            Log = new Log(Tc);
        }
    }

    public abstract class TelemetryServiceBase : ServiceBase
    {
        public TelemetryClient Tc { get; }
        public Log Log { get; private set; }

        //internal TelemetryServiceBase(TelemetryClient tc)
        //{
        //    Tc = tc;
        //    Log = new Log(Tc);
        //}

        internal TelemetryServiceBase()
        {
            if (Tc == null)
            {
                Tc = new TelemetryClient();
            }
            Log = new Log(Tc);
        }
    }
}