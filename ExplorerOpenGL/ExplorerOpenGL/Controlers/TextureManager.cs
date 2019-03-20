using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers
{
    public class TextureManager
    {
        private GraphicsDeviceManager graphics;
        private ContentManager Content; 

        public TextureManager(GraphicsDeviceManager Graphics, ContentManager content)
        {
            Content = content; 
            graphics = Graphics; 
        }
        public Texture2D CreateTexture(int width, int height, Func<int, Color> paint)
        {
            Texture2D texture = new Texture2D(graphics.GraphicsDevice, width, height);

            Color[] data = new Color[width * height];
            for (int pixel = 0; pixel < data.Count(); pixel++)
            {
                data[pixel] = paint(pixel);
            }
            texture.SetData(data);

            return texture;
        }

        public Texture2D LoadNoneContentLoadedTexture(string path)
        {
            var image = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(path);

            int ImageWidth = image.Width;
            int ImageHeight = image.Height;

            Texture2D texture = new Texture2D(graphics.GraphicsDevice, ImageWidth, ImageHeight);

            Color[] data = new Color[ImageWidth * ImageHeight];
            
            for(int i = 0; i< ImageWidth; i++)
            {
                for(int j = 0; j < ImageHeight; j ++)
                {
                    data[ImageWidth * j + i] = SystemDrawingColorToXnaColor(image.GetPixel(i, j));  
                }
            }
            texture.SetData(data); 

            return texture; 
        }


        private Color SystemDrawingColorToXnaColor( System.Drawing.Color color)
        {
            return new Color(color.R, color.G, color.B);
        }

        public Texture2D LoadTexture(string path)
        {
            return Content.Load<Texture2D>(path);
        }
    }
}
