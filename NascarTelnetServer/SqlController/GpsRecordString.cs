using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GpsData
{
    public class GpsRecordString
    {
        public string Record { get; set; }
        public DateTime ReceiveTime { get; set; }
        public int Id { get; private set; }

        // There are currently 5 incoming fields (sessionid, NGMT, Vitc, North, and East) that we do not save to the table.
        // We added 4 fields (parsestatus, rawRecord, parsemessage and receivetime) to the record saved in the database. Thus the incoming fields(properties)
        // is equal to the GpsRecord properties + (5-4)
        // 
        static public int incomingFields { get; } = GpsRecord.numProperties + 1;

        public GpsRecordString(string s, int id=0)
        {
            Record = s;
            Id = id;
            ReceiveTime = DateTime.Now;
        }
    }
}
