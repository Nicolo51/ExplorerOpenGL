using ExplorerOpenGL.Model.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model.Interfaces
{
    public interface IFocusable
    {
        public bool IsFocused { get; set; }
        public void AddChar(char c);
        public string Validate();
        public void UnFocus();
        public void Focus(List<Sprite> focusables);
        public bool ToggleFocus(List<Sprite> focusables); 
        public void RemoveChar();
        public void MoveCursor(int i);
    }
}
