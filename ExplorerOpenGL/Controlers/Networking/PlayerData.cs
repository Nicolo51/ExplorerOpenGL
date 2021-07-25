using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers.Networking
{
    public class PlayerData
    {
        public int ID { get; private set; }
        public string Name { get; set; }
        public Vector2 ServerPosition { get; set; }
        public Vector2 InGamePosition { get; set; }
        public float LookAtRadian { get; set; }
        public float FeetRadian { get; set; }
        public Texture2D playerTexture { get; set; }
        public Texture2D playerFeetTexture { get; set; }
        public Vector2 origin { get ; set; }
        public float scale;
        public float opacity;



        public int idTexture { get; set; }
        public int idFeetTexture { get; set; }
        public SpriteEffects Effects { get; set; }
        public float layerDepth { get; private set; }

        public PlayerData(int id, string name)
        {
            this.Name = name; 
            scale = 1f;
            opacity = 1f; 
            ID = id;
        }
        public PlayerData(int id)
        {
            scale = 1f;
            opacity = 1f;
            ID = id;
        }

        private Vector2 getOrigin(Texture2D texture)
        {
            return new Vector2(texture.Width / 2, texture.Height / 2);
        }

        public override string ToString()
        {
            return $"ID:{ID}, Position:{ServerPosition.ToString()}, LookAtRadian:{LookAtRadian}, FeetRadian:{FeetRadian}"; 
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(playerFeetTexture, ServerPosition, null, Color.White * opacity, FeetRadian, getOrigin(playerFeetTexture), scale, Effects, layerDepth);
            spriteBatch.Draw(playerTexture, ServerPosition, null, Color.White * opacity, LookAtRadian, getOrigin(playerTexture), scale*.5f, Effects, layerDepth+.01f); 

        }
    }
}
