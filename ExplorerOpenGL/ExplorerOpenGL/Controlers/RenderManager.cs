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
    }
}
