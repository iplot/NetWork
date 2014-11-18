using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NetWork.FTP
{
    public class FTPClient
    {
        public delegate void status(long percent);

        public delegate void error(Exception ex);

        public event status Read_or_Write_event;
        public event error Error_event;

        private readonly NetworkCredential _credentials;
        private Uri _first_uri;

        private Uri _uri;

        public FTPClient()
        {
            _uri = new Uri("ftp://nouri");
            _first_uri = new Uri("ftp://nouri");

            _credentials = new NetworkCredential();
        }

        public FTPClient(Uri uri)
        {
            if (uri.Scheme != Uri.UriSchemeFtp)
                throw new Exception("Not FTP URI"); //not ftp _uri exception

            _uri = uri;

            _credentials = new NetworkCredential();

            //определяем адрес к серверу
            int pos = uri.AbsoluteUri.IndexOf('/', uri.AbsoluteUri.LastIndexOf('.'));

            if (pos != -1)
                _first_uri = new Uri(uri.AbsoluteUri.Remove(pos));
            else
                _first_uri = new Uri(_uri.AbsoluteUri);
        }

        public string Login
        {
            set { _credentials.UserName = value; }
        }

        public string Password
        {
            set { _credentials.Password = value; }
        }

        public void setUri(Uri uri)
        {
            if (uri.Scheme != Uri.UriSchemeFtp)
                throw new Exception("Not FTP URI");

            _uri = uri;

            //определяем адрес к серверу
            int pos = uri.AbsoluteUri.IndexOf('/', uri.AbsoluteUri.LastIndexOf('.'));

            if (pos != -1)
                _first_uri = new Uri(uri.AbsoluteUri.Remove(pos));
            else
                _first_uri = new Uri(_uri.AbsoluteUri);
        }

        private FtpWebRequest _create_request()
        {
            var request = (FtpWebRequest) WebRequest.Create(_uri);
            request.Credentials = _credentials;

            return request;
        }

        public async Task<string> get_directoryName()
        {
            var request = _create_request();

            request.Method = WebRequestMethods.Ftp.PrintWorkingDirectory;

            string str = string.Empty;
            try
            {
                str = await _get_response_txt(request);
            }
            catch (WebException ex)
            {
                throw ex;
//                throw new WebException("Can not get response", WebExceptionStatus.ReceiveFailure);
            }

            return str;
        }

        public async Task<List<FTPFile>> get_directoryList()
        {
            var request = _create_request();

            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.UsePassive = true;

            List<string> workDir = null;
            List<FTPFile> ftpFiles = null;

            try
            {
                ftpFiles = await _get_richDirectoryList();

                string str = await _get_response_txt(request);

                workDir = str
                        .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                for (int i = 0; i < workDir.Count; i++)
                {
                    //чистим имя файла от названия папки
                    if (workDir[i].IndexOf('/') != -1)
                    {
                        workDir[i] = workDir[i].Substring(
                            workDir[i].IndexOf('/') + 1
                            );
                    }

                    ftpFiles[i].Name = workDir[i];
                }
            }
            catch (WebException ex)
            {
                throw ex;
//                throw new WebException("Can not get response", WebExceptionStatus.ReceiveFailure);
            }

            return ftpFiles;
        } 

        private async Task<List<FTPFile>> _get_richDirectoryList()
        {
            var request = _create_request();

            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.UsePassive = true;

            List<String> workDir = null;
            List<FTPFile> ftpFiles = new List<FTPFile>();

            try
            {
                //разбиваем сполшную строку с названиями на массив названий
                string text = await _get_response_txt(request);
                workDir = 
                    text
                        .Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                foreach (String str in workDir)
                {
                    ftpFiles.Add(new FTPFile(str));    
                }

            }
            catch (WebException ex)
            {
                throw ex;
//                throw new WebException("Can not get response", WebExceptionStatus.ReceiveFailure);
            }

            return ftpFiles;
        }

        public void up_dir(FTPFile directoryName)
        {
            if(!directoryName.IsDir)
                throw new Exception("It's not a directory");

            _uri = new Uri(_uri.AbsoluteUri + String.Format("/{0}", directoryName.Name));

            FtpWebRequest request = _create_request();
        }

        public void down_dir()
        {
            if (_uri.AbsoluteUri.Equals(_first_uri.AbsoluteUri))
                throw new Exception("You in the root directory");

            string path = _uri.AbsoluteUri;
            int position = path.LastIndexOf('/');

            path = path.Remove(position);
            _uri = new Uri(path);

            FtpWebRequest request = _create_request();
        }

        public void download_file(FTPFile serv_file, string local_fileName)
        {
            _uri = new Uri(_uri.AbsoluteUri + 
                String.Format("/{0}", serv_file.Name));

            var request = _create_request();

            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.UsePassive = true;

            try
            {
                AutoResetEvent reset = new AutoResetEvent(false);
                AsyncParam param = new AsyncParam(request, local_fileName, serv_file.Size, reset);

                request.BeginGetResponse(new AsyncCallback(_get_file_bin), param);
//                reset.WaitOne();
            }
            catch (WebException ex)
            {
                throw ex;
//                throw new WebException("Can not get response");
            }
        }

        private void _get_file_bin(IAsyncResult result)
        {
            AsyncParam param = (AsyncParam) result.AsyncState;

            try
            {
                var response = param.request.EndGetResponse(result);
                Stream dataStream = response.GetResponseStream();

                byte[] bytes = new byte[30000];

                FileStream file = new FileStream(param.fileName, FileMode.Create);

                int readBytes = 0;
                int count = 0;

                do
                {
                    readBytes = dataStream.Read(bytes, 0, bytes.Length);
                    file.Write(bytes, 0, readBytes);

                    count += readBytes;
                    Read_or_Write_event((count*100)/param.fileSize);

                } while (readBytes != 0);

                file.Close();
                response.Close();
            }
            catch (WebException ex)
            {
                throw ex;
//                throw new WebException("Can not get response");
            }
            finally
            {
//                param.resetEvent.Set();
            }
        }

        private Task<string> _get_response_txt(FtpWebRequest request)
        {
            return Task.Run(() =>
            {
                string data = "";
                try
                {
                    var response = (FtpWebResponse) request.GetResponse();

                    Stream dataStream = response.GetResponseStream();

                    var dataReader = new StreamReader(dataStream);

                    data = dataReader.ReadToEnd();

                    response.Close();
                }
                catch (WebException ex)
                {
//                    throw new WebException("Can not get response");
                    Error_event(ex);
                }

                return data;
            });
        }

        //IOException
        //WebException
        //InvalidOperation
        public void upload_file(string local_fileName, long fileSize)
        {
            _uri = new Uri(_uri.AbsoluteUri +
                String.Format("/{0}", Path.GetFileName(local_fileName)));

            FtpWebRequest request = _create_request();
            request = _create_request();
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UsePassive = false;

            try
            {
                AutoResetEvent reset = new AutoResetEvent(false);
                AsyncParam param = new AsyncParam(request, local_fileName, fileSize, reset);

                request.BeginGetRequestStream(new AsyncCallback(_save_file), param);
//                reset.WaitOne();
            }
            catch (WebException ex)
            {
                throw ex;
            }
            catch (IOException ex)
            {
                throw ex;
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
        }

        private void _save_file(IAsyncResult result)
        {
            AsyncParam param = (AsyncParam)result.AsyncState;

            try
            {
                var requestStream = param.request.EndGetRequestStream(result);

                FileStream localStream = new FileStream(param.fileName, FileMode.Open);

                byte[] bytes = new byte[20000];

                int readBytes = 0;
                int count = 0;

                do
                {
                    readBytes = localStream.Read(bytes, 0, bytes.Length);
                    requestStream.Write(bytes, 0, bytes.Length);

                    count += readBytes;
                    Read_or_Write_event((count*100)/param.fileSize);

                } while (readBytes != 0);

                requestStream.Close();

                param.request.BeginGetResponse(new AsyncCallback(_get_upload_response), param);
            }
            catch (WebException ex)
            {
                throw ex;
            }
            catch (IOException ex)
            {
                throw ex;
//                throw new IOException("Local file not found");
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
//                throw new InvalidOperationException("File already exists or you have no rights for operation");
            }
            finally
            {
//                param.resetEvent.Set();
            }
        }

        private void _get_upload_response(IAsyncResult result)
        {
            AsyncParam param = (AsyncParam) result.AsyncState;

            try
            {
                param.request.EndGetResponse(result);
            }
            catch (WebException ex)
            {
                throw ex;
//                throw new WebException("Can not upload file");
            }
        }
    }
}