using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Managers.Networking.EventArgs
{
    public class RequestResponseEventArgs : NetworkEventArgs
    {
        public string Request { get; set; }
        public string[] Arguments { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public string Response { get; set; }
    }

    public enum RequestStatus
    {
        NotAuthorized,
        Failed,
        Success, 
        Pending, 
    }
}
