using System.IO;
using System.Net;
using Module.Hsnr.Timetable.Data;

namespace Module.Hsnr.Timetable
{
    public class TimetableConnector
    {
        private const string Address = "https://mpl-server.kr.hs-niederrhein.de/fb03/sp/_Stundenplan.php";

        internal string PostData(FormData data)
        {
            var result = string.Empty;
            var request = (HttpWebRequest)WebRequest.Create(Address);
            var dataString = data.ToParameters();
            request.Method = "POST";
            request.ContentLength = dataString.Length;
            request.ContentType = "application/x-www-form-urlencoded";

            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(dataString);
            }

            var response = (HttpWebResponse)request.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }
    }
}