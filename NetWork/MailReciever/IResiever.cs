using System.Collections.Generic;

namespace NetWork.MailReciever
{
    public interface IResiever
    {
        void SetServer(string host, int port);
        void SetCredentials(string login, string password);
        IEnumerable<Message_obj> getAllMessages();
        IEnumerable<Message_obj> GetIncomingMails(IEnumerable<string> uids);
    }
}