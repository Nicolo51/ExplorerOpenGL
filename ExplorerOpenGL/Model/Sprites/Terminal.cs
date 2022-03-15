using ExplorerOpenGL.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExplorerOpenGL.Model.Sprites
{
    public class Terminal : Sprite 
    {
        List<ChatElement> messages;
        SpriteFont font;
        public Color FontColor; 
        private KeyboardManager keyboardManager;
        private NetworkManager networkManager;
        private FontManager fontManager;
        private TextureManager textureManager;
        private TextinputBox terminalTexintput;
        private int height;
        private int width; 


        private bool IsNewMessage { get 
            {
                if(messages[messages.Count-1].Opacity <= 0f)
                {
                    return false;
                }
                return true; 
            } 
        }

        public bool IsFocused { get; set; }

        public Terminal(Texture2D texture, SpriteFont Font)
            :base(texture)
        {
            networkManager = NetworkManager.Instance; 
            keyboardManager = KeyboardManager.Instance;
            fontManager = FontManager.Instance;
            textureManager = TextureManager.Instance;
            height = 500; 
            width = texture.Width;
            IsHUD = true; 
            font = Font;
            layerDepth = .1f; 
            _texture = texture; 
            Opacity = .5f;

            terminalTexintput = new TextinputBox(textureManager.CreateTextureThread(700, 35, paint => Color.Black * .8f), fontManager.GetFont("Default"), true, true) { IsHUD = true, Position = new Vector2(0, 695), Opacity = 0f, };
            terminalTexintput.Validated += OnTextinputValidation;
            keyboardManager.KeyPressedSubTo(Keys.Enter, OnEnterPress);
            keyboardManager.KeyPressedSubTo(Keys.Escape, OnEscapePress);
            keyboardManager.TextInputed += KeyboardManager_TextInputed;


            keyboardManager.OnSpriteAdded(terminalTexintput, this);
            messages = new List<ChatElement>();
        }

        private void KeyboardManager_TextInputed(TextInputEventArgs e)
        {
            if (e.Character == '/' && !terminalTexintput.IsFocused && !keyboardManager.IsTextInputBoxFocused)
            {
                terminalTexintput.Clear();
                terminalTexintput.Focus();
            }
        }

        private void OnEnterPress()
        {
            if (keyboardManager.focusedTextInput == null || keyboardManager.focusedTextInput == terminalTexintput)
                terminalTexintput.ToggleFocus(true);
        }
        private void OnEscapePress()
        {
            terminalTexintput.UnFocus();
        }

        public void OnTextInput(object sender, TextInputEventArgs e)
        {
           
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
                    Name = name,
                    DisplayName = (i==0), 
                };
                messages.Add(chatElement);
            }
        }

        public void AddMessageToTerminal(string message)
        {
            if (gameManager.Player != null)
            {
                AddMessageToTerminal(message, gameManager.Player.Name, Color.White);
                return; 
            }
            AddMessageToTerminal(message, "System", Color.White);
        }

        public override void Update(Sprite[] sprites)
        {
            for(int i = 0; i < messages.Count; i++)
            {
                ChatElement message = messages[i]; 
                if (i > messages.Count)
                    continue;
                if (message.Timer < 0)
                    message.Opacity -= .01f;
                else
                    message.Timer -= timeManager.ElapsedUpdate.TotalSeconds;
            }
            if (keyboardManager.IsKeyDown(Keys.H) && !keyboardManager.IsTextInputBoxFocused){
                for (int i = 0; i < messages.Count; i++)
                {
                    if (i > messages.Count)
                        continue;
                    messages[i].Opacity = .5f;
                }
            }
            terminalTexintput.Update( sprites); 
            base.Update(sprites);
        }



        public override void Draw(SpriteBatch spriteBatch, float lerpAmount)
        {
            for (int i = messages.Count - 1; i >= 0; i--)
            {
                spriteBatch.Draw(_texture, new Vector2(Position.X, Position.Y + height - ((messages.Count - 1 - i) * 30)), null, Color.White * messages[i].Opacity, 0f, new Vector2(0, 30), 1f ,  SpriteEffects.None, layerDepth - .01f);
                spriteBatch.DrawString(font, messages[i].ToString(), new Vector2(Position.X, Position.Y + height - ((messages.Count - 1 - i) *25)), messages[i].Color * messages[i].Opacity * 2, 0f, new Vector2(0, 30), 1f, SpriteEffects.None, layerDepth - .02f);
            }
            terminalTexintput.Draw(spriteBatch, lerpAmount);
        }
        
        public void OnTextinputValidation(string s, TextinputBox t)
        {
            if (checkQuery(s))
            {
                return; 
            }
            if (networkManager.IsConnectedToAServer)
            {
                networkManager.SendMessageToServer(s);
                return; 
            }
            AddMessageToTerminal(s);
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
            if (gameManager.Player == null)
            {
                AddMessageToTerminal("Can't find instance of a player", "Error", Color.Red);
                return;
            }
            if (networkManager.IsConnectedToAServer)
            {
                networkManager.RequestNameChange(commande[1]); 
                return; 
            }
            else
            {
                gameManager.Player.ChangeName(commande[1]);
                AddMessageToTerminal("Successfully changed name to : " + commande[1], "Info", Color.Green);
                return; 
            }
        }
    }
    
    class ChatElement
    {
        public string Message;
        public DateTime Date;
        public string Name;
        public Color Color;
        public double Timer; 
        public float Opacity;
        public bool DisplayName;
        public ChatElement()
        {
            Opacity = .5f;
            Timer = 5f; 
        }

        public override string ToString()
        {
            if(DisplayName)
                return "[" + Name + "] : " + Message;
            return Message;

        }
    }
}
