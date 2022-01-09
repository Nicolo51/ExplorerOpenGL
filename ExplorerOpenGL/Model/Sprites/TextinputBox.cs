using ExplorerOpenGL.Controlers;
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
    public class TextinputBox : Sprite 
    {
        StringBuilder inputText;
        readonly Dictionary<Keys, KeyCodes> azerty;
        readonly Dictionary<Keys, KeyCodes> qwerty;
        SpriteFont spriteFont;
        KeyboardUtils keyboardUtils; 
        Dictionary<Keys, KeyCodes> inUse;
        private int indexStartDrawing;
        private int indexEndDrawing;

        private int cursorIndex;
        private Vector2 cursorPosition;
        private int cursorOpacity;
        private float cursorTimer; 

        public int width { get { return _texture.Width; } }

        public bool IsFocused { get; private set; }
        public bool DoEraseWhenUnfocused { get; private set; }

        public delegate void ValidateEventHandler(string message, TextinputBox textinput);
        public event ValidateEventHandler Validated;

        public TextinputBox(Texture2D texture, SpriteFont SpriteFont, KeyboardUtils KeyboardUtils, bool eraseWhenUnfocused)
            :base(texture)
        {
            DoEraseWhenUnfocused = eraseWhenUnfocused; 
            cursorIndex = 0;
            cursorOpacity = 1;
            cursorTimer = 0f; 

            indexStartDrawing = 0; 
            MouseClicked += OnMouseClick;
            _texture = texture;
            spriteFont = SpriteFont;
            Opacity = 1f;
            layerDepth = .09f;
            IsFocused = false;
            IsClickable = true;
            inputText = new StringBuilder();
            azerty = new Dictionary<Keys, KeyCodes>();
            keyboardUtils = KeyboardUtils; 
            InitAzerty();
            inUse = azerty;
        }
        
        public void Clear()
        {
            inputText.Clear(); 
        }

        public void AddChar(char c)
        {
            inputText.Insert(cursorIndex, c); 
            ComputeIndexRangeToDraw();
            cursorIndex++;
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
        private Vector2 ComputeIndexRangeToDraw()
        {
            for (indexStartDrawing = 0; spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing)).X > width; ) ;
            return new Vector2(indexStartDrawing, indexEndDrawing); 
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
            if (IsFocused)
            {
                cursorTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                //if (cursorTimer > .9f)
                //{
                //    cursorOpacity = (cursorOpacity == 1) ? 0 : 1;
                //    cursorTimer = 0f;
                //}
                if(cursorIndex - indexStartDrawing < 2 && cursorIndex > 2)
                {
                    indexStartDrawing -= 2; 
                }
                cursorPosition = new Vector2(spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing, cursorIndex - indexStartDrawing)).X - 3, -1) + Position;
            }
            else
                cursorOpacity = 1; 

            base.Update(gameTime, sprites, controler);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.DrawString(spriteFont, cursorIndex.ToString(), Vector2.Zero, Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth - .01f);
            spriteBatch.DrawString(spriteFont, "|", cursorPosition, Color.White, 0f, Vector2.Zero, cursorOpacity, SpriteEffects.None, layerDepth - .01f);
            spriteBatch.DrawString(spriteFont, inputText.ToString().Substring(indexStartDrawing), Position, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth - .01f);
        }

        public void OnMouseClick(object sender, List<Sprite> sprites, Controler controler)
        {
            Focus(sprites.Where(s => s is TextinputBox).ToList());
        }

        public string Validate()
        {
            string output = inputText.ToString();
            cursorIndex = 0;
            cursorPosition.X = 0; 
            inputText.Clear();
            Validated?.Invoke(output.Trim(), this);
            return output;
        }

        public void MoveCursor(int i)
        {
            if (!IsFocused)
                return;
            if(cursorIndex + i >= 0 && cursorIndex +i <= inputText.Length)
                cursorIndex += i; 
        }

        public void UnFocus()
        {
            keyboardUtils.KeyPressed -= ArrowKeyPressed;
            Opacity = 0f;
            if (DoEraseWhenUnfocused)
                inputText.Clear(); 
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

        internal void SlashPressed()
        {
            throw new NotImplementedException();
        }

        public void RemoveChar(bool nextChar = false)
        {
            if (inputText.Length > 0 && ((!nextChar && cursorIndex>0) || (nextChar && cursorIndex < inputText.Length)))
            {
                inputText.Remove(cursorIndex - (nextChar ? 0: 1), 1);
                cursorIndex += nextChar ? 0 : -1;
                //ComputeIndexStartDrawing(); 
                return;
            }
        }

        public void Focus(List<Sprite> focusables)
        {
            foreach (TextinputBox t in focusables)
            {
                if(t != this)
                    t.UnFocus(); 
            }
            IsFocused = true;
            Opacity = 1f; 
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

        public bool ToggleFocus(List<Sprite> sprites, bool validate = false)
        {
            if (!IsFocused)
            {
                Focus(sprites);
            }
            else if (IsFocused)
            {
                if (validate)
                    Validate();
                UnFocus(); 
            }
            return IsFocused; 
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
