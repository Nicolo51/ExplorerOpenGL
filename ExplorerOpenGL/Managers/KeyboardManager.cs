using ExplorerOpenGL.Model;
using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers
{
    public class KeyboardManager
    {
        private GameManager gameManager; 

        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;

        public delegate void KeyPressedEventHandler(Keys[] keys, KeyboardManager KeyboardManager);
        public event KeyPressedEventHandler KeyPressed;

        public delegate void KeyReleasedEventHandler(Keys[] keys, KeyboardManager KeyboardManager);
        public event KeyReleasedEventHandler KeyRealeased;

        public delegate void TextInputedEventHandler(TextInputEventArgs e);
        public event TextInputedEventHandler TextInputed;

        private List<TextinputBox> textinputBoxes;
        private TextinputBox focusedTextInput; 
        public bool IsTextInputBoxFocused { get { return (focusedTextInput != null); } }

        public static event EventHandler Initialized;
        private static KeyboardManager instance;
        public static KeyboardManager Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new KeyboardManager();
                    Initialized?.Invoke(instance, EventArgs.Empty);
                    return instance; 
                }
                return instance;
            }
        }

        private KeyboardManager()
        {
            currentKeyboardState = Keyboard.GetState();
        }

        public void InitDependencies()
        {
            gameManager = GameManager.Instance;
            gameManager.SpriteAdded += OnSpriteAdded;
        }

        private void OnSpriteAdded(Sprite sprite, object issuer)
        {
            if(sprite is TextinputBox)
            {
                textinputBoxes.Add(sprite as TextinputBox); 
            }
        }

        public bool Contains(Keys[] keys, Keys seekingKey)
        {
            int index = Array.IndexOf(keys, seekingKey);
            if (index > -1)
                return true;
            return false; 
        }

        public void OnTextInput(object sender, TextInputEventArgs e)
        {
            if (e.Character == '/' && !gameManager.TerminalTexintput.IsFocused && focusedTextInput != null)
            {
                gameManager.TerminalTexintput.Clear();
                gameManager.TerminalTexintput.Focus();
            }
            if(focusedTextInput != null)
                ProcessTextInput(e, focusedTextInput);
        }
        public void Update()
        {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            if (previousKeyboardState == currentKeyboardState)
                return;

            Keys[] currentPressedKeys = currentKeyboardState.GetPressedKeys();
            Keys[] previousPressedKeys = previousKeyboardState.GetPressedKeys();

            Keys[] NewKeys = GetPressedKey(currentPressedKeys, previousPressedKeys).GetArray();
            Keys[] LostKeys = GetReleasedKey(currentPressedKeys, previousPressedKeys).GetArray();


            if (NewKeys.Length > 0) 
            {
                OnKeyPressed(NewKeys);
            }
            if (LostKeys.Length > 0) 
            {
                OnKeyRelease(LostKeys); 
            }

        }

        public void ProcessTextInput(TextInputEventArgs e, TextinputBox t)
        {
            switch(e.Key)
            {
                case Keys.Delete:
                    t.RemoveChar(true);
                    break; 
                case Keys.Back:
                    t.RemoveChar();
                    break;
                case Keys.Escape:
                    t.UnFocus();
                    break;
                case Keys.Enter:
                    break; 
                default: 
                    t.AddChar(e.Character);
                    break; 
            }
        }

        private KeysArray GetReleasedKey(Keys[] currentPressedKeys, Keys[] previousPressedKeys)
        {
            List<Keys> KeyReleased = new List<Keys>();

            for (int i = 0; i < previousPressedKeys.Length; i++)
            {
                bool Released = true;
                for (int j = 0; j < currentPressedKeys.Length; j++)
                {
                    if (previousPressedKeys[i] == currentPressedKeys[j])
                    {
                        Released = false;
                        break;
                    }
                }
                if (Released)
                {
                    KeyReleased.Add(previousPressedKeys[i]);
                }
            }
            return new KeysArray(KeyReleased.ToArray()); 
        }

        private KeysArray GetPressedKey(Keys[] currentPressedKeys, Keys[] previousPressedKeys)
        {

            List<Keys> KeyPressed = new List<Keys>();

            for (int i = 0; i < currentPressedKeys.Length; i++)
            {
                bool Pressed = true;
                for (int j = 0; j < previousPressedKeys.Length; j++)
                {
                    if (previousPressedKeys[j] == currentPressedKeys[i])
                    {
                        Pressed = false;
                        break;
                    }
                }
                if (Pressed)
                {
                    KeyPressed.Add(currentPressedKeys[i]);
                }
            }
            return new KeysArray(KeyPressed.ToArray());
        }

        public bool IsKeyDown(Keys key)
        {
            return Keyboard.GetState().IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key)
        {
            return Keyboard.GetState().IsKeyUp(key); 
        }

        private void OnKeyRelease(Keys[] keys)
        {
            KeyRealeased?.Invoke(keys, this);
        }

        private void OnKeyPressed(Keys[] keys)
        {
            KeyPressed?.Invoke(keys, this);
        }

        public void UnFocusTextInputBox(TextinputBox ti)//ti is the one which is going to be focused
        {
            foreach(var t in textinputBoxes)
            {
                t.UnFocus(); 
            }
            focusedTextInput = ti; 
        }
    }
}
