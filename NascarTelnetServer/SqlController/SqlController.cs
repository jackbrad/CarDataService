using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Microsoft.ApplicationInsights;


namespace GpsData
{
    // define our delegate for sending record complete events
    //
    public delegate void GpsRecordCompleteHandler(object sender, GpsRecordCompleteEventArgs e);

    public class GpsRecordCompleteEventArgs : EventArgs
    {
        public GpsRecordCompleteEventArgs(ParsedGpsRecord r =null)
        {
            TimeGroupId = r.Id;
            Status = r.Status;  
        }
        public int TimeGroupId { get; }
        public ParsedGpsRecord.ParseStatus Status { get;  }
    }

    public class SqlController
    {
        // define our record complete event
        //
        public event GpsRecordCompleteHandler RecordComplete;

        public int BatchSize { get; set; } = 40;
        public int AzureBatchSize { get; set; } = 1000;
        public string SqlConnectionString { get; set; } = null;
        public string AzureSqlConnectionString { get; set; } = null;
        public bool ReplicateToAzure { get; set; } = false;
        public string GpsTableName { get; set; } = null;
        public string AzureGpsTableName { get; set; } = null;
        public int BatchTriggerInterval { get; set; } = 25;
        public int maxDegreeOfParallelism { get; set; } = 1;
        public TelemetryClient TelemetryClient { get; set; } = null;
        public string GpsTimeFormat
        {
            get { return ParsedGpsRecord.GpsTimeFormat; }
            set { ParsedGpsRecord.GpsTimeFormat = value; }
        }
     
        // dataflow blocks
        //
        public BufferBlock<GpsRecordString> gpsRecordStringBuffer;
        private ActionBlock<ParsedGpsRecord[]> writeGpsRecords;
        private ActionBlock<ParsedGpsRecord[]> writeAzureGpsRecords;

        public int RecordsWritten;
        public int ErrorRecords;
        public int FieldParseErrors;

        public string azureTableName = GpsRecord.TableName;
        public string gpsTableName = GpsRecord.TableName;

        private string sqlConnectionString;
        private Timer batchTimer;


        public SqlController(string sqlConnectionString)
        {
            this.sqlConnectionString = sqlConnectionString;
        }

        protected virtual void OnComplete(ParsedGpsRecord rec)
        {
            RecordComplete?.Invoke(this, new GpsRecordCompleteEventArgs(rec));
        }

        // Build and configure the dataflow pipeline
        //
        public BufferBlock<GpsRecordString> Initialize(int batchSize=40)
        {
            BatchSize = batchSize;

            RecordsWritten = 0;
            ErrorRecords = 0;
            FieldParseErrors = 0;

            // Build our pipeline
            //
            if (ReplicateToAzure)
                BuildReplicationPipeline();
            else
                BuildLocalOnlyPipeline();

            return gpsRecordStringBuffer;
        }

        // Post is the entry point for producers to send us GpsRecordStrings
        //
        public void Post (GpsRecordString s)
        {
             gpsRecordStringBuffer.Post(s);
        }
        // SendAsync is the async entry point for producers to send us GpsRecordStrings
        //
        public async Task<bool> SendAsync(GpsRecordString s)
        {
            return await gpsRecordStringBuffer.SendAsync(s);
        }

        // called by our consumer when they are done with the controller
        //
        public Task Complete(bool bCallCompete)
        {
            if (bCallCompete)
                gpsRecordStringBuffer.Complete();

            Task t;
            if (ReplicateToAzure)
                t = Task.WhenAll(writeGpsRecords.Completion, writeAzureGpsRecords.Completion);
            else
                t = Task.WhenAll(writeGpsRecords.Completion);

            TelemetryClient?.Flush();
            return t;
        }

