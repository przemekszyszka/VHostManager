using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHostManager
{
    public class Host
    {
        public string AdresIp { get; set; }
        public string DomainName { get; set; }
        public string Country { get; set; }
        public IList<Host> VirtualHosts { get; set; } 
    }
}
