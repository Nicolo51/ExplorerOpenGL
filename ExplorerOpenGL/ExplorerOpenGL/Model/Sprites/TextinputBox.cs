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
        Dictionary<Keys, KeyCodes> inUse;
        public bool isFocused { get; private set; }
        public delegate void ValidateEventHandler(string message);
        public event ValidateEventHandler OnValidation;
        public TextinputBox(Texture2D Texture, SpriteFont SpriteFont)
        {
            _texture = Texture;
            spriteFont = SpriteFont;
            opacity = 1f;
            layerDepth = .09f;
            isFocused = false;
            inputText = new StringBuilder();
            azerty = new Dictionary<Keys, KeyCodes>();
            InitAzerty();
            inUse = azerty;
        }

        public void AddKeyStroke(Keys input, KeyAlterer keyAlterer)
        {
            if(input == Keys.Enter && keyAlterer != KeyAlterer.Cap)
            {
                Validate(inputText.ToString());
                inputText.Clear();
                isFocused = false;
                return;
            }
            if (input == Keys.Back && inputText.Length > 0)
            {
                inputText.Remove(inputText.Length - 1, 1);
                return;
            }
                if (!inUse.ContainsKey(input) || !isFocused)
                return;
            switch (keyAlterer)
            {
                case KeyAlterer.None:
                    inputText.Append(inUse[input].Reg);
                    break;
                case KeyAlterer.Cap:
                    inputText.Append(inUse[input].Cap);
                    break;
                case KeyAlterer.AltGr:
                    inputText.Append(inUse[input].AltGr);
                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(spriteFont, inputText.ToString(), Position, Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth - .01f);
            base.Draw(spriteBatch);
        }
        protected virtual void Validate(string message)
        {
            OnValidation?.Invoke(message);
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
            azerty.Add(Keys.A, new KeyCodes("aA "));
            azerty.Add(Keys.B, new KeyCodes("bB "));
            azerty.Add(Keys.C, new KeyCodes("cC "));
            azerty.Add(Keys.D, new KeyCodes("dD "));
            azerty.Add(Keys.E, new KeyCodes("eE "));
            azerty.Add(Keys.F, new KeyCodes("fF "));
            azerty.Add(Keys.G, new KeyCodes("gG "));
            azerty.Add(Keys.H, new KeyCodes("hH "));
            azerty.Add(Keys.I, new KeyCodes("iI "));
            azerty.Add(Keys.J, new KeyCodes("jJ "));
            azerty.Add(Keys.K, new KeyCodes("kK "));
            azerty.Add(Keys.L, new KeyCodes("lL "));
            azerty.Add(Keys.M, new KeyCodes("mM "));
            azerty.Add(Keys.N, new KeyCodes("nN "));
            azerty.Add(Keys.O, new KeyCodes("oO "));
            azerty.Add(Keys.P, new KeyCodes("pP "));
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
