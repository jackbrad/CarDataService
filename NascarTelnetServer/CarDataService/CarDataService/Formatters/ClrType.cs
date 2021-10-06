using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CarDataService.Formatters
{
    internal class ClrType : IRowConverter
    {
        public string ConvertToString<T>(string rawData) where T : new()
        {
            throw new NotImplementedException();
        }

        public T Convert<T>(string rawData) where T : new()
        {
            throw new NotImplementedException("will do this when there's a need");
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
            return t;
        }

        public string ConvertToStringSimple(string rawData, bool useCache = true)
        {
            throw new NotImplementedException();
        }

        public byte[] ConvertToBytesSimple(string rawData, bool useCache = true)
        {
            throw new NotImplementedException();
        }
    }
}
