using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

namespace Client
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.ReadLine(); 
            HttpClient client = new HttpClient();
            byte[] imageArray = System.IO.File.ReadAllBytes("test.jpg");
            string base64Image = Convert.ToBase64String(imageArray);
            StringContent content = new StringContent(JsonSerializer.Serialize(
                new
                {
                    image = base64Image,
                    textureName = "test.png",
                    mapName = "test",
                }
                )) ;
            Task<HttpResponseMessage> task = client.PostAsync("http://localhost:8000/MapTexture", content);

            while (!task.IsCompleted)
                Thread.Sleep(1);
            Console.ReadLine(); 
        }
    }
}