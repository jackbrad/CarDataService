using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
/*
#r "System.Net"
#r "Newtonsoft.Json"
#r "System.Xml.Linq"
#r "System.Data"
#r "System.Configuration"
*/
using System.Net;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Configuration;


namespace PitRoadSpeedingNotification
{
    public class TraceWriter
    {
        public void Info(string info)
        {
            Console.WriteLine(info);
        }
    }
    class Program
    {
        static TraceWriter log;

        static void Main(string[] args)
        {
            string msg = "1;146938678242400;$NMGT;88;47601.207062;1456078403.600000;20160121-18:13:23.599;29.188077;-81.071615;-18.950000;-0.045016;1764684.258334;633315.761411;0.000000;-159.960938;0;0.068359;0;0;0;0.000000;0.000000";

            var tw = new TraceWriter();
            log = tw;
            Run(msg, tw);
        }

        public static void SendText(string Message)
        {
            try
            {
                string AccountSid = ConfigurationManager.AppSettings["TwilioAccountSid"];
                string AuthToken = ConfigurationManager.AppSettings["TwilioAuthToken"];
                var twilio = new TwilioRestClient(AccountSid, AuthToken);
                var message = twilio.SendMessage(ConfigurationManager.AppSettings["TextMsgFromNum"], ConfigurationManager.AppSettings["TextMsgToNum"], Message);
                log.Info(message.Sid);
            }
            catch (Exception e)
            {
                log.Info("Text send failed: " + e.Message + " " + e.StackTrace + " " + e.InnerException.Message);
            }
            
           
           
        }


        public static void Run(string myEventHubMessage, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myEventHubMessage}");
            var msg = myEventHubMessage;
            //split incoming message
            //example input string
            //inpits,sessionid,$NMGT,Car,Vitc,GPSTime,GPSString,Lat,Lon,Alt,Roll,North,East,CourseHead,VelocityHead,Lap,Speed,RPM,Throttle,Brake,LatAccel,LongAccel

            //used to set time
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            //get the values from the string
            string[] parts = msg.Split(';');

            string inpit, sessionid, car, lat, lon, alt, speed, rpm;
            inpit = parts[0];
            sessionid = parts[1];
            car = parts[3];
            lat = parts[7];
            lon = parts[8];
            alt = parts[9];
            speed = parts[16];
            rpm = parts[17];


            //make a session object -- because I want to 
            dynamic session = new
            {
                InPits = inpit == "1" ? true : false,
                SessionID = sessionid,
                Car = car,
                ProbableLocation = GetStreamLocation(lat, lon),
                Lat = lat,
                Lon = lon,
                Alt = alt,
                Speed = speed,
                RPM = rpm,
                StartUTC = epoch.AddSeconds(Convert.ToDouble(parts[5])),
                StartGpsTime = Convert.ToDecimal(parts[5]),
                OriginalMsg = myEventHubMessage
            };

            //Insert in DB
            InsertInSVDB(session, log);
            //build message
            var violationMsg = $"A pit road speed violation occur at {session.StartGpsTime}. Location: {session.ProbableLocation} Car: {session.Car} Speed:{session.Speed}";

            //SendText
            SendText(violationMsg);
        }

        private static void InsertInSVDB(dynamic session, TraceWriter log)
        {
            string queryString = @"INSERT INTO [dbo].[PitRoadSpeeding]
           ([SessionId]
           ,[Car]
           ,[ProbableLocation]
           ,[Lat]
           ,[Lon]
           ,[Alt]
           ,[Speed]
           ,[RPM]
           ,[Stamp]
           ,[GpsTime]
           ,[OriginalMsg])
     VALUES
           (";
            queryString += $@"{session.SessionID}
           ,{session.Car}
           ,'{session.ProbableLocation}'
           ,{session.Lat}
           ,{session.Lon}
           ,{session.Alt}
           ,{session.Speed}
           ,{session.RPM}
           ,'{session.StartUTC}'
           ,{session.StartGpsTime}
           ,'{session.OriginalMsg}')";
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SVSqlDB"].ConnectionString))
            {
                log.Info("Writing to DB");
                try
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    connection.Open();
                    command.ExecuteNonQuery();
                    log.Info("db insert succeeded.");
                }
                catch (Exception e)
                {
                    log.Info("db insert failed. " + e.Message);
                }

            }
        }

        private static bool VerifiedInDB(string name)
        {
            string queryString = "SELECT * from Tracks";
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SVSqlDB"].ConnectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        if (name.Equals(reader.GetString(reader.GetOrdinal("Track"))))
                        {
                            log.Info("Track found in db.");
                            return true;
                        }
                    }
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                }
            }
            log.Info("Track not found in db.");
            return false;
        }

        private static string GetStreamLocation(string lat, string lon)
        {
            //ask bing what the most site are around this that are possibly race tracks
            var estimatedsite = "Unknown";
            XElement root = XElement.Parse(NearByTracksXML(lat, lon));
            var entries = root.Descendants(XName.Get("DisplayName", "http://schemas.microsoft.com/ado/2007/08/dataservices"));

            //get the names only
            IEnumerable<string> names = from e in entries
                                        select e.Value;
            //check the names in the DB and see if we have a known match
            foreach (var name in names)
            {
                if (VerifiedInDB(name))
                {
                    estimatedsite = name;
                    break;
                }
            }
            try
            {
                //we did not find a track name
                if (estimatedsite == "Unknown")
                {
                    //just log the location
                    dynamic gc = GeocodeData(lat, lon);
                    estimatedsite = gc.resourceSets[0].resources[0].name;
                }
            }
            catch (Exception)
            {
                log.Info("Site find failed");
                estimatedsite = "Unknown";
            }
            log.Info("Site find returned: " + estimatedsite);
            return estimatedsite;
        }

        public static string NearByTracksXML(string lat, string lon)
        {
            WebClient client = new WebClient();
            var uri = $"http://spatial.virtualearth.net/REST/v1/data/f22876ec257b474b82fe2ffcb8393150/NavteqNA/NavteqPOIs?spatialFilter=nearby({lat},{lon},15)&$filter=EntityTypeID%20eq%20'7940'&$select=EntityID,DisplayName,Latitude,Longitude,__Distance&$top=3&key=Ajlvx1MRWtrOm8sYYJFUhGqyi4-PY6XCUPpmDtsdzreRbocbV3XSQRi4A-FfVXsF";
            var s = client.DownloadString(uri);
            return s;
        }

        public static dynamic GeocodeData(string lat, string lon)
        {
            var response = GetGeocodeJson(lat, lon);
            dynamic geocode = JObject.Parse(response);
            return geocode;
        }
        public static string GetGeocodeJson(string lat, string lon)
        {
            WebClient client = new WebClient();
            var s = client.DownloadString($"http://dev.virtualearth.net/REST/v1/Locations/{lat},{lon}?o=json&key=Ajlvx1MRWtrOm8sYYJFUhGqyi4-PY6XCUPpmDtsdzreRbocbV3XSQRi4A-FfVXsF");
            return s;
        }

    }
}
