using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static ExplorerOpenGL.Managers.RenderManager;

namespace ExplorerOpenGL.Managers
{
    public class XmlManager
    {
        private static XmlManager instance;
        public static event EventHandler Initialized;
        public static XmlManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new XmlManager();
                    Initialized?.Invoke(instance, EventArgs.Empty);
                    return instance;
                }
                return instance;
            }
        }
        private GameManager gameManager;
        private TextureManager textureManager;
        private RenderManager renderManager; 
        private XmlManager()
        {

        }

        public void InitDependencies()
        {
            gameManager = GameManager.Instance;
            textureManager = TextureManager.Instance;
            renderManager = RenderManager.Instance; 
        }

        public XmlDocument ReadXml(string path)
        {
            var doc = new XmlDocument();
            doc.LoadXml(path);
            return doc; 
        }

        public Sprite[] LoadMap(string pathMap)
        {
            List<Sprite> map = new List<Sprite>(); 
            var xmlMap = new XmlDocument();
            xmlMap.Load(pathMap);
            XmlNode sprites = xmlMap.DocumentElement.SelectSingleNode("/map/sprites");
            foreach(XmlNode node in sprites.ChildNodes)
            {
                map.Add(GenerateSpriteFromXml(node, xmlMap.SelectSingleNode("/map/name").InnerText)); 
            }
            return map.ToArray();  
        }

        public void SaveMap(Sprite[] sprites, string mapName)
        {
            string path = $@"./maps/{mapName}";
            var xmlMap = new XDocument();
            xmlMap.Add(new XElement("map", new XElement("sprites", ""), new XElement("name", mapName)));
            string[] assets = Directory.GetFiles(path); 
            foreach(var a in assets)
            {
                File.Delete(a); 
            }
            File.Delete(path + ".xml");

            int count = 0;
            foreach (Sprite s in sprites)
            {
                count++;
                SaveTextureAsPngArg arg = new SaveTextureAsPngArg(path + "/" + count + ".png", s.Texture);
                gameManager.AddActionToUIThread(new Action<object>(renderManager.SaveTextureAsPng), arg); 
                XElement concat = new XElement("sprite",
                    new XElement("type", s.GetType().Name),
                    new XElement("texture", count),
                    new XElement("position", s.Position.X + "/" + s.Position.Y),
                    new XElement("bounds", s.Bounds.X + "/" + s.Bounds.Y),
                    new XElement("alignOption", s.AlignOption.ToString()), 
                    new XElement("effect", s.Effects.ToString())
                    );
                xmlMap.Element("map").Element("sprites").Add(concat); 
            }
            xmlMap.Save(path + ".xml"); 
        }

        public Sprite GenerateSpriteFromXml(XmlNode node, string mapName)
        {
            if (node.Name != "sprite")
                throw new Exception("The node passed as argument is note a Xml sprite");

            string type = node.SelectSingleNode("type").InnerText;
            string texture = node.SelectSingleNode("texture").InnerText;
            string position = node.SelectSingleNode("position").InnerText;
            string alignOption = node.SelectSingleNode("alignOption").InnerText;
            string effect = node.SelectSingleNode("effect").InnerText;
            string bounds = node.SelectSingleNode("bounds").InnerText;

            Type spriteType = Type.GetType($"ExplorerOpenGL.Model.Sprites.{type}");
            Texture2D spriteTexture = null;

            if (File.Exists($@"./maps/{mapName}/{texture}.png"))
                spriteTexture = textureManager.LoadNoneContentLoadedTexture($@"./maps/{mapName}/{texture}.png");
            else
                spriteTexture = textureManager.LoadTexture(texture);

            Vector2 spritePosition = new Vector2(Int32.Parse(position.Split('/')[0]), Int32.Parse(position.Split('/')[1]));
            Vector2 spriteBounds = new Vector2(Int32.Parse(bounds.Split('/')[0]), Int32.Parse(bounds.Split('/')[1]));
            AlignOptions spriteAlignOption = (AlignOptions)Enum.Parse(typeof(AlignOptions), alignOption);
            SpriteEffects spriteEffect = (SpriteEffects)Enum.Parse(typeof(SpriteEffects), effect);

            Sprite sprite = GetInstance(spriteType);
            sprite.SetTexture(spriteTexture); 
            sprite.SetPosition(spritePosition);
            sprite.SetAlignOption(spriteAlignOption);
            sprite.Effects = spriteEffect;
            sprite.isDraggable = true;
            sprite.Bounds = spriteBounds; 
            return sprite; 
        }

        public Sprite GetInstance(Type type)
        {
            var sprite = Activator.CreateInstance(type);
            return (Sprite)sprite;
        }
    }
}
