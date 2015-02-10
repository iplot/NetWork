using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetWork.MailReciever
{
    public interface IMailMess
    {
        string Uid { get; set; }
        string Date { get; set; }
    }
}
