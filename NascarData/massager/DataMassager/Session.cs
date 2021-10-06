using System;

namespace DataMassager
{
    public struct Session
    {
        public long SessionId;
        public string ProbableLocation;
        public decimal Lat;
        public decimal Lon;
        public decimal Alt;
        public DateTime StartUTC;
        public DateTime EndUTC;
        public decimal StartGpsTime;
        public decimal EndGpsTime;
    }
}