using System.Xml.Linq;

namespace Nascar.Sv.Dash
{
    public class Drivers
    {
        //http://developer.sportradar.us/files/nascar_v3_drivers_example.xml

        public Drivers()
        {
            var xd = XDocument.Load("http://developer.sportradar.us/files/nascar_v3_drivers_example.xml");
            //xd.
        }
    }

    public class Driver
    {
        public string Name { get; set; }
        public int CarNumber { get; set; }
        public string TeamName { get; set; }
        public string Manufacturer { get; set; }
        public string Sponsors { get; set; }
    }
}
