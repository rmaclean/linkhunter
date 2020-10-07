namespace linkhunter
{
    using System;
    using System.IO;
    using static System.ConsoleColor;
    using System.Text.RegularExpressions;
    using System.Net.Http;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Konsole;
    using System.Linq;
    using Konsole.Forms;

    class Program
    {
        private static void EnableUTFConsole()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("\xfeff"); // bom = byte order mark
        }

        static async Task Main(string fileToHunt, bool includeFound = false)
        {
            EnableUTFConsole();
            var console = Window.Open();
            if (!File.Exists(fileToHunt))
            {
                console.WriteLine(Red, $"⚠ {fileToHunt} not found");
                return;
            }

            var content = File.ReadAllText(fileToHunt);
            // https://stackoverflow.com/a/20651284
            var regex = new Regex(@"(http|https|ftp|)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?([a-zA-Z0-9\-\?\,\'\/\+&%\$#_]+)");
            var matches = regex.Matches(content);
            var client = new HttpClient();
            var links = new List<LinkInfo>(matches.Count);

            var urls = matches.Select(_ => _.Value.ToLower()).Distinct();
            var progressBar = new ProgressBar(urls.Count());

            foreach (var url in urls)
            {                
                var clientResult = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                var contentType = clientResult.Content.Headers.GetValues("Content-Type").FirstOrDefault() ?? "Unknown";
                links.Add(new LinkInfo {
                    URL = url,
                    Found = clientResult.IsSuccessStatusCode,
                    ContentType = contentType,
                });

                progressBar.Next(url);
            }

            Console.Clear();
            foreach (var link in links.Where(_ => !_.Found || includeFound))
            {
                Console.WriteLine(link.URL);
            }
        }
    }

    class LinkInfo {
        public string URL {get; set;}
        public bool Found {get;set;}
        public string ContentType {get;set;}
    }
}
