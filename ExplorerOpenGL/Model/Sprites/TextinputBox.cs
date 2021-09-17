using ExplorerOpenGL.Controlers;
using ExplorerOpenGL.Model.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model.Sprites
{
    public class TextinputBox : Sprite, IFocusable
    {
        StringBuilder inputText;
        readonly Dictionary<Keys, KeyCodes> azerty;
        readonly Dictionary<Keys, KeyCodes> qwerty;
        SpriteFont spriteFont;
        KeyboardUtils keyboardUtils; 
        Dictionary<Keys, KeyCodes> inUse;
        private int indexStartDrawing; 
        public bool isFocused;

        private int cursorIndex;
        private Vector2 cursorPosition;
        private int cursorOpacity;
        private float cursorTimer; 

        public int width { get { return _texture.Width; } }

        public bool IsFocused { get => IsFocused; set => IsFocused = value; }

        public delegate void ValidateEventHandler(string message);
        public event ValidateEventHandler OnValidation;

        public TextinputBox(Texture2D Texture, SpriteFont SpriteFont, KeyboardUtils KeyboardUtils)
        {
            cursorIndex = 0;
            cursorOpacity = 1;
            cursorTimer = 0f; 

            indexStartDrawing = 0; 
            MouseClicked += OnMouseClick;
            _texture = Texture;
            spriteFont = SpriteFont;
            opacity = 1f;
            layerDepth = .09f;
            isFocused = false;
            IsClickable = true;
            inputText = new StringBuilder();
            azerty = new Dictionary<Keys, KeyCodes>();
            keyboardUtils = KeyboardUtils; 
            InitAzerty();
            inUse = azerty;
        }
            
        public void AddChar(char c)
        {
            inputText.Insert(cursorIndex, c); 
            ComputeIndexStartDrawing();
            cursorIndex++;
        }

        private int ComputeIndexStartDrawing()
        {
            for (indexStartDrawing = 0; spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing)).X > width; indexStartDrawing++) ;
            return indexStartDrawing; 
        }

        public void AddKeyStroke(Keys input, KeyAlterer keyAlterer)
        {
            //if(input == Keys.Enter && keyAlterer != KeyAlterer.Cap)
            //{
            //    Validate(inputText.ToString());
            //    inputText.Clear();
            //    isFocused = false;
            //    return;
            //}
            //if (input == Keys.Back && inputText.Length > 0)
            //{
            //    inputText.Remove(inputText.Length - 1, 1);
            //    return;
            //}
            //    if (!inUse.ContainsKey(input) || !isFocused)
            //    return;

            //switch (keyAlterer)
            //{
            //    case KeyAlterer.None:
            //        inputText.Append(inUse[input].Reg);
            //        break;
            //    case KeyAlterer.Cap:
            //        inputText.Append(inUse[input].Cap);
            //        break;
            //    case KeyAlterer.AltGr:
            //        inputText.Append(inUse[input].AltGr);
            //        break;
            //}
        }

        public override void Update(GameTime gameTime, List<Sprite> sprites, Controler controler)
        {
            //cursorTimer += (float)gameTime.ElapsedGameTime.TotalSeconds; 
            if(cursorTimer > .9f)
            {
                cursorOpacity = (cursorOpacity == 1) ? 0 : 1;
                cursorTimer = 0f; 
            }
            cursorPosition = new Vector2(spriteFont.MeasureString(inputText.ToString().Substring(0, cursorIndex)).X-3, -1) + Position; 
            base.Update(gameTime, sprites, controler);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.DrawString(spriteFont, cursorIndex.ToString(), Vector2.Zero, Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth - .01f);
            spriteBatch.DrawString(spriteFont, "|", cursorPosition, Color.Black, 0f, Vector2.Zero, cursorOpacity, SpriteEffects.None, layerDepth - .01f);
            spriteBatch.DrawString(spriteFont, inputText.ToString().Substring(indexStartDrawing), Position, Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth - .01f);
        }

        public void OnMouseClick(object sender, List<Sprite> sprites, Controler controler)
        {
            Focus(sprites.Where(s => s is IFocusable).ToList());
        }

        public string Validate()
        {
            string output = inputText.ToString();
            cursorIndex = 0; 
            inputText.Clear();
            return output;
        }

        public void MoveCursor(int i)
        {
            if (!isFocused)
                return;
            if(cursorIndex + i >= 0 && cursorIndex +i <= inputText.Length)
                cursorIndex += i; 
        }

        public void UnFocus()
        {
            keyboardUtils.KeyPressed -= ArrowKeyPressed; 
            IsFocused = false;
        }

        private void InitAzerty()
        {
            azerty.Add(Keys.Space, new KeyCodes("   "));
            azerty.Add(Keys.D0, new KeyCodes("à0@"));
            azerty.Add(Keys.D1, new KeyCodes("&1 "));
            azerty.Add(Keys.D2, new KeyCodes("é2~"));
            azerty.Add(Keys.D3, new KeyCodes("\"3#"));
            azerty.Add(Keys.D4, new KeyCodes("\'4{"));
            azerty.Add(Keys.D5, new KeyCodes("(5["));
            azerty.Add(Keys.D6, new KeyCodes("-6|"));
            azerty.Add(Keys.D7, new KeyCodes("è7`"));
            azerty.Add(Keys.D8, new KeyCodes("`_8\\"));
            azerty.Add(Keys.D9, new KeyCodes("ç9^"));
            azerty.Add(Keys.OemPlus, new KeyCodes("=+}"));
            azerty.Add(Keys.A, new KeyCodes("aA*"));
            azerty.Add(Keys.B, new KeyCodes("bBù"));
            azerty.Add(Keys.C, new KeyCodes("cC^"));
            azerty.Add(Keys.D, new KeyCodes("dD "));
            azerty.Add(Keys.E, new KeyCodes("eE$"));
            azerty.Add(Keys.F, new KeyCodes("fF "));
            azerty.Add(Keys.G, new KeyCodes("gG "));
            azerty.Add(Keys.H, new KeyCodes("hH "));
            azerty.Add(Keys.I, new KeyCodes("iI "));
            azerty.Add(Keys.J, new KeyCodes("jJ "));
            azerty.Add(Keys.K, new KeyCodes("kK "));
            azerty.Add(Keys.L, new KeyCodes("lL "));
            azerty.Add(Keys.M, new KeyCodes("mM "));
            azerty.Add(Keys.N, new KeyCodes("nN "));
            azerty.Add(Keys.O, new KeyCodes("oOµ"));
            azerty.Add(Keys.P, new KeyCodes("pP%"));
            azerty.Add(Keys.Q, new KeyCodes("qQ "));
            azerty.Add(Keys.R, new KeyCodes("rR "));
            azerty.Add(Keys.S, new KeyCodes("sS "));
            azerty.Add(Keys.T, new KeyCodes("tT "));
            azerty.Add(Keys.U, new KeyCodes("uU "));
            azerty.Add(Keys.V, new KeyCodes("vV "));
            azerty.Add(Keys.W, new KeyCodes("wW "));
            azerty.Add(Keys.X, new KeyCodes("xX "));
            azerty.Add(Keys.Y, new KeyCodes("yY "));
            azerty.Add(Keys.Z, new KeyCodes("zZ "));
            azerty.Add(Keys.OemComma, new KeyCodes(",? "));
            azerty.Add(Keys.OemSemicolon, new KeyCodes(";. "));
            azerty.Add(Keys.OemBackslash, new KeyCodes("<> "));
            azerty.Add(Keys.Enter, new KeyCodes(" \n "));
        }

        public void RemoveChar()
        {
            if (inputText.Length > 0)
            {
                inputText.Remove(cursorIndex - 1, 1);
                cursorIndex--;
                ComputeIndexStartDrawing(); 
                return;
            }
        }

        public void Focus(List<Sprite> sprites)
        {
            foreach (Sprite s in sprites)
            {
                if (s is IFocusable)
                    (s as IFocusable).IsFocused = false;
                else
                    throw new ArgumentException("You need to send a List<IFocusable to this function>");
            }
            isFocused = true;
            keyboardUtils.KeyPressed += ArrowKeyPressed;
        }

        private void ArrowKeyPressed(Keys[] keys, KeyboardUtils keyboardUtils)
        {
            if (keyboardUtils.Contains(keys, Keys.Left))
            {
                MoveCursor(-1); 
            }
            if (keyboardUtils.Contains(keys, Keys.Right))
            {
                MoveCursor(1);
            }
        }

        public bool ToggleFocus(List<Sprite> sprites)
        {
            if (!isFocused)
            {
                foreach (Sprite s in sprites)
                {
                    if (s is IFocusable)
                        (s as IFocusable).IsFocused = false;
                    else
                        throw new ArgumentException("You need to send a List<IFocusable to this function>");
                }
            }
            isFocused = !isFocused;
            return isFocused; 
        }

    }
    class KeyCodes
    {
        public char Reg { get; private set; }
        public char Cap { get; private set; }
        public char AltGr { get; private set; }

        public KeyCodes(string regCapAltGr)
        {
            if (regCapAltGr.Length > 4)
                throw new Exception("too much char");
            Reg = regCapAltGr[0];
            Cap = regCapAltGr[1];
            AltGr = regCapAltGr[2];
        }

    }
    public enum KeyAlterer
    {
        None = 0,
        Cap = 1,
        AltGr = 2,
    }
    
}
