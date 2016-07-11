using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using System.Xml.Linq;
using System.IO;

namespace eClock
{
    public static class TimezoneAPI
    {
        public static string GetTimezoneFilePath()
        {
            return Path.Combine(Application.Context.FilesDir.Path, "timezone.txt");
        }

        // Call the timezonedb api
        public static ApiResponse GetTimezone()
        {
            ApiResponse result = new ApiResponse();

            try
            {
                string tzone = File.ReadAllText(GetTimezoneFilePath());

                string key = "#YOUR_TIMEZONEDB_API_KEY#";
                string url = string.Format("http://api.timezonedb.com/v2/get-time-zone?key={0}&format=xml&by=zone&zone={1}", key, tzone);
                XDocument doc = XDocument.Load(url);

                // XML Document to Dictionary
                var dict = doc.Root.Elements()
                   .ToDictionary(e => e.Name.ToString(),
                                 e => e.Value.ToString());

                result.zoneName = tzone;
                result.status = dict["status"];
                result.message = dict["message"];
                result.countryName = dict["countryName"];
                result.abbreviation = dict["abbreviation"];
                result.gmtOffset = Convert.ToInt32(dict["gmtOffset"]);
                result.dstEnd = Convert.ToInt32(dict["dstEnd"]);
                result.dstStart = Convert.ToInt32(dict["dstStart"]);
                result.timestamp = Convert.ToInt32(dict["timestamp"]);
            }
            catch (Exception ex)
            {
                result.status = "";
                result.message = ex.Message;
            }

            return result;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }

    }

    public class Timezone
    {
        public string country_code { get; set; }
        public string country_name { get; set; }
        public string time_zone { get; set; }
    }

    public class ApiResponse
    {
        public string status { get; set; }
        public string message { get; set; }
        public string countryName { get; set; }
        public string zoneName { get; set; }
        public string abbreviation { get; set; }
        public int gmtOffset { get; set; }
        public int dstStart { get; set; }
        public int dstEnd { get; set; }
        public int timestamp { get; set; }
    }
}