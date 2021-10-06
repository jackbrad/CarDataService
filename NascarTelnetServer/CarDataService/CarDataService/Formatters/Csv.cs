using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CarDataService.Formatters
{
    public class Csv : IRowConverter
    {
        private string _header = string.Empty;
        private int _headerFields = 0;
        public bool CacheSet { get; private set; }

        public Csv(string sampleData)
        {
            FillHeaderCache(sampleData);
        }

        public Csv()
        {
            CacheSet = false;
        }

        public void FillHeaderCache(string sample)
        {
            var fieldNames = ConfigurationManager.AppSettings["OrderedFieldNames"];
            _header = !string.IsNullOrEmpty(fieldNames) ? DeriveHeader(sample, fieldNames) : DeriveHeader(sample);
            _headerFields = _header.Split(',').Length;
        }

        public string ConvertToStringSimple(string rawData, bool useCache = true)
        {
            var data = rawData.Replace("$NMGT;", string.Empty).Replace(';', ',').Split(',');
            if (useCache && _header == string.Empty)
            {
                FillHeaderCache(rawData);
            }

            if (useCache && data.Length == _headerFields) //use cache
            {
                return $"{_header}\r\n{string.Join(",", data)}\r\n";
            }

            var headerRow = DeriveHeader(rawData);
            return $"{string.Join(",", headerRow)}\r\n{string.Join(",", data)}\r\n";
        }

        public byte[] ConvertToBytesSimple(string rawData, bool useCache = true)
        {
            return Encoding.UTF8.GetBytes(ConvertToStringSimple(rawData, useCache));
        }

        public string ConvertToString<T>(string rawData) where T : new()
        {
            throw new NotImplementedException("pending");
            var t = new T();
            var fieldList = t.GetType().GetFields().ToList();
            var data = rawData.Replace("$NMGT;", string.Empty).Split(';');

            var i = 0;
            var values = new List<string>();
            foreach (var f in fieldList)
            {
                var stringValue = data[i];
                var dataType = f.FieldType.ToString();
                string value;
                switch (dataType)
                {
                    case "System.Int32":
                        {
                            var testVal = 0;
                            int.TryParse(stringValue, out testVal);
                            value = testVal.ToString();
                            break;
                        }
                    case "System.Double":
                        {
                            var testVal = 0d;
                            double.TryParse(stringValue, out testVal);
                            value = testVal.ToString(CultureInfo.InvariantCulture);
                            break;
                        }
                    case "System.DateTime":
                        {
                            var testVal = DateTime.UtcNow;
                            if (!DateTime.TryParse(stringValue, out testVal))
                            {
                                value = DateTime.UtcNow.ToString("o");
                                break;
                            }
                            value = testVal.ToString("o");
                            break;
                        }
                    default:
                        value = stringValue;
                        break;
                }
                values.Add(value);
                i++;
            }

            var stringColumnNames = fieldList.Select(x => x.Name).ToList();
            if (data.Length > fieldList.Count) //more data than fields; add extra headers
            {
                var delta = data.Length - fieldList.Count;
                for (var j = 0; j < delta; j++)
                {
                    stringColumnNames.Add($"U{j}");
                }
            }

            //format out
            var headerRow = $"{string.Join(",", stringColumnNames)}";
            var dataRow = $"{string.Join(",", values)}";
            return $"{headerRow}\r\n{dataRow}\r\n";
        }

        public T Convert<T>(string rawData) where T : new()
        {
            throw new NotImplementedException();
        }

        private static string DeriveHeader(string sample, string headers = "")
        {
            var data = sample.Replace("$NMGT;", string.Empty).Split(';');

            if (!string.IsNullOrEmpty(headers)) //e.g., include the data row passed in as the headers
            {
                var headerData = headers.Split(';').ToList();
                if (data.Length <= headerData.Count) return $"{string.Join(",", headerData)}";
                var delta = data.Length - headerData.Count;
                for (var i = 0; i < delta; i++)
                {
                    headerData.Add($"u{i}");
                }

                return $"{string.Join(",", headerData)}";
            }

            var headerRow = new List<string>();
            var a = 'a';

            for (var j = 0; j < data.Length; j++)
            {
                var col = a;
                var suffix = j / 26;
                headerRow.Add($"{col}{(suffix > 0 ? suffix.ToString() : string.Empty)}");
                if (a == 'z')
                {
                    a = 'a';
                }
                else
                {
                    a++;
                }
            }
            return string.Join(",", headerRow);
        }
    }

    //private static string Csv(string data, object pd, List<string> ignoreList)
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
}