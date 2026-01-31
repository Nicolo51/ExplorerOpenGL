using ExplorerOpenGL2.Model;
using ExplorerOpenGL2.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Managers.Networking
{
    public class PlayerData : Sprite
    {
        public string Name { get; set; }
        private string RenderName { get; set; }
        public Vector2 ServerPosition { get; set; }
        public Vector2 InGamePosition { get; set; }
        public float LookAtRadian { get; set; }
        public float FeetRadian { get; set; }
        public Texture2D playerTexture { get; set; }
        public Texture2D playerFeetTexture { get; set; }
        public Texture2D TextureName{ get; set; }
        public Vector2 PositionName { get { return new Vector2(Position.X, Position.Y - 10); }}
        public Vector2 PositionHealth { get { return new Vector2(Position.X, Position.Y - 50); } }
        public string CurrentAnimationName { get; set; }
        public Vector2 OriginName { get; set; }
        public float opacity;
        public bool NameHasChange { get; set; }
        private Vector2 originFeet;
        private TextureManager textureManager;
        private SpriteFont font; 
        public double Health { get; set; }

        private NetworkManager networkManager;
        
        public int idTexture { get; set; }
        public int idFeetTexture { get; set; }

        public PlayerData(int id, string name)
            :base()
        {
            this.Name = name;
            Init(id);
        }
        public PlayerData(int id)
            : base()
        {
            Init(id);
        }

        private void Init(int id)
        {
            textureManager = TextureManager.Instance;
            networkManager = NetworkManager.Instance;
            if (networkManager.IDClient != id)
            {
                Animation walking = textureManager.GetAnimation("walk");
                Animation standing = textureManager.GetAnimation("idle");
                Animation running = textureManager.GetAnimation("run");
                Animation jump = textureManager.GetAnimation("jump");
                Animation falling = textureManager.GetAnimation("falling");

                Animation[] animations = textureManager.NormalizeHeights(walking, standing, running, jump, falling);

                _animation.Add(animations[0]); 
                _animation.Add(animations[1]); 
                _animation.Add(animations[2]); 
                _animation.Add(animations[3]); 
                _animation.Add(animations[4]);
                Play(standing);
                Bounds = _animation.GetBounds(); 
            }
            NameHasChange = false;
            Scale = 1f;
            opacity = 1f;
            ID = id;
            LayerDepth = .9f;
        }

        public override void Update(List<Sprite> sprites, GameTime gametime, NetGameState netGameState)
        {
            if(!string.IsNullOrWhiteSpace(CurrentAnimationName))
                Play(CurrentAnimationName);

            if(RenderName != Name)
            {
                GenerateTexture(textureManager);
            }
            base.Update(sprites, gametime, netGameState);
        }

       

        private void SetTextures(Texture2D texture, Texture2D textureFeet)
        {
            playerFeetTexture = textureFeet;
            playerTexture = texture;
            origin = new Vector2(playerTexture.Width / 2, playerTexture.Height / 2);
            originFeet = new Vector2(playerFeetTexture.Width / 2, playerFeetTexture.Height / 2);
        }

        private void GenerateTexture(TextureManager tm)
        {
            TextureName = tm.OutlineText(Name, "Default", Color.Black, Color.White, 2);
            OriginName = new Vector2(TextureName.Width / 2, TextureName.Height / 2);
            RenderName = Name; 
        }

        public void ChangeName (string name)
        {
            Name = name; 
        }

        public override string ToString()
        {
            return $"ID:{ID}, Position:{ServerPosition.ToString()}, LookAtRadian:{LookAtRadian}, FeetRadian:{FeetRadian}"; 
        }

        public override void Draw(SpriteBatch spriteBatch,  GameTime gameTime, float lerpAmount = 1, params ShaderArgument[] shaderArgs)
        {
            if (TextureName != null)
            {
                //spriteBatch.DrawString(font, Health.ToString("#"), PositionHealth, Color.White, 0f, font.MeasureString(Health.ToString("#")) / 2, 1f, SpriteEffects.None, layerDepth); 
                spriteBatch.Draw(TextureName, PositionName, null, Color.White, 0f, OriginName, .75f, SpriteEffects.None, LayerDepth);
                //spriteBatch.Draw(playerFeetTexture, InGamePosition, null, Color.White * opacity, FeetRadian, originFeet, scale , Effects, layerDepth + .01f);
                //spriteBatch.Draw(playerTexture, InGamePosition, null, Color.White * opacity, LookAtRadian, origin, scale * .5f, Effects, layerDepth);
            }
            base.Draw(spriteBatch, gameTime, lerpAmount); 
        }
    }
}
