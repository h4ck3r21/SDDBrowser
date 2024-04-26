using SDDWebBrowser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SDDBrowser
{
    internal class Testing
    {
        public List<string> domains;

        internal Testing() 
        {
            StreamReader reader = File.OpenText("domain_names.csv");
            domains = new List<string>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                domains.Add(values[0]);
            }
            List<string> TestData = new List<string>
            {
                "google.com",
                "google",
                " google/",
                "C://",
                "file:",
                "123",
                "123.123.123.123",
                "123.123.123.1",
                "123.123.123.",
                "123.123.123./",
                "123 /",
                "123.2 /",
                "http://",
                "int.to",
                "url.com.com.sda",
                "https:url.com.com.sda",
                "http:sd",
                "www.test",
                "www.123",
            };
            foreach (string s in TestData)
            {
                Debug.WriteLine(CheckUrl(s));
            }
        }


        protected bool CheckUrl(string url)
        {
            string urlWithProtocol = "http://" + url;
            string domainPattern = string.Join("|", domains.Select(x =>x + "/"));
            bool result =
                Uri.IsWellFormedUriString(url.ToString(), UriKind.Absolute)
                || (url.EndsWith("/") && Regex.IsMatch(url, @"[a-zA-Z.]"))
                || (char.IsLetterOrDigit(url[0]) && Regex.IsMatch(url, domainPattern))
                || domains.Any(x => url.EndsWith(x))
                || Regex.IsMatch(url, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")
                || Directory.Exists(url)
                || url.StartsWith("http://");
                //|| url.StartsWith("https://");
            return result;
        }

    }

}
