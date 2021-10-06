using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVTextFileReader
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = System.IO.File.ReadAllLines(args[0]);
            var lines = data.Where(x => x.StartsWith("$NMGT;")).Select(l => Encoding.UTF8.GetBytes(l));
            try
            {
                foreach (var line in lines)
                {
                    Console.WriteLine(line);   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }
    }
}
