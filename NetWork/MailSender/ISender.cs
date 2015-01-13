using System.IO;

namespace NetWork.MailSender
{
    public interface ISender
    {
        void CreateMessage(string body, string subjetc = "");
        bool AddReceivers(params string[] receivers);
        bool AddAttachment(Stream dataStream, string fileName);
        void SendMessage();
    }
}