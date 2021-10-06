using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.Concurrent;


namespace GpsData
{
    // GpsRecord is a helper model class for the gps record in the database.  We use this class to the bulk copy, so it must match the 
    // gps record in the sportvisiondata table.
    public class GpsRecord
    {
        // Start of gps record defintion for the SportVisionData table
        //

        // these fields are parsed from the incoming raw gps record string
        //
        public string VehicleNumber { get; set; } = null;
        public decimal? GPSTimeRaw { get; set; } = null;
        public DateTime? GPSTime { get; set; } = null;
        public decimal? Latitude { get; set; } = null;
        public decimal? Longitude { get; set; } = null;
        public decimal? Altitude { get; set; } = null;
        public decimal? Roll { get; set; } = null;
        public decimal? CourseHeading { get; set; } = null;
        public decimal? VelocityHeading { get; set; } = null;
        public int? LapNumber { get; set; } = null;
        public decimal? Speed { get; set; } = null;
        public int? Rpm { get; set; } = null;
        public int? Throttle { get; set; } = null;
        public int? Brake { get; set; } = null;
        public decimal? LateralAcceleration { get; set; } = null;
        public decimal? LongitudinalAcceleration { get; set; } = null;
        public decimal? LapFractional { get; set; } = null;
        public int? RealBrake { get; set; } = null;
        public int? GPSquality { get; set; } = null;

        // these are additional fields we track in the database
        //
        public int? ParseStatus { get; set; } = null;
        public string rawRecord { get; set;  } = null;
        public string ParseMessage { get; set;  } = null;
        public DateTime? ReceiveTime { get; set; } = null;

        //
        // End of gps record defintion for the SportVisionData table

        // table name in the database
        //
        public static string TableName = "dbo.SportVisionData";

        // gps record propoerties - matches fields in the table
        //
        public static PropertyInfo[] Properties = Type.GetType("GpsData.GpsRecord").GetProperties();
        public static int numProperties = Properties.Count(); 

        public GpsRecord()
        {
            VehicleNumber = "999";
        }
        public GpsRecord(GpsRecord gpsRecord)
        {
            VehicleNumber = gpsRecord.VehicleNumber;
            GPSTimeRaw = gpsRecord.GPSTimeRaw;
            GPSTime = gpsRecord.GPSTime;
            Latitude = gpsRecord.Latitude;
            Longitude = gpsRecord.Longitude;
            Altitude = gpsRecord.Altitude;
            Roll = gpsRecord.Roll;
            CourseHeading = gpsRecord.CourseHeading;
            VelocityHeading = gpsRecord.VelocityHeading;
            LapNumber = gpsRecord.LapNumber;
            Speed = gpsRecord.Speed;
            Rpm = gpsRecord.Rpm;
            Throttle = gpsRecord.Throttle;
            Brake = gpsRecord.Brake;
            LateralAcceleration = gpsRecord.LateralAcceleration;
            LongitudinalAcceleration = gpsRecord.LongitudinalAcceleration;
            LapFractional = gpsRecord.LapFractional;
            RealBrake = gpsRecord.RealBrake;
            GPSquality = gpsRecord.GPSquality;
            ReceiveTime = gpsRecord.ReceiveTime;
        }  
    }
}
