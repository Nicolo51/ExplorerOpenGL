using SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers.Networking.EventArgs
{
    public class RequestEventArgs : NetworkEventArgs
    {
        public RequestTypes RequestType { get; set; }
        public RequestEventArgs()
        {
            
        }
    }

}
