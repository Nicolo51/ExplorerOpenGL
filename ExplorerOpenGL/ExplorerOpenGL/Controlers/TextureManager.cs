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
        private SpriteBatch spriteBatch; 

        public TextureManager(GraphicsDeviceManager Graphics, ContentManager content, SpriteBatch spriteBatch)
        {
            Content = content; 
            graphics = Graphics;
            this.spriteBatch = spriteBatch;
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

        public Texture2D CreateBorderedTexture(int width, int height, int thickness, int distanceBorder, Func<int, Color> borderPaint, Func<int, Color> backgroundPaint)
        {
            Texture2D texture = new Texture2D(graphics.GraphicsDevice, width, height);
            Color[] data = new Color[width * height];

            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                   data[i * width + j] = backgroundPaint(i * width + j);
                }
            }
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if ((i == distanceBorder && (j > distanceBorder && j < width - 1 - distanceBorder)) || (i == height - 1 - distanceBorder && (j > distanceBorder && j < width - 1 - distanceBorder)) || (j == distanceBorder && (i > distanceBorder && i < height - 1 - distanceBorder)) || (j == width - 1 - distanceBorder && (i > distanceBorder && i < height - 1 - distanceBorder)))
                    {
                        for (int x = 0 - thickness / 2; x < thickness / 2; x++)
                        {
                            for (int y = 0 - thickness / 2; y < thickness / 2; y++)
                            {
                                int index = i * width + j + y + (x * width);

                                if (index > data.Length-1 || index < 0)
                                    continue;

                                data[index] = borderPaint(i * width + j);
                            }
                        }
                    }
                }
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

        public Texture2D OutlineText(string input, SpriteFont font, Color borderColor, Color textColor, int Thickness)
        {
            Vector2 stringDimension = font.MeasureString(input);
            Texture2D texture = new Texture2D(graphics.GraphicsDevice, (int)stringDimension.X, (int)stringDimension.Y);

            RenderTarget2D target = new RenderTarget2D(
                graphics.GraphicsDevice,
                (int)stringDimension.X,
                (int)stringDimension.Y,
                false,
                graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            Color[] data = new Color[(int)stringDimension.X * (int)stringDimension.Y];

            spriteBatch.GraphicsDevice.SetRenderTarget(target);
            spriteBatch.GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };

            spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred,
                              BlendState.AlphaBlend,
                              SamplerState.PointClamp,
                              null, null, null, null);
        
            spriteBatch.DrawString(font, input, Vector2.Zero, textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            spriteBatch.End(); 
            target.GetData(data);

            spriteBatch.GraphicsDevice.SetRenderTarget(null);

            List<int> contour = new List<int>();

            for (int x = 0; x < (int)stringDimension.X; x++)
            {
                for (int y = 0; y < (int)stringDimension.Y; y++)
                {
                    int coord = y * (int)stringDimension.X + x; 
                    if (data[coord] != Color.Transparent && GetPiexelArround(data, coord, stringDimension,Thickness))
                    {
                        contour.Add(coord);
                    }
                }
            }

            //for (int x = 0; x < (int)stringDimension.X; x++)
            //{
            //    for( int y = 0; y < (int)stringDimension.Y; y++)
            //    {
            //        int pos = y * (int)stringDimension.X + x;
            //        int row = (int)stringDimension.X;

                    

            //        if ( data[pos] == textColor)
            //        {
            //            if (!(pos - 1 < 0) && !(pos + 1 > data.Length) && !(pos - row * Thickness < 0) && !(pos + row * Thickness > data.Length))
            //            {
            //                if (data[pos + 1] == Color.Transparent)
            //                {
            //                    data[pos + 1] = borderColor;
            //                }
            //                if (data[pos - 1] == Color.Transparent )
            //                {
            //                    data[pos - 1] = borderColor;
            //                }
            //                if (data[pos + row] == Color.Transparent)
            //                {
            //                    data[pos + row] = borderColor;
            //                }
            //                if (data[pos - row] == Color.Transparent)
            //                {
            //                    data[pos - row] = borderColor;
            //                }
            //            }
            //            //if (!(pos - row * Thickness < 0) && !(pos + row * Thickness > data.Length))
                        //{
                        //    if (data[pos + row * Thickness] == Color.Transparent && data[pos] == textColor)
                        //    {
                        //        for (int i = 0; i < Thickness; i++)
                        //        {
                        //            data[pos + row * i] = borderColor;
                        //        }
                        //    }
                        //    if (data[pos - row * Thickness] == Color.Transparent && data[pos] == textColor)
                        //    {
                        //        for (int i = 0; i < Thickness; i++)
                        //        {
                        //            data[pos - row * i] = borderColor;
                        //        }
                        //    }
                        //}
                    }

        private bool IsColoredPixelsArround(Color[] data, int coord, Vector2 stringDimension, int thickness)
        {
            bool output = false;
            for(coord)
        }
    }
            }

            texture.SetData(data);

            target.Dispose(); 

            data = null;

            return texture;
        }
    }
}
