using ExplorerOpenGL.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExplorerOpenGL.Model.Sprites
{
    public class TextinputBox : Sprite
    {
        StringBuilder inputText;
        SpriteFont spriteFont;
        KeyboardManager keyboardManager;
        private int indexStartDrawing;
        private int indexEndDrawing;

        private int cursorIndex;
        private int viewChar;
        private Vector2 cursorPosition;
        private int cursorOpacity;
        private float cursorTimer;
        private bool TriggerMouseOver; 
        public int width { get { return Texture.Width; } }

        public bool IsFocused { get; private set; }
        public bool DoEraseWhenUnfocused { get; private set; }
        public bool MakeItTransparentWhenUnfocused { get; set; }

        public delegate void ValidateEventHandler(string message, TextinputBox textinput);
        public event ValidateEventHandler Validated;

        public delegate void UnfocusEventHandler(string message, TextinputBox textinput);
        public event UnfocusEventHandler Unfocused; 

        public string Text { get { return inputText.ToString().Trim(); } set { Clear(); AddRange(value); } }

        public TextinputBox(Texture2D texture, SpriteFont SpriteFont = null,  bool eraseWhenUnfocused = false, bool makeItTransparentUnfocused = false)
            : base(texture)
        {
            if (SpriteFont == null)
                SpriteFont = FontManager.Instance.GetFont();
            TriggerMouseOver = true; 
            DoEraseWhenUnfocused = eraseWhenUnfocused;
            MakeItTransparentWhenUnfocused = makeItTransparentUnfocused;
            cursorIndex = 0;
            cursorOpacity = 1;
            cursorTimer = 0f;

            indexStartDrawing = 0;
            //_texture = texture;
            spriteFont = SpriteFont;
            Opacity = 1f;
            LayerDepth = .09f;
            IsFocused = false;
            IsClickable = true;
            inputText = new StringBuilder();
            keyboardManager = KeyboardManager.Instance;
            keyboardManager.KeyPressed += ArrowKeyPressed;
            keyboardManager.KeyPressedSubTo(Keys.C, OnCopyShortCut); 
            keyboardManager.KeyPressedSubTo(Keys.V, OnPasteShortCut);
        }

        private void OnPasteShortCut()
        {
            debugManager.AddEvent("textinput event raised");
            if (!this.IsFocused)
                return; 
            if (keyboardManager.IsKeyDown(Keys.RightControl) || keyboardManager.IsKeyDown(Keys.LeftControl))
            {
                string clipText = TextCopy.ClipboardService.GetText();
                if (string.IsNullOrWhiteSpace(clipText))
                    return;
                AddRange(clipText);
            }
        }
        private void OnCopyShortCut()
        {
            if(keyboardManager.IsKeyDown(Keys.RightControl) || keyboardManager.IsKeyDown(Keys.LeftControl))
                TextCopy.ClipboardService.SetText(Text);
        }
        public void Clear()
        {
            inputText.Clear();
            indexEndDrawing = 0;
            indexStartDrawing = 0;
            cursorIndex = 0;
        }

        public void AddRange(string s)
        {
            foreach (char c in s)
                AddChar(c); 
        }


        public void AddChar(char c)
        {
            cursorOpacity = 1;
            cursorTimer = 0f;
            inputText.Insert(cursorIndex, c);
            cursorIndex++;
            if (cursorIndex != inputText.Length)
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
            while (indexStartDrawing + indexEndDrawing < inputText.Length && spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing, indexEndDrawing)).X < width - 15)
            {
                indexEndDrawing++;
            }

            if (indexStartDrawing + viewChar < inputText.Length)
            {
                float lengthStringToDisplay = spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing, indexEndDrawing)).X;
                float lengthAfterInput = spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing, viewChar)).X;

                while (lengthAfterInput > lengthStringToDisplay || lengthAfterInput > width)
                {
                    indexStartDrawing++;
                    viewChar--;
                    lengthAfterInput = spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing, viewChar)).X;
                }

            }
            cursorPosition.X = spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing)).X - 3;
            return Vector2.Zero;
        }

        public override void Update(Sprite[] sprites)
        {
            base.Update(sprites);
            if (IsFocused)
            {
                cursorTimer += (float)timeManager.ElapsedBetweenUpdates.TotalSeconds;
                if (cursorTimer > .9f)
                {
                    cursorOpacity = (cursorOpacity == 1) ? 0 : 1;
                    cursorTimer = 0f;
                }
                if (spriteFont.MeasureString(inputText.ToString()).X < width)
                {
                    indexStartDrawing = 0;
                    indexEndDrawing = inputText.Length;
                }
                viewChar = cursorIndex - indexStartDrawing;
                if (viewChar < 0)
                {
                    indexStartDrawing -= 8;
                    if (indexStartDrawing < 0)
                        indexStartDrawing = 0;
                    ComputeIndexCurrentToDraw();
                    viewChar = 0;
                    //cursorIndex++;
                }
                if (spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing, viewChar)).X > width - 15)
                {
                    indexStartDrawing += 8;
                    if (indexStartDrawing + indexEndDrawing > inputText.Length)
                    {
                        ComputeIndexEndToDraw();
                    }
                    else
                    {
                        ComputeIndexCurrentToDraw();
                    }
                }
                CheckSubstring();
                cursorPosition = new Vector2(spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing, viewChar)).X - 3, -1) + Position;
            }
            else
                cursorOpacity = 0;

        }

        private void CheckSubstring()
        {
            while (indexStartDrawing + viewChar > inputText.Length)
                viewChar--;
            while (indexStartDrawing + indexEndDrawing > inputText.Length)
                indexEndDrawing--;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float lerpAmount, params ShaderArgument[] shaderArgs)
        {
            base.Draw(spriteBatch, gameTime, lerpAmount);
            if (!IsDisplayed)
                return; 
            spriteBatch.DrawString(spriteFont, "|", cursorPosition - Origin, Color.White, 0f, Vector2.Zero, cursorOpacity, SpriteEffects.None, LayerDepth - .01f);
            spriteBatch.DrawString(spriteFont, inputText.ToString().Substring(indexStartDrawing, indexEndDrawing), Position - Origin, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, LayerDepth - .01f);
        }

        public override void OnMouseClick(Sprite[] sprites, Vector2 clickPosition, MousePointer mousePointer)
        {
            if (gameManager.GameState == GameState.Pause)
                return;
            if (IsFocused)
            {
                SetCursor(clickPosition);
                return;
            }
            Focus();
            base.OnMouseClick(sprites, clickPosition, mousePointer);
        }

        public override void OnMouseOver(Sprite[] sprites, MousePointer mousePointer)
        {
            if (!TriggerMouseOver)
                return; 
            mousePointer.SetCursorIcon(MousePointerType.Text);
            base.OnMouseOver(sprites, mousePointer); 
        }
        
        public override void OnMouseLeave(Sprite[] sprites, MousePointer mousePointer)
        {
            mousePointer.SetCursorIcon(MousePointerType.Default);
            base.OnMouseLeave(sprites, mousePointer);

        }

        public void DisableMouseOver()
        {
            TriggerMouseOver = false; 
        }

        public void EnableMouseOver()
        {
            TriggerMouseOver = true;
        }

        public string Validate(bool unFocus = true)
        {
            string output = inputText.ToString();
            cursorIndex = 0;
            cursorPosition.X = 0;
            Clear();
            Validated?.Invoke(output.Trim(), this);
            if (unFocus)
                UnFocus(); 
            return output;
        }

        public void SetCursor(Vector2 position)
        {
            int index = 0;
            while ((index + indexStartDrawing < inputText.Length) && (spriteFont.MeasureString(inputText.ToString().Substring(indexStartDrawing, index)).X < position.X))
            {
                index++;
            }
            cursorIndex = index + indexStartDrawing;
        }

        public void MoveCursor(int i)
        {
            if (!IsFocused)
                return;
            if (cursorIndex + i >= 0 && cursorIndex + i <= inputText.Length)
            {
                cursorIndex += i;
                cursorOpacity = 1;
                cursorTimer = 0f;
            }
        }

        public void UnFocus()
        {
            if (!IsFocused)
                return; 
            if(MakeItTransparentWhenUnfocused)
                Opacity = 0f;
            if (DoEraseWhenUnfocused)
                Clear();
            if(IsFocused)
                keyboardManager.UnFocusTextinputBox(); 
            IsFocused = false;
            Unfocused?.Invoke(Text, this);
        }

        public void RemoveChar(bool nextChar = false)
        {
            if (inputText.Length > 0 && ((!nextChar && cursorIndex > 0) || (nextChar && cursorIndex < inputText.Length)))
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
                    if (cursorIndex == inputText.Length)
                    {
                        ComputeIndexEndToDraw();
                    }
                    else
                    {
                        ComputeIndexCurrentToDraw();
                    }
                }
                return;
            }
        }
        public override void Remove()
        {
            keyboardManager.KeyPressedUnsubTo(Keys.C, OnCopyShortCut);
            keyboardManager.KeyPressedUnsubTo(Keys.V, OnPasteShortCut);
            base.Remove();
        }
        public void Focus()
        {
            
            keyboardManager.FocusTextinput(this);
            gameManager.ChangeGameState(GameState.Typing);
            IsFocused = true;
            Opacity = 1f;
        }

        private void ArrowKeyPressed(KeysArray keys)
        {
            if (!IsFocused)
                return;
            if (keys.Contains(Keys.Left))
            {
                MoveCursor(-1);
            }
            if (keys.Contains(Keys.Right))
            {
                MoveCursor(1);
            }
        }

        public bool ToggleFocus(bool validate = false)
        {
            if (!IsFocused)
            {
                Focus();
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
}
