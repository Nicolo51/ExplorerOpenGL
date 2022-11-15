using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
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
        delegate void ShaderHandler(Sprite sprite, Effect effect, params ShaderArgument[] args); 
        private Dictionary<Effect, ShaderHandler> shaderHandler; 

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
            InitShaderHandler();
        }

        public void InitShaderHandler()
        {
            shaderHandler = new Dictionary<Effect, ShaderHandler>()
            {
                {LoadShader("Normal"), NormalHandler }, 
                {LoadShader("Outline"), OutlineHandler },
            };
            
        }

        

        public Effect GetDefaultShader()
        {
            return LoadShader("Normal"); 
        }

        public Effect LoadShader(string path)
        {
            if (LoadedShader.ContainsKey("Effects/" + path))
                return LoadedShader["Effects/" + path];
            Effect effect = content.Load<Effect>("Effects/" + path);
            LoadedShader.Add("Effects/" + path, effect);
            return effect; 
        }



        public void Apply(Sprite sprite, Effect effect, params ShaderArgument[] args)
        {
            if (effect == null)
                return; 
            shaderHandler[effect](sprite, effect, args);
            effect.CurrentTechnique.Passes[0].Apply();
        }

        private void ApplyParameters(Effect effect, string param, object arg)
        {
            switch (arg)
            {
                case Vector4[] av4:
                    effect.Parameters[param].SetValue(av4); 
                    break; 
                case Vector4 v4:
                    effect.Parameters[param].SetValue(v4); 
                    break;
                case Vector3[] av3:
                    effect.Parameters[param].SetValue(av3); 
                    break;
                case Texture t:
                    effect.Parameters[param].SetValue(t); 
                    break;
                case Vector2[] av2:
                    effect.Parameters[param].SetValue(av2); 
                    break;
                case Vector2 v2:
                    effect.Parameters[param].SetValue(v2); 
                    break;
                case float[] af:
                    effect.Parameters[param].SetValue(af); 
                    break;
                case float f:
                    effect.Parameters[param].SetValue(f); 
                    break;
                case Quaternion q:
                    effect.Parameters[param].SetValue(q); 
                    break;
                case Matrix[] am:
                    effect.Parameters[param].SetValue(am); 
                    break;
                case Matrix m:
                    effect.Parameters[param].SetValue(m); 
                    break;
                case int i:
                    effect.Parameters[param].SetValue(i); 
                    break;
                case bool b:
                    effect.Parameters[param].SetValue(b); 
                    break;
                case Vector3 v3:
                    effect.Parameters[param].SetValue(v3); 
                    break;
                case Color c:
                    effect.Parameters[param].SetValue(c.ToVector4());
                    break;
            }
        }

        private void NormalHandler(Sprite sprite, Effect effect, params ShaderArgument[] args)
        {
            
        }
        private void OutlineHandler(Sprite sprite, Effect effect, params ShaderArgument[] args)
        {
            if (CheckShaderParamaters(effect, args))
            {
                var texSize = new Vector2(2f / ((sprite.Texture).Width - 1f), 2f / ((sprite.Texture).Height + 1f));
                foreach (var arg in args)
                {
                    if (arg.ParamaterName == "thickness")
                    {
                        ApplyParameters(effect, arg.ParamaterName, new Vector2((arg.Value as Vector2?).Value.X / ((sprite.Texture).Width - 1f), (arg.Value as Vector2?).Value.Y / ((sprite.Texture).Height + 1f)));
                        continue;
                    }
                    ApplyParameters(effect, arg.ParamaterName, arg.Value); 
                }
                return; 
            }
            effect.Parameters["thickness"].SetValue(new Vector2(2f / ((sprite.Texture).Width - 1f), 2f / ((sprite.Texture).Height + 1f)));
            effect.Parameters["outlineColor"].SetValue(Color.Red.ToVector4());
        }
        private bool CheckShaderParamaters(Effect effect, ShaderArgument[] args)
        {
            if (args == null)
                return false; 
            if (args.Length != effect.Parameters.Count-1 )
            {
                string[] p = effect.Parameters.Where(pa => args.Any(a => a.ParamaterName == pa.Name)).Select(pa => pa.Name).ToArray();

                return false; 
            }
            return true; 
        }
    }

    public class ShaderArgument
    {
        public string ParamaterName { get; set; }
        public object Value { get; set; }
        public ShaderArgument(string paramaterName, object value)
        {
            ParamaterName = paramaterName;
            Value = value; 
        }
    }

}
