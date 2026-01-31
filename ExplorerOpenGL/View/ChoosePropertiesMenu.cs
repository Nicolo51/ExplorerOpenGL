using ExplorerOpenGL2.Model.Attributes;
using ExplorerOpenGL2.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.View
{
    public class ChoosePropertiesMenu : MessageBoxIG
    {
        public int Height = 250;
        public const int Width = 350;

        TextZone description; 

        TextZone[] labels;
        TextinputBox[] textinputs;
        Dictionary<TextinputBox, PropertyInfo> textinputToProperty;
        Button close;
        Button validate;

        private Sprite sprite; 

        public ChoosePropertiesMenu(Sprite sprite)
            : base()
        {
            this.sprite = sprite; 
            SetPosition(new Vector2(gameManager.Width / 2, gameManager.Height / 2));
            SpriteFont font = fontManager.GetFont("Default");
            Title = "Properties :";
            var properties = sprite.GetType().GetProperties().Where(p => p.IsDefined(typeof(MapEditable), true)).ToArray();

            labels = new TextZone[properties.Length]; 
            textinputs = new TextinputBox[properties.Length];
            description = new TextZone("Properties for this element :");
            close = new Button(textureManager.CreateTexture(30, 30, paint => Color.Transparent))
            {
                TextOnTop = new TextZone("[X]", AlignOptions.Center),
            }; 
            validate = new Button(textureManager.OutlineText("Validate", "Default", Color.CornflowerBlue, Color.Black, 1), textureManager.OutlineText("Validate", "Default", Color.CornflowerBlue, Color.Black, 2));

            textinputToProperty = new Dictionary<TextinputBox, PropertyInfo>(); 

            for(int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i]; 

                TextZone textZone = new TextZone(property.Name);
                textZone.Position = new Vector2(50, 100 + i * 60);
                textZone.SetAlignOption(AlignOptions.Left);
                
                TextinputBox textinputBox = new TextinputBox(textureManager.CreateTexture(150, 25, paint => Color.Black), fontManager.GetFont());
                textinputBox.Validated += TextinputBox_Validated;
                textinputBox.Unfocused += TextinputBox_Validated;
                textinputBox.Position = new Vector2(50, 100 + 25 + i * 60); 
                textinputBox.SetAlignOption(AlignOptions.Left);
                textinputBox.AddRange(property.GetValue(sprite).ToString());

                labels[i] = textZone;
                textinputs[i] = textinputBox;
                textinputToProperty.Add(textinputBox, property);
                Height = 200 + 25 + i * 60;
            }

            validate.Position = new Vector2(Width / 2, Height - 50);
            close.Position = new Vector2(Width, 0);
            description.Position = new Vector2(25, 50);

            close.MouseClicked += Close_MouseClicked;
            validate.MouseClicked += Validate_MouseClicked;

            validate.SetAlignOption(AlignOptions.Center);
            close.SetAlignOption(AlignOptions.TopRight); 

            SetTexture(textureManager.CreateBorderedTexture(Width, Height, 3, 0, paint => Color.Black, paint => (paint < (Width * 30) ? new Color(22, 59, 224) : new Color(245, 231, 213))));
            SourceRectangle = new Rectangle(0, 0, Texture.Width, Texture.Height);

            SetAlignOption(AlignOptions.Center);

        }

        private void Validate_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            foreach (var ti in textinputs)
                TextinputBox_Validated(ti.Text, ti); 
        }

        private void Close_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            sprite = null;
            labels = null;
            description = null;
            textinputToProperty.Clear();
            textinputToProperty = null; 
            this.Close(); 
        }

        private void TextinputBox_Validated(string message, TextinputBox textinput)
        {
            if (textinputToProperty == null)
                return; 
            PropertyInfo property = textinputToProperty[textinput];
            Type type = property.GetValue(sprite).GetType();
            object value = Convert.ChangeType(message.Replace(".", ","), type);
            property.SetValue(sprite, value); 
        }

        public override void Show()
        {
            foreach (var s in labels)
                AddChildSprite(s);
            foreach (var s in textinputs)
                AddChildSprite(s);

            AddChildSprite(description);
            AddChildSprite(validate);
            AddChildSprite(close);
            base.Show();
        }
    }
}
