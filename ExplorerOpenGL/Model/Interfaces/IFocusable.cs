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
         bool IsFocused { get; set; }
         void AddChar(char c);
         string Validate();
         void UnFocus();
         void Focus(List<Sprite> focusables);
         bool ToggleFocus(List<Sprite> focusables);
         void RemoveChar(bool nextChar = false );
         void MoveCursor(int i);
    }
}
