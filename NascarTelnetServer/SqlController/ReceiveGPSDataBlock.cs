using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GpsData
{
    public class ReceiveGPSData
    {
        public static IPropagatorBlock<GpsRecordBytes, GpsRecordString> RecieveGpsData(BufferBlock<GpsRecordString> bufferBlock, byte newRecordDelimiter=0x24)
        {
            var queue = new Queue<byte>();
            var source = bufferBlock;

            byte previoustByte = 0;
            byte CR = 0x0D;
            byte LF = 0x0A;

            GpsRecordBytes b = null;

            var target = new ActionBlock<GpsRecordBytes>(async x =>
            {
                try
                {
                    for (int i=0; i < x.Bytes.Count(); i++)
                    {
                        // check for our end of record delimiter CRLF
                        //
                        if (x.Bytes[i] == LF && previoustByte == CR)
                        {
                            // turn our bytes into a string and post it
                            //
                            GpsRecordString s = new GpsRecordString(Encoding.ASCII.GetString(queue.ToArray()), x.Id);
                            await source.SendAsync(s);

                            // clear our queue
                            //
                            queue.Clear();

                            // reset the previous byte
                            //
                            previoustByte = 0;
                        }
                        else
                        {
                            // check for previous CR and since it was not followed by a LF
                            // add it to the queue
                            //
                            if (previoustByte == CR)
                                queue.Enqueue(previoustByte);

                            // if we have a CR, do not add it yet
                            //
                            if (x.Bytes[i] != CR)
                                queue.Enqueue(x.Bytes[i]);

                            previoustByte = x.Bytes[i];
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
            });

            // send any remaining items
            //
            target.Completion.ContinueWith(async t =>
            {
                if (queue.Any())
                {
                    GpsRecordString s = new GpsRecordString(Encoding.Unicode.GetString(queue.ToArray()), b.Id);
                    await source.SendAsync(s);
                }
                Trace.WriteLine("ReceiveGPSData.Target complete.");
                source.Complete();
            });

            return DataflowBlock.Encapsulate(target, source);
        }
    }

}
