using System.IO;

namespace NetWork.MailSender
{
    public interface ISender
    {
        void CreateMessage(string body, bool isHtml, string subjetc = "");
        bool AddReceivers(params string[] receivers);
        bool AddAttachment(Stream dataStream, string fileName);
        void SendMessage();
        void SetServer(string serverHost, int serverPort);
        void SetCredentials(string login, string password);
    }
}