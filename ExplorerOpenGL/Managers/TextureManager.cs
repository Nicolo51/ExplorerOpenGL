using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers
{
    public class TextureManager
    {
        private GraphicsDeviceManager graphics;
        private ContentManager content;
        private SpriteBatch spriteBatch;

        private RenderManager renderManager;
        private FontManager fontManager;

        public Dictionary<string, Texture2D> LoadedTextures;

        public Dictionary<int, Texture2D> IssuedTextures;
        public Dictionary<int, OutlineTextRenderArgs> OutlineTextToDraw; 

        private static TextureManager instance;

        public static event EventHandler Initialized; 

        public static TextureManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TextureManager();
                    Initialized?.Invoke(instance, EventArgs.Empty);
                    return instance;
                }
                return instance;
            }
        }

        public TextureManager()
        {
            LoadedTextures = new Dictionary<string, Texture2D>();
            IssuedTextures = new Dictionary<int, Texture2D>();
            OutlineTextToDraw = new Dictionary<int, OutlineTextRenderArgs>(); 
        }

        public void Update()
        {
            lock (OutlineTextToDraw)
            {
                for (int i = 0; i < OutlineTextToDraw.Count; i++)
                {
                    OutlineTextRenderArgs ra = OutlineTextToDraw[i];
                    Texture2D texture = OutlineText(ra.Input, ra.Font, ra.BorderColor, ra.Color, ra.OutlineThickness);
                    IssuedTextures[ra.ID] = texture;

                }
                OutlineTextToDraw.Clear(); 
            }
        }

        public void InitDependencies(GraphicsDeviceManager graphics, ContentManager content, SpriteBatch spriteBatch)
        {
            this.content = content;
            this.spriteBatch = spriteBatch; 
            this.graphics = graphics;
            renderManager = RenderManager.Instance;
            fontManager = FontManager.Instance;
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

        public Texture2D CreateBorderedTextureMainThread(int width, int height, int thickness, int distanceBorder, Func<int, Color> borderPaint, Func<int, Color> backgroundPaint)
        {
            Texture2D texture = new Texture2D(graphics.GraphicsDevice, width, height);
            Color[] data = new Color[width * height];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
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

                                if (index > data.Length - 1 || index < 0)
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
        public Texture2D CreateBorderedTexture(int width, int height, int thickness, int distanceBorder, Func<int, Color> borderPaint, Func<int, Color> backgroundPaint)
        {
            int idTexture;
            for (idTexture = 0; IssuedTextures.ContainsKey(idTexture); idTexture++) ;
            //Add Ticket
            

            while (IssuedTextures[idTexture] == null) ;
            Texture2D output = IssuedTextures[idTexture];
            IssuedTextures.Remove(idTexture);
            return output;
        }

        public Texture2D LoadNoneContentLoadedTexture(string path)
        {
            var image = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(path);

            int ImageWidth = image.Width;
            int ImageHeight = image.Height;

            Texture2D texture = new Texture2D(graphics.GraphicsDevice, ImageWidth, ImageHeight);

            Color[] data = new Color[ImageWidth * ImageHeight];

            //Parallel.For(0, ImageWidth, i => Parallel.For(0, ImageHeight, j => { lock (data) { data[ImageWidth * j + i] = SystemDrawingColorToXnaColor(image.GetPixel(i, j)); } }));

            for (int i = 0; i < ImageWidth; i++)
            {
                for (int j = 0; j < ImageHeight; j++)
                {
                    data[ImageWidth * j + i] = SystemDrawingColorToXnaColor(image.GetPixel(i, j));
                }
            }
            texture.SetData(data);
            LoadedTextures.Add(path, texture);
            return texture;
        }


        private Color SystemDrawingColorToXnaColor(System.Drawing.Color color)
        {
            return new Color(color.R, color.G, color.B);
        }

        public Texture2D LoadTexture(string path)
        {
            if (LoadedTextures.ContainsKey(path))
            {
                return LoadedTextures[path]; 
            }
            Texture2D texture = content.Load<Texture2D>(path);
            LoadedTextures.Add(path, texture);
            return texture; 
        }

        public Texture2D OutlineTextThread(string input, string font, Color borderColor, Color textColor, int Thickness)
        {
            int idTexture;
            lock (OutlineTextToDraw)
            {
                lock (IssuedTextures)
                {
                    for (idTexture = 0; IssuedTextures.ContainsKey(idTexture) || OutlineTextToDraw.ContainsKey(idTexture); idTexture++) ;
                    //Add Ticket
                    OutlineTextRenderArgs ra = new OutlineTextRenderArgs()
                    {
                        Input = input,
                        Font = font,
                        Color = textColor,
                        BorderColor = borderColor,
                        OutlineThickness = Thickness,
                        ID = idTexture,
                    };

                    OutlineTextToDraw.Add(idTexture, ra);
                    IssuedTextures.Add(idTexture, null);
                }
            }
            while (IssuedTextures[idTexture] == null) ;
            Texture2D output = IssuedTextures[idTexture];
            IssuedTextures.Remove(idTexture);
            return output;
        }

        public Texture2D OutlineText(string input, string font, Color borderColor, Color textColor, int Thickness)
        {
            Texture2D textTexture = renderManager.RenderTextToTexture(input, fontManager.GetFont(font), textColor, Thickness);
            Vector2 stringDimension = new Vector2(textTexture.Width, textTexture.Height);
            Color[] data = new Color[textTexture.Width* textTexture.Height];

            spriteBatch.GraphicsDevice.SetRenderTarget(null);
            textTexture.GetData(data);

            List<int> contour = new List<int>();
            for (int x = 0; x < (int)stringDimension.X; x++)
            {
                for (int y = 0; y < (int)stringDimension.Y; y++)
                {
                    int coord = y * (int)stringDimension.X + x;
                    if (data[coord] != Color.Transparent)
                    {
                        data[coord] = textColor;
                    }
                }// Remove Antialiasing.
            }
            for (int i = 0; i < Thickness; i++)
            {
                for (int x = 0; x < (int)stringDimension.X; x++)
                {
                    for (int y = 0; y < (int)stringDimension.Y; y++)
                    {
                        int coord = y * (int)stringDimension.X + x;
                        if (data[coord] == Color.Transparent)
                        {
                            if (IsColoredPixelsArround(data, coord, (int)stringDimension.X, Thickness))
                            {
                                contour.Add(coord);
                            }
                        }
                    }
                }

                foreach (int coord in contour)
                {
                    data[coord] = borderColor;
                }
            }
            textTexture.SetData(data);
            data = null;

            return textTexture;

        }

        private bool IsColoredPixelsArround(Color[] data, int coord, int length, int thickness)
        {
            if (data.Length <= coord + 1 || 0 >= coord - 1 || 0 >= coord - length || data.Length <= coord + length)
            {
                return false; 
            }

            if (data[coord + 1] != Color.Transparent || data[coord - 1] != Color.Transparent || data[coord + length] != Color.Transparent || data[coord - length] != Color.Transparent)
            {
                return true; 
            }
            return false;
        }

        public Texture2D TextureText(string text, string font, Color color)
        {
            return renderManager.RenderTextToTexture(text, fontManager.GetFont(font), color, 0);
        }
    }
    public struct OutlineTextRenderArgs
    {
        public int ID { get; set; }
        public string Input { get; set; }
        public string Font { get; set; }
        public Color Color { get; set; }
        public Color BorderColor { get; set; }
        public int OutlineThickness { get; set; }
    }
}