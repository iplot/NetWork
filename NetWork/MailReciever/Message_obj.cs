using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace NetWork.MailReciever
{
    public class Message_obj
    {
        public string From { get; set; }

        public string Date { get; set; }

        public string Subject { get; set; }

        public string Text { get; set; }

        public List<Attachment> Attachments { get; set; }
    }

    public class Attachment
    {
        public string Name { get; set; }

        public Stream Data { get; set; }
    }
}
