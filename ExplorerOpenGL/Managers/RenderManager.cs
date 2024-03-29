﻿using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers
{
    public class RenderManager
    {
        private List<Sprite> _sprites;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private KeyboardManager keyboardManager;
        private ShaderManager shaderManager; 
        
        private static RenderManager instance;
        public static event EventHandler Initialized;
        public static RenderManager Instance {
            get
            {
                if(instance == null)
                {
                    instance = new RenderManager();
                    Initialized?.Invoke(instance, EventArgs.Empty);
                    return instance;
                }
                return instance;
            }
        }

        public RenderManager()
        {

        }

        public void InitDependencies(GraphicsDeviceManager graphics, List<Sprite> sprites, SpriteBatch spriteBatch)
        {
            _sprites = sprites;
            this.spriteBatch = spriteBatch;
            this.graphics = graphics;
            keyboardManager = KeyboardManager.Instance;
            shaderManager = ShaderManager.Instance; 

        }

        public Texture2D RenderSceneToTexture()
        {
            int width = graphics.PreferredBackBufferWidth;
            int height = graphics.PreferredBackBufferHeight; 

            Texture2D texture = new Texture2D(graphics.GraphicsDevice, width, height);

            RenderTarget2D target = new RenderTarget2D(graphics.GraphicsDevice, width, height, false, graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            Color[] data = new Color[width * height];

            spriteBatch.GraphicsDevice.SetRenderTarget(target);
            spriteBatch.GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };

            spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.BackToFront,
                              BlendState.AlphaBlend,
                              SamplerState.PointClamp,
                              null, null, null, null);

            for(int i = 0; i < _sprites.Count; i++)
            {
                _sprites[i].Draw(spriteBatch, new GameTime(), 1); 
            }

            spriteBatch.End();
            target.GetData(data);

            spriteBatch.GraphicsDevice.SetRenderTarget(null);

            texture.SetData(data);

            target.Dispose();
            data = null; 

            return texture;
        }

        public void SaveTextureAsPng(object args)
        {
            if (args.GetType() != typeof(SaveTextureAsPngArg))
                throw new Exception("the argument of this function need to be of type SaveTextureAsPngArg");
            Texture2D texture = (args as SaveTextureAsPngArg).Texture;

            Stream stream = File.Create((args as SaveTextureAsPngArg).Path);
            texture.SaveAsPng(stream, texture.Width, texture.Height);
            stream.Dispose();
        }

        public Texture2D RenderTextToTexture(string input, SpriteFont font, Color textColor, int outlineOffset)
        {
            StringBuilder temp = new StringBuilder();
            temp.Append(" ");
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\n')
                    temp.Append(" \n ");
                else
                    temp.Append(input[i]);
            }

            string textToRender = temp.ToString(); 
            Vector2 stringDimension = font.MeasureString(temp.ToString());
            Vector2 targetBounds = new Vector2(stringDimension.X + outlineOffset *2 , stringDimension.Y + outlineOffset * 2); 

            Texture2D texture = new Texture2D(graphics.GraphicsDevice, (int)targetBounds.X, (int)targetBounds.Y);

            RenderTarget2D target = new RenderTarget2D(
                graphics.GraphicsDevice,
                (int)targetBounds.X,
                (int)targetBounds.Y,
                false,
                graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            Color[] data = new Color[(int)targetBounds.X * (int)targetBounds.Y];

            spriteBatch.GraphicsDevice.SetRenderTarget(target);
            spriteBatch.GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };

            spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.BackToFront,
                              BlendState.AlphaBlend,
                              SamplerState.PointClamp,
                              null, null, null, null);

            spriteBatch.DrawString(font, textToRender, targetBounds / 2, textColor, 0f, stringDimension / 2, 1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.GraphicsDevice.SetRenderTarget(null);
            target.GetData(data);
            texture.SetData(data);
            data = null; 
            target.Dispose();
            return texture;
        }

        private void WrappText(string[] input, SpriteFont font, int length)
        {

            /*
                 A TESTER 
            */
            List<string> StringtoRender = new List<string>();

            for (int i = 0; i < input.Length; i++)
            {
                if (font.MeasureString(input[i]).X > length)
                {
                    List<string> WrappedText = new List<string>();
                    string[] results = FitStringinAverage(input[i], font, length);
                    WrappedText.Add(results[0]);
                    while (results[1] != "")
                    {
                        results = FitStringinAverage(results[1], font, length);
                        WrappedText.Add(results[0]);
                    }
                    WrappedText.Add(results[0]);
                    StringtoRender.AddRange(WrappedText);
                }
                else
                {
                    StringtoRender.Add(input[i]);
                }
            }
        }


        public void DrawString(SpriteFont font, string text, Vector2 position, Color color, float radian, Vector2 origin, float scale, SpriteEffects spriteEffects, float layerDepth)
        {
            shaderManager.LoadShader("FontEffect").CurrentTechnique.Passes[0].Apply(); 
            spriteBatch.DrawString(font, text, position, color, radian, origin, scale, spriteEffects, layerDepth); 
        }

        private string[] FitStringinAverage(string input, SpriteFont font, double Dim)
        {
            string[] words = input.Split(' ');
            StringBuilder rightLengthString = new StringBuilder();
            StringBuilder LeftString = new StringBuilder(); 

            bool IsOk = true;
            for(int i = 0; i < words.Length; i++)
            {
                if(!IsOk)
                {
                    LeftString.Append(" " + words[i]);
                    continue; 
                }
                if(font.MeasureString(rightLengthString.ToString()).X + font.MeasureString(words[i]).X > Dim)
                {
                    IsOk = false;
                    continue; 
                }
                rightLengthString.Append(" " + words[i]);
            }
            string[] outputs = new string[2];
            outputs[0] = rightLengthString.ToString();
            outputs[1] = LeftString.ToString();

            return outputs; 
        }

        public class SaveTextureAsPngArg
        {
            public SaveTextureAsPngArg(string path, Texture2D texture)
            {
                Path = path;
                Texture = texture; 
            }
            public string Path { get; set; }
            public Texture2D Texture { get; set; }
        }

    }
    
}
