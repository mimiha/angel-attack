using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices; 
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace guiCreator
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        LinkedList<Sprite> currentLevel;

        MouseState mouseState = Mouse.GetState();
        KeyboardState currentKeyboardState = Keyboard.GetState();
        KeyboardState oldKeyboardState = Keyboard.GetState();

        /*
         * The Gamer Services functionality must be initialized before you call this method. 
         * The easiest way to do that is to add a GamerServicesComponent to the Game.Components collection in the constructor of your Game class.
        */
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            //graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            
            // TODO: Add your initialization logic here
            currentLevel = new LinkedList<Sprite>();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // TODO: use this.Content to load your game content here
            string[] commandArgs = Environment.GetCommandLineArgs();
            if (commandArgs.Length > 1)
            {
                string fileName = commandArgs[1];
                if (File.Exists(fileName))
                {
                    currentLevel = new LinkedList<Sprite>();
                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(fs);
                    string type;
                    int numArgs;
                    Type argType;
                    object[] args;
                    LinkedList<object> llArgs;
                    string data = sr.ReadLine();
                    while (data != null)
                    {
                        type = data;
                        data = sr.ReadLine();
                        numArgs = Convert.ToInt32(data);
                        args = new object[numArgs];
                        llArgs = new LinkedList<object>();
                        data = sr.ReadLine();
                        for (int i = 0; i < numArgs; i++)
                        {
                            argType = Type.GetType(data);
                            data = sr.ReadLine();
                            args[i] = data;
                            args[i] = Convert.ChangeType(args[i], argType);
                            llArgs.AddLast(args[i]);
                            data = sr.ReadLine();
                        }
                        Sprite a = (Sprite)Activator.CreateInstance(Type.GetType(type), args);
                        a.LoadContent(this.Content);
                        currentLevel.AddLast(a);
                    }
                }
                else
                {
                    this.Exit();
                }
            }
            else
            {
                this.Exit();
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();
            // Allows the game to exit
            if (state.IsKeyDown(Keys.Escape) == true)
                this.Exit();

            // TODO: Add your update logic here
            LinkedListNode<Sprite> n = currentLevel.First;
            bool baseExists = false;
            while (n != null)
            {
                if (n.Value.GetType().ToString() == typeof(Protectee).ToString())
                {
                    baseExists = true;
                }
                currentLevel = n.Value.Update(gameTime, currentLevel, this.Content);
                n = n.Next;
            }
            if (!baseExists)
            {
                MessageBox(new IntPtr(0), "Game Over!", "Angel Attack", 0);
                this.Exit();
            }
           
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
           
            // TODO: Add your drawing code here
            spriteBatch.Begin();
            foreach (Sprite n in currentLevel)
            {
                n.Draw(this.spriteBatch);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint MessageBox(IntPtr hWnd, String text, String caption, uint type);
    }
}
