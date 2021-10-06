namespace CarDataService.Formatters
{
    public interface IRowConverter
    {
        string ConvertToString<T>(string rawData) where T : new();
        T Convert<T>(string rawData) where T : new();
        string ConvertToStringSimple(string rawData, bool useCache = true);
        byte[] ConvertToBytesSimple(string rawData, bool useCache = true);
    }
}