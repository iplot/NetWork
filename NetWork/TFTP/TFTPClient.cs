using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tftp.Net;

namespace NetWork.TFTP
{
    public class TFTPClient
    {
        public delegate void onFinish_print(string message);
        public delegate void onError_print(string message);
        public delegate void onProgress_print(string message);

        public event onFinish_print finishE;
        public event onError_print errorE;
        public event onProgress_print progressE;

        private TftpClient _client;

        private AutoResetEvent _reset;
        
        public TFTPClient(string serverHost)
        {
            _client = new TftpClient(serverHost);
            _reset = new AutoResetEvent(false);
        }

        public void downloadFile(string server_fileName, string local_fileName)
        {
            var transfer = _client.Download(server_fileName);

            Stream fileStream = new FileStream(local_fileName, FileMode.OpenOrCreate);

            _set_transferEvent(transfer);

            transfer.Start(fileStream);
            _reset.WaitOne();
        }

        public void uploadFile(FileInfo file)
        {
            Stream fileStream = new FileStream(file.FullName, FileMode.Open);

            var transfer = _client.Upload(file.Name);

            _set_transferEvent(transfer);

            transfer.Start(fileStream);
            _reset.WaitOne();
        }

        private void _set_transferEvent(ITftpTransfer transfer)
        {
            transfer.OnError += _onError;
            transfer.OnProgress += _onProgress;
            transfer.OnFinished += _onFinished;
        }

        public IEnumerable<TFTPFile> directory_list()
        {
            var transfer = _client.Download("dir_ls?");

            Stream stream = new FileStream("dirlist.txt", FileMode.OpenOrCreate);

            _set_transferEvent(transfer);

            transfer.Start(stream);
            _reset.WaitOne();

            stream = new FileStream("dirlist.txt", FileMode.OpenOrCreate);
            StreamReader reader = new StreamReader(stream);
            string dirList = reader.ReadToEnd();
            stream.Close();
            reader.Close();
            File.Delete("dirlist.txt");

            //deserializa json
            var fileList = JsonConvert.DeserializeObject<List<TFTPFile>>(dirList);

            return fileList;
        }

        private void _onError(ITftpTransfer transfer, TftpTransferError error)
        {
            errorE(error.ToString());
        }

        private void _onProgress(ITftpTransfer transfer, TftpTransferProgress progress)
        {
            progressE(progress.ToString());
        }

        private void _onFinished(ITftpTransfer transfer)
        {
            finishE(string.Format("{0} {1} bytes", transfer.Filename, transfer.ExpectedSize)
                );
            transfer.Dispose();
            _reset.Set();
        }
    }
}
