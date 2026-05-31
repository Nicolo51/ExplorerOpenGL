using ExplorerOpenGL2.Model;
using ExplorerOpenGL2.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace ExplorerOpenGL2.Managers
{
    public class TextureManager
    {
        static GraphicsDeviceManager Graphics;
        static ContentManager Content;
        static SpriteBatch SpriteBatch;

        static Dictionary<string, Texture2D> LoadedTextures = new Dictionary<string, Texture2D>();
        static Dictionary<string, Animation> Animations = new Dictionary<string, Animation>();

        static Dictionary<int, Texture2D> IssuedTextures = new Dictionary<int, Texture2D>();
        static Dictionary<int, OutlineTextRenderArgs> OutlineTextToDraw = new Dictionary<int, OutlineTextRenderArgs>();
        static Dictionary<int, CreateBorderTextureRenderArgs> CreateBorderTextureToDraw = new Dictionary<int, CreateBorderTextureRenderArgs>();
        static Dictionary<int, CreateTextureRenderArgs> CreateTextureToDraw = new Dictionary<int, CreateTextureRenderArgs>();
        static Dictionary<int, TextToTextureRenderArgs> TextToTextureToDraw = new Dictionary<int, TextToTextureRenderArgs>();
        static Dictionary<int, LoadTextureRenderArgs> LoadTextureToDraw = new Dictionary<int, LoadTextureRenderArgs>();

        public static Texture2D DefaultTextInputTexture { get; private set; }

        public static bool WaitForRendering { get { return (IssuedTextures.Count > 0 || OutlineTextToDraw.Count > 0 || CreateBorderTextureToDraw.Count > 0 || CreateTextureToDraw.Count > 0 || TextToTextureToDraw.Count > 0 ); } }

        public static void Update()
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
            lock (LoadTextureToDraw)
            {
                if(LoadTextureToDraw.Count > 0)
                {
                    foreach (KeyValuePair<int, LoadTextureRenderArgs> entry in LoadTextureToDraw)
                    {
                        LoadTextureRenderArgs ra = entry.Value;
                        Texture2D texture = LoadTexture(ra.Asset);
                        lock (IssuedTextures)
                            IssuedTextures[ra.ID] = texture;
                    }
                    LoadTextureToDraw.Clear();
                }
            }
        }
        public static void InitDependencies(GraphicsDeviceManager graphics, ContentManager content, SpriteBatch spriteBatch)
        {
            Content = content;
            SpriteBatch = spriteBatch;
            Graphics = graphics;
        }

        public static void InitDefaultTextures()
        {
            DefaultTextInputTexture = CreateTexture(150, 25, paint => Color.Black);
        }

        public static Texture2D CreateAnimationFromTextures(params Texture2D[] textures)
        {
            if (GameManager.MainThreadID == Thread.CurrentThread.ManagedThreadId)
                return CreateAnimationFromTexturesThread(textures);
            return null;
        }

        static Texture2D CreateAnimationFromTexturesThread(Texture2D[] textures)
        {
            int widthOut = 0;
            int heightOut = 0; 

            for(int i = 0; i < textures.Length; i++)
            {
                Texture2D texture = textures[i];
                widthOut += texture.Width;
                if (heightOut < texture.Height)
                    heightOut = texture.Height;
            }

            Texture2D output = new Texture2D(Graphics.GraphicsDevice, widthOut, heightOut);
            Color[] dataOut = new Color[widthOut * heightOut];

            int columnOffset = 0; 
            for (int i = 0; i < textures.Length; i++)
            {
                Texture2D texture = textures[i];
                Color[] data = new Color[texture.Width * texture.Height];
                texture.GetData(data);
                int row = -1; 

                for(int j = 0; j < data.Length; j++)
                {
                    if (j % texture.Width == 0)
                        row++;

                    dataOut[(j % texture.Width)+ columnOffset + widthOut * row] = data[j]; 
                }
                columnOffset += texture.Width; 
            }

            output.SetData(dataOut); 

            return output; 
        }

        static Texture2D CreateTextureThread(int width, int height, Func<int, Color> paint)
        {
            Texture2D texture = new Texture2D(Graphics.GraphicsDevice, width, height);

            Color[] data = new Color[width * height];
            for (int pixel = 0; pixel < data.Count(); pixel++)
            {
                data[pixel] = paint(pixel);
            }
            texture.SetData(data);

            return texture;
        }
        public static Texture2D CreateTexture(int width, int height, Func<int, Color> paint)
        {
            if (GameManager.MainThreadID == Thread.CurrentThread.ManagedThreadId)
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

        static Texture2D CreateBorderedTextureThread(int width, int height, int thickness, int distanceBorder, Func<int, Color> borderPaint, Func<int, Color> backgroundPaint)
        {
            Texture2D texture = new Texture2D(Graphics.GraphicsDevice, width, height);
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
        public static Texture2D CreateBorderedTexture(int width, int height, int thickness, int distanceBorder, Func<int, Color> borderPaint, Func<int, Color> backgroundPaint)
        {
            if (GameManager.MainThreadID == Thread.CurrentThread.ManagedThreadId)
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
        static Texture2D OutlineTextThread(string input, string font, Color borderColor, Color textColor, int Thickness)
        {
            Texture2D textTexture = RenderManager.RenderTextToTexture(input, FontManager.GetFont(font), textColor, Thickness);
            Vector2 stringDimension = new Vector2(textTexture.Width, textTexture.Height);
            Color[] data = new Color[textTexture.Width * textTexture.Height];

            textTexture.GetData(data);
            SpriteBatch.GraphicsDevice.SetRenderTarget(null);

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

        public static Texture2D OutlineText(string input, string font, Color borderColor, Color textColor, int Thickness)
        {
            if (GameManager.MainThreadID == Thread.CurrentThread.ManagedThreadId)
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
        static Texture2D TextureTextThread(string text, SpriteFont font, Color color)
        {
            return RenderManager.RenderTextToTexture(text, font, color, 0);
        }
        public static Texture2D TextureText(string text, string font, Color color)
        {
            return TextureText(text, FontManager.GetFont(font), color); 
        }
        public static Texture2D TextureText(string text, SpriteFont font, Color color)
        {
            if (GameManager.MainThreadID == Thread.CurrentThread.ManagedThreadId)
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

        static int getIdTicket()
        {
            int i = 0; 
            lock (IssuedTextures)
            {
                for (i = 0; IssuedTextures.ContainsKey(i); i++) ;
                IssuedTextures.Add(i, null);
            }
            return i; 
        }

        static Texture2D waitTexture(int id)
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

        static bool IsColoredPixelsArround(Color[] data, int coord, int length, int thickness)
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
        public static Texture2D LoadNoneContentLoadedTexture(string path)
        {
            if (LoadedTextures.ContainsKey(path))
                return LoadedTextures[path];

            SixLabors.ImageSharp.Image imagedata = SixLabors.ImageSharp.Image.Load(path);
            SixLabors.ImageSharp.Image<Rgba32> image = imagedata.CloneAs<Rgba32>();

            int ImageWidth = image.Width;
            int ImageHeight = image.Height;

            Texture2D texture = new Texture2D(Graphics.GraphicsDevice, ImageWidth, ImageHeight);

            Color[] data = new Color[ImageWidth * ImageHeight];

            //Parallel.For(0, ImageWidth, i => Parallel.For(0, ImageHeight, j => { lock (data) { data[ImageWidth * j + i] = SystemDrawingColorToXnaColor(image.GetPixel(i, j)); } }));

            for (int i = 0; i < ImageWidth; i++)
            {
                for (int j = 0; j < ImageHeight; j++)
                {
                    data[ImageWidth * j + i] = ImageSharpToXnaColor(image[i, j]);
                }
            }
            texture.SetData(data);
            LoadedTextures.Add(path, texture);
            image.Dispose(); 
            return texture;
        }


        static Color ImageSharpToXnaColor(Rgba32 color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }

        static Texture2D LoadTextureThread(string path)
        {
            if (LoadedTextures.ContainsKey(path))
                return LoadedTextures[path];

            Texture2D texture = Content.Load<Texture2D>(path);
            LoadedTextures.Add(path, texture);
            return texture;
        }

        public static Texture2D LoadTexture(string path)
        {
            if (GameManager.MainThreadID == Thread.CurrentThread.ManagedThreadId)
                return LoadTextureThread(path);
            int id = getIdTicket();
            var ra = new LoadTextureRenderArgs()
            {
                ID = id,
                Asset = path,
            };
            lock (LoadTextureToDraw)
                LoadTextureToDraw.Add(id, ra);
            return waitTexture(id);
            
        }

        public static Texture2D ScaleTexture(Texture2D texture, int scale)
        {
            Color[] dataout = new Color[texture.Width * texture.Height * scale*scale];
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            int row  = - scale;
            int col = 0; 
            for (int i = 0; i < data.Length; i++)
            {
                col = i % texture.Width; 
                if (col == 0) 
                    row+=scale;
                for(int j = 0; j < scale; j++)
                {
                    for(int k = 0; k < scale; k++)
                    {
                        dataout[row * texture.Width * scale + col * scale + j * texture.Width * scale + k] = data[i];
                        //Debug.WriteLine(row * texture.Width + i * scale + j * texture.Width * scale + k);
                    }
                }
            }

            var textureOut = new Texture2D(Graphics.GraphicsDevice, texture.Width * scale, texture.Height * scale);
            textureOut.SetData(dataout);
            return textureOut;
        }

        public static Animation LoadAnimation(string textureName, int nbrFrame, int looptime, AlignOptions alignOption = AlignOptions.None) 
        {
            string name = textureName.Split('/')[textureName.Split('/').Length-1];
            if (Animations.ContainsKey(textureName))
                return Animations[textureName]; 
            Animation animation = new Animation(LoadTexture(textureName), nbrFrame, looptime, name, alignOption);
            Animations.Add(animation.Name, animation); 
            return animation; 
        }

        public static Texture2D TrimAnimation(Texture2D texture, bool verticalTrim = true, bool debug = false)
        {
            int biggestWidth = 0; 
            //Vertical Trim have to rotate 'cause to dumb not to lol
            Color[] data = new Color[texture.Width * texture.Height];
            List<Color> dataVertical = new List<Color>();
            List<Color> dataVerticalNormalizeSpacing = new List<Color>();

            bool islastColumnBlank = true; 
            
            texture.GetData(data);

            


            int widthSubTexture = 1; 


            for (int j = 0; j < texture.Width; j++)
            {
                bool iscolumnblank = true;
                for (int i = 0; i < data.Length && iscolumnblank; i += texture.Width)
                {
                    if (data[j + i] != Color.Transparent)
                    {
                        var color = data[j + i]; 
                        iscolumnblank = false;
                        if (!islastColumnBlank)
                            widthSubTexture++;
                        islastColumnBlank = false; 
                        for (int k = 0; k < data.Length; k += texture.Width)
                        {
                            dataVertical.Add(data[j + k]);
                        }
                    }
                }
                if(iscolumnblank && !islastColumnBlank)
                {
                    for (int k = 0; k < data.Length && iscolumnblank; k += texture.Width)
                    {
                        dataVertical.Add(data[j + k]);
                    }
                    islastColumnBlank = true;
                }
                if (widthSubTexture > biggestWidth)
                    biggestWidth = widthSubTexture;
                if (iscolumnblank)
                    widthSubTexture = 1; 
            }

            //add normalize horizontal line in rotated texture
            int verticallineAdded = 0; 

            int newWidth = dataVertical.Count / texture.Height; //Height before rotating
            int newHeight = texture.Height; //Width before rotating 

            if (debug == true)
            {
                var fs = File.Create("C:\\Users\\nicol\\Desktop\\rotatetrim.png");
                Texture2D debugTexture = new Texture2D(Graphics.GraphicsDevice, newWidth, newHeight);
                debugTexture.SetData(dataVertical.ToArray());
                debugTexture.SaveAsPng(fs, debugTexture.Height, debugTexture.Width);
                fs.Close();
            }


            int verticalline = 0; 
            widthSubTexture = 1; 
            for(int i = 0; i < newWidth; i++)
            {
                bool isBlank = true;
                for(int j = 0; j < newHeight && isBlank; j++)
                {
                    if(dataVertical[j+ i* newHeight] != Color.Transparent)
                    {
                        for (int k = 0; k < newHeight ; k++)
                        {
                            dataVerticalNormalizeSpacing.Add(dataVertical[k + i * newHeight]);
                        }
                        verticalline++;
                        isBlank = false;
                        widthSubTexture++;
                    }
                }
                if (isBlank)
                {
                    for (int k = 0; k < newHeight; k++)
                    {
                        dataVerticalNormalizeSpacing.Add(dataVertical[k + i * newHeight]);
                    }
                    if (widthSubTexture < biggestWidth)
                    {
                        for(widthSubTexture = widthSubTexture;  widthSubTexture < biggestWidth; widthSubTexture++)
                        {
                            for (int k = 0; k < newHeight; k++)
                            {
                                dataVerticalNormalizeSpacing.Add(Color.Transparent);
                            }
                            verticallineAdded++; 
                        }
                    }
                    widthSubTexture = 1; 
                }
            }

            newWidth += verticallineAdded;

            if (debug == true)
            {
                var fs = File.Create("C:\\Users\\nicol\\Desktop\\rotateNormalizeTrim.png");
                Texture2D debugTexture = new Texture2D(Graphics.GraphicsDevice, newWidth, newHeight);
                debugTexture.SetData(dataVerticalNormalizeSpacing.ToArray());
                debugTexture.SaveAsPng(fs, debugTexture.Height, debugTexture.Width);
                fs.Close();
            }

            //Re Rotate
            Color[] outData = new Color[dataVerticalNormalizeSpacing.Count];
            for(int i = 0; i < newWidth; i++)
            {
                for(int j = 0; j < newHeight; j++)
                {
                    outData[j * newWidth + i] = dataVerticalNormalizeSpacing[j + i * newHeight];
                }
            }

            //DeleteTopAndBot
            int trimedVerticalHeight = newHeight; 
            bool isRowBlank = true; 
            List<Color> trimedVertical = new List<Color>();
            for (int j = 0; j < newHeight; j++)
            {
                isRowBlank = true; 
                for (int i = 0; i < newWidth && isRowBlank; i++)
                {
                    if (outData[i + j * newWidth] != Color.Transparent)
                    {
                        for(int k  = 0; k < newWidth; k++)
                        {
                            trimedVertical.Add(outData[k + j * newWidth]);
                            isRowBlank = false;
                        }
                    }
                }
                if (isRowBlank)
                    trimedVerticalHeight--; 
            }

            var textureOut = new Texture2D(Graphics.GraphicsDevice, newWidth, verticalTrim ? trimedVerticalHeight : newHeight);
            textureOut.SetData(verticalTrim ? trimedVertical.ToArray() : outData);
            return textureOut; 
        }

        public static Animation[] NormalizeHeights(params Animation[] animations)
        {
            int maxHeight = 0;
            maxHeight = (int)animations.Select(e => e.Bounds.Y).Max();
            for (int a = 0; a < animations.Length; a++)
            {
                Animation anim = animations[a];
                if (anim.Bounds.Y == maxHeight)
                    continue;

                int heightMissing = (maxHeight - anim.Texture.Height) * anim.Texture.Width;
                Color[] textureData = new Color[anim.Texture.Width * anim.Texture.Height];
                anim.Texture.GetData(textureData);
                Color[] normalizeTextureData = new Color[textureData.Length + heightMissing];
                for (int i = 0; i < heightMissing; i++)
                {
                    normalizeTextureData[i] = Color.Transparent;
                }
                for (int i = heightMissing; i < normalizeTextureData.Length; i++)
                {
                    normalizeTextureData[i] = textureData[i - heightMissing];
                }
                Texture2D normalizeTexture = new Texture2D(Graphics.GraphicsDevice, anim.Texture.Width, maxHeight);
                normalizeTexture.SetData(normalizeTextureData);
                animations[a] = new Animation(normalizeTexture, anim.FrameCount, anim.LoopTime, anim.Name, anim.AlignOption, anim.IsLooping);
            }
            return animations;
        }

        public static Animation LoadAnimation(Texture2D texture, int nbrFrame, int looptime, string name, AlignOptions alignOption = AlignOptions.None)
        {
            Animation animation = new Animation(texture, nbrFrame, looptime, name, alignOption);
            Animations.Add(animation.Name, animation);
            return animation;
        }
        public static Animation GetAnimation(string name)
        {
            if (Animations.ContainsKey(name))
                return (Animation)Animations[name].Clone();


            return null; 
        }

        public static void SaveTexture(Texture2D t, string path = "./texture.png")
        {
            if(File.Exists(path))
                File.Delete(path);
            var fs = File.Create(path);
            t.SaveAsPng(fs, t.Width, t.Height);
            fs.Close();
        }

        public static byte[] GetTextureBytes(Sprite sprite)
        {
            if(sprite.Texture == null)
                return new byte[0];

            byte[] data = new byte[sprite.Texture.Width * sprite.Texture.Height*4];
            sprite.Texture.GetData(data);

            byte[] output = new byte[(sizeof(int) * 2) + data.Length];
            byte[] width = BitConverter.GetBytes(sprite.Texture.Width);
            byte[] height = BitConverter.GetBytes(sprite.Texture.Height);


            Array.Copy(data, 0, output, sizeof(int) * 2, data.Length); 
            Array.Copy(width, 0, output, 0, width.Length); 
            Array.Copy(height, 0, output, sizeof(int), height.Length);

            SaveTexture(GetTextureFromBytes(output)); 

            return output;
        }

        public static Texture2D GetTextureFromBytes(byte[] data)
        {
            byte[] textureData = new byte[data.Length - (sizeof(int) * 2)];

            Array.Copy(data, sizeof(int) * 2, textureData, 0, textureData.Length);
            int width = BitConverter.ToInt32(data, 0); 
            int height = BitConverter.ToInt32(data, sizeof(int));
            
            Texture2D output = new Texture2D(Graphics.GraphicsDevice, width, height);


            output.SetData(textureData);

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
        public SpriteFont Font { get; set; }
        public Color Color { get; set; }
    }

    public class LoadTextureRenderArgs
    {
        public int ID { get; set; }
        public string Asset { get; set; }
    }

}
