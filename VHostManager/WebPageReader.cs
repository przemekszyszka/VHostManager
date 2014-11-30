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
        public string[] findVirtualHostsByIp(string ip)
        {
            var html = getWebPage("http://api.hackertarget.com/reverseiplookup/?q=" + ip);
            return splitByNewLines(html);
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

        public string getDataByTagWithAttribute(string html, string tag, string attribute, string value)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);


            var findClasses = doc.DocumentNode.Descendants(tag);

            if (attribute != String.Empty && value != String.Empty)
            {
                findClasses = findClasses.Where(d => d.Attributes.Contains(attribute) && d.Attributes["class"].Value.Contains(value));
            }
            
            return findClasses.FirstOrDefault().InnerHtml.ToString();
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
