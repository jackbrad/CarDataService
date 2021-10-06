using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GpsData
{
    public class ParsedGpsRecord
    {
        public static string GpsTimeFormat = "yyyyMMdd-H:mm:ss.fff";
        public int Id { get { return gpsRecordString.Id;  } }
        public string RawRecordString { get { return gpsRecordString?.Record; } }
        public DateTime? ReceiveTime { get { return gpsRecordString?.ReceiveTime; } }
        public ParseStatus Status { get; set; }
        public string ParseErrorMessage { get; set; } = null;
        public bool ParseSuccessful { get { return Status == ParseStatus.Success || Status == ParseStatus.Warning; } }
        public bool ParseWarnings { get { return Status == ParseStatus.Warning; } }
        public bool ParseError { get { return (Status == ParseStatus.Error || Status == ParseStatus.Exception); } }
        public enum ParseStatus { Success=0, Warning = 1, Error = 2, Exception=3 }
        public GpsRecord gpsRecord { get; set; } = null;


        private GpsRecordString gpsRecordString { get; set; } = null;
        private List<string> fieldParseErrors = null;

        public ParsedGpsRecord(GpsRecordString s = null)
        {
            gpsRecordString = s;
            Status = ParseStatus.Success;
        }

        public ParsedGpsRecord (ParsedGpsRecord g)
        {
            this.gpsRecordString = g.gpsRecordString;
            this.ParseErrorMessage = g.ParseErrorMessage;
            this.Status = g.Status;
        }

        // parse incoming gps string into a gps record
        //
        public ParseStatus ParseAndCreateGpsRecord()
        {
            try
            {
                // the fields are delimited by ';', so split it into our property fields
                //
                var props = gpsRecordString.Record.Split(';');

                // verify we have the correct no of properties
                //
                if (props.Count() != GpsRecordString.incomingFields)
                {
                    Status = ParseStatus.Error;
                    ParseErrorMessage = $"Parse error: {props.Count()} fields is invalid.  Valid record has {GpsRecord.numProperties} fields.";
                    return Status;
                }

                // set our parse status
                //
                Status = ParseStatus.Success;

                // parse and load fields into a gps record
                //
                gpsRecord = new GpsRecord();

                // skip session id (0) and NGMT fields (1)
                //
                gpsRecord.VehicleNumber = ParseVehicleNumber(props[2], "VehicleNumber");
                // skip Vitc field (3)
                //
                gpsRecord.GPSTimeRaw = TryParseDecimal(props[4], "GPSTimeRaw");
                gpsRecord.GPSTime = TryParseGpsDateTime(props[5], "GPSTime");
                gpsRecord.Latitude = TryParseDecimal(props[6], "Latitude");
                gpsRecord.Longitude = TryParseDecimal(props[7], "Longitude");
                gpsRecord.Altitude = TryParseDecimal(props[8], "Altitude");
                gpsRecord.Roll = TryParseDecimal(props[9], "Roll");
                // skip North (10) and east (11) fields
                //
                gpsRecord.CourseHeading = TryParseDecimal(props[12], "CourseHeading");
                gpsRecord.VelocityHeading = TryParseDecimal(props[13], "VelocityHeading");
                gpsRecord.LapNumber = TryParseInt(props[14], "LapNumber");
                gpsRecord.Speed = TryParseDecimal(props[15], "Speed");
                gpsRecord.Rpm = TryParseInt(props[16], "Rpm");
                gpsRecord.Throttle = TryParseInt(props[17], "Throttle");
                gpsRecord.Brake = TryParseInt(props[18], "Brake");
                gpsRecord.LateralAcceleration = TryParseDecimal(props[19], "LateralAcceleration");
                gpsRecord.LongitudinalAcceleration = TryParseDecimal(props[20], "LongitudinalAcceleration");
                gpsRecord.LapFractional = TryParseDecimal(props[21], "LapFractional");
                gpsRecord.RealBrake = TryParseInt(props[22], "RealBrake");
                gpsRecord.GPSquality = TryParseInt(props[23], "GPSquality");

                // if there werer any field parse errors, set our error message
                //
                if (fieldParseErrors != null)
                    ParseErrorMessage = "Unable to parse: " + String.Join(",", fieldParseErrors);
            }
            catch (Exception ex)
            {
                Status = ParseStatus.Exception ;
                ParseErrorMessage = $"Parse exception: {ex.Message}";
                throw ex;
            }

            return Status;
        }

        // parse vehicle number
        //
        private string ParseVehicleNumber(string s, string n)
        {
            // vehicle number should not be null, empty, or all whitespace
            //
            if (String.IsNullOrWhiteSpace(s))
            {
                AddFieldParseError(n,"NULL");
                return null;
            }

            return s;
        }

        // parse int
        //
        private int? TryParseInt(string v, string n)
        {
            if (String.IsNullOrWhiteSpace(v))
                return null;

            int i;
            if (int.TryParse(v, out i))
                return i;

            AddFieldParseError(n,v);
            return null;
        }

        // parse decimal
        //
        private Decimal? TryParseDecimal(string v, string n)
        {
            if (String.IsNullOrWhiteSpace(v))
                return null;

            Decimal d;
            if (Decimal.TryParse(v, out d))
                return d;

            AddFieldParseError(n, v);
            return null;
        }

        // <new> added to specifically parse the GPS time string in "yyyyMMdd-HH:mm:ss.fff" format
        //
        private DateTime? TryParseGpsDateTime(string v, string n)
        {
            if (String.IsNullOrWhiteSpace(v))
                return null;

            DateTime dt;
            if (DateTime.TryParseExact(v, GpsTimeFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
                return dt;

            AddFieldParseError(n, v);
            return null;
        }   
        
        // parse date time
        //
        private DateTime? TryParseDateTime(string v, string n)
        {
            if (String.IsNullOrWhiteSpace(v))
                return null;

            DateTime dt;
            if (DateTime.TryParse(v, out dt))
                return dt;

            AddFieldParseError(n, v);
            return null;
        }

        // collect any field parsing errors
        //
        private void AddFieldParseError(string name, string value)
        {
            Status = ParseStatus.Warning;

            if (fieldParseErrors == null)
                fieldParseErrors = new List<string>(20);

            fieldParseErrors.Add($"{name}:<{value}>");
        }
    }
}
