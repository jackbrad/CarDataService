using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DataMassager
{
    //    USE[SportsVizRaceData]
    //GO

    //INSERT INTO[dbo].[StreamSession]
    //           ([SessionID],[ProbableLocation],[Lat],[Lon],[Alt],[StartUTC],[EndUTC],[StartGpsTime],[EndGpsTime])
    //VALUES
    //           (<SessionID, uniqueidentifier,>
    //           ,<ProbableLocation, nvarchar(200),>
    //           ,<Lat, decimal(18,6),>
    //           ,<Lon, decimal(18,6),>
    //           ,<Alt, decimal(18,6),>
    //           ,<StartUTC, datetime,>
    //           ,<EndUTC, datetime,>
    //           ,<StartGpsTime, decimal(18,6),>
    //           ,<EndGpsTime, decimal(18,6),>)
    //GO
    public class SessionDataManager
    {
        private DateTime _epoch = DateTime.Parse("1970-1-1");

        public void GetSessionDateDiffs()
        {
            var db = new SqlConnection(ConfigurationManager.ConnectionStrings["nascar-prod"].ConnectionString);
            var cmd = new SqlCommand(@";select sessionid, count(*) as rc from cardata where sessionid is not null group by sessionid order by sessionid asc")
            {
                Connection = db,
                CommandTimeout = 3000
            };
            var dt = new DataTable();
            using (var sda = new SqlDataAdapter(cmd))
            {
                sda.Fill(dt);
            }

            var rows = dt.Rows.OfType<DataRow>().ToList();

            var sessions = rows.Where(y => y["sessionid"] != null).Select(x =>
                {
                    var session = (long)x["sessionid"];
                    var divisor = session > 10000000000000 ? 100 : 1;
                    return new
                    {
                        SessionId = x["sessionid"],
                        StartTime = _epoch.AddMilliseconds(session / divisor),
                        Count = x["rc"]
                    };
                }
            ).ToList();

            //.ForEach(z => Console.WriteLine($"Session {z.SessionId}: started {z.StartTime}, rows: {z.Count}"););
            sessions.ForEach(
                x =>
                    Console.WriteLine(
                        $"Session {x.SessionId}: started {x.StartTime.ToLocalTime()} ({x.StartTime.ToShortTimeString()} UTC), rows: {x.Count}"));

            //get first/last data point for each
            foreach (var s in sessions)
            {

            }

        }

        public void AddMissingSessions()
        {
            var sql = @"select * From (select distinct(sessionid) from cardata) as exp1 where sessionid not in (select sessionid from streamsession)";
            var rows = ExecuteSql(sql);
            var g = new Geocoder();
            foreach (var r in rows)
            {
                var sessionId = (long)r["sessionid"];
                Console.WriteLine($"Looking for {sessionId}...");
                var result = ExecuteSql($@"select top 1 * from CarData where sessionid = {sessionId} order by GpsTime asc");
                var endTime = ExecuteSql($@"select top 1 * from CarData where sessionid = {sessionId} order by GpsTime desc");
                if (!result.Any() || !endTime.Any())
                {
                    Console.WriteLine($"Couldn't find any rows for session ID {sessionId}");
                    return;
                }

                var row = result.First();
                var end = endTime.First();

                var s = new Session()
                {
                    SessionId = sessionId,
                    Alt = (decimal)row["Alt"],
                    Lat = (decimal)row["Lat"],
                    Lon = (decimal)row["Lon"],
                    StartGpsTime = (decimal)row["GpsTime"],
                    StartUTC = (DateTime)row["Stamp"],
                    EndGpsTime = (decimal)end["GpsTime"],
                    EndUTC = (DateTime)end["Stamp"]
                };
                var poi = g.GetPoi(s.Lat, s.Lon);
                Console.WriteLine($"Found {poi}!");
                s.ProbableLocation = poi;
                AddSessionToDatabase(s);
            }
        }

        public List<DataRow> ExecuteSql(string sql, bool nonQuery = false)
        {
            var db = new SqlConnection(ConfigurationManager.ConnectionStrings["nascar-prod"].ConnectionString);
            var cmd = new SqlCommand(sql) { Connection = db, CommandTimeout = 3000 };

            if (nonQuery)
            {
                db.Open();
                cmd.ExecuteNonQuery();
                db.Close();
                return null;
            }

            var dt = new DataTable();
            using (var sda = new SqlDataAdapter(cmd))
            {
                sda.Fill(dt);
            }

            return dt.Rows.OfType<DataRow>().ToList();
        }

        public void Go()
        {
            var db =
                new SqlConnection(
                    @"Server=tcp:racecardata.database.windows.net,1433;Data Source=racecardata.database.windows.net;Initial Catalog=SportsVizRaceData;Persist Security Info=False;User ID=nascar-admin;Password=wert##44;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=300;");
            var cmd = new SqlCommand(
                    @";with cte as	
                       (
                            select gpstime, stamp, datediff(minute, stamp, lead(stamp, 1, 0) over (order by stamp)) leaddiff,
                            datediff(minute, stamp, lag(stamp, 1, 0) over (order by stamp)) lagdiff,
		                    row_number() over (order by stamp) rn from cardata
                            --where stamp > '2016-06-28'
	                   )
                       select gpstime, leaddiff, lagdiff, stamp from cte
                       where leaddiff > 5 or lagdiff < -5
                       --and rn <> 1
                       order by stamp")
            { Connection = db, CommandTimeout = 3000 };
            var dt = new DataTable();
            using (var sda = new SqlDataAdapter(cmd))
            {
                sda.Fill(dt);
            }

            var things = new List<Session>();
            var rows = dt.Rows.OfType<DataRow>().ToList();

            for (var i = 0; i < rows.Count; i = i + 2)
            {
                var pair = rows.Skip(i).Take(2).ToList();
                var t = new Session()
                {
                    SessionId = (long)((decimal)pair.First()["gpstime"] * 100),
                    StartUTC = (DateTime)pair.First()["stamp"],
                    EndUTC = (DateTime)pair.Last()["stamp"],
                    StartGpsTime = (decimal)pair.First()["gpstime"],
                    EndGpsTime = (decimal)pair.Last()["gpstime"],
                };
                things.Add(t);
            }

            foreach (var t in things)
            {
                var span = t.EndUTC - t.StartUTC;
                var pdt = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                var start = t.StartUTC.ToLocalTime();
                var end = t.EndUTC.ToLocalTime();
                //if (t.StartUTC > new DateTime(2016, 06, 20))
                //{
                //    start = TimeZoneInfo.ConvertTimeFromUtc(t.StartUTC, pdt);
                //    end = TimeZoneInfo.ConvertTimeFromUtc(t.EndUTC, pdt);
                //}
                Console.Write(
                    $"adding session {t.SessionId.ToString().Substring(0, 4)}<snip/>: lasted {span.Hours.ToString("00")}:{span.Minutes.ToString("00")}:{span.Seconds.ToString("00")} ({start} - {end})...");
                AddSessionToDatabase(t);
                Console.WriteLine("done.");
            }
        }

        private void AddSessionToDatabase(Session s)
        {
            //if (CheckExists(s))
            //{
            //    Console.WriteLine($"Found! {s.StartGpsTime}-{s.EndGpsTime}");
            //    return;
            //}

            ExecuteSql($@"insert into streamsession ([SessionID],[ProbableLocation],[Alt],[Lat],[Lon],[StartUTC],[EndUTC],[StartGpsTime],[EndGpsTime]) 
                          values ('{s.SessionId}', '{s.ProbableLocation}', {s.Alt}, {s.Lat}, {s.Lon}, '{s.StartUTC}', '{s.EndUTC}', '{s.StartGpsTime}', '{s.EndGpsTime}')", true);
            Console.WriteLine($"Added {s.SessionId}: {s.ProbableLocation} to db");
        }

        private bool CheckExists(Session s)
        {
            var db =
                new SqlConnection(
                    @"Server=tcp:racecardata.database.windows.net,1433;Data Source=racecardata.database.windows.net;Initial Catalog=SportsVizRaceData;Persist Security Info=False;User ID=nascar-admin;Password=wert##44;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=300;");
            var cmd =
                new SqlCommand(
                        $@"select count(*) from streamsession where StartGpsTime = {s.StartGpsTime} and EndGpsTime = {s
                            .EndGpsTime}")
                { Connection = db, CommandTimeout = 300 };
            db.Open();
            var thing = (int)cmd.ExecuteScalar();
            Console.WriteLine($"Found {thing} existing timespans...");
            return thing > 0;
        }

        public void AggregateLocationsBySession()
        {
            var db =
                new SqlConnection(
                    @"Server=tcp:racecardata.database.windows.net,1433;Data Source=racecardata.database.windows.net;Initial Catalog=SportsVizRaceData;Persist Security Info=False;User ID=nascar-admin;Password=wert##44;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=300;");
            var cmd = new SqlCommand(@"select * From streamsession where ProbableLocation is null")
            {
                Connection = db,
                CommandTimeout = 300
            };
            ;
            var dt = new DataTable();
            using (var sda = new SqlDataAdapter(cmd))
            {
                sda.Fill(dt);
            }

            foreach (var row in dt.Rows.OfType<DataRow>())
            {
                var startGps = row["StartGpsTime"];
                var endGps = row["EndGpsTime"];
                var table = new DataTable();
                using (var sda = new SqlDataAdapter(new SqlCommand($@"select count(*) as c, * from (
                                                select CONCAT(ROUND(Lat, 2), ',', round(lon, 2)) as loc from CarData
                                                where gpstime >= {startGps} and gpstime <= {endGps}
                                                ) as exp1
                                                group by loc
                                                order by c desc", db)))
                {
                    sda.Fill(table);
                    var dr = table.Rows.OfType<DataRow>().First();
                    var loc = dr["loc"];
                    var split = loc.ToString().Split(',');
                    var spot = new Geocoder().GetPoi(split[0], split[1]);
                    //Console.WriteLine($"Found {spot} for {row["SessionId"]}!");

                    var updateCmd =
                        new SqlCommand(
                            $@"update StreamSession set Lat = '{split[0]}', Lon = '{split[1]}', ProbableLocation = '{spot}' where SessionId = '{row
                                ["SessionId"]}'", db);
                    db.Open();
                    Console.WriteLine(
                        $@"update StreamSession set Lat = '{split[0]}', Lon = '{split[1]}', ProbableLocation = '{spot}' where SessionId = '{row
                            ["SessionId"]}'");
                    updateCmd.ExecuteNonQuery();
                    db.Close();
                    //update db
                    //else
                    //{
                    //    var numbers = table.Rows.OfType<DataRow>().Select(x => x["c"].ToString());
                    //    Console.WriteLine($"Found {table.Rows.Count} rows, {string.Join(",", numbers)}");
                    //}
                }
            }
        }

    }
}