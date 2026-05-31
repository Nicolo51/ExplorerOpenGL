using ExplorerOpenGL.Model;
using ExplorerOpenGL.Model.Interface;
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
    public class CheckBox : Sprite, IUserControl
    {
        private Texture2D check;
        public bool IsCheck { get; set; }
        public string Description { get; set; }
        public Func<IUserControl, AssertResult> Assert { get; set; }

        public CheckBox(params Texture2D[] textures)
        {
            if (textures.Length != 2)
                throw new Exception("Tickbox need 2 textures to work correctly");
            this.SetTexture(textures[0]);
            check = textures[1];

            IsClickable= true;
            MouseClicked += TickBox_MouseClicked;
            MouseOvered += CheckBox_MouseOvered;
            MouseLeft += CheckBox_MouseLeft;
        }

        private void CheckBox_MouseLeft(object sender, MousePointer mousePointer)
        {
            GameManager.MousePointer.SetCursorIcon(MousePointerType.Default);
        }

        private void CheckBox_MouseOvered(object sender, MousePointer mousePointer)
        {
            GameManager.MousePointer.SetCursorIcon(MousePointerType.Pointer);
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

        public object GetValueOfUC()
        {
            return IsCheck; 
        }

        public Type GetTypeOfUC()
        {
            return IsCheck.GetType(); 
        }
        public void SetValueOfUC(object value)
        {
            if(value.GetType() != GetTypeOfUC())
                IsCheck = (bool)value;
        }

        public string ToConfigFile()
        {
            return IsCheck.ToString();
        }
    }
}
