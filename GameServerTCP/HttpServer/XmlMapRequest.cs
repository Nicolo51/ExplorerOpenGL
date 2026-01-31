using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace GameServerTCP.HttpServer
{
    public class XmlMapRequest
    {
        public string Xml { get; set; }
        public string FileName { get; set; }
        public XmlMapRequest(string body)
        {
            XmlMapRequest tmr = JsonSerializer.Deserialize<XmlMapRequest>(body);
            Xml = tmr.Xml;
            FileName = tmr.FileName;
            GameServer.Log("Received " + FileName + " map.");
        }

        public XmlMapRequest()
        {

        }

        public void SaveData()
        {
            if (!Directory.Exists($"./maps/{FileName.Substring(0, FileName.Length-4)}"))
                Directory.CreateDirectory($"./maps/{FileName.Substring(0, FileName.Length - 4)}");

            byte[] data = Encoding.UTF8.GetBytes(Xml); 
            using (var file = new FileStream($"./maps/{FileName}", FileMode.Create))
            {
                file.Write(data, 0, data.Length);
                file.Flush();
            }
        }
    }
}
