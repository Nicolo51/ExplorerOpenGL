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

        public delegate void KeySwitchEventHandler(KeysArray keys);
        public event KeySwitchEventHandler KeyPressed;
        public event KeySwitchEventHandler KeyRealeased;

        public delegate void TextInputedEventHandler(TextInputEventArgs e);
        public event TextInputedEventHandler TextInputed;

        public delegate void SpecificKeySwitch();
        private Dictionary<Keys, SpecificKeySwitch> specificKeyPressed;
        private Dictionary<Keys, SpecificKeySwitch> specificKeyReleased;

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

        public void KeyPressedSubTo(Keys key, SpecificKeySwitch callback)
        {
            if (specificKeyPressed.ContainsKey(key))
            {
                specificKeyPressed[key] += callback;
                return;
            }
            specificKeyPressed.Add(key, callback);
        }

        public void KeyReleasedSubTo(Keys key, SpecificKeySwitch callback)
        {
            if (specificKeyReleased.ContainsKey(key))
            {
                specificKeyReleased[key] += callback;
                return;
            }
            specificKeyReleased.Add(key, callback);
        }

        private void RaiseSpecificKeyPressed(Keys[] keys)
        {
            foreach(Keys k in keys)
            {
                if (specificKeyPressed.ContainsKey(k))
                {
                    specificKeyPressed[k]?.Invoke();
                }
            }
        }

        private void RaiseSpecificKeyReleased(Keys[] keys)
        {
            foreach (Keys k in keys)
            {
                if (specificKeyReleased.ContainsKey(k))
                {
                    specificKeyReleased[k]?.Invoke();
                }
            }
        }

        private KeyboardManager()
        {
            currentKeyboardState = Keyboard.GetState();
            textinputBoxes = new List<TextinputBox>();
            specificKeyPressed = new Dictionary<Keys, SpecificKeySwitch>();
            specificKeyReleased = new Dictionary<Keys, SpecificKeySwitch>();
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

        public void OnTextInput(object sender, TextInputEventArgs e)
        {
            if (e.Character == '/' && !gameManager.TerminalTexintput.IsFocused && focusedTextInput == null)
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

            Keys[] newKeys = GetPressedKey(currentPressedKeys, previousPressedKeys).GetArray();
            Keys[] lostKeys = GetReleasedKey(currentPressedKeys, previousPressedKeys).GetArray();

            if (newKeys.Length > 0) 
            {
                RaiseSpecificKeyPressed(newKeys);
                OnKeyPressed(newKeys);
            }
            if (lostKeys.Length > 0) 
            {
                RaiseSpecificKeyReleased(lostKeys);
                OnKeyRelease(lostKeys); 
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
                    t.Validate(); 
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
            KeyRealeased?.Invoke(new KeysArray(keys));
        }

        private void OnKeyPressed(Keys[] keys)
        {
            KeyPressed?.Invoke(new KeysArray(keys));
        }

        public void UnFocusTextinputBox()
        {
            focusedTextInput = null; 
        }

        public void FocusTextinput(TextinputBox ti)//ti is the one which is going to be focused
        {
            foreach(var t in textinputBoxes)
            {
                t.UnFocus(); 
            }
            focusedTextInput = ti; 
        }
    }
}
