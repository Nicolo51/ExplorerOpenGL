using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GameServerTCP.HttpServer
{
    public class ServerWeb
    {
        public static HttpListener listener;
        public static string url { get { return "http://*:" + port + "/"; } }
        public static int port = 8000; 
        public static int pageViews = 0;
        public static int requestCount = 0;
        public static string pageData =
            "<!DOCTYPE>" +
            "<html>" +
            "  <head>" +
            "    <title>HttpListener Example</title>" +
            "  </head>" +
            "  <body>" +
            "    <p>Page Views: {0}</p>" +
            "    <form method=\"post\" action=\"shutdown\">" +
            "      <input type=\"submit\" value=\"Shutdown\" {1}>" +
            "    </form>" +
            "  </body>" +
            "</html>";

        static Task listenTask; 

        public static void Start(int p = 8000)
        {
            port = p; 
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            listener.BeginGetContext(HandleIncomingConnections, null);
            Console.WriteLine("Http Server listening at {0}", url);

            // Handle requests
            //listenTask = HandleIncomingConnections();
            
        }

        public static void Reset()
        {
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            listener.BeginGetContext(HandleIncomingConnections, null);
            Console.WriteLine("Http server reseted.");
        }

        public static void Close()
        {
            listenTask.GetAwaiter().GetResult();
            listener.Close();
        }

        public static void HandleIncomingConnections(IAsyncResult ar)
        {
            HttpListenerContext ctx;
            try
            {
                ctx = listener.EndGetContext(ar);
                listener.BeginGetContext(HandleIncomingConnections, null);
            }
            catch
            {
                Reset(); 
                return;
            }
            HttpListenerRequest req = ctx.Request;
            HttpListenerResponse resp = ctx.Response;

            // Print out some info about the request
            Console.WriteLine("Request #: {0}", ++requestCount);
            Console.WriteLine(req.Url.ToString());
            Console.WriteLine(req.HttpMethod);
            Console.WriteLine(req.UserHostName);
            Console.WriteLine(req.UserAgent);
            Console.WriteLine();

            // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
            if (req.HttpMethod == "POST")
                HandlePost(req, resp);
            if (req.HttpMethod == "GET")
                HandleGet(req, resp); 
            
        }

        public static void HandlePost(HttpListenerRequest req, HttpListenerResponse resp)
        {
            bool Success = true; 
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(req.InputStream))
            {
                body = reader.ReadToEnd();
            }
            switch (req.Url.AbsolutePath.ToLower())
            {
                case "/maptexture":
                    var tmr = new TextureMapRequest(body);
                    tmr.SaveData();
                    break;
                case "/mapxml":
                    var xmr = new XmlMapRequest(body);
                    xmr.SaveData(); 
                    break;
            }
            if (req.HasEntityBody)
            {

            }
            byte[] bytes = Encoding.UTF8.GetBytes(Success ? "1" : "0"); 
            resp.ContentType = "text/html";
            resp.ContentEncoding = Encoding.UTF8;
            resp.ContentLength64 = bytes.Length;

            resp.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            resp.Close(); 
        }

        public static void HandleGet(HttpListenerRequest req, HttpListenerResponse resp)
        {
            if (req.Url.AbsolutePath != "/favicon.ico")
                pageViews += 1;

            // Write the response info
            byte[] data = Encoding.UTF8.GetBytes(String.Format(pageData, pageViews));
            resp.ContentType = "text/html";
            resp.ContentEncoding = Encoding.UTF8;
            resp.ContentLength64 = data.LongLength;

            // Write out to the response stream (asynchronously), then close it
            resp.OutputStream.WriteAsync(data, 0, data.Length);
            resp.Close();
        }
    }
}
