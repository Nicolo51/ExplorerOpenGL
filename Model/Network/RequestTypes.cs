using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Network
{
    public enum RequestTypes : int
    {
        MoveObject = 1, 
        DeleteObject = 2, 
        ModifyPlayerHealth = 3, 
        MovePlayer = 4, 
        CreateObject = 5,
    }
}
