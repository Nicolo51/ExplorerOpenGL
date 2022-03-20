using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
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
        private GameManager gameManager;

        private Dictionary<string, Texture2D> LoadedTextures;

        private Dictionary<int, Texture2D> IssuedTextures;

        private Dictionary<int, OutlineTextRenderArgs> OutlineTextToDraw;
        private Dictionary<int, CreateBorderTextureRenderArgs> CreateBorderTextureToDraw;
        private Dictionary<int, CreateTextureRenderArgs> CreateTextureToDraw;
        private Dictionary<int, TextToTextureRenderArgs> TextToTextureToDraw;

        public static event EventHandler Initialized;

        private static TextureManager instance;

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
            TextToTextureToDraw = new Dictionary<int, TextToTextureRenderArgs>();
            CreateTextureToDraw = new Dictionary<int, CreateTextureRenderArgs>();
            CreateBorderTextureToDraw = new Dictionary<int, CreateBorderTextureRenderArgs>();
        }

        public void Update()
        {
            lock (OutlineTextToDraw)
            {
                if (OutlineTextToDraw.Count > 0)
                {
                    foreach (KeyValuePair<int, OutlineTextRenderArgs> entry in OutlineTextToDraw)
                    {
                        OutlineTextRenderArgs ra = entry.Value;
                        Texture2D texture = OutlineTextThread(ra.Input, ra.Font, ra.BorderColor, ra.Color, ra.OutlineThickness);
                        lock (IssuedTextures)
                            IssuedTextures[ra.ID] = texture;
                    }
                    OutlineTextToDraw.Clear();
                }
            }
            lock (CreateBorderTextureToDraw)
            {
                if (CreateBorderTextureToDraw.Count > 0)
                {
                    foreach (KeyValuePair<int, CreateBorderTextureRenderArgs> entry in CreateBorderTextureToDraw)
                    {
                        CreateBorderTextureRenderArgs ra = entry.Value;
                        Texture2D texture = CreateBorderedTextureThread(ra.Width, ra.Height, ra.Thickness, ra.DistanceToBorder, ra.BorderPaint, ra.BackgroundPaint);
                        lock (IssuedTextures)
                            IssuedTextures[ra.ID] = texture;
                    }
                    CreateBorderTextureToDraw.Clear();
                }
            }
            lock (CreateTextureToDraw)
            {
                if (CreateTextureToDraw.Count > 0)
                {
                    foreach (KeyValuePair<int, CreateTextureRenderArgs> entry in CreateTextureToDraw)
                    {
                        CreateTextureRenderArgs ra = entry.Value;
                        Texture2D texture = CreateTextureThread(ra.Width, ra.Height, ra.paint);
                        lock (IssuedTextures)
                            IssuedTextures[ra.ID] = texture;
                    }
                    CreateTextureToDraw.Clear();
                }
            }
            lock (TextToTextureToDraw)
            {
                if (TextToTextureToDraw.Count > 0)
                {
                    foreach (KeyValuePair<int, TextToTextureRenderArgs> entry in TextToTextureToDraw)
                    {
                        TextToTextureRenderArgs ra = entry.Value;
                        Texture2D texture = TextureTextThread(ra.Text, ra.Font, ra.Color);
                        //texture = OutlineTextThread(ra.Text, ra.Font, Color.Transparent, ra.Color, 0);
                        lock (IssuedTextures)
                            IssuedTextures[ra.ID] = texture;
                    }
                    TextToTextureToDraw.Clear();
                }
            }
        }
        public void InitDependencies(GraphicsDeviceManager graphics, ContentManager content, SpriteBatch spriteBatch)
        {
            this.content = content;
            this.spriteBatch = spriteBatch;
            this.graphics = graphics;
            renderManager = RenderManager.Instance;
            fontManager = FontManager.Instance;
            gameManager = GameManager.Instance; 
        }

        private Texture2D CreateTextureThread(int width, int height, Func<int, Color> paint)
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
        public Texture2D CreateTexture(int width, int height, Func<int, Color> paint)
        {
            if (gameManager.MainThreadID == Thread.CurrentThread.ManagedThreadId)
                return CreateTextureThread(width, height, paint);
                int id = getIdTicket();
            var ra = new CreateTextureRenderArgs()
            {
                ID = id,
                Width = width,
                Height = height,
                paint = paint, 
            };
            lock (CreateTextureToDraw)
                CreateTextureToDraw.Add(id, ra);
            return waitTexture(id);
        }

        private Texture2D CreateBorderedTextureThread(int width, int height, int thickness, int distanceBorder, Func<int, Color> borderPaint, Func<int, Color> backgroundPaint)
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
            if (gameManager.MainThreadID == Thread.CurrentThread.ManagedThreadId)
                return CreateBorderedTextureThread(width, height, thickness, distanceBorder, borderPaint, backgroundPaint);
            int id = getIdTicket();
            var ra = new CreateBorderTextureRenderArgs()
            {
                ID = id,
                Width = width, 
                Height = height, 
                Thickness = thickness, 
                DistanceToBorder = distanceBorder, 
                BorderPaint = borderPaint, 
                BackgroundPaint = backgroundPaint
            };
            lock (CreateBorderTextureToDraw)
                CreateBorderTextureToDraw.Add(id, ra);
            return waitTexture(id);
        }
        private Texture2D OutlineTextThread(string input, string font, Color borderColor, Color textColor, int Thickness)
        {
            Texture2D textTexture = renderManager.RenderTextToTexture(input, fontManager.GetFont(font), textColor, Thickness);
            Vector2 stringDimension = new Vector2(textTexture.Width, textTexture.Height);
            Color[] data = new Color[textTexture.Width * textTexture.Height];

            textTexture.GetData(data);
            spriteBatch.GraphicsDevice.SetRenderTarget(null);

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
        public Texture2D OutlineText(string input, string font, Color borderColor, Color textColor, int Thickness)
        {
            if (gameManager.MainThreadID == Thread.CurrentThread.ManagedThreadId)
                return OutlineTextThread(input, font, borderColor, textColor, Thickness);
            int id = getIdTicket();
            var ra = new OutlineTextRenderArgs()
            {
                BorderColor = borderColor, 
                Input = input, 
                Color = textColor, 
                OutlineThickness = Thickness, 
                ID = id,
                Font = font,
            };
            lock (OutlineTextToDraw)
                OutlineTextToDraw.Add(id, ra);
            return waitTexture(id);
        }
        private Texture2D TextureTextThread(string text, string font, Color color)
        {
            return renderManager.RenderTextToTexture(text, fontManager.GetFont(font), color, 0);
        }
        public Texture2D TextureText(string text, string font, Color color)
        {
            if (gameManager.MainThreadID == Thread.CurrentThread.ManagedThreadId)
                return TextureTextThread(text, font, color);
            int id = getIdTicket();
            var ra = new TextToTextureRenderArgs()
            {
                Color = color,
                ID = id,
                Font = font,
                Text = text,
            };
            lock (TextToTextureToDraw)
                TextToTextureToDraw.Add(id, ra);
            return waitTexture(id); 
        }

        private int getIdTicket()
        {
            int i = 0; 
            lock (IssuedTextures)
            {
                for (i = 0; IssuedTextures.ContainsKey(i); i++) ;
                IssuedTextures.Add(i, null);
            }
            return i; 
        }

        private Texture2D waitTexture(int id)
        {
            KeyValuePair<int, Texture2D> valuePair = new KeyValuePair<int, Texture2D>(); 
            while(valuePair.Value == null)
            {
                lock (IssuedTextures)
                {
                    valuePair = new KeyValuePair<int, Texture2D>(id, IssuedTextures[id]); 
                }
                Thread.Sleep(1); 
            }
            lock (IssuedTextures)
                IssuedTextures.Remove(id);
            return valuePair.Value; 
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
                return LoadedTextures[path];

            Texture2D texture = content.Load<Texture2D>(path);
            LoadedTextures.Add(path, texture);
            return texture;
        }

    }
    public class OutlineTextRenderArgs
    {
        public int ID { get; set; }
        public string Input { get; set; }
        public string Font { get; set; }
        public Color Color { get; set; }
        public Color BorderColor { get; set; }
        public int OutlineThickness { get; set; }
    }
    public class CreateTextureRenderArgs
    { 
        public int ID { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Func<int, Color> paint { get; set; }
    }
    public class CreateBorderTextureRenderArgs
    {
        public int ID { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Thickness { get; set; }
        public int DistanceToBorder { get; set; }
        public Func<int, Color> BorderPaint { get; set; }
        public Func<int, Color> BackgroundPaint { get; set; }
    }
    public class TextToTextureRenderArgs
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public string Font { get; set; }
        public Color Color { get; set; }
    }
}
