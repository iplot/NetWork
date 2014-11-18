using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tftp.Net;

namespace NetWork.TFTP
{
    public class TFTPServer
    {
        public delegate void onFinish_print(string message);
        public delegate void onError_print(string message);
        public delegate void onProgress_print(string message);

        public event onFinish_print finishE;
        public event onError_print errorE;
        public event onProgress_print progressE;

        private string _currentDir;
        private TftpServer _server;

        private AutoResetEvent _reset;

        public TFTPServer(string curentDir)
        {
            _currentDir = curentDir;

            createServer();

            _reset = new AutoResetEvent(false);
        }

        private void createServer()
        {
            _server = new TftpServer();
            _server.OnReadRequest += _sendFile;
            _server.OnWriteRequest += _recievFile;

            _server.Start();
        }

        public TFTPServer()
        {
            _currentDir = Environment.CurrentDirectory;

            createServer();

            _reset = new AutoResetEvent(false);
        }

        private void _sendFile(ITftpTransfer transfer, EndPoint client)
        {
            if (transfer.Filename.Equals("dir_ls?"))
            {
                string[] files = Directory.GetFiles(_currentDir);
                string filesString = "";
                foreach (var s in files)
                {
                    filesString += s + "\r\n";
                }

                Stream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(filesString);
                writer.Flush();
                stream.Position = 0;

                _set_transferEvents(transfer);

                transfer.Start(stream);
                _reset.WaitOne();
            }
            else
            {
                Stream fileStream = null;

                string fileName = Path.Combine(_currentDir, transfer.Filename);

                if (!File.Exists(fileName))
                {
                    transfer.Cancel(TftpErrorPacket.FileNotFound);
                    return;
                }

                fileStream = new FileStream(fileName, FileMode.Open);

                _set_transferEvents(transfer);

                transfer.Start(fileStream);
                _reset.WaitOne();
            }
        }

        private void _set_transferEvents(ITftpTransfer transfer)
        {
            transfer.OnError += _onError;
            transfer.OnProgress += _onProgress;
            transfer.OnFinished += _onFinish;
        }

        private void _recievFile(ITftpTransfer transfer, EndPoint client)
        {
            string fileName = Path.Combine(_currentDir, transfer.Filename);

            Stream fileStream = new FileStream(fileName, FileMode.OpenOrCreate);

            _set_transferEvents(transfer);

            transfer.Start(fileStream);
            _reset.WaitOne();
        }

        private void _onError(ITftpTransfer transfer, TftpTransferError error)
        {
            errorE(error.ToString());
        }

        private void _onProgress(ITftpTransfer transfer, TftpTransferProgress progress)
        {
            progressE(progress.ToString());
        }

        private void _onFinish(ITftpTransfer transfer)
        {
            finishE(string.Format("Server finish\n {0} {1} bytes", transfer.Filename, transfer.ExpectedSize)
                );
            transfer.Dispose();

            _reset.Set();
        }
    }
}
