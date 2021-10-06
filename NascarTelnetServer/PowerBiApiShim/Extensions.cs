using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using PowerBiRealTime.PowerBi;

namespace PowerBiRealTime
{
    public static class JsonBuilder
    {
        public static string ToJson(this object obj, JavaScriptSerializer serializer)
        {
            var jsonBuilder = new StringBuilder();

            jsonBuilder.Append($"{"{"}\"rows\":");
            jsonBuilder.Append(serializer.Serialize(obj));
            jsonBuilder.Append($"{"}"}");

            return jsonBuilder.ToString();
        }

        public static string ToDatasetJson(this object obj, string datasetName)
        {
            var jsonSchemaBuilder = new StringBuilder();
            jsonSchemaBuilder.Append($"{"{"}\"name\": \"{datasetName}\",\"tables\": [");
            jsonSchemaBuilder.Append(obj.ToTableSchema(obj.GetType().Name));
            jsonSchemaBuilder.Append("]}");
            return jsonSchemaBuilder.ToString();
        }

        public static string ToTableSchema(this object obj, string tableName)
        {
            var jsonSchemaBuilder = new StringBuilder();

            jsonSchemaBuilder.Append($"{"{"}\"name\": \"{tableName}\", ");
            jsonSchemaBuilder.Append("\"columns\": [");

            var fields = obj.GetType().GetFields();
            var properties = obj.GetType().GetProperties();

            foreach (var f in fields)
            {
                var sPropertyTypeName = f.FieldType.Name;
                if (sPropertyTypeName.StartsWith("Nullable") && f.FieldType.GenericTypeArguments != null &&
                    f.FieldType.GenericTypeArguments.Length == 1)
                    sPropertyTypeName = f.FieldType.GenericTypeArguments[0].Name;
                string typeName;
                switch (sPropertyTypeName)
                {
                    case "Int32":
                    case "Int64":
                        typeName = "Int64";
                        break;
                    case "Double":
                        typeName = "Double";
                        break;
                    case "Boolean":
                        typeName = "bool";
                        break;
                    case "DateTime":
                        typeName = "DateTime";
                        break;
                    case "String":
                        typeName = "string";
                        break;
                    default:
                        typeName = null;
                        break;
                }

                if (typeName == null)
                    throw new Exception("type not supported");

                jsonSchemaBuilder.Append($"{"{"} \"name\": \"{f.Name}\", \"dataType\": \"{typeName}\"{"}"},");
            }

            foreach (var p in properties)
            {
                var sPropertyTypeName = p.PropertyType.Name;
                if (sPropertyTypeName.StartsWith("Nullable") && p.PropertyType.GenericTypeArguments != null &&
                    p.PropertyType.GenericTypeArguments.Length == 1)
                    sPropertyTypeName = p.PropertyType.GenericTypeArguments[0].Name;
                string typeName;
                switch (sPropertyTypeName)
                {
                    case "Int32":
                    case "Int64":
                        typeName = "Int64";
                        break;
                    case "Double":
                        typeName = "Double";
                        break;
                    case "Boolean":
                        typeName = "bool";
                        break;
                    case "DateTime":
                        typeName = "DateTime";
                        break;
                    case "String":
                        typeName = "string";
                        break;
                    default:
                        typeName = null;
                        break;
                }

                if (typeName == null)
                    throw new Exception("type not supported");

                jsonSchemaBuilder.Append($"{"{"} \"name\": \"{p.Name}\", \"dataType\": \"{typeName}\"{"}"},");
            }

            jsonSchemaBuilder.Remove(jsonSchemaBuilder.ToString().Length - 1, 1);
            jsonSchemaBuilder.Append("]}");
            return jsonSchemaBuilder.ToString();
        }

        public static dataset GetDataset(this dataset[] datasets, string datasetName)
        {
            return datasets.FirstOrDefault(x => x.Name == datasetName);
        }

        public static group GetGroup(this group[] groups, string groupName)
        {
            return groups.FirstOrDefault(x => x.Name == groupName);
        }
    }

    public class JavaScriptConverter<T> : JavaScriptConverter where T : new()
    {
        private const string DateFormat = "MM/dd/yyyy";

        public override IEnumerable<Type> SupportedTypes => new[] { typeof(T) };

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            var p = new T();
            var props = typeof(T).GetProperties();
            var fields = typeof(T).GetFields();
            foreach (var key in dictionary.Keys)
            {
                var prop = props.FirstOrDefault(t => t.Name == key);
                prop?.SetValue(p, prop.PropertyType == typeof(DateTime) ? DateTime.ParseExact(dictionary[key] as string, DateFormat, DateTimeFormatInfo.InvariantInfo) : dictionary[key], null);
            }
            return p;
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            var p = (T)obj;
            var serialized = new Dictionary<string, object>();

            foreach (var pi in typeof(T).GetProperties())
            {
                if (pi.PropertyType == typeof(DateTime))
                {
                    serialized[pi.Name] = ((DateTime)pi.GetValue(p, null)).ToString("O");
                }
                else
                {
                    serialized[pi.Name] = pi.GetValue(p, null);
                }
            }
            foreach (var f in typeof(T).GetFields())
            {
                if (f.FieldType == typeof(DateTime))
                {
                    serialized[f.Name] = ((DateTime)f.GetValue(p)).ToString("O");
                }
                else
                {
                    serialized[f.Name] = f.GetValue(p);
                }
            }
            return serialized;
        }

        public static JavaScriptSerializer GetSerializer()
        {
            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new JavaScriptConverter<T>() });
            return serializer;
        }
    }
}