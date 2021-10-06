using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GpsData
{
    public class GpsRecordBytes
    {
        public byte[] Bytes { get; set; } 
        public int Id { get;  }
        public GpsRecordBytes(byte[] b, int id)
        {
            Bytes = b;
            Id = id;
        }
    }
}
