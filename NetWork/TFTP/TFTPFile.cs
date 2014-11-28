using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NetWork.TFTP
{
    [JsonObject]
    public class TFTPFile
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "size")]
        public long Size { get; set; }

        public override string ToString()
        {
            return String.Format("{0} {1} bytes", Name, Size);
        }
    }
}
