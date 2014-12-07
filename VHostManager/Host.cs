using System.Collections.Generic;

namespace VHostDetector
{
    enum ChartEnum
    {
        ColumnChart1,
        ColumnChart2,
        PieChart1,
        PieChart2
    }

    public class Host
    {
        public string AdresIp { get; set; }
        public string DomainName { get; set; }
        public string Country { get; set; }
        public IList<Host> VirtualHosts { get; set; } 
    }

    public class CountryCount
    {
        public string Key { get; set; }
        public int Value { get; set; }

        public KeyValuePair<string, int> ToKeyValuePair()
        {
            return new KeyValuePair<string, int>(this.Key, this.Value);
        }
    }
}
