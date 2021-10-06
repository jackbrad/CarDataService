using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using CarDataService.Formatters;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data;
using System.Configuration;
using GpsData;

namespace CarDataService.Subscribers
{
    public class SqlSubscriber : SubscriberBase, IAsyncObserver<string>
    {
        private readonly string _connectionString;

        // <new> SqlController 
        //
        SqlController sqlController = null;

        // <todo> remove once we validate the sql controller
        //
        // values for stopwatch logging
        //Stopwatch parseStopWatch = new Stopwatch();
        //Stopwatch insertStopWatch = new Stopwatch();
        //long parseTime;
        //long insertTime;
        string filename = $@"C:\Program Files (x86)\NASCAR-Microsoft\StopwatchLogs\{DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")}.txt";


        public SqlSubscriber(IRowConverter converter) : base(converter) { }

        public SqlSubscriber(string connectionString)
        {
            _connectionString = connectionString;
            Log.Event("SQL Subscriber Created", new Dictionary<string, string>() { { "ConnectionString", connectionString } });

            // <new>  create and initialilze our sql controller 
            // <todo>  verify getting the configuration settings work
            //
            sqlController = new SqlController(connectionString);
            sqlController.BatchSize = Convert.ToInt32(ConfigurationManager.AppSettings["SqlSubscriber.GpsBatchSize"]);
            sqlController.AzureBatchSize = Convert.ToInt32(ConfigurationManager.AppSettings["SqlSubscriber.AzureBatchSize"]); 
            sqlController.SqlConnectionString  = ConfigurationManager.AppSettings["SqlSubscriber.ConnectionString"];
            sqlController.AzureSqlConnectionString = ConfigurationManager.AppSettings["SqlSubscriber.AzureConnectionString"];
            sqlController.ReplicateToAzure = Convert.ToBoolean(ConfigurationManager.AppSettings["SqlSubscriber.ReplicateToAzure"]); ;
            sqlController.BatchTriggerInterval  = Convert.ToInt32(ConfigurationManager.AppSettings["SqlSubscriber.BatchTriggerInterval"]);
            sqlController.maxDegreeOfParallelism  = Convert.ToInt32(ConfigurationManager.AppSettings["SqlSubscriber.maxDegreeOfParallelism"]);
            sqlController.GpsTimeFormat = ConfigurationManager.AppSettings["SqlSubscriber.GpsTimeFormat"];
            sqlController.TelemetryClient = Tc;
            sqlController.Initialize();
        }

        public async void OnCompleted()
        {
            // <new> call complete to gracefully shutdown the sql controller
            //
            await sqlController?.Complete(true);
            Log.Event("SQL Subscriber Destroyed", new Dictionary<string, string>() { { "ConnectionString", _connectionString } });
        }

        public void OnError(Exception error)
        {
            Log.Error(error);
        }

        public async void OnNext(string value)
        {
            //Task.Run(() => { OnNextAsync(value); });

            // <new>  pass the value to the sql controller
            //
            await sqlController.SendAsync(new GpsRecordString(value));
        }

        public void OnNextAsync(string value)
        {
            OnNextAsync(value, CancellationToken.None);
        }

        public async void OnNextAsync(string value, CancellationToken token)
        {
            await sqlController.SendAsync(new GpsRecordString(value));
        }

        #region OldCode

        // <todo> remove once we verify the sql controller works correctly
        //
        //public async void OnNextAsync(string value, CancellationToken token)
        //{
        //try
        //{
        //    parseStopWatch.Start();
        //    var data = value.Split(';');

        //    string VehicleNumber;
        //    decimal? GPSTimeRaw;
        //    string GPSTime;
        //    decimal? latitude;
        //    decimal? longitude;
        //    decimal? altitude;
        //    decimal? roll;
        //    decimal? courseHeading;
        //    decimal? velocityHeading;
        //    int? lapNumber;
        //    decimal? speed;
        //    int? rpm;
        //    int? throttle;
        //    int? brake;
        //    decimal? lateralAcceleration;
        //    decimal? longitudinalAcceleration;
        //    decimal? lapFractional;


        //    //If the data has 22 items, parse it and insert it into the SportVisionData table
        //    if (data.Length == 22)
        //    {

        //        VehicleNumber = data[2];

        //        decimal GPSTimeRawResult;
        //        if (decimal.TryParse(data[4], out GPSTimeRawResult)) { GPSTimeRaw = GPSTimeRawResult; }
        //        else { GPSTimeRaw = null; }

        //        GPSTime = data[5].Replace('-', ' ');

        //        decimal latitudeResult;
        //        if (decimal.TryParse(data[6], out latitudeResult)) { latitude = latitudeResult; }
        //        else { latitude = null; }

        //        decimal longitudeResult;
        //        if (decimal.TryParse(data[7], out longitudeResult)) { longitude = longitudeResult; }
        //        else { longitude = null; }

        //        decimal altitudeResult;
        //        if (decimal.TryParse(data[8], out altitudeResult)) { altitude = altitudeResult; }
        //        else { altitude = null; }

        //        decimal rollResult;
        //        if (decimal.TryParse(data[9], out rollResult)) { roll = rollResult; }
        //        else { roll = 0; }

        //        decimal courseHeadingResult;
        //        if (decimal.TryParse(data[12], out courseHeadingResult)) { courseHeading = courseHeadingResult; }
        //        else { courseHeading = null; }

        //        decimal velocityHeadingResult;
        //        if (decimal.TryParse(data[13], out velocityHeadingResult)) { velocityHeading = velocityHeadingResult; }
        //        else { velocityHeading = null; }

        //        int lapNumberResult;
        //        if (int.TryParse(data[14], out lapNumberResult)) { lapNumber = lapNumberResult; }
        //        else { lapNumber = null; }

        //        decimal speedResult;
        //        if (decimal.TryParse(data[15], out speedResult)) { speed = speedResult; }
        //        else { speed = null; }

        //        int rpmResult;
        //        if (int.TryParse(data[16], out rpmResult)) { rpm = rpmResult; }
        //        else { rpm = null; }

        //        int throttleResult;
        //        if (int.TryParse(data[17], out throttleResult)) { throttle = throttleResult; }
        //        else { throttle = null; }

        //        int brakeResult;
        //        if (int.TryParse(data[18], out brakeResult)) { brake = brakeResult; }
        //        else { brake = null; }

        //        decimal lateralAccelerationResult;
        //        if (decimal.TryParse(data[19], out lateralAccelerationResult)) { lateralAcceleration = lateralAccelerationResult; }
        //        else { lateralAcceleration = null; }

        //        decimal longitutinalAccelerationResult;
        //        if (decimal.TryParse(data[20], out longitutinalAccelerationResult)) { longitudinalAcceleration = longitutinalAccelerationResult; }
        //        else { longitudinalAcceleration = null; }

        //        decimal lapFractionalResult;
        //        if (decimal.TryParse(data[21], out lapFractionalResult)) { lapFractional = lapFractionalResult; }
        //        else { lapFractional = null; }

        //        parseStopWatch.Stop();
        //        parseTime = parseStopWatch.ElapsedMilliseconds;
        //        parseStopWatch.Restart();

        //        try
        //        {
        //            insertStopWatch.Start();
        //            using (var xon = new SqlConnection(_connectionString))
        //            {
        //                xon.Open();
        //                using (var cmd = new SqlCommand("dbo.AddGPSRecord", xon))
        //                {
        //                    cmd.CommandType = CommandType.StoredProcedure;
        //                    cmd.Parameters.Add(new SqlParameter("@VehicleNumber", VehicleNumber));
        //                    cmd.Parameters.Add(new SqlParameter("@GPSTimeRaw", GPSTimeRaw));
        //                    cmd.Parameters.Add(new SqlParameter("@GPSTime", GPSTime));
        //                    cmd.Parameters.Add(new SqlParameter("@Latitude", latitude));
        //                    cmd.Parameters.Add(new SqlParameter("@Longitude", longitude));
        //                    cmd.Parameters.Add(new SqlParameter("@Altitude", altitude));
        //                    cmd.Parameters.Add(new SqlParameter("@Roll", roll));
        //                    cmd.Parameters.Add(new SqlParameter("@CourseHeading", courseHeading));
        //                    cmd.Parameters.Add(new SqlParameter("@VelocityHeading", velocityHeading));
        //                    cmd.Parameters.Add(new SqlParameter("@LapNumber", lapNumber));
        //                    cmd.Parameters.Add(new SqlParameter("@Speed", speed));
        //                    cmd.Parameters.Add(new SqlParameter("@RPM", rpm));
        //                    cmd.Parameters.Add(new SqlParameter("@Throttle", throttle));
        //                    cmd.Parameters.Add(new SqlParameter("@Brake", brake));
        //                    cmd.Parameters.Add(new SqlParameter("@LateralAcceleration", lateralAcceleration));
        //                    cmd.Parameters.Add(new SqlParameter("@LongitudinalAcceleration", longitudinalAcceleration));
        //                    cmd.Parameters.Add(new SqlParameter("@LapFractional", lapFractional));

        //                    await cmd.ExecuteReaderAsync();
        //                }
        //                xon.Close();

        //                insertStopWatch.Stop();
        //                insertTime = insertStopWatch.ElapsedMilliseconds;
        //                insertStopWatch.Restart();
        //                WritetoLog();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error(ex);
        //        }
        //    }

        //    //If the data does not have 22 values, insert it into the error table
        //    else
        //    {
        //        try
        //        {
        //            using (var xon = new SqlConnection(_connectionString))
        //            {
        //                xon.Open();
        //                using (var cmd = new SqlCommand("INSERT INTO [nascar.gps].[dbo].[ErrorData] (Data) VALUES (@Data)", xon))
        //                {
        //                    cmd.Parameters.Add(new SqlParameter("@Data", value));

        //                    await cmd.ExecuteNonQueryAsync();
        //                }
        //                xon.Close();

        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error(ex);
        //        }
        //    }
        //}

        //catch(Exception ex)
        //{
        //    Log.Error(ex);
        //}
        //}

        //private void WritetoLog()
        //{
        //    try
        //    {
        //        string line = parseTime + ";" + insertTime;
        //        string[] lines = { line };
        //        System.IO.File.AppendAllLines(filename, lines);
        //    }
        //    catch { }
        //}
        #endregion
    }
}