using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers.Networking
{
    public class PlayerData : Sprite
    {
        public int ID { get; private set; }
        public string Name { get; set; }
        private string RenderName { get; set; }
        public Vector2 ServerPosition { get; set; }
        public Vector2 InGamePosition { get; set; }
        public float LookAtRadian { get; set; }
        public float FeetRadian { get; set; }
        public Texture2D playerTexture { get; set; }
        public Texture2D playerFeetTexture { get; set; }
        public Texture2D TextureName{ get; set; }
        public Vector2 PositionName { get; set; }
        public Vector2 OriginName { get; set; }
        public float opacity;
        public bool NameHasChange { get; set; }
        private Vector2 originFeet;
        private TextureManager textureManager;
        public double Health { get; set; }

        private NetworkManager networkManager; 

        public int idTexture { get; set; }
        public int idFeetTexture { get; set; }

        public PlayerData(int id, string name)
        {
            textureManager = TextureManager.Instance; 
            NameHasChange = false; 
            this.Name = name; 
            scale = 1f;
            opacity = 1f; 
            ID = id;
            layerDepth = .9f;
            networkManager = NetworkManager.Instance;
        }
        public PlayerData(int id)
        {
            textureManager = TextureManager.Instance;
            NameHasChange = false;
            scale = 1f;
            opacity = 1f;
            ID = id;
            layerDepth = .9f;
            networkManager = NetworkManager.Instance;
        }

        public override void Update(GameTime gameTime, List<Sprite> sprites)
        {
            PositionName = new Vector2(ServerPosition.X, ServerPosition.Y + 50);
            if(RenderName != Name)
            {
                GenerateTexture(textureManager);
            }
            if(playerFeetTexture == null || playerTexture == null)
            {
                SetTextures(textureManager.LoadedTextures["player"], textureManager.LoadedTextures["playerfeet"]);
            }
            base.Update(gameTime, sprites);
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (TextureName != null && playerFeetTexture != null && playerTexture != null)
            {
                spriteBatch.Draw(TextureName, PositionName, null, Color.White, 0f, OriginName, .75f, SpriteEffects.None, layerDepth);
                spriteBatch.Draw(playerFeetTexture, ServerPosition, null, Color.White * opacity, FeetRadian, originFeet, scale , Effects, layerDepth + .01f);
                spriteBatch.Draw(playerTexture, ServerPosition, null, Color.White * opacity, LookAtRadian, origin, scale * .5f, Effects, layerDepth);
            }
            
        }
    }
}
