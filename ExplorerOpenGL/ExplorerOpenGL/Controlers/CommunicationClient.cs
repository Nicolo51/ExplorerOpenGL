using ExplorerOpenGL.Model.Sprites;
using ExplorerOpenGLInterfaces;
using System.Diagnostics;
using System.ServiceModel;

namespace ExplorerOpenGL.Controlers
{
    public class CommunicationClient
    {
        public int IDClient { get; private set; }
        private Player player;

        public CommunicationClient()
        {
            
        }

        public void Start(Player player)
        {
            this.player = player;
            ChannelFactory<IServer> channelFactory = new ChannelFactory<IServer>("*");

            IServer proxy = channelFactory.CreateChannel();

            IDClient = proxy.Connect(player);

            var players = proxy.GetPlayersInfo();
        }
    }
}