        // Build and configure the pipeline to local sql database and replicate to Azure Sql database
        //
        private void BuildReplicationPipeline()
        {
            // create our dataflow blocks
            //
            gpsRecordStringBuffer = new BufferBlock<GpsRecordString>();                                         // create a buffer block to receive incoming gps record strings
            var gpsRecordBatchBuffer = new BatchBlock<ParsedGpsRecord>(BatchSize);                              // create a batch block to hold gps records for local bulk inserts
            var azureRecordBatchBuffer = new BatchBlock<ParsedGpsRecord>(1000);                                 // create a batch block tp hold gps records for Sql Azure bulk inserts
            var broadcastBlock = new BroadcastBlock<ParsedGpsRecord>(x => x);                                   // create a broadcast block to send to local and azure batch buffers
            var parseGpsRecordBlock = CreateParseBlock();                                                       // create a tranform block to convert the string into a parsed gps record
            batchTimer = CreateTimer(gpsRecordBatchBuffer);                                                     // create a timer and transform block to fire and flush any remmaining records in our batch block
            writeGpsRecords = CreateWriteGpsRecordsBlock();                                                     // create an action block to bulk insert gps records into local sql server
            writeAzureGpsRecords = CreateWriteAzureGpsRecordsBlock();                                           // create an action block to bulk insert gps records into Azure sql server

            // link our blocks to create the pipeline
            //
            gpsRecordStringBuffer.LinkTo(parseGpsRecordBlock);                                                  // connect  incoming gps strings to parse block
            parseGpsRecordBlock.LinkTo(broadcastBlock);                                                         // coonect our parsed records to our broadcast block 
            broadcastBlock.LinkTo(gpsRecordBatchBuffer);                                                        // broadcast to the local gps batch buffer and the azure batch buffer
            broadcastBlock.LinkTo(azureRecordBatchBuffer);                                                      
            gpsRecordBatchBuffer.LinkTo(writeGpsRecords);                                                       // connect our gps record batch block to our local write block
            azureRecordBatchBuffer.LinkTo(writeAzureGpsRecords);                                                // connect our azure gps record batch block to our azure write block

            // setup our completion flow
            //
            // <todo> replace Trace writeline with logging
            //
            gpsRecordStringBuffer.Completion.ContinueWith(t =>                                                  
            {
                BatchTriggerInterval = Timeout.Infinite;
                DataFlowBlockComplete("gpsRecordStringBuffer", new List<IDataflowBlock> { parseGpsRecordBlock });
            });

            parseGpsRecordBlock.Completion.ContinueWith(t =>
            {
                DataFlowBlockComplete("parseGpsRecordBlock", new List<IDataflowBlock> { broadcastBlock });
            });

            broadcastBlock.Completion.ContinueWith(t =>
            {
                DataFlowBlockComplete("broadcastBlock", new List<IDataflowBlock> { gpsRecordBatchBuffer, azureRecordBatchBuffer });
            });

            gpsRecordBatchBuffer.Completion.ContinueWith(t =>
            {
                DataFlowBlockComplete("gpsRecordBatchBuffer", new List<IDataflowBlock> { writeGpsRecords });
            });

            azureRecordBatchBuffer.Completion.ContinueWith(t =>
            {
                DataFlowBlockComplete("azureRecordBatchBuffer", new List<IDataflowBlock> { writeAzureGpsRecords });
            });

            writeGpsRecords.Completion.ContinueWith(t =>
            {
                DataFlowBlockComplete("writeGpsRecords", null);
            });

            writeAzureGpsRecords?.Completion.ContinueWith(t =>
            {
                DataFlowBlockComplete("writeAzureGpsRecords", null);
            });
        }

