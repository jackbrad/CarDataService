using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

public class Ok
{
    public static void Run(string ehIn, out string ehOut)//, TraceWriter log)
    {
        var message = Decompress(ehIn);
        var fields = new List<string>()
        {
            "CarNumber",
            "Vitc",
            "GPSRaw",
            "GPSString",
            "Latitude",
            "Longitude",
            "Altitude",
            "Roll",
            "Northing",
            "Easting",
            "CourseHeading",
            "VelocityHeading",
            "LapNumber",
            "Speed",
            "RPM",
            "Throttle",
            "Brake",
            "LateralAcceleration",
            "LongitudalAcceleration"
        };
        var csv = Csv(message, fields, new List<string>());
        //log.Verbose($"Transformed to {csv}");
        ehOut = csv;
    }

    private static string Decompress(string wiredata)
    {
        string outString;
        var output = Convert.FromBase64String(wiredata);
        using (var targetStream = new MemoryStream(output))
        {
            using (var gzout = new GZipStream(targetStream, CompressionMode.Decompress))
            {
                using (var reader = new StreamReader(gzout, System.Text.Encoding.UTF8))
                {
                    outString = reader.ReadToEnd();
                }
            }
        }
        return outString;
    }

    private static string Csv(string data, List<string> fields, List<string> ignoreList)
    {
        var badData = data.IndexOf("#") > -1;
        var props = fields.Where(x => !ignoreList.Contains(x)).ToList();
        data = data.Replace("$NMGT;", string.Empty).Replace("{m:", string.Empty).Replace("}", string.Empty);
        var headerRow = string.Join(",", props);
        var csv = data.Replace(';', ',').Split(',');
        if (badData)
        {
            var working = csv.ToList();
            var bad = working.Where(x => x.Contains("#")).ToList();
            foreach (var index in bad.Select(b => working.IndexOf(b)))
            {
                csv[index] = "0";
            }
        }
        csv[3] = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(double.Parse(csv[2]) - 17.0).ToString("O");
        var csvString = string.Join(",", csv);
        if (csv.Length == props.Count) return $"{headerRow}\r\n{csvString}\r\n";
        //new fields
        if (csv.Length > props.Count)
        {
            var newFieldCount = csv.Length - props.Count;
            for (var i = 0; i < newFieldCount; i++)
            {
                headerRow = $"{headerRow},Unknown{i}";
            }
            return $"{headerRow}\r\n{csvString}\r\n";
        }
        var delta = props.Count - csv.Length;
        for (var i = 0; i < delta; i++)
        {
            csvString = csvString + ",";
        }
        return $"{headerRow}\r\n{csvString}\r\n";
    }

}


//// Type: FileReformatter.Program
//// Assembly: FileReformatter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
//// Assembly location: D:\cloudtfs\Squirrel Please\NascarTelnetServer\FileReformatter\bin\x64\Debug\FileReformatter.exe

//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;

//namespace FileReformatter
//{
//    internal class Program2
//    {
//        private static DateTime GpsTimeToUtc(double gpsTime)
//        {
//            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(gpsTime - 17.0);
//        }

