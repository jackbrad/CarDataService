using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using GPSWebServices.Models;
using System;

namespace GPSWebServices.Controllers
{
    [Route("api/[controller]")]
    public class GPSController : Controller
    {
        private GPSDBContext _context;

        public GPSController(GPSDBContext context)
        {
            _context = context;
        }
        public IEnumerable<GPSData> GetAll()
        {
            return _context.SportVisionData;
        }

        [Route("getMaxSpeed/vehicleNumber={vehicleNumber}&startTime={startTime}&endTime={endTime}")]
        [HttpGet]
        public IActionResult GetMaxSpeed(string vehicleNumber, string startTime, string endTime)
        {
            DateTime start = new DateTime();
            DateTime end = new DateTime();
            try
            {
                //Currently expects date formatted YYYY-MM-DD-HH-MM-SS-mmm
                //datetime = DateTime.Parse("2016-04-03 16:59:27.200");
                start = DateTime.Parse(startTime.Replace("-", " "));
                end = DateTime.Parse(endTime.Replace("-", " "));
            }
            catch { }

            var items = _context.SportVisionData.Where(i => ((i.VehicleNumber == vehicleNumber) && (i.GPSTime >= start) && (i.GPSTime <= end)));
            var maxSpeed = items.OrderByDescending(i => i.Speed).First().Speed;
            if (items == null)
            {
                return NotFound();
            }
            return new ObjectResult(maxSpeed);
        }

        [Route("getFreeze={time}")]
        [HttpGet]
        public IActionResult GetFreeze(string time)
        {
            DateTime datetime = new DateTime();
            try {
                //Currently expects date formatted YYYY-MM-DD-HH-MM-SS-mmm
                //datetime = DateTime.Parse("2016-04-03 16:59:27.200");
                datetime = DateTime.Parse(time.Replace("-", " "));
            }
            catch { }

            var closestTime = _context.SportVisionData.Where(i => i.GPSTime <= datetime).OrderByDescending(i => i.GPSTime).First().GPSTime;
            
            var items = _context.SportVisionData.Where(i => i.GPSTime == closestTime);
            if (items == null)
            {
                return NotFound();
            }
            return new ObjectResult(items);
        }

        [Route("vehicleNumber={id}")]
        [HttpGet]
        public IActionResult GetVehicleNumberPosition(string id)
        {

            var item = _context.SportVisionData.FirstOrDefault(i => i.VehicleNumber == id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

    }
}
