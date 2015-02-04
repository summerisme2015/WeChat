using System;
using System.Xml.Serialization;

namespace WechatPostXML.Models
{
    [Serializable]
    [XmlRootAttribute(ElementName="xml")]
    public class TextMessage {
        public TextMessage(){
            this.MsgType = "text";
        }
        public string ToUserName { get; set; }
        public string FromUserName { get; set; }
        public string CreateTime {get; set;}
        public string MsgType {get; set;}
        public string Content {get;set;}

    }
}