using ExplorerOpenGL.Controlers;
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
        private int viewChar; 
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
            keyboardUtils.KeyPressed += ArrowKeyPressed;
        }
        
        public void Clear()
        {
            inputText.Clear();
            indexEndDrawing = 0;
            indexStartDrawing = 0; 
        }

        public void AddChar(char c)
        {
            cursorOpacity = 1;
            cursorTimer = 0f; 
            inputText.Insert(cursorIndex, c); 
            cursorIndex++;
            if(cursorIndex != inputText.Length)
            {
                ComputeIndexCurrentToDraw(); 
            }
            else
            {
                ComputeIndexEndToDraw(); 
            }
        }
        
        private Vector2 ComputeIndexEndToDraw()
        {
            for (indexStartDrawing = 0; spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing)).X > width; indexStartDrawing++) ;
            indexEndDrawing = inputText.Length - indexStartDrawing;
            cursorPosition.X = spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing)).X - 3;
            return Vector2.Zero; 
        }

        private Vector2 ComputeIndexCurrentToDraw()
        {
            viewChar = cursorIndex - indexStartDrawing;
            if (viewChar < 1)
            {
                indexStartDrawing -= 8;
                if (indexStartDrawing < 0)
                    indexStartDrawing = 0; 
                viewChar = 1;
            }
            indexEndDrawing = 0; 
            while(indexStartDrawing + indexEndDrawing < inputText.Length && spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing, indexEndDrawing)).X < width - 30)
            {
                indexEndDrawing++;
            }

            float lengthStringToDisplay = spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing, indexEndDrawing)).X;
            float lengthAfterInput = spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing, viewChar)).X;

            while(lengthAfterInput > lengthStringToDisplay || lengthAfterInput > width)
            {
                indexStartDrawing++;
                viewChar--;
                lengthAfterInput = spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing, viewChar)).X;
            }

            Debug.WriteLine(lengthStringToDisplay + "|" + lengthAfterInput);

            //if()


            cursorPosition.X = spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing)).X - 3;
            return Vector2.Zero;
        }

        private Vector2 ComputeIndexStartToDraw()
        {
            for (indexEndDrawing = 0; indexStartDrawing + indexEndDrawing + 1 > inputText.Length && spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing, indexEndDrawing)).X > width; indexEndDrawing++) ; 
            cursorPosition.X = spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing)).X - 3;
            return Vector2.Zero;
        }

        public override void Update(GameTime gameTime, List<Sprite> sprites, Controler controler)
        {
            if (IsFocused)
            {
                cursorTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (cursorTimer > .9f)
                {
                    cursorOpacity = (cursorOpacity == 1) ? 0 : 1;
                    cursorTimer = 0f;
                }
                if(spriteFont.MeasureString(inputText.ToString()).X < width)
                {
                    indexStartDrawing = 0;
                    indexEndDrawing = inputText.Length; 
                }
                viewChar = cursorIndex - indexStartDrawing;
                if (viewChar < 0)
                {
                    indexStartDrawing -= 5;
                    if (indexStartDrawing < 0)
                        indexStartDrawing = 0; 
                    ComputeIndexCurrentToDraw(); 
                    viewChar = 0;
                    //cursorIndex++;
                }

                cursorPosition = new Vector2(spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing, viewChar)).X - 3, -1) + Position; 
            }
            else
                cursorOpacity = 0; 

            base.Update(gameTime, sprites, controler);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.DrawString(spriteFont, cursorIndex.ToString() +'|' + viewChar.ToString(), Vector2.Zero, Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth - .01f);
            spriteBatch.DrawString(spriteFont, "|", cursorPosition, Color.White, 0f, Vector2.Zero, cursorOpacity, SpriteEffects.None, layerDepth - .01f);
            spriteBatch.DrawString(spriteFont, inputText.ToString().Substring(indexStartDrawing, indexEndDrawing), Position, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth - .01f);
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
            Clear();
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
            Opacity = 0f;
            if (DoEraseWhenUnfocused)
                Clear(); 
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
                if (nextChar)
                {
                    inputText.Remove(cursorIndex, 1);
                    ComputeIndexCurrentToDraw(); 
                }
                else
                {
                    inputText.Remove(cursorIndex - 1, 1);
                    cursorIndex -= 1;
                    viewChar = cursorIndex - indexStartDrawing;
                    if (viewChar < 1)
                    {
                        ComputeIndexCurrentToDraw(); 
                    }
                    else
                    {
                        ComputeIndexCurrentToDraw();
                    }
                }
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
        }

        private void ArrowKeyPressed(Keys[] keys, KeyboardUtils keyboardUtils)
        {
            if (!IsFocused)
                return;
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