        // Build and configure the pipeline to write to local sql server
        //
        private void BuildLocalOnlyPipeline()
        {
            // create our dataflow blocks
            //
            gpsRecordStringBuffer = new BufferBlock<GpsRecordString>();                                         // create a buffer block to receive incoming gps record strings
            var gpsRecordBatchBuffer = new BatchBlock<ParsedGpsRecord>(BatchSize);                              // create a batch block to hold gps records for local bulk inserts
            var parseGpsRecordBlock = CreateParseBlock();                                                       // create a tranform block to convert the string into a parsed gps record
            batchTimer = CreateTimer(gpsRecordBatchBuffer);                                                     // create a timer and transform block to fire and flush any remmaining records in our batch block
            writeGpsRecords = CreateWriteGpsRecordsBlock();                                                     // create an action block to bulk insert gps records into local sql server

            // link our blocks to create the pipeline
            //
            gpsRecordStringBuffer.LinkTo(parseGpsRecordBlock);                                                  // connect  incoming gps strings to parse block
            parseGpsRecordBlock.LinkTo(gpsRecordBatchBuffer);                                                   // coonect our parsed records to ocal gps batch buffer 
            gpsRecordBatchBuffer.LinkTo(writeGpsRecords);                                                       // connect our gps record batch block to our local write block

            // setup our completion flow
            //
            gpsRecordStringBuffer.Completion.ContinueWith(t =>
            {
                BatchTriggerInterval = Timeout.Infinite;
                DataFlowBlockComplete("gpsRecordStringBuffer", new List<IDataflowBlock> { parseGpsRecordBlock });
            });

            parseGpsRecordBlock.Completion.ContinueWith(t =>
            {
                DataFlowBlockComplete("parseGpsRecordBlock", new List<IDataflowBlock> { gpsRecordBatchBuffer });
            });

            gpsRecordBatchBuffer.Completion.ContinueWith(t =>
            {
                DataFlowBlockComplete("gpsRecordBatchBuffer", new List<IDataflowBlock> { writeGpsRecords });
            });

            writeGpsRecords.Completion.ContinueWith(t =>
            {
                DataFlowBlockComplete("writeGpsRecords", null);
            });
        }

        // create a tranform block to convert the string into a parsed gps record
        //
        private TransformBlock<GpsRecordString, ParsedGpsRecord> CreateParseBlock()
        {

            return new TransformBlock<GpsRecordString, ParsedGpsRecord>(s =>                            
            {
                ParsedGpsRecord g = ParseGpsRecordString(s);

                // set a timer to fire and flush any remaining records from the batch
                // 
                batchTimer.Change(BatchTriggerInterval, Timeout.Infinite);
                return g;
            });
        }

        // create a timer to fire and flush any remmaining records in our batch block
        //
        private Timer CreateTimer(BatchBlock<ParsedGpsRecord> gspBatchBlock)
        {

           return new Timer(_ =>
            {
                gspBatchBlock.TriggerBatch();
            });
        }

        // create an action block to bulk insert gps records into local sql server
        //
        private ActionBlock<ParsedGpsRecord[]> CreateWriteGpsRecordsBlock()
        {

            return new ActionBlock<ParsedGpsRecord[]>(async x =>
            {
                await LocalBulkInsertGpsRecords(x);
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            });
        }

        // create an action block to bulk insert gps records into azure sql server
        //
        private ActionBlock<ParsedGpsRecord[]> CreateWriteAzureGpsRecordsBlock()
        {

            return new ActionBlock<ParsedGpsRecord[]>(async x =>
            {
                await AzureBulkInsertGpsRecords(x);
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            });
        }

        // parse the gps string to create a gsp record
        //
        private  ParsedGpsRecord ParseGpsRecordString(GpsRecordString s)
        {
            ParsedGpsRecord x = new ParsedGpsRecord(s);
            try 
            {
                x.ParseAndCreateGpsRecord();
            }
            catch (Exception ex)
            {
                x.Status = ParsedGpsRecord.ParseStatus.Exception;
                x.ParseErrorMessage = ex.ToString();
                TelemetryClient?.TrackException(ex, new Dictionary<string, string>() {{"ParseException", s.Record }});
            }

            return x;
        }

