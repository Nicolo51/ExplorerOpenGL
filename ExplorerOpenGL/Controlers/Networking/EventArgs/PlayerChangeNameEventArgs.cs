namespace ExplorerOpenGL.Controlers.Networking.EventArgs
{
    public class PlayerChangeNameEventArgs : NetworkEventArgs
    {
        public int IDPlayer  { get; set; }
        public string Name { get; set; }
    }
}