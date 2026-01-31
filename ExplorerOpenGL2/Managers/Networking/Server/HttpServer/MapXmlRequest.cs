using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace GameServerTCP.HttpServer
{
    public class MapXmlRequest
    {
        public string xml { get; set; }
        public string mapName { get; set; }
        public MapXmlRequest(string body)
        {
            MapXmlRequest mxr = JsonSerializer.Deserialize<MapXmlRequest>(body);
            xml = mxr.xml;
            mapName= mxr.mapName;
        }
        public MapXmlRequest()
        {

        }
    }
}