        private void DataFlowBlockComplete(string dataflowBlock, List<IDataflowBlock> dataflowBlocks)
        {
            string s = $"{dataflowBlock}.Complete";
            Trace.WriteLine(s);
            TelemetryClient?.TrackEvent(s, new Dictionary<string, string>() { { "DataFlowBlock", dataflowBlock } });
            dataflowBlocks?.ForEach(x =>
            {
                x.Complete();
            });
        }

        // bulk insert records into local sql server
        //
        private async Task<int> LocalBulkInsertGpsRecords(ParsedGpsRecord[] gpsBatchRecords)
        {
            try
            {
                if (gpsBatchRecords.Count() != BatchSize)
                    Trace.WriteLine($"Received {gpsBatchRecords.Count()} records from batch block.");

                // insert this batch
                //
                await SqlBulkInsertGpsRecords(gpsBatchRecords, sqlConnectionString, gpsTableName);

                // update our counters and notify any subscribers
                //
                Interlocked.Add(ref RecordsWritten, gpsBatchRecords.Count());
                Interlocked.Add(ref ErrorRecords, gpsBatchRecords.Count(x => x.ParseError));
                Interlocked.Add(ref FieldParseErrors, gpsBatchRecords.Count(x => x.ParseWarnings));

                foreach (ParsedGpsRecord g in gpsBatchRecords)
                {
                    OnComplete(g);
                }


                // testing only
                //
                //throw new Exception("This is a BulkInsertGpsRecords exception test");
            }
            catch (Exception ex)
            {
                TelemetryClient?.TrackException(ex, new Dictionary<string, string>() { { "SqlException", "LocalBulkInsertGpsRecords" } });
                Trace.WriteLine($"BulkInsertGpsRecords: {ex.ToString()}");
            }
            return 0;
        }

        // bulk insert records into azure sql server
        //
        private async Task<int> AzureBulkInsertGpsRecords(IEnumerable<ParsedGpsRecord> gpsBatchRecords)
        {
            try
            {
                // insert this batch
                //
                await SqlBulkInsertGpsRecords(gpsBatchRecords, AzureSqlConnectionString, azureTableName);
            }
            catch (Exception ex)
            {
                TelemetryClient?.TrackException(ex, new Dictionary<string, string>() { { "SqlException", "AzureBulkInsertGpsRecords" } });
                Trace.WriteLine($"AzureSqlBulkInsertGpsRecords exeption: {ex.ToString()}");
            }

            return 0;
        }

        // perform the actual bulk insert
        //
        private async Task<int> SqlBulkInsertGpsRecords(IEnumerable<ParsedGpsRecord> gpsBatchRecords, string sqlConnectionString, string table)
        {
            // define our gps table
            //
            DataTable gpsTable = new DataTable();
            foreach (var p in GpsRecord.Properties)
            {
                gpsTable.Columns.Add(p.Name, Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType);
            }

            foreach (ParsedGpsRecord g in gpsBatchRecords)
            {
                // add the data to our table
                //
                gpsTable.Rows.Add(  g?.gpsRecord?.VehicleNumber,
                                    g?.gpsRecord?.GPSTimeRaw,
                                    g?.gpsRecord?.GPSTime,
                                    g?.gpsRecord?.Latitude,
                                    g?.gpsRecord?.Longitude,
                                    g?.gpsRecord?.Altitude,
                                    g?.gpsRecord?.Roll,
                                    g?.gpsRecord?.CourseHeading,
                                    g?.gpsRecord?.VelocityHeading,
                                    g?.gpsRecord?.LapNumber,
                                    g?.gpsRecord?.Speed,
                                    g?.gpsRecord?.Rpm,
                                    g?.gpsRecord?.Throttle,
                                    g?.gpsRecord?.Brake,
                                    g?.gpsRecord?.LateralAcceleration,
                                    g?.gpsRecord?.LongitudinalAcceleration,
                                    g?.gpsRecord?.LapFractional,
                                    g?.gpsRecord?.RealBrake,
                                    g?.gpsRecord?.GPSquality,
                                    g.Status,
                                    g.ParseError ? g.RawRecordString: null,
                                    g.ParseErrorMessage,
                                    g.ReceiveTime);
            }

            // insert this batch of records using bulk copy
            //
            using (var sqlBulk = new SqlBulkCopy(sqlConnectionString))
            {
                sqlBulk.DestinationTableName = table;
                for (int j = 0; j < GpsRecord.Properties.Count(); j++)
                {
                    sqlBulk.ColumnMappings.Add(j, j + 1);
                }

                await sqlBulk.WriteToServerAsync(gpsTable);
            }

            return 0;
        }
        #region old

