using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VHostManager
{
    public class WebPageReader
    {
        public IList<string> findVirtualHostsByIp(string ip)
        {
            var html = getWebPage("http://api.hackertarget.com/reverseiplookup/?q=" + ip);
            return splitByNewLines(html).ToList();
        }

        public IList<string> findOtherVirtualHostsByIp(string ip)
        {
            var ints = ip.Split('.');

            System.Net.WebClient client = new System.Net.WebClient();

            var html = getWebPage("https://www.robtex.com/en/advisory/ip/" + ints[0] + "/" + ints[1] + "/" + ints[2] + "/" + ints[3] + "/shared.html");
            
            
            var names = getDataByTagWithAttribute(html, "ol", "class", "xbul");

            return names;
        }

        public string getWebPage(string address)
        {
            string responseText;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader responseStream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")))
                {
                    responseText = responseStream.ReadToEnd();
                }
            }

            return responseText;
        }

        public IList<string> getDataByTagWithAttribute(string html, string tag, string attribute, string value)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);


            var findClasses = doc.DocumentNode.Descendants(tag);

            if (attribute != String.Empty && value != String.Empty)
            {
                findClasses = findClasses.Where(d => d.Attributes.Contains(attribute) && d.Attributes["class"].Value.Contains(value));
            }

            string innerHTML = findClasses.ElementAt(1).InnerHtml;
            doc.LoadHtml(innerHTML);
            findClasses = doc.DocumentNode.Descendants("code");
            
            IList<string> elements = new List<string>();
            findClasses.ToList().ForEach(x => elements.Add(x.InnerText));

            return elements;
        }

        public string[] splitByChar(string toSplit, char[] spliter)
        {
            return toSplit.Split(spliter);
        }

        public string[] splitByNewLines(string toSplit)
        {
            var splited = toSplit.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            return splited.Where(x => x != String.Empty).ToArray();
        }
    }
}
