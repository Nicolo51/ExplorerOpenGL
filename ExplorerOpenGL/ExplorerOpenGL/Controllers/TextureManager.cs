using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controllers
{
    public class TextureManager
    {
        public GraphicsDeviceManager graphics; 
        public TextureManager(GraphicsDeviceManager Graphics)
        {
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
    }
}
