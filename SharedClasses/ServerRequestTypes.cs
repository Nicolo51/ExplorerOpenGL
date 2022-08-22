using System;
using System.Collections.Generic;
using System.Text;

namespace SharedClasses
{
    public enum ServerRequestTypes : int
    {
        MoveObject = 1, 
        DeleteObject = 2, 
        ModifyPlayerHealth = 3, 
    }
}