        //public BufferBlock<GpsRecordString> Initialize(int batchSize, int maxDegreeParallelism = 1, int errorsBatchSize = 10, int batchTimerInterval = 5000)
        //{
        //    this.batchSize = batchSize;

        //    RecordsWritten = 0;
        //    ErrorRecords = 0;
        //    FieldParseErrors = 0;

        //    // create a buffer block to receive incoming gps record strings
        //    //
        //    gpsRecordStringBuffer = new BufferBlock<GpsRecordString>();

        //    // create a batch blocks for gps and error records to batch records for bulk inserts
        //    //
        //    var gpsRecordBatchBuffer = new BatchBlock<ParsedGpsRecord>(batchSize);
        //    // <todo> 
        //    //errorsBatchBuffer = new BatchBlock<ErrorRecord>(errorsBatchSize);

        //    // create a tranform block to convert the string into a parsed gps record
        //    //
        //    parseGpsRecordBlock = new TransformBlock<GpsRecordString, ParsedGpsRecord>(s =>
        //    {
        //        ParsedGpsRecord g = ParseGpsRecordString(s);

        //        // set a timer to fire and flush any remaining records from the batch
        //        // 
        //        batchTimer.Change(batchTimerInterval, Timeout.Infinite);
        //        return g;
        //    });

        //    // create a timer and transform block to fire and
        //    // flush any remmaining records in our batch block
        //    //
        //    batchTimer = new Timer(_ =>
        //    {
        //        gpsRecordBatchBuffer.TriggerBatch();
        //    });

        //    var timerBlock = new TransformBlock<GpsRecordString, GpsRecordString>(x =>
        //    {
        //        batchTimer.Change(batchTimerInterval, Timeout.Infinite);
        //        return x;
        //    });

        //    // <todo> 
        //    // create a transformblock to convert parsedgpsrecords into errorrecords
        //    //
        //    var createErrorRecord = new TransformBlock<ParsedGpsRecord, ErrorRecord>(g =>
        //    {
        //        return CreateErrorRecord(g);
        //    });

        //    // <todo>
        //    //  create an action block bulk insert error records into sql server
        //    //
        //    writeErrorRecords = new ActionBlock<ErrorRecord[]>(async x =>
        //    {
        //        await BulkInsertErrorRecords(x);
        //    });

        //    // create an action block to bulk insert gps records into sql server
        //    //
        //    writeGpsRecords = new ActionBlock<ParsedGpsRecord[]>(async x =>
        //    {
        //        await BulkInsertGpsRecords(x);
        //    }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeParallelism });

        //    // link our blocks to create our dataflow
        //    //
        //    gpsRecordStringBuffer.LinkTo(parseGpsRecordBlock);                                                    // link the incoming gps to the block to parse strings into gps records
        //    timerBlock.LinkTo(parseGpsRecordBlock);                                                      // link the timer block to flush any batch records every x msecs
        //    parseGpsRecordBlock.LinkTo(gpsRecordBatchBuffer);                                                     // link our parsed records to our batch block        
        //    parseGpsRecordBlock.LinkTo(createErrorRecord, x => x.ParseError);                            // link any records that we could not parse to a block to transform them into error records
        //    createErrorRecord.LinkTo(errorsBatchBuffer);                                                 // link our error records to the error batch block
        //    gpsRecordBatchBuffer.LinkTo(writeGpsRecords);                                                         // link our gps record batch block to our bulk write block
        //    errorsBatchBuffer.LinkTo(writeErrorRecords);                                                 // link our error record batch block to our bulk write block

