using System;

namespace guiCreator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (GuiGame game = new GuiGame())
            {
                game.Run();
            }
        }
    }
}