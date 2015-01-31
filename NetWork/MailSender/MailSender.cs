using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace NetWork.MailSender
{
    public class MailSender : ISender
    {
        private MailConfig _mailboxConfig;

        private NetworkCredential _credentials;

        private MailMessage _curentMessage;

        private Dictionary<string, string> _additionalHeaders = new Dictionary<string, string>();

        public MailSender(string serverHost, int serverPort, string login, string password)
        {
            _mailboxConfig = new MailConfig
            {
                SMTP_ServerHost = serverHost,
                SMTP_ServerPort = serverPort
            };

            _credentials = new NetworkCredential(login, password);
        }

        public MailSender()
        {
            
        }

        public void AddAditionalHeader(string key, string value)
        {
            _additionalHeaders.Add(key, value);
        }

        public void SetServer(string serverHost, int serverPort)
        {
            _mailboxConfig = new MailConfig
            {
                SMTP_ServerHost = serverHost,
                SMTP_ServerPort = serverPort
            };
        }

        public void SetCredentials(string login, string password)
        {
            _credentials = new NetworkCredential(login, password);
        }

        private SmtpClient _create_SmtpClient()
        {
            SmtpClient client = new SmtpClient(_mailboxConfig.SMTP_ServerHost, _mailboxConfig.SMTP_ServerPort);
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            client.Credentials = _credentials;

            return client;
        }

        public void CreateMessage(string body, bool isHTML, string subjetc = "")
        {
            _curentMessage = new MailMessage();

            _curentMessage.Body = body;
            _curentMessage.Subject = subjetc;

            _curentMessage.IsBodyHtml = isHTML;
//            _curentMessage.BodyEncoding = Encoding.UTF8;

            _curentMessage.From = new MailAddress(_credentials.UserName);

            //Дополнительные хедеры для шифрования
            foreach (string key in _additionalHeaders.Keys)
            {
                _curentMessage.Headers.Add(key, _additionalHeaders[key]);
            }
        }
        
        public bool AddReceivers(params string[] receivers)
        {
            if (_curentMessage == null)
                return false;

            foreach (string receiver in receivers)
            {
                _curentMessage.To.Add(new MailAddress(receiver));
            }

            return true;
        }

        public bool AddAttachment(Stream dataStream, string fileName)
        {
            if (_curentMessage == null)
                return false;

            Attachment attachment = new Attachment(dataStream, fileName);
            _curentMessage.Attachments.Add(attachment);

            return true;
        }

        public void SendMessage()
        {
            try
            {
                if (_curentMessage == null)
                    return;

                using (var sender = _create_SmtpClient())
                {
                    sender.Send(_curentMessage);
                }

                _curentMessage.Dispose();
                _additionalHeaders.Clear();
            }
            catch (Exception ex)
            {
                //!!!
                throw ex;
            }
        }
    }
}
