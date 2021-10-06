using System.Collections.Generic;
using System.Linq;


namespace CarDataService
{
    public class MessageBuffer
    {
        private readonly List<string> _buffer = new List<string>();

        public List<string> AddData(string data)
        {
            if (_buffer.Any()) { data = $"{_buffer.First()}{data}"; }
            // <new> 
            // updated to parse record based on CRLF delimmiter
            //
            string[] delimiter = { "\r\n" };
            var lines = data.Split(delimiter, System.StringSplitOptions.None).Where(x => x.Length > 0).ToList();
            //var lines = data.Split("$".ToCharArray()).Where(x => x.Trim().Length > 0).Select(y => "$" + y).ToList();
            _buffer.Clear();
            var carryoverLine = lines.Last();
            _buffer.Add(carryoverLine);
            lines.Remove(carryoverLine);
            return lines;
        }
    }
}