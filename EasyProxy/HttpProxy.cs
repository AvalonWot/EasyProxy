using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Buffers.Text;

namespace EasyProxy
{
    public class HttpProxy : IWebProxy
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        ICredentials IWebProxy.Credentials
        {
            get
            {
                string user = "Fake";
                var raw = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Headers));
                string pwd = Convert.ToBase64String(raw, Base64FormattingOptions.None);
                return new NetworkCredential(user, pwd);
            }
            set
            {

            }
        }

        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public HttpProxy(string host, int port)
        {
            Host = host;
            Port = port;
        }

        Uri IWebProxy.GetProxy(Uri destination)
        {
            return new Uri($"http://{Host}:{Port}");
        }

        bool IWebProxy.IsBypassed(Uri host)
        {
            return false;
        }

        public async Task<TcpClient> CreateConnect(string targetHost, int targetPort, CancellationToken cancellationToken)
        {
            var tcp = new TcpClient();
            try
            {
                await tcp.ConnectAsync(Host, Port);
                NetworkStream ns = tcp.GetStream();
                StringBuilder sb = new StringBuilder($"CONNECT {targetHost}:{targetPort} HTTP/1.0\r\n");
                foreach (var item in Headers)
                    sb.AppendFormat("{}: {}\r\n", item.Key, item.Value);
                sb.Append("\r\n");
                var handshake = Encoding.ASCII.GetBytes(sb.ToString());
                await ns.WriteAsync(handshake, 0, handshake.Length, cancellationToken);
                using (var sr = new StreamReader(ns, Encoding.ASCII, false, 256, true))
                {
                    string line = await sr.ReadLineAsync().ConfigureAwait(false);
                    if (!line.StartsWith("HTTP/1.0 200"))
                        throw new ProxyException($"代理服务器返回错误: {line}");
                    while (!string.IsNullOrEmpty(line))
                    {
                        line = await sr.ReadLineAsync().ConfigureAwait(false);
                    }
                }
                return tcp;
            }
            catch (ProxyException e)
            {
                tcp.Dispose();
                throw e;
            }
            catch (Exception e)
            {
                tcp.Dispose();
                throw new ProxyException("连接代理服务器异常", e);
            }
        }
    }
}
