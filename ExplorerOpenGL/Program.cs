using ExplorerOpenGL.Model;
using ExplorerOpenGL.Model.Attributes;
using System;
using System.Diagnostics;

namespace ExplorerOpenGL
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new Game1())
                game.Run();
        }
    }
}
