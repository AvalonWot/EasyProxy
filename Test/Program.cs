using EasyProxy;
using System;
using System.Net;
using System.Net.Http;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new HttpProxy("127.0.0.1", 8080);
            p.Headers.Add("Client-Id", "testest");
            var fd = p.CreateConnect("www.baidu.com", 80, default).Result;
            fd.Close();
            //var client = new HttpClient(new HttpClientHandler
            //{
            //    AutomaticDecompression = DecompressionMethods.All,
            //    Proxy = p,
            //}, true);
            //using (client)
            //{
            //    var r = client.GetAsync("https://www.baidu.com").Result;
            //}
        }
    }
}
