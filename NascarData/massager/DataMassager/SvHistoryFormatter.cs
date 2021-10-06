using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMassager
{
    class SvHistoryFormatter
    {
        static void Go()
        {
            for (int i = 49141; i < 49153; i++)
            {
                string raceIdentifier = i.ToString();

                Dictionary<string, string> vehicleDictionary = new Dictionary<string, string>();

                DateTime raceStartTime;
                decimal unixEpoch = 0.0M;

                int counter = 0;
                StringBuilder outputLine = new StringBuilder();


                string lookupFilename = $"C:\\Customers\\NASCAR Race Management\\SportVision Data\\2016 Race Data\\{raceIdentifier}\\metadata{raceIdentifier}.csv";
                using (System.IO.StreamReader file = new System.IO.StreamReader(lookupFilename))
                {
                    while (file.Peek() > 0)
                    {
                        var line = file.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            var segments = line.Split('\t');
                            vehicleDictionary.Add(segments[6], segments[5]);
                            if (decimal.TryParse(segments[3], out unixEpoch))
                            {
                                raceStartTime = FromUnixTime((double)unixEpoch);
                            }
                        }
                    }
                }
                using (System.IO.StreamReader file = new System.IO.StreamReader($"C:\\Customers\\NASCAR Race Management\\SportVision Data\\2016 Race Data\\{raceIdentifier}\\pos{raceIdentifier}.csv"))
                using (System.IO.StreamWriter outputfile = new System.IO.StreamWriter($"C:\\Customers\\NASCAR Race Management\\SportVision Data\\2016 Race Data\\{raceIdentifier}\\{raceIdentifier}formatted.txt"))
                {
                    while (file.Peek() > 0)
                    {
                        var line = file.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            var data = line.Split('\t');
                            string vehicleNumber = string.Empty;
                            if (vehicleDictionary.ContainsKey(data[0]))
                                vehicleNumber = vehicleDictionary[data[0]];
                            decimal timeOffset;
                            decimal.TryParse(data[2], out timeOffset);
                            decimal recordTime = (timeOffset / 1000.0M) + unixEpoch;
                            DateTime recordDateTime = FromUnixTime((double)recordTime);
                            string formattedTime = recordTime.ToString("0000000000.000000");
                            string formattedDateTime = recordDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            decimal lapFractional;
                            decimal.TryParse(data[3], out lapFractional);
                            int lapNumber = (int)Math.Truncate(lapFractional);
                            string formattedLapFractional = lapFractional.ToString("###0.000000");
                            decimal altitude;
                            decimal.TryParse(data[6], out altitude);
                            string formattedAltitude = altitude.ToString("#####.000");
                            decimal heading;
                            decimal.TryParse(data[10], out heading);
                            string formattedHeading = heading.ToString("#####.000");
                            decimal speed;
                            decimal.TryParse(data[16], out speed);
                            speed = speed / 100.0M;
                            string formattedSpeed = speed.ToString("###.00");
                            var correctedLine = $"{vehicleNumber},{formattedTime},{formattedDateTime},{data[21]},{data[20]},{formattedAltitude},0.00,{formattedHeading},{formattedHeading},{lapNumber},{formattedSpeed},{data[15]},{data[12]},{data[13]},0.0,0.0,{formattedLapFractional}";
                            outputfile.WriteLine(correctedLine);
                            if (counter % 1000 == 0)
                            {
                                Console.WriteLine($"Processing: {counter}");
                            }
                        }
                        counter++;
                    }
                }
                Console.WriteLine($"Total Record Count: {counter}");
            }
        }
        public static DateTime FromUnixTime(double unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(unixTime * 1000.0);
        }
    }
}