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

        public Dictionary<string, Texture2D> LoadedTextures;

        public Dictionary<int, Texture2D> IssuedTextures;

        public Dictionary<int, OutlineTextRenderArgs> OutlineTextToDraw;
        public Dictionary<int, CreateBorderTextureRenderArgs> CreateBorderTextureToDraw;
        public Dictionary<int, CreateTextureRenderArgs> CreateTextureToDraw;
        public Dictionary<int, TextToTextureRenderArgs> TextToTextureToDraw;

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
        }
        public void InitDependencies(GraphicsDeviceManager graphics, ContentManager content, SpriteBatch spriteBatch)
        {
            this.content = content;
            this.spriteBatch = spriteBatch;
            this.graphics = graphics;
            renderManager = RenderManager.Instance;
            fontManager = FontManager.Instance;
        }

        public void CreateTexture(int width, int height, Func<int, Color> paint)
        {
            int idTexture;
            eToDraw)
{
                lock (IssuedTextures)
                {
                    for (idTexture = 0; IssuedTextures.ContainsKey(idTexture); idTexture++) ;
                    CreateTextureRenderArgs ra = new CreateTextureRenderArgs()
                    {
                        ID = idTexture,
                        Height = height,
                        Width = width,
                        paint = paint,
                        Callback = callback,
                    };
                    raidTexture, );
                    IssuedTextures.Add(idTexture, null);
                }

            }
        }
        public Texture2D CreateTextureThread(int width, int hwhile (IssuedTextures[idTexture] == null);
        Texture2D output = IssuedTextures[idTexture];
lock (IssuedTextures)
IssuedTextures.Remove(idTexture);
return output;
eight, Func<int, Color> paint)
{
Texture2D texture = new Texture2D(graphics.GraphicsDevice, width, height);

        Color[] data = new Color[width * height];
for (int pixel = 0; pixel<data.Count(); pixel++)
{
data[pixel] = paint(pixel);
    }
    texture.SetData(data);

return texture;
}

public Texture2D CreateBorderedTextureThread(int width, int height, int thickness, int distanceBorder, Func<int, Color> borderPaint, Func<int, Color> backgroundPaint)
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
public void CreateBorderedTexture(int width, int height, int thickness, int disTexture2DeBorder, Func<int, Color> borderPaint, Func<int, Color> backgroundPaint, Action<Texture2D> callback)
{
    int idTexture;
    lock (CreateBorderTextureToDraw)
    {
        lock (IssuedTextures)
        {
            for (idTexture = 0; IssuedTextures.ContainsKey(idTexture); idTexture++) ;
            CreateBorderTextureRenderArgs ra = new CreateBorderTextureRenderArgs()
            {
                ID = idTexture,
                BackgroundPaint = backgroundPaint,
                BorderPaint = borderPaint,
                DistanceToBorder = distanceBorder,
                Height = height,
                Width = width,
                Thickness = thickness,
                Callback = callback,
            };
            w.Add(ridTexture, a);
            IssuedTextures.Add(idTexture, null);
        }
    }
}

public Texture2D LoadNoneContentLoadedTextureThreadwhile(IssuedTextures[idTexture] == null);
Texture2D output = IssuedTextures[idTexture];
lock(IssuedTextures)
IssuedTextures.Remove(idTexture);
return output;
(string path)
{
var image = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(path);

int ImageWidth = image.Width;
int ImageHeight = image.Height;

Texture2D texture = new Texture2D(graphics.GraphicsDevice, ImageWidth, ImageHeight);

Color[] data = new Color[ImageWidth * ImageHeight];

//Parallel.For(0, ImageWidth, i => Parallel.For(0, ImageHeight, j => { lock (data) { data[ImageWidth * j + i] = SystemDrawingColorToXnaColor(image.GetPixel(i, j)); } }));

for (int i = 0; i<ImageWidth; i++)
{
for (int j = 0; j<ImageHeight; j++)
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




public void OutlineText(string input, string font, Color borderColor, Color texTexture2Dor, int Thickness, Action<Texture2D> callback)
{
    int idTexture;
    oDraw)
{
        lock (IssuedTextures)
        {
            for (idTexture = 0; IssuedTextures.ContainsKey(idTexture); idTexture++) ;
            OutlineTextRenderArgs ra = new OutlineTextRenderArgs()
            {
                Input = input,
                Font = font,
                Color = textColor,
                BorderColor = borderColor,
                OutlineThickness = Thickness,
                ID = idTexture,
                Callback = callback,
            };
            idTexture,a);
            IssuedTextures.Add(idTexture, null);
        }
    }
}
public Texture2D OutlineTextThread(string input, striwhile (IssuedTextures[idTexture] == null);
Texture2D output = IssuedTextures[idTexture];
lock(IssuedTextures)
IssuedTextures.Remove(idTexture);
return output;
ng font, Color borderColor, Color textColor, int Thickness)
{
Texture2D textTexture = renderManager.RenderTextToTexture(input, fontManager.GetFont(font), textColor, Thickness);
Vector2 stringDimension = new Vector2(textTexture.Width, textTexture.Height);
Color[] data = new Color[textTexture.Width * textTexture.Height];

textTexture.GetData(data);

List<int> contour = new List<int>();
for (int x = 0; x<(int)stringDimension.X; x++)
{
for (int y = 0; y<(int)stringDimension.Y; y++)
{
int coord = y * (int)stringDimension.X + x;
if (data[coord] != Color.Transparent)
{
data[coord] = textColor;
}
}// Remove Antialiasing.
}
for (int i = 0; i<Thickness; i++)
{
for (int x = 0; x<(int)stringDimension.X; x++)
{
for (int y = 0; y<(int)stringDimension.Y; y++)
{
int coord = y * (int)stringDimension.X + x;
if (data[coord] == Color.Transparent)
{
if (IsColoredPixelsArround(data, coord, (int) stringDimension.X, Thickness))
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
public void TextureText(string text, string font, Color color, Action<Texture2DTexture2Dllback)
{
    int idTexture;
    eToDraw)
{
        lock (IssuedTextures)
        {
            for (idTexture = 0; IssuedTextures.ContainsKey(idTexture); idTexture++) ;
            TextToTextureRenderArgs ra = new TextToTextureRenderArgs()
            {
                ID = idTexture,
                Text = text,
                Font = font,
                Color = color,
                Callback = callback,
            };

            idTexture, );
            IssuedTextures.Add(idTexture, null);
        }
    }
}
public Texture2D TextureTextThread(string text, strinwhile (IssuedTextures[idTexture] == null);
Texture2D output = IssuedTextures[idTexture];
lock (IssuedTextures)
IssuedTextures.Remove(idTexture);
return output;
g font, Color color)
{
Texture2D output = renderManager.RenderTextToTexture(text, fontManager.GetFont(font), color, 0);
return output;
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
    public Action<Texture2D> Callback { get; set; }
}
pu
 int ID { get; set; }
public int Width { get; set; }
public int Height { get; set; }
public Func<int, Color> paint { get; set; }
public Action<Texture2D> Callback { get; set; }
}
public class CreateBorderTextureRendecArgs
{
    public int ID { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Thickness { get; set; }
    public int DistanceToBorder { get; set; }
    public Func<int, Color> BorderPaint { get; set; }
    public Func<int, Color> BackgroundPaint { get; set; }
    public Action<Texture2D> Callback { get; set; }

}
public class TextToTextureRenderArgc
{
    public int ID { get; set; }
    public string Text { get; set; }
    public string Font { get; set; }
    public Color Color { get; set; }
    public Action<Texture2D> Callback { get; set; }

}
}