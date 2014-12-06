using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace VHostManager
{
    public class WebPageReader
    {
        public IList<string> FindVirtualHostsByIp(string ip)
        {
            var html = GetWebPage("http://api.hackertarget.com/reverseiplookup/?q=" + ip);
            return SplitByNewLines(html).ToList();
        }

        public IList<string> FindOtherVirtualHostsByIp(string ip)
        {
            var ints = ip.Split('.');

            WebClient client = new WebClient();

            var html = GetWebPage("https://www.robtex.com/en/advisory/ip/" + ints[0] + "/" + ints[1] + "/" + ints[2] + "/" + ints[3] + "/shared.html");

            if (string.IsNullOrEmpty(html))
                return new List<string>();

            var names = GetDataByTagWithAttribute(html, "ol", "class", "xbul");

            return names;
        }

        public string GetWebPage(string address)
        {
            string responseText;

            try
            {
                var request = (HttpWebRequest) WebRequest.Create(address);

                using (var response = (HttpWebResponse) request.GetResponse())
                {
                    using (
                        var responseStream = new StreamReader(response.GetResponseStream(),
                            Encoding.GetEncoding("utf-8")))
                    {
                        responseText = responseStream.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                responseText = string.Empty;
            }
            

            return responseText;
        }

        public IList<string> GetDataByTagWithAttribute(string html, string tag, string attribute, string value)
        {
            var doc = new HtmlDocument();
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

        public string[] SplitByChar(string toSplit, char[] spliter)
        {
            return toSplit.Split(spliter);
        }

        public string[] SplitByNewLines(string toSplit)
        {
            var splited = toSplit.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            return splited.Where(x => x != String.Empty).ToArray();
        }
    }
}
