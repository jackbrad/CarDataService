using System;

namespace PowerBiRealTime
{
    public class CarData
    {
        public int CarNumber;
        public string Vitc;
        public double GPSRaw;
        public string GPSString;
        public double Latitude;
        public double Longitude;
        public double Altitude;
        public double Roll;
        public double Northing;
        public double Easting;
        public double CourseHeading;
        public double VelocityHeading;
        public int LapNumber;
        public double Speed;
        public double RPM;
        public double Throttle;
        public double Brake;
        public double LateralAcceleration;
        public double LongitudalAcceleration;

        public DateTime ReportedTime => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(GPSRaw - 17.0);

        public DateTime ArrivalTime => DateTime.UtcNow;



        //public string CarNumber;
        //public string Vitc;
        //public string GPSRaw;
        //public string GPSString;
        //public string Latitude;
        //public string Longitude;
        //public string Altitude;
        //public string Roll;
        //public string Northing;
        //public string Easting;
        //public string CourseHeading;
        //public string VelocityHeading;
        //public string LapNumber;
        //public string Speed;
        //public string RPM;
        //public string Throttle;
        //public string Brake;
        //public string LateralAcceleration;
        //public string LongitudalAcceleration;
    }
}