using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetWork.FTP
{
    public class FTPFile
    {
        public bool IsDir { get; private set; }
        public long Size { get; private set; }

        public string Name { get; set; }

        public FTPFile(long size, string name)
        {
            Size = size;
            Name = name;
        }

        public FTPFile(string fileInfo)
        {
            string[] info = fileInfo.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries).ToArray();

            IsDir = (info[0][0] == 'd') ? true : false;

            Size = Convert.ToInt64(info[4]);
        }
    }
}
