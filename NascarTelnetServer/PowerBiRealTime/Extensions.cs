using System.Web;

namespace PowerBiRealTime
{
    public static class Extensions
    {
        public static string Encode(this string s)
        {
            return HttpUtility.UrlEncode(s);
        }
    }
}