        //    // setup our completions
        //    //

        //    gpsRecordStringBuffer.Completion.ContinueWith(t =>
        //    {
        //        Console.WriteLine("gpsRecordStringBuffer.Complete");
        //        batchTimerInterval = Timeout.Infinite;
        //        parseGpsRecordBlock.Complete();
        //    });

        //    timerBlock.Completion.ContinueWith(t =>
        //    {
        //        Console.WriteLine("timerBlock.Complete");
        //        parseGpsRecordBlock.Complete();
        //    });

        //    parseGpsRecordBlock.Completion.ContinueWith(t =>
        //    {
        //        Console.WriteLine("parseGpsRecordBlock.Complete");
        //        gpsRecordBatchBuffer.Complete();
        //        createErrorRecord.Complete();
        //    });


        //    createErrorRecord.Completion.ContinueWith(t =>
        //    {
        //        Console.WriteLine("createErrorRecord.Complete");
        //        errorsBatchBuffer.Complete();
        //    });

        //    gpsRecordBatchBuffer.Completion.ContinueWith(t =>
        //    {
        //        Console.WriteLine("gpsRecordBatchBuffer.Complete");
        //        writeGpsRecords.Complete();
        //    });


        //    errorsBatchBuffer.Completion.ContinueWith(t =>
        //    {
        //        Console.WriteLine("errorsBatchBuffer.Complete");
        //        writeErrorRecords.Complete();
        //    });

        //    writeGpsRecords.Completion.ContinueWith(t =>
        //    {
        //        Console.WriteLine("writeGpsRecords.Complete");
        //    });


        //    writeErrorRecords.Completion.ContinueWith(t =>
        //    {
        //        Console.WriteLine("writeErrorRecords.Complete");
        //    });

        //    return gpsRecordStringBuffer;
        //}


        // <todo> 
        //private async Task<int> BulkInsertErrorRecords(ErrorRecord[] errorRecords)
        //{
        //    try
        //    {
        //        await SqlBulkInsertErrors(errorRecords);
        //        Interlocked.Add(ref ErrorRecords, errorRecords.Count());
        //    }
        //    catch (Exception ex)
        //    {
        //        ((IDataflowBlock)writeErrorRecords).Fault(ex);
        //    }
        //    return 0;
        //}

        //private ErrorRecord CreateErrorRecord(ParsedGpsRecord g)
        //{
        //    try
        //    {
        //        // testing only
        //        //
        //        //throw new Exception("This is a CreateErrorRecord exception test");
        //        return new ErrorRecord(g);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ErrorRecord("Error converting ParsedGpsRecord into ErrorRecord", ex.ToString());
        //    }
        //}

        //private async Task<int> SqlBulkInsertErrors(IEnumerable<ErrorRecord> errorRecords)
        //{
        //    // define our error table
        //    //
        //    DataTable errorsTable = new DataTable();
        //    foreach (var p in ErrorRecord.Properties)
        //    {
        //        errorsTable.Columns.Add(p.Name, Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType);
        //    }

        //    foreach (ErrorRecord e in errorRecords)
        //    {
        //        // add the data to our table, if IsError is set this parse error, otherwise it is field parse error
        //        //                
        //        errorsTable.Rows.Add(e.Record, e.Error, e.ReceiveTime);
        //    }

        //    // insert this batch of records using bulk copy
        //    //
        //    using (var sqlBulk = new SqlBulkCopy(sqlConnectionString))
        //    {
        //        sqlBulk.DestinationTableName = ErrorRecord.TableName;
        //        for (int j = 0; j < ErrorRecord.numProperties; j++)
        //        {
        //            sqlBulk.ColumnMappings.Add(j, j + 1);
        //        }

        //        await sqlBulk.WriteToServerAsync(errorsTable);
        //    }
        //    return 0;
        //}
        #endregion
    }
}

