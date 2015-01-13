using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using OpenPop.Mime;
using OpenPop.Pop3;

namespace NetWork.MailReciever
{
    public class MailResiever
    {
        private NetworkCredential _userCredential;

        private string _serverHost;

        private int _serverPort;

        public MailResiever(string host, int port, string login, string password)
        {
            _serverHost = host;
            _serverPort = port;

            _userCredential = new NetworkCredential(login, password);
        }

        public IEnumerable<Message_obj> getAllMessages()
        {
            List<Message_obj> messages = null;

            using (Pop3Client pop3Client = new Pop3Client())
            {
                pop3Client.Connect(_serverHost, _serverPort, true);

                pop3Client.Authenticate(_userCredential.UserName, _userCredential.Password);

                messages = new List<Message_obj>(pop3Client.GetMessageCount());
                for (int i = pop3Client.GetMessageCount() - 9; i > 0; i--)
                {
                    Message message = pop3Client.GetMessage(i);

                    messages.Add(_addMessage(message));
                }
            }

            return messages;
        }

        private Message_obj _addMessage(Message m)
        {
            Message_obj message = new Message_obj
            {
                From = m.Headers.From.Address.ToString(),
                Subject = m.Headers.Subject.ToString(),
                Date = m.Headers.Date.ToString(),
                Text = m.FindFirstPlainTextVersion().GetBodyAsText()
            };

            List<Attachment> list = new List<Attachment>();
            for (int i = 0; i < m.FindAllAttachments().Count; i++)
            {
                MemoryStream stream = new MemoryStream();

                m.FindAllAttachments()[i].Save(stream);

                list.Add(new Attachment{Name = m.FindAllAttachments()[i].FileName, Data = stream});
            }

            if(list.Count != 0)
                message.Attachments = list;

            return message;
        }
    }
}
