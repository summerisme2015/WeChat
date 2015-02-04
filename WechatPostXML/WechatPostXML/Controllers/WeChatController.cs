using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Serialization;
using WechatPostXML.Models;

namespace WechatPostXML.Controllers
{
    public class WeChatController : ApiController {
        private readonly string _token = "mywechat";

        public HttpResponseMessage Get() {
            var nameValuePairs = Request.GetQueryNameValuePairs();
            string echostr = nameValuePairs.Where(x => x.Key == "echostr").FirstOrDefault().Value;
            HttpResponseMessage resp;
            if (ValidateSignature()) {
                resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Content = new StringContent(echostr, Encoding.UTF8, "text/html");
            }
            else {
                resp = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                resp.Content = new StringContent("Your are not authorized.", Encoding.UTF8, "text/html");
            }
            return resp;
        }


        public async Task<HttpResponseMessage> Post() {
            TextMessage recievedMessage;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(TextMessage));
            
            Stream stream = await Request.Content.ReadAsStreamAsync();
            using (var reader = new StreamReader(stream)) {
                
                recievedMessage = (TextMessage)xmlSerializer.Deserialize(reader);
            }
            
            TextMessage sendingMessage = new TextMessage() { 
                ToUserName = recievedMessage.FromUserName,
                FromUserName = recievedMessage.ToUserName,
                CreateTime = ConvertToUnixTimestamp(DateTime.Now).ToString(),
                Content = recievedMessage.Content
            };

            Trace.TraceInformation(Request.Headers.Accept.ToString());

            string messageStr = string.Empty;
            using (StringWriter textWriter = new StringWriter()) {
                var xns = new XmlSerializerNamespaces();
                xns.Add(string.Empty, string.Empty);
                xmlSerializer.Serialize(textWriter, sendingMessage, xns);
                messageStr = textWriter.ToString();
            }

            return new HttpResponseMessage() {
                Content = new StringContent(
                    messageStr,
                    Encoding.UTF8,
                    "text/html"
                )
            };
        }

        public double ConvertToUnixTimestamp(DateTime date) {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        private bool ValidateSignature() {
            var nameValuePairs = Request.GetQueryNameValuePairs();
            string signature = nameValuePairs.Where(x => x.Key == "signature").FirstOrDefault().Value;
            string nonce = nameValuePairs.Where(x => x.Key == "nonce").FirstOrDefault().Value;
            string timestamp = nameValuePairs.Where(x => x.Key == "timestamp").FirstOrDefault().Value;

            string[] tmpArr = new[] { _token, timestamp, nonce };
            Array.Sort<string>(tmpArr);
            string tmpStr = String.Join<string>("", tmpArr);

            string hashString;
            using (SHA1Managed sha1 = new SHA1Managed()) {
                byte[] hashData;
                hashData = sha1.ComputeHash(Encoding.Default.GetBytes(tmpStr));
                hashString = BitConverter.ToString(hashData).Replace("-", "");
            }

            return String.Equals(hashString, signature, StringComparison.OrdinalIgnoreCase);
        }
        
    }
}