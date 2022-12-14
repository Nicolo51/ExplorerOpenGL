using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.Json;

namespace GameServerTCP.HttpServer
{
    public class TextureMapRequest
    {
        public string image { get; set; }
        public string textureName { get; set; }
        public string mapName { get; set; }
        public TextureMapRequest(string body)
        {
            TextureMapRequest tmr = JsonSerializer.Deserialize<TextureMapRequest>(body);
            image = tmr.image;
            textureName = tmr.textureName;
            mapName = tmr.mapName; 
        }

        public TextureMapRequest()
        {

        }

        public void SaveData()
        {
            if (!Directory.Exists($"./maps/{mapName}"))
                Directory.CreateDirectory($"./maps/{mapName}");

            byte[] imageBytes = Convert.FromBase64String(image);
            using (var imageFile = new FileStream($"./maps/{mapName}/{textureName}", FileMode.Create))
            {
                imageFile.Write(imageBytes, 0, imageBytes.Length);
                imageFile.Flush();
            }
        }
    }
}
