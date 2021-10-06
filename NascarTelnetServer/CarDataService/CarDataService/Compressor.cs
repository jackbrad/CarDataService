using System;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace CarDataService
{
    public class Compressor
    {
        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())  
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs); 
                }
                return mso.ToArray(); 
            }
        }

        public static string ZipToBase64(string str)
        { 
            return Convert.ToBase64String(Zip(str));
        }

        public static byte[] ZipToBase64Array(string str)
        {
            return Encoding.UTF8.GetBytes(ZipToBase64(str));
        }
    }
}