using System.Net;
using System.Threading;

namespace NetWork.FTP
{
    internal struct AsyncParam
    {
        public FtpWebRequest request;
        public string fileName;
        public long fileSize;
        public AutoResetEvent resetEvent;

        public AsyncParam(FtpWebRequest request, string fileName,  long fileSize,AutoResetEvent resetEvent) 
            : this()
        {
            this.request = request;
            this.fileName = fileName;
            this.fileSize = fileSize;
            this.resetEvent = resetEvent;
        }
    }
}