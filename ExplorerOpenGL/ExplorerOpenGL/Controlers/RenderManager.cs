using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers
{
    public class RenderManager
    {
        private List<Sprite> _sprites;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch; 

        public RenderManager(List<Sprite> sprites, GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
            this.graphics = graphics; 
            this._sprites = sprites; 
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
                _sprites[i].Draw(spriteBatch); 
            }

            spriteBatch.End();
            target.GetData(data);

            spriteBatch.GraphicsDevice.SetRenderTarget(null);

            texture.SetData(data);

            target.Dispose();
            data = null; 

            return texture;
        }

        public Texture2D RenderTextToTexture(string input, SpriteFont font, Color textColor)
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
            spriteBatch.Begin(SpriteSortMode.BackToFront,
                              BlendState.AlphaBlend,
                              SamplerState.PointClamp,
                              null, null, null, null);

            spriteBatch.DrawString(font, textToRender, Vector2.Zero, textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            spriteBatch.End();
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
    }
}
