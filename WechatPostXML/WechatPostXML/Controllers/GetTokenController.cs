using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Http;
using WechatPostXML.Models;

namespace WechatPostXML.Controllers
{
    public class GetTokenController : ApiController
    {
        public void Get()
        {
            string appId = "wx2988abc65279dbba";
            string appSecret = "2bf1461b8a4405e938bfdcd91356269a";

            string baseUrl = "https://api.weixin.qq.com/cgi-bin/token";
            string param = string.Format("?grant_type=client_credential&appid={0}&secret={1}", appId, appSecret);

            GetData(baseUrl, param);
        }

        //Post方法：
        private string Post(string url, string param, string json)
        {
            string result = string.Empty;
            string status = "-1";

            try
            {
                ASCIIEncoding encoding = new ASCIIEncoding();

                byte[] data = encoding.GetBytes(json);

                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url + param);

                //myRequest.ContentType = "application/x-www-form-urlencoded";

                myRequest.Method = "POST";
                myRequest.ContentLength = data.Length;

                using (Stream newStream = myRequest.GetRequestStream())
                {
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                }

                HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
                StreamReader reader = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);

                result = reader.ReadToEnd();

                var tokenConvert = new DefaultJsonConverter();

                var token = tokenConvert.Deserialize<AccessToken>(result);

                reader.Close();
            }
            catch
            {
                Console.WriteLine(DateTime.Now + "   HTTP Post: " + status);
            }

            Console.WriteLine(status);

            return status;
        }

        //Get方法：
        private string GetData(string url, string param)
        {
            string result = string.Empty;
            string status = string.Empty;

            try
            {
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url + param);

                httpRequest.Timeout = 10000;
                httpRequest.Method = "GET";

                HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                StreamReader sr = new StreamReader(httpResponse.GetResponseStream(), System.Text.Encoding.UTF8);

                result = sr.ReadToEnd();

                var tokenConvert = new DefaultJsonConverter();

                var token = tokenConvert.Deserialize<AccessToken>(result);

                sr.Close();
            }
            catch
            {
                Console.WriteLine(DateTime.Now + "   HTTP Get: " + status);
            }

            Console.WriteLine(status);

            return status;
        }

        private static byte[] ReadFully(Stream stream)
        {
            byte[] buffer = new byte[128];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }
    }


    public class DefaultJsonConverter
    {
        public string Serialize(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        public string Serialize(object o, JsonFormat format)
        {
            return JsonConvert.SerializeObject(o, format == JsonFormat.Indented ? Formatting.Indented : Formatting.None);
        }

        public dynamic Deserialize(string json)
        {
            dynamic result = JObject.Parse(json);
            return result;
        }

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }

    public enum JsonFormat
    {
        None,
        Indented
    }
}