//        private static void Main2(string[] args)
//        {
//            Console.CursorVisible = false;
//            string str1 = "c:\\temp\\nascar\\Atlanta-CUP-Data-5202.log";
//            Console.WriteLine(string.Format("Using file {0}, loading...", (object)str1));
//            FileInfo fileInfo = new FileInfo(str1);
//            string[] stuff = File.ReadAllLines(str1);
//            Console.WriteLine(string.Format("Found {0} lines, {1} bytes, {2} bytes per line", (object)stuff.Length, (object)fileInfo.Length, (object)(fileInfo.Length / (long)stuff.Length)));
//            string str2 = Enumerable.First<string>((IEnumerable<string>)stuff);
//            char[] chArray1 = new char[1];
//            int index1 = 0;
//            int num1 = 59;
//            chArray1[index1] = (char)num1;
//            DateTime dateTime1 = Program2.GpsTimeToUtc(double.Parse(str2.Split(chArray1)[3]));
//            string str3 = Enumerable.Last<string>((IEnumerable<string>)stuff);
//            char[] chArray2 = new char[1];
//            int index2 = 0;
//            int num2 = 59;
//            chArray2[index2] = (char)num2;
//            DateTime dateTime2 = Program2.GpsTimeToUtc(double.Parse(str3.Split(chArray2)[3]));
//            Console.WriteLine(string.Format("Race start: {0} UTC ({1})", (object)dateTime1, (object)dateTime1.ToLocalTime()));
//            Console.WriteLine(string.Format("Race end: {0} UTC ({1})", (object)dateTime2, (object)dateTime2.ToLocalTime()));
//            TimeSpan timeSpan = dateTime2 - dateTime1;
//            string format = "Race duration: {0}, {1} events received. {2} events per second, {3}kBps";
//            object[] objArray = new object[4];
//            int index3 = 0;
//            // ISSUE: variable of a boxed type
//            __Boxed<TimeSpan> local1 = (ValueType)timeSpan;
//            objArray[index3] = (object)local1;
//            int index4 = 1;
//            // ISSUE: variable of a boxed type
//            __Boxed<int> local2 = (ValueType)stuff.Length;
//            objArray[index4] = (object)local2;
//            int index5 = 2;
//            // ISSUE: variable of a boxed type
//            __Boxed<double> local3 = (ValueType)((double)stuff.Length / timeSpan.TotalSeconds);
//            objArray[index5] = (object)local3;
//            int index6 = 3;
//            // ISSUE: variable of a boxed type
//            __Boxed<double> local4 = (ValueType)Math.Round((double)fileInfo.Length / timeSpan.TotalSeconds / 1000.0, 4);
//            objArray[index6] = (object)local4;
//            Console.WriteLine(string.Format(format, objArray));
//            Program.ToJsonObjectStrings(stuff, fileInfo.Length, timeSpan.TotalSeconds, fileInfo.Name.Substring(0, fileInfo.Name.IndexOf(".", StringComparison.Ordinal)));
//            Console.WriteLine("All finished.");
//            Console.ReadLine();
//        }

//        private static void ToJsonObjectStrings(string[] stuff, long originalSize, double raceTime, string originalFileName)
//        {
//            // ISSUE: object of a compiler-generated type is created
//            // ISSUE: variable of a compiler-generated type
//            Program.\u003C\u003Ec__DisplayClass2_0 cDisplayClass20 = new Program.\u003C\u003Ec__DisplayClass2_0();
//            // ISSUE: reference to a compiler-generated field
//            cDisplayClass20.stuff = stuff;
//            // ISSUE: reference to a compiler-generated field
//            cDisplayClass20.originalFileName = originalFileName;
//            List<Type> list1 = new List<Type>()
//      {
//        typeof (FullHeader),
//        typeof (SingleCharacterHeader)
//      };
//            // ISSUE: reference to a compiler-generated field
//            cDisplayClass20.ops = new List<Func<string, object, List<string>, string>>()
//      {
//        new Func<string, object, List<string>, string>(Program.Csv)
//      };
//            // ISSUE: reference to a compiler-generated method
//            // ISSUE: reference to a compiler-generated method
//            List<Result> list2 = Enumerable.ToList<Result>(Enumerable.SelectMany<Type, Func<string, object, List<string>, string>, Result>((IEnumerable<Type>)list1, new Func<Type, IEnumerable<Func<string, object, List<string>, string>>>(cDisplayClass20.\u003CToJsonObjectStrings\u003Eb__0), new Func<Type, Func<string, object, List<string>, string>, Result>(cDisplayClass20.\u003CToJsonObjectStrings\u003Eb__1)));
//            Console.WriteLine("=== result ===");
//            // ISSUE: reference to a compiler-generated field
//            // ISSUE: reference to a compiler-generated field
//            // ISSUE: reference to a compiler-generated field
//            // ISSUE: reference to a compiler-generated method
//            foreach (Result result in (IEnumerable<Result>)Enumerable.OrderBy<Result, long>((IEnumerable<Result>)list2, Program.\u003C\u003Ec.\u003C\u003E9__2_2 ?? (Program.\u003C\u003Ec.\u003C\u003E9__2_2 = new Func<Result, long>(Program.\u003C\u003Ec.\u003C\u003E9.\u003CToJsonObjectStrings\u003Eb__2_2))))
//      {
//                string format = "{0} with {1}: {2} bytes - {3}% vs original, {4}kBps";
//                object[] objArray = new object[5];
//                int index1 = 0;
//                string function = result.Function;
//                objArray[index1] = (object)function;
//                int index2 = 1;
//                string type = result.Type;
//                objArray[index2] = (object)type;
//                int index3 = 2;
//                // ISSUE: variable of a boxed type
//                __Boxed<long> local1 = (ValueType)result.Size;
//                objArray[index3] = (object)local1;
//                int index4 = 3;
//                // ISSUE: variable of a boxed type
//                __Boxed<double> local2 = (ValueType)Math.Round((double)result.Size / (double)originalSize * 100.0, 2);
//                objArray[index4] = (object)local2;
//                int index5 = 4;
//                // ISSUE: variable of a boxed type
//                __Boxed<double> local3 = (ValueType)Math.Round((double)result.Size / raceTime / 1000.0, 4);
//                objArray[index5] = (object)local3;
//                Console.WriteLine(string.Format(format, objArray));
//            }
//        }

