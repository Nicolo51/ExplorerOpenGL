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
        static MousePointer mousePointer; 

        static MouseState currentMouseState; 
        static MouseState previousMouseState;

        public delegate void ButtonClickEventHandler(ButtonState state);
        public static event ButtonClickEventHandler LeftClicked;
        public static event ButtonClickEventHandler RightClicked;
        public static event ButtonClickEventHandler MiddleClicked;
        public static event ButtonClickEventHandler X1Clicked;
        public static event ButtonClickEventHandler X2Clicked;

        public delegate void ScrollWheelEventHandler(int wheelValue);
        public static event ScrollWheelEventHandler MouseWheeled; 

        public static void InitDependencies(MousePointer mouse)
        {
            mousePointer = mouse; 
        }

        public static void Update(Sprite[] sprites)
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
