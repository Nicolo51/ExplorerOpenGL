using ExplorerOpenGL2.Model.Sprites;
using ExplorerOpenGL2.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static ExplorerOpenGL2.Managers.RenderManager;

namespace ExplorerOpenGL2.Managers
{
    public class XmlManager
    {
        public static void InitDependencies()
        {

        }

        public static MapXml[] ReadXml(string xml)
        {
            List<MapXml> map = new List<MapXml>();
            var xmlMap = new XmlDocument();
            xmlMap.LoadXml(xml);
            XmlNode sprites = xmlMap.DocumentElement.SelectSingleNode("/map/sprites");
            foreach (XmlNode n in sprites.ChildNodes)
            {
                map.Add(new MapXml() { node = n, mapName = xmlMap.SelectSingleNode("/map/name").InnerText });
            }
            return map.ToArray();
        }

        public static MapXml[] LoadMap(string pathMap, LoadingScreen loadingScreen = null)
        {
            List<MapXml> map = new List<MapXml>(); 
            var xmlMap = new XmlDocument();
            xmlMap.Load($"./maps/{pathMap}.xml");
            XmlNode sprites = xmlMap.DocumentElement.SelectSingleNode("/map/sprites");
            foreach(XmlNode n in sprites.ChildNodes)
            {
                map.Add(new MapXml() { node = n, mapName = xmlMap.SelectSingleNode("/map/name").InnerText }); 
            }
            return map.ToArray();  
        }
        public static MapXml[] LoadMapFromString(string xmlIn, LoadingScreen loadingScreen = null)
        {
            List<MapXml> map = new List<MapXml>(); 
            var xmlMap = new XmlDocument();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xmlIn));
            xmlMap.Load(ms);
            XmlNode sprites = xmlMap.DocumentElement.SelectSingleNode("/map/sprites");
            foreach(XmlNode n in sprites.ChildNodes)
            {
                map.Add(new MapXml() { node = n, mapName = xmlMap.SelectSingleNode("/map/name").InnerText }); 
            }
            ms.Close();
            ms.Dispose();
            return map.ToArray();  
        } 

        public static void SaveMap(Sprite[] sprites, string mapName)
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
                GameManager.AddActionToUIThread(new Action<object>(RenderManager.SaveTextureAsPng), arg); 
                XElement concat = new XElement("sprite",
                    new XElement("type", s.GetType().Name),
                    new XElement("texture", count),
                    new XElement("position", s.Position.X + "/" + s.Position.Y),
                    new XElement("bounds", s.Bounds.X + "/" + s.Bounds.Y),
                    new XElement("alignOption", s.AlignOption.ToString()), 
                    new XElement("effect", s.Effect.ToString())
                    );
                xmlMap.Element("map").Element("sprites").Add(concat); 
            }
            xmlMap.Save(path + ".xml"); 
        }

        public static string GetMapXmlBySprites(Sprite[] sprites, string mapName)
        {
            var xmlMap = new XDocument();
            xmlMap.Add(new XElement("map", new XElement("sprites", ""), new XElement("name", mapName)));
            int count = 0;
            foreach (Sprite s in sprites)
            {
                count++;
                XElement concat = new XElement("sprite",
                    new XElement("ID", s.ID),
                    new XElement("type", s.GetType().Name),
                    new XElement("texture", Convert.ToBase64String(TextureManager.GetTextureBytes(s))),
                    new XElement("position", s.Position.X + "/" + s.Position.Y),
                    new XElement("bounds", s.Bounds.X + "/" + s.Bounds.Y),
                    new XElement("alignOption", s.AlignOption.ToString()),
                    new XElement("effect", s.Effect.ToString()), 
                    new XElement("IsPartOfGameState", s.IsPartOfGameState)
                    ); 
                xmlMap.Element("map").Element("sprites").Add(concat);
            }
            return xmlMap.ToString(); 
        }

        public static Sprite GenerateSpriteFromXml(XmlNode node, string mapName)
        {
            if (node.Name != "sprite")
                throw new Exception("The node passed as argument is note a Xml sprite");

            string type = node.SelectSingleNode("type").InnerText;
            string texture = node.SelectSingleNode("texture").InnerText;
            string position = node.SelectSingleNode("position").InnerText;
            string alignOption = node.SelectSingleNode("alignOption").InnerText;
            string effect = node.SelectSingleNode("effect").InnerText;
            string bounds = node.SelectSingleNode("bounds").InnerText;

            bool hasId = node.SelectSingleNode("ID") != null;
            string id = "0";
            if (hasId)
                 id = node.SelectSingleNode("ID").InnerText;

            Type spriteType = Type.GetType($"ExplorerOpenGL2.Model.Sprites.{type}");
            Texture2D spriteTexture = null;

            if (texture.Length > 100)
            {
                spriteTexture = TextureManager.GetTextureFromBytes(Convert.FromBase64String(texture));
                TextureManager.SaveTexture(spriteTexture); 
            }
            else
            {
                if (File.Exists($@"./maps/{mapName}/{texture}.png"))
                    spriteTexture = TextureManager.LoadNoneContentLoadedTexture($@"./maps/{mapName}/{texture}.png");
                else
                    spriteTexture = TextureManager.LoadTexture(texture);
            }

            Vector2 spritePosition = new Vector2(Int32.Parse(position.Split('/')[0]), Int32.Parse(position.Split('/')[1]));
            Vector2 spriteBounds = new Vector2(Int32.Parse(bounds.Split('/')[0]), Int32.Parse(bounds.Split('/')[1]));
            AlignOptions spriteAlignOption = (AlignOptions)Enum.Parse(typeof(AlignOptions), alignOption);
            SpriteEffects spriteEffect = (SpriteEffects)Enum.Parse(typeof(SpriteEffects), effect);
            int spriteID = int.Parse(id);

            Sprite sprite = GetInstance(spriteType);
            sprite.SetTexture(spriteTexture); 
            sprite.SetPosition(spritePosition);
            sprite.SetAlignOption(spriteAlignOption);
            sprite.Effect = spriteEffect;
            sprite.Bounds = spriteBounds;
            sprite.ID= spriteID; 
            return sprite; 
        }

        public static Sprite[] GenerateSpritesFromXml(MapXml[] mapXmls)
        {
            Sprite[] sprites = new Sprite[mapXmls.Length];
            for(int i = 0; i < mapXmls.Length ; i++) 
            {
                sprites[i] = GenerateSpriteFromXml(mapXmls[i].node, mapXmls[i].mapName);
            }
            return sprites;
        }

        public static Sprite GetInstance(Type type)
        {
            var sprite = Activator.CreateInstance(type);
            return (Sprite)sprite;
        }

        public string GetMapName(string xml)
        {
            var xmlDoc = XDocument.Load(xml);
            return xmlDoc.Descendants("name").First().Value;
        }

        public string[] GetMapTextureNames(string xml)
        {
            var xmlDoc = XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)));
            return xmlDoc.Descendants("texture").Select(e => e.Value).ToArray();
        }

        public static void AddGameStateID(MapXml map, int id)
        {

        }

        public static void AddTexture(MapXml map, byte[] data)
        {
            //map.node.SelectSingleNode("/map/texture").InnerText
        }

    }
    public class MapXml
    {
        public XmlNode node { get; set; }
        public string mapName { get; set; }
    }
}
