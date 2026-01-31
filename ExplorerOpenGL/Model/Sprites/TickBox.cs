using ExplorerOpenGL2.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Model.Sprites
{
    public class TickBox : Sprite
    {
        private Texture2D check;
        public bool IsCheck { get; private set; }
        public TickBox(params Texture2D[] textures)
        {
            if (textures.Length != 2)
                throw new Exception("Tickbox need 2 textures to work correctly");
            this.SetTexture(textures[0]);
            check = textures[1];

            IsClickable= true;
            MouseClicked += TickBox_MouseClicked;
        }

        private void TickBox_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            IsCheck = !IsCheck; 
        }

        public override void Update(List<Sprite> sprites, GameTime gametime, NetGameState netGameState)
        {
            base.Update(sprites, gametime, netGameState);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float lerpAmount, params ShaderArgument[] shaderArgs)
        {
            base.Draw(spriteBatch, gameTime, lerpAmount, shaderArgs);
            if(IsCheck)
                spriteBatch.Draw(check, new Rectangle((int)Position.X, (int)Position.Y, (int)(Bounds.X * Scale), (int)(Bounds.Y * Scale)), SourceRectangle, Color.White * Opacity, Radian, Origin, Effect, LayerDepth-0.001f);
        }
    }
}
