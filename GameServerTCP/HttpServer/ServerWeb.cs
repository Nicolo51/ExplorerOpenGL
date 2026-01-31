using Model;
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
        private static string currentMap = "default"; 
        static Task listenTask; 

        public static void Start(int p = 8000)
        {
            port = p; 
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            listener.BeginGetContext(HandleIncomingConnections, null);
            GameServer.Log($"Http Server listening at {url}");

            // Handle requests
            //listenTask = HandleIncomingConnections();
            
        }

        public static bool ChangeCurrentMap(string map)
        {
            if (File.Exists("./maps/" + map + ".xml"))
            {
                currentMap = map;
                GameServer.Log($"Map has been changed to {map} successfully !");
                return true;
            }
            else
            {
                GameServer.Log($"Map does not exist.");
                return false;
            }
        }

        public static void SetOnlineMap(string map)
        {
            if (string.IsNullOrWhiteSpace(map))
                return; 
            if(!File.Exists($"./map/{map}.xml"))
                return;
            currentMap = map;
        } 

        public static void Reset()
        {
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            listener.BeginGetContext(HandleIncomingConnections, null);
            GameServer.Log("Http server reseted.");
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
            //GameServer.Log(req.Url.ToString());
            //GameServer.Log(req.HttpMethod);
            //GameServer.Log(req.UserHostName);
            //GameServer.Log(req.UserAgent);

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
                case "/changemap":
                    var cmr = new ChangeMapRequest(body); 
                    ChangeCurrentMap(cmr.MapName); 
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
            byte[] data = null; 
            if (req.Url.AbsolutePath == "/favicon.ico")
            {
                resp.Close();
                return; 
            }

            string[] URL = req.Url.AbsolutePath.Split("/");
            if(URL.Length < 2)
            {
                resp.Redirect("www.google.com"); 
            }
            switch (URL[1].ToLower())
            {
                case "currentmap":
                    data = Encoding.UTF8.GetBytes(currentMap);
                    break; 
                case "map":
                    if(!File.Exists($"./maps/{URL[2]}.xml"))
                    {
                        resp.StatusCode = 404;
                        data = Encoding.UTF8.GetBytes("404 not found. This file does not exist.");
                        break; 
                    }
                    data = Encoding.UTF8.GetBytes(File.ReadAllText($"./maps/{URL[2]}.xml")); 
                    break;
                case "texture": //URL 1 texture | URL 2 mapname | URL 3 nomtexture
                    if(URL.Length != 4) 
                    {
                        resp.StatusCode = 400;
                        data = Encoding.UTF8.GetBytes("400 bad resquest. Argument does not match expected data.");
                        break;
                    }
                    if (!File.Exists($"./maps/{URL[2]}/{URL[3]}"))
                    {
                        resp.StatusCode = 404;
                        data = Encoding.UTF8.GetBytes("404 not found. This file does not exist.");
                        break;
                    }
                    data = File.ReadAllBytes($"./maps/{URL[2]}/{URL[3]}");
                    break;
                default:
                    data = Encoding.UTF8.GetBytes("404 not found.");
                    break;

            }
            // Write the response info
            resp.ContentType = "text";
            resp.ContentEncoding = Encoding.UTF8;
            resp.ContentLength64 = data.LongLength;

            // Write out to the response stream (asynchronously), then close it
            resp.OutputStream.WriteAsync(data, 0, data.Length);
            resp.Close();
        }
    }
}
