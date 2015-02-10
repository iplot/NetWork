using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
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
            using (Pop3Client pop3Client = new Pop3Client())
            {
                //Если нет соединения, то кинем ошибку и там должны понять о проблемах
                try
                {
                    pop3Client.Connect(_serverHost, _serverPort, true);

                    pop3Client.Authenticate(_userCredential.UserName, _userCredential.Password);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                for (int i = pop3Client.GetMessageCount(); i > 0; i--)
                {
                    Message message = pop3Client.GetMessage(i);

                    if (!uids.Any(m => m == message.Headers.MessageId))
                    {
                        Message_obj messageObj = _getMessageObj(message); //!!!
                        yield return messageObj; //!!!!
                    }
                }

            }
            
        } 

        private Message_obj _getMessageObj(Message m)
        {
            Message_obj message = new Message_obj
            {
                From = m.Headers.From.Address.ToString(),
                Subject = m.Headers.Subject ?? "",
                Date = m.Headers.Date ?? "",
                To = _userCredential.UserName,  //Потом можно изменить, что бы получить всех адресатов
                Uid = m.Headers.MessageId
            };

            //Если сообщение зашифровано, получаем длину ключа и вектора инициализации
            if (m.Headers.UnknownHeaders["KeyLen"] != null)
            {
                message.KeyLength = Convert.ToInt32(m.Headers.UnknownHeaders["KeyLen"]);
            }

            //Если письмо подписано
            if (m.Headers.UnknownHeaders["Sign"] != null)
            {
                message.Sign = m.Headers.UnknownHeaders["Sign"];
            }

            //Содержимое письма (текст или html)
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