//        private static Result Go(Type t, string[] stuff, Func<string, object, List<string>, string> thingToDo, string originalFile)
//        {
//            // ISSUE: object of a compiler-generated type is created
//            // ISSUE: variable of a compiler-generated type
//            Program.\u003C\u003Ec__DisplayClass3_0 cDisplayClass30_1 = new Program.\u003C\u003Ec__DisplayClass3_0();
//            // ISSUE: reference to a compiler-generated field
//            cDisplayClass30_1.thingToDo = thingToDo;
//            string format1 = "C:\\temp\\nascar\\{0}-{1}-{2}.{3}";
//            object[] objArray1 = new object[4];
//            int index1 = 0;
//            string str1 = originalFile;
//            objArray1[index1] = (object)str1;
//            int index2 = 1;
//            // ISSUE: reference to a compiler-generated field
//            string name1 = cDisplayClass30_1.thingToDo.Method.Name;
//            objArray1[index2] = (object)name1;
//            int index3 = 2;
//            string name2 = t.Name;
//            objArray1[index3] = (object)name2;
//            int index4 = 3;
//            // ISSUE: reference to a compiler-generated field
//            string name3 = cDisplayClass30_1.thingToDo.Method.Name;
//            objArray1[index4] = (object)name3;
//            string str2 = string.Format(format1, objArray1);
//            // ISSUE: variable of a compiler-generated type
//            Program.\u003C\u003Ec__DisplayClass3_0 cDisplayClass30_2 = cDisplayClass30_1;
//            List<string> list = new List<string>();
//            string str3 = "GPSString";
//            list.Add(str3);
//            string str4 = "d";
//            list.Add(str4);
//            // ISSUE: reference to a compiler-generated field
//            cDisplayClass30_2.ignoreList = list;
//            // ISSUE: reference to a compiler-generated field
//            // ISSUE: reference to a compiler-generated field
//            Console.WriteLine(string.Format("Writing data to {0} using {1} type, ignoring {2}...", (object)cDisplayClass30_1.thingToDo.Method.Name, (object)t.Name, (object)string.Join(",", (IEnumerable<string>)cDisplayClass30_1.ignoreList)));
//            int num = 0;
//            // ISSUE: reference to a compiler-generated field
//            cDisplayClass30_1.thing = Activator.CreateInstance(t);
//            Stopwatch stopwatch = new Stopwatch();
//            using (FileStream fileStream = new FileStream(str2, FileMode.Create))
//            {
//                stopwatch.Start();
//                // ISSUE: reference to a compiler-generated field
//                // ISSUE: reference to a compiler-generated field
//                // ISSUE: reference to a compiler-generated method
//                // ISSUE: reference to a compiler-generated field
//                // ISSUE: reference to a compiler-generated field
//                // ISSUE: reference to a compiler-generated field
//                // ISSUE: reference to a compiler-generated method
//                foreach (string s in Enumerable.Where<string>(Enumerable.Select<string, string>((IEnumerable<string>)stuff, cDisplayClass30_1.\u003C\u003E9__0 ?? (cDisplayClass30_1.\u003C\u003E9__0 = new Func<string, string>(cDisplayClass30_1.\u003CGo\u003Eb__0))), Program.\u003C\u003Ec.\u003C\u003E9__3_1 ?? (Program.\u003C\u003Ec.\u003C\u003E9__3_1 = new Func<string, bool>(Program.\u003C\u003Ec.\u003C\u003E9.\u003CGo\u003Eb__3_1))))
//        {
//                    byte[] bytes = Encoding.UTF8.GetBytes(s);
//                    fileStream.Write(bytes, 0, bytes.Length);
//                    ++num;
//                }
//              ((Stream)fileStream).Flush();
//                stopwatch.Stop();
//            }
//            FileInfo fileInfo = new FileInfo(str2);
//            string format2 = "\tWrote {0} lines in {1}. Size: {2} bytes, {3} MB/s";
//            object[] objArray2 = new object[4];
//            int index5 = 0;
//            // ISSUE: variable of a boxed type
//            __Boxed<int> local1 = (ValueType)num;
//            objArray2[index5] = (object)local1;
//            int index6 = 1;
//            // ISSUE: variable of a boxed type
//            __Boxed<TimeSpan> local2 = (ValueType)stopwatch.Elapsed;
//            objArray2[index6] = (object)local2;
//            int index7 = 2;
//            // ISSUE: variable of a boxed type
//            __Boxed<long> local3 = (ValueType)fileInfo.Length;
//            objArray2[index7] = (object)local3;
//            int index8 = 3;
//            // ISSUE: variable of a boxed type
//            __Boxed<double> local4 = (ValueType)((double)fileInfo.Length / stopwatch.Elapsed.TotalSeconds / 1000.0 / 1000.0);
//            objArray2[index8] = (object)local4;
//            Console.WriteLine(string.Format(format2, objArray2));
//            // ISSUE: reference to a compiler-generated field
//            return new Result()
//            {
//                Function = cDisplayClass30_1.thingToDo.Method.Name,
//                Type = t.Name,
//                Size = fileInfo.Length
//            };
//        }

