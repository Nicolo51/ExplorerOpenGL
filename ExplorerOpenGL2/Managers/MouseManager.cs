using ExplorerOpenGL2.Model.Sprites;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL2.Managers
{
    public class MouseManager
    {
        MousePointer mousePointer; 

        private MouseState currentMouseState; 
        private MouseState previousMouseState;

        public delegate void ButtonClickEventHandler(ButtonState state);
        public event ButtonClickEventHandler LeftClicked;
        public event ButtonClickEventHandler RightClicked;
        public event ButtonClickEventHandler MiddleClicked;
        public event ButtonClickEventHandler X1Clicked;
        public event ButtonClickEventHandler X2Clicked;

        public delegate void ScrollWheelEventHandler(int wheelValue);
        public event ScrollWheelEventHandler MouseWheeled; 

        public static event EventHandler Initialized;
        private static MouseManager instance;
        public static MouseManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MouseManager();
                    Initialized?.Invoke(instance, EventArgs.Empty);
                    return instance;
                }
                return instance;
            }
        }

        private MouseManager()
        {

        }
        public void InitDependencies(MousePointer mouse)
        {
            mousePointer = mouse; 
        }

        public void Update(Sprite[] sprites)
        {
            //mousePointer.Update(sprites);
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            if (previousMouseState == currentMouseState)
                return;
            if (previousMouseState.LeftButton != currentMouseState.LeftButton)
                LeftClicked?.Invoke(currentMouseState.LeftButton);
            if (previousMouseState.RightButton != currentMouseState.RightButton)
                RightClicked?.Invoke(currentMouseState.RightButton);
            if (previousMouseState.MiddleButton != currentMouseState.MiddleButton)
                MiddleClicked?.Invoke(currentMouseState.MiddleButton);
            if (previousMouseState.XButton1 != currentMouseState.XButton1)
                X1Clicked?.Invoke(currentMouseState.XButton1);
            if (previousMouseState.XButton2 != currentMouseState.XButton2)
                X2Clicked?.Invoke(currentMouseState.XButton2);
            if (previousMouseState.ScrollWheelValue != currentMouseState.ScrollWheelValue)
                MouseWheeled?.Invoke(currentMouseState.ScrollWheelValue); 
        }
    }
}
