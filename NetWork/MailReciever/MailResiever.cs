using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using OpenPop.Mime;
using OpenPop.Pop3;

namespace NetWork.MailReciever
{
    public class MailResiever : IResiever
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

        public MailResiever()
        {
        }

        public void SetServer(string host, int port)
        {
            _serverHost = host;
            _serverPort = port;
        }

        public void SetCredentials(string login, string password)
        {
            _userCredential = new NetworkCredential(login, password);
        }

        //Мтоды будут лагать, если использовать конструктор без параметров и забыть указать персональные данные
        //и данные сервера

        public IEnumerable<Message_obj> getAllMessages()
        {
            List<Message_obj> messages = null;

            using (Pop3Client pop3Client = new Pop3Client())
            {
                pop3Client.Connect(_serverHost, _serverPort, true);

                pop3Client.Authenticate(_userCredential.UserName, _userCredential.Password);

                messages = new List<Message_obj>(pop3Client.GetMessageCount());
                for (int i = pop3Client.GetMessageCount(); i > 0; i--)
                {
                    Message message = pop3Client.GetMessage(i);

                    messages.Add(_getMessageObj(message));
                }
            }

            return messages;
        }

        public IEnumerable<Message_obj> GetIncomingMails(IEnumerable<string> uids)
        {
            List<Message_obj> messages = null;

            using (Pop3Client pop3Client = new Pop3Client())
            {
                pop3Client.Connect(_serverHost, _serverPort, true);

                pop3Client.Authenticate(_userCredential.UserName, _userCredential.Password);

                messages = new List<Message_obj>(pop3Client.GetMessageCount());

                for (int i = pop3Client.GetMessageCount(); i > 0; i--)
                {
                    Message message = pop3Client.GetMessage(i);

                    if (!uids.Any(m => m == message.Headers.MessageId))
                    {
                        messages.Add(_getMessageObj(message));
                    }
                }
            }

            return messages;
        } 

        private Message_obj _getMessageObj(Message m)
        {
            Message_obj message = new Message_obj
            {
                From = m.Headers.From.Address.ToString(),
                Subject = m.Headers.Subject.ToString(),
                Date = m.Headers.Date.ToString(),
//                Text = m.FindFirstPlainTextVersion().GetBodyAsText() ?? "",
                Uid = m.Headers.MessageId
            };

            if (m.FindFirstPlainTextVersion() != null)
            {
                message.Text = m.FindFirstPlainTextVersion().GetBodyAsText();
            }
            else if (m.FindFirstHtmlVersion() != null)
            {
                message.Text = m.FindFirstHtmlVersion().GetBodyAsText();
            }
            

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
