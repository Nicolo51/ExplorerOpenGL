using ExplorerOpenGL.Controlers;
using ExplorerOpenGL.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model.Sprites
{
    public class Chat : Sprite
    {
        List<ChatElement> messages;
        bool seeAllMessages;
        bool IsTakingKeyboardInput;
        SpriteFont chatFont;
        public string Name;
        public Color FontColor; 
        public TextinputBox TextinputBox;
        
        private bool IsNewMessage { get 
            {
                if(messages[messages.Count-1].Opacity <= 0f)
                {
                    return false;
                }
                return true; 
            } 
        }
        public Chat(Texture2D Texture, SpriteFont ChatFont, TextinputBox textinputBox)
        {
            TextinputBox = textinputBox;
            bool IsTakingKeyboardInput = false ;
            chatFont = ChatFont; 
            layerDepth = .1f; 
            _texture = Texture; 
            opacity = .5f;
            seeAllMessages = true;
            ChatElement InitMessage = new ChatElement()
            {
                Color = Color.White,
                Date = DateTime.Now, 
                Message = "Chat w&é\"\'(-è_çà)^$*ùll Initialized !", 
                Name = "Info", 
                Opacity = 5f, 
            }; 
            messages = new List<ChatElement>() { InitMessage };
            origin = new Vector2(0, Texture.Height);
            TextinputBox.OnValidation += OnValidation;
        }

        private void OnValidation(string message)
        {
            AddMessageToChat(message, Name, FontColor); 
        }

        public void AddMessageToChat(string message, string name, Color color)
        {
            if (name == null || color == null || message == null)
                return; 
            ChatElement chatElement = new ChatElement()
            {
                Color = color,
                Date = DateTime.Now,
                Message = message,
                Opacity = 5f,
                Name = name,
            };
            messages.Add(chatElement);
        }

        public void AddMessageToChat(string message)
        {
            if (messages != null)
            {
                ChatElement chatElement = new ChatElement()
                {
                    Color = Color.Black,
                    Date = DateTime.Now,
                    Message = message,
                    Opacity = 5f,
                    Name = "Me",
                };
                messages.Add(chatElement);
            }
        }

        public void ToggleChatInputOn()
        {
            IsTakingKeyboardInput = !IsTakingKeyboardInput;
        }

        public void KeyboardListener(Keys[] keys, KeyboardUtils keyboardUtils)
        {
            KeyAlterer keyAlterer = KeyAlterer.None;
            if (keyboardUtils.IsKeyDown(Keys.RightAlt))
                keyAlterer = KeyAlterer.AltGr;
            if (keyboardUtils.IsKeyDown(Keys.RightShift))
                keyAlterer = KeyAlterer.Cap;
            
            foreach (Keys k in keys)
            {
                TextinputBox.AddKeyStroke(k, keyAlterer);
            }
        }

        public override void Update(GameTime gameTime, List<Sprite> sprites, Controler controler)
        {
            TextinputBox.Update(gameTime, sprites, controler); 
            if (TextinputBox.isFocused)
                controler.KeyboardUtils.IsTextInputFocused = true;
            else
                controler.KeyboardUtils.IsTextInputFocused = false; 

            if(controler.KeyboardUtils.IsKeyDown(Keys.H)){
                seeAllMessages = true;
            }
            else{
                seeAllMessages = false; 
            }

            if (IsNewMessage)
            {
                for(int i = 0;  i < messages.Count; i++)
                {
                    ChatElement message = messages[i]; 
                    if (!message.IsDisplayed)
                        continue;
                    message.Opacity -= 0.05f;
                    if (message.Opacity < 0f)
                        message.IsDisplayed = false; 
                }
            }
            Position = new Vector2((controler.Camera.Position.X - controler.Camera.Bounds.X / 2), (controler.Camera.Position.Y - controler.Camera.Bounds.Y / 2 + controler.Bounds.Y));
            base.Update(gameTime, sprites, controler);
        }


        public override void Draw(SpriteBatch spriteBatch)
        {

            int j = messages.Count - 1;
            if (j == -1)
            { return; }
            for (int i = 0; i < messages.Count; i++)
            {
                spriteBatch.DrawString(chatFont, messages[j].ToString(), new Vector2(Position.X, Position.Y - (i*25)), messages[j].Color, 0f, new Vector2(0, 65), 1f, SpriteEffects.None, layerDepth - .01f);
                j--;
            }
            TextinputBox.Draw(spriteBatch);
            base.Draw(spriteBatch);
        }
    }
    
    
    class ChatElement
    {
        public bool IsDisplayed; 
        public string Message;
        public DateTime Date;
        public string Name;
        public Color Color;
        public float Opacity; 
        public override string ToString()
        {
            return "[" + Name + "] : " + Message;
        }
    }
}
