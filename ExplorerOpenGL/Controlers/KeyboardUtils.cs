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

namespace ExplorerOpenGL.Controlers
{
    public class KeyboardUtils
    {
        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;

        public delegate void KeyPressedEventHandler(Keys[] keys, KeyboardUtils keyboardUtils);
        public event KeyPressedEventHandler KeyPressed;

        public delegate void KeyReleasedEventHandler(Keys[] keys, KeyboardUtils keyboardUtils);
        public event KeyReleasedEventHandler KeyRealeased;

        public delegate void TextInputedEventHandler(TextInputEventArgs e);
        public event TextInputedEventHandler TextInputed;


        public KeyboardUtils()
        {
            currentKeyboardState = Keyboard.GetState(); 
        }

        public bool Contains(Keys[] keys, Keys seekingKey)
        {
            int index = Array.IndexOf(keys, seekingKey);
            if (index > -1)
                return true;
            return false; 
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

        public void OnTextInput(object sender, TextInputEventArgs e, TextinputBox t)
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
    }
}
