using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GPSWebServices.Models
{
    public class GPSData
    {

        public string VehicleNumber { get; set; }
        public decimal GPSTimeRaw { get; set; }
        [Key]
        public DateTime GPSTime { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal Altitude { get; set; }
        public decimal Roll { get; set; }
        public decimal CourseHeading { get; set; }
        public decimal VelocityHeading { get; set; }
        public int LapNumber { get; set; }
        public decimal Speed { get; set; }
        public int RPM { get; set; }
        public int Throttle { get; set; }
        public int Brake { get; set; }
        public decimal LateralAcceleration { get; set; }
        public decimal LongitudinalAcceleration { get; set; }
        public decimal LapFractional { get; set; }
    }
}
