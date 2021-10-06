using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarDataService
{

    // define our delegate for sending record parsed events
    //
    public delegate void RecordParsedHandler(object sender, RecordParsedEventArgs e);

    public class RecordParsedEventArgs : EventArgs
    {
        public string Record;
        public RecordParsedEventArgs(string rec)
        {
            Record = rec;
        }
    }
    class GpsParser
    {

        private readonly byte CR = 0x0D;
        private readonly byte LF = 0x0A;
        private readonly int Max = 1000;

        private Queue<byte> queue = new Queue<byte>();
        private byte previousByte = 0;

        // define our record complete event
        //
        public event RecordParsedHandler RecordParsed;

        public void ParseBytes(byte[] bytes, int numBytes)
        {
            // walk the bytes
            //
            for (int i = 0; i < numBytes; i++)
            {
                // check for our end of record delimiter CRLF
                //
                if (bytes[i] == LF && previousByte == CR)
                {
                    // we hit our record delimiter, get the string, and fire a parsed record event
                    //
                    string s = Encoding.ASCII.GetString(queue.ToArray());
                    RecordParsed?.Invoke(this, new RecordParsedEventArgs(s));

                    // clear our queue
                    //
                    queue.Clear();
                    previousByte = 0;
                }
                else
                {
                    // safety check - if the queue has reached Max, just skip bytes 
                    // until we hit a record delimiter
                    //
                    if (queue.Count() < Max)
                    {
                        // check for previous CR and since it was not followed by a LF
                        // add it to the queue
                        //
                        if (previousByte == CR)
                            queue.Enqueue(previousByte);

                        // if we have a CR, do not add it yet
                        //
                        if (bytes[i] != CR)
                            queue.Enqueue(bytes[i]);
                    }

                    // save the previous byte
                    //
                    previousByte = bytes[i];                   
                }
            }

        }
    }
}
