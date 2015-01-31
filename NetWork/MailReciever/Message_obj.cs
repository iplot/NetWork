using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NetWork.MailReciever
{
    [Serializable]
    public class Message_obj
    {
        [XmlElement]
        public string From { get; set; }

        public int KeyLength { get; set; }

        [XmlElement]
        public string To { get; set; }

        [XmlElement]
        public string Date { get; set; }

        [XmlElement]
        public string Subject { get; set; }

        [XmlElement]
        public string Text { get; set; }

        [XmlElement]
        public string Uid { get; set; }

        [XmlIgnore]
        public List<Attachment> Attachments { get; set; }
    }

    public class Attachment
    {
        public string Name { get; set; }

        public Stream Data { get; set; }
    }
}
