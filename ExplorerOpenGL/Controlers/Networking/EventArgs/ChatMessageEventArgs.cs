using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Controlers.Networking.EventArgs
{
    public class ChatMessageEventArgs : NetworkEventArgs
    {
        public string Sender { get; set; }
        public DateTime Time { get; set; }
        public string Text { get; set; }
        public Color TextColor { get; set; }
        public Color SenderColor{ get; set; }

    }
}
