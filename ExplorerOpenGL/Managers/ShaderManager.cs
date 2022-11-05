using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers
{
    public class ShaderManager
    {
        private ContentManager content;
        private SpriteBatch spriteBatch;
        private GraphicsDeviceManager graphics;

        private Dictionary<string, Effect> LoadedShader; 

        public static event EventHandler Initialized;
        private static ShaderManager instance;
        public static ShaderManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ShaderManager();
                    Initialized?.Invoke(instance, EventArgs.Empty);
                    return instance;
                }
                return instance;
            }
        }


        private ShaderManager()
        {
            LoadedShader = new Dictionary<string, Effect>(); 
        }
        public void InitDependencies(GraphicsDeviceManager graphics, ContentManager content, SpriteBatch spriteBatch)
        {
            this.content = content;
            this.graphics = graphics;
            this.spriteBatch = spriteBatch;
        }

        public Effect LoadShader(string path)
        {
            if (LoadedShader.ContainsKey(path))
                return LoadedShader[path];
            Effect effect = content.Load<Effect>(path);
            LoadedShader.Add(path, effect);
            return effect; 
        }
    }
}
