using ExplorerOpenGL.Controlers;
using ExplorerOpenGL.Model.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExplorerOpenGL.Model.Sprites
{
    public class Terminal : Sprite, IFocusable
    {
        List<ChatElement> messages;
        bool seeAllMessages;
        public int width { get { return _texture.Width; } }
        bool IsTakingKeyboardInput;
        SpriteFont font;
        public string Name;
        public Color FontColor; 
        public TextinputBox TextinputBox;
        private Controler controler; 
        
        private bool IsNewMessage { get 
            {
                if(messages[messages.Count-1].Opacity <= 0f)
                {
                    return false;
                }
                return true; 
            } 
        }

        public bool IsFocused { get => TextinputBox.isFocused ; set => TextinputBox.isFocused = value; }

        public Terminal(Texture2D Texture, SpriteFont Font, Controler Controler, TextinputBox textinputBox)
        {
            TextinputBox = textinputBox;
            TextinputBox.MouseClicked -= TextinputBox.OnMouseClick;
            TextinputBox.MouseClicked += OnMouseClick;
            TextinputBox.IsHUD = true;
            Name = "Me";
            this.controler = Controler;
            //Position = Vector2.Zero; 
            TextinputBox.Position = new Vector2(0, Texture.Height);
            IsHUD = true; 
            font = Font;
            layerDepth = .1f; 
            _texture = Texture; 
            opacity = .5f;
            seeAllMessages = true;
            ChatElement InitMessage = new ChatElement()
            {
                Color = Color.White,
                Date = DateTime.Now,
                Message = "Chat Initialized !",
                Name = "Info",
                Opacity = 5f,
                DisplayName = true, 
            }; 
            messages = new List<ChatElement>() { InitMessage };
            //origin = new Vector2(0, -Texture.Height);
            TextinputBox.OnValidation += OnValidation;
        }

        private void OnMouseClick(object sender, List<Sprite> sprites, Controler controler)
        {
            Focus(sprites.Where(s => s is IFocusable).ToList());
        }

        private void OnValidation(string message)
        {
            AddMessageToTerminal(message, Name, FontColor); 
        }

        public void AddMessageToTerminal(string message, string name, Color color)
        {
            if (name == null || color == null || message == null)
                return;
           //Wrap 
            string[] SplitMessage = message.Split('\n');
            List<string> wrappedMessages = new List<string>(); 
            for(int j = 0; j < SplitMessage.Length; j++)
            {
                string[] words = SplitMessage[j].Split(' ');
                StringBuilder sb = new StringBuilder();
                List<string> wrappedLine = new List<string>(); 
                for (int i = 0; i < words.Length; i++)
                {
                    if (font.MeasureString("[" + name + "] : " + sb.ToString() + words[i] + " ").X < width)
                    {
                        sb.Append(words[i] + " ");
                    }
                    else if (font.MeasureString(sb.ToString() + words[i] + " ").X < width && j != 0 )
                    {
                        sb.Append(words[i] + " ");
                    }
                    else
                    {
                        wrappedLine.Add(sb.ToString());
                        sb.Clear();
                        sb.Append(words[i] + " ");
                    }
                }
                if(!string.IsNullOrWhiteSpace(sb.ToString()))
                    wrappedLine.Add(sb.ToString());
                wrappedMessages.AddRange(wrappedLine); 
            }
            //endWrap
            for(int i = 0; i < wrappedMessages.Count; i++)
            {
                ChatElement chatElement = new ChatElement()
                {
                    Color = color,
                    Date = DateTime.Now,
                    Message = wrappedMessages[i],
                    Opacity = 5f,
                    Name = name,
                    DisplayName = (i==0), 
                };
                messages.Add(chatElement);
            }
        }

        public void AddMessageToTerminal(string message)
        {
            AddMessageToTerminal(message, Name, Color.Black); 
        }

        public void ToggleChatInputOn()
        {
            IsTakingKeyboardInput = !IsTakingKeyboardInput;
        }

        public void KeyboardListener(Keys[] keys, KeyboardUtils keyboardUtils)
        {
            if (!IsFocused)
                return; 

            if (keyboardUtils.Contains(keys, Keys.Left))
                MoveCursor(-1);
            if (keyboardUtils.Contains(keys, Keys.Right))
                MoveCursor(1);

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
            base.Update(gameTime, sprites, controler);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            base.Draw(spriteBatch);
            int j = messages.Count - 1;
            if (j == -1)
            { return; }
            for (int i = 0; i < messages.Count; i++)
            {
                spriteBatch.DrawString(font, messages[j].ToString(), new Vector2(Position.X, Position.Y + _texture.Height - (i*25)), messages[j].Color, 0f, new Vector2(0, 30), 1f, SpriteEffects.None, layerDepth - .01f);
                j--;
            }
            TextinputBox.Draw(spriteBatch);
        }
        public bool ToggleFocus(List<Sprite> focusables)
        {
            if (!TextinputBox.isFocused)
            {
                foreach (Sprite s in focusables)
                {
                    if (s is IFocusable)
                        (s as IFocusable).IsFocused = false;
                    else
                        throw new ArgumentException("You need to send a List<IFocusable to this function>");
                }
            }
            TextinputBox.isFocused = !TextinputBox.isFocused;
            return TextinputBox.isFocused; 
        }
        public void Focus(List<Sprite> focusables)
        {
            foreach(Sprite s in focusables)
            {
                if (s is IFocusable)
                    (s as IFocusable).IsFocused = false;
                else
                    throw new ArgumentException("You need to send a List<IFocusable to this function>"); 
            }
            TextinputBox.isFocused = true; 
        }
        public void AddChar(char c)
        {
            TextinputBox.AddChar(c);
        }
        public void RemoveChar(bool nextChar)
        {
            TextinputBox.RemoveChar(nextChar);
        }

        public string Validate()
        {
            string text = TextinputBox.Validate();
            if (checkQuery(text))
            {
                return text; 
            }
            if (controler.NetworkManager.IsConnectedToAServer)
            {
                controler.NetworkManager.SendMessageToServer(text);
                UnFocus(); 
                return text; 
            }
            AddMessageToTerminal(text);
            UnFocus(); 
            return text; 
        }

        private bool checkQuery(string text)
        {
            if(text.Length > 0 && text[0] == '/')
            {
                string[] commande = text.Split(' ');
                switch (commande[0].ToLower())
                {
                    case "/tp":

                        break;
                    case "/changename":
                        if (commande.Length == 2)
                            changeName(commande);
                        else
                            AddMessageToTerminal("Parameters problem", "Error", new Color(181, 22, 11));
                        break;
                    case "/w":

                        break;
                    case "/help":
                        AddMessageToTerminal("/tp <X> <Y> or <Username> - Teleporte someone at specific coordinate or to someone \n/changeName <newName> - Change your name\n/w <Username> Send a private message to someone", "Info", Color.White); 
                        break;
                    default:
                        AddMessageToTerminal("Unknown query '" + commande[0] + "', type /help for more information.", "Error", new Color(181, 22, 11));
                        break; 
                }
                return true; 
            }
            else
                return false; 
        }

        private void changeName(string[] commande)
        {
            if (controler.NetworkManager.IsConnectedToAServer)
            {
                controler.NetworkManager.RequestNameChange(commande[1]); 
                return; 
            }
            else
            {
                
                Name = commande[1];
                AddMessageToTerminal("Successfully changed name to : " + Name, "Info", Color.Green);
                return; 
            }
        }

        public void UnFocus()
        {
            IsFocused = false;
        }

        public void MoveCursor(int i)
        {
            TextinputBox.MoveCursor(i);
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
        public bool DisplayName;
        public override string ToString()
        {
            if(DisplayName)
                return "[" + Name + "] : " + Message;
            return Message;

        }
    }
}