//        private static string Json(string data, object pd, List<string> ignoreList)
//        {
//            List<FieldInfo> list = Enumerable.ToList<FieldInfo>(Enumerable.Where<FieldInfo>((IEnumerable<FieldInfo>)pd.GetType().GetFields(), (Func<FieldInfo, bool>)(x => !ignoreList.Contains(x.Name))));
//            data = data.Replace("$NMGT;", string.Empty);
//            string str = data;
//            char[] chArray = new char[1];
//            int index1 = 0;
//            int num = 59;
//            chArray[index1] = (char)num;
//            string[] strArray = str.Split(chArray);
//            if (strArray.Length < list.Count)
//                return string.Empty;
//            int index2 = 0;
//            foreach (FieldInfo fieldInfo in list)
//            {
//                fieldInfo.SetValue(pd, (object)strArray[index2]);
//                ++index2;
//            }
//            return JsonConvert.SerializeObject(pd, Formatting.None);
//        }

//        private static string CsvSimple(string data, object pd, List<string> ignoreList)
//        {
//            List<FieldInfo> list1 = Enumerable.ToList<FieldInfo>(Enumerable.Where<FieldInfo>((IEnumerable<FieldInfo>)pd.GetType().GetFields(), (Func<FieldInfo, bool>)(x => !ignoreList.Contains(x.Name))));
//            data = data.Replace("$NMGT;", string.Empty);
//            // ISSUE: reference to a compiler-generated field
//            // ISSUE: reference to a compiler-generated field
//            // ISSUE: reference to a compiler-generated field
//            // ISSUE: reference to a compiler-generated method
//            string str1 = string.Join(",", Enumerable.Select<FieldInfo, string>((IEnumerable<FieldInfo>)list1, Program.\u003C\u003Ec.\u003C\u003E9__5_1 ?? (Program.\u003C\u003Ec.\u003C\u003E9__5_1 = new Func<FieldInfo, string>(Program.\u003C\u003Ec.\u003C\u003E9.\u003CCsvSimple\u003Eb__5_1))));
//            string str2 = data.Replace(';', ',');
//            char[] chArray = new char[1];
//            int index1 = 0;
//            int num1 = 44;
//            chArray[index1] = (char)num1;
//            List<string> list2 = Enumerable.ToList<string>((IEnumerable<string>)str2.Split(chArray));
//            list2.RemoveAt(3);
//            string str3 = string.Join(",", (IEnumerable<string>)list2);
//            if (list2.Count == list1.Count)
//                return string.Format("{0}\r\n{1}\r\n", (object)str1, (object)str3);
//            int num2 = list1.Count - list2.Count;
//            for (int index2 = 0; index2 < num2; ++index2)
//                str3 = str3 + ",";
//            return string.Format("{0}\r\n{1}\r\n", (object)str1, (object)str3);
//        }

//        private static string Csv(string data, object pd, List<string> ignoreList)
//        {
//            List<FieldInfo> list1 = Enumerable.ToList<FieldInfo>(Enumerable.Where<FieldInfo>((IEnumerable<FieldInfo>)pd.GetType().GetFields(), (Func<FieldInfo, bool>)(x => !ignoreList.Contains(x.Name))));
//            data = data.Replace("$NMGT;", string.Empty);
//            string str1 = data;
//            char[] chArray = new char[1];
//            int index1 = 0;
//            int num = 59;
//            chArray[index1] = (char)num;
//            List<string> list2 = Enumerable.ToList<string>((IEnumerable<string>)str1.Split(chArray));
//            list2.RemoveAt(3);
//            if (list2.Count < list1.Count)
//                return string.Empty;
//            int index2 = 0;
//            string str2 = "";
//            foreach (FieldInfo fieldInfo in list1)
//            {
//                string s = list2[index2];
//                string str3 = fieldInfo.FieldType.ToString();
//                if (!(str3 == "System.Int32"))
//                {
//                    if (str3 == "System.Double")
//                    {
//                        if (s.IndexOf("#", StringComparison.Ordinal) > -1)
//                            return string.Empty;
//                        str2 = str2 + (object)double.Parse(s);
//                    }
//                    else
//                        str2 = str2 + s;
//                }
//                else
//                    str2 = str2 + (object)int.Parse(s);
//                if (index2 < list1.Count - 1)
//                    str2 = str2 + ",";
//                ++index2;
//            }
//            // ISSUE: reference to a compiler-generated field
//            // ISSUE: reference to a compiler-generated field
//            // ISSUE: reference to a compiler-generated field
//            // ISSUE: reference to a compiler-generated method
//            return string.Format("{0}\r\n{1}\r\n", (object)string.Join(",", Enumerable.Select<FieldInfo, string>((IEnumerable<FieldInfo>)list1, Program.\u003C\u003Ec.\u003C\u003E9__6_1 ?? (Program.\u003C\u003Ec.\u003C\u003E9__6_1 = new Func<FieldInfo, string>(Program.\u003C\u003Ec.\u003C\u003E9.\u003CCsv\u003Eb__6_1)))), (object)str2);
//        }
//    }
//}
