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

/*------------------------------EMMANUEL'S EDITS--------------------------------
 * 01. Changed "LoadContent()" to work with new file format
*/
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
                    /*currentLevel = new LinkedList<Sprite>();
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
                    }*/
                    /*----------FUNC KEY---------
                 * STM = Stream
                 * ARG = Argument
                 * ARY = Array
                 * STR = string
                 * LL  = LinkedList<>
                 * SPR = Sprite
                 * REP = Representate/Representation
                 * DSPROBJ = Derived Sprite Object
                             */

                    // STMs for File & Reading. FileSTM opens the "fileName" file located here
                    // "angelattack\guiCreator\bin\x86\Debug" And ReadSTM reads from that file.
                    FileStream FS = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    StreamReader SR = new StreamReader(FS);

                    string SpriteTypeData = SR.ReadLine();      // Gets the SPROBJ for the ARGs
                    // If SR.ReadLine() spits out "Cats love catfish" the below Split() is told to 
                    // make FileLine[] = {"Cats", "love", "catfish"}. ReadLine() returns a STR
                    string[] FileLine = SR.ReadLine().Split(new char[] { ' ' });
                    object[] ArgumentValues;                    // Stores SPROBJ ARG values

                    // Needed for loop control, ARY initialization & ARY iteration 
                    int ArgumentLength = 0;
                    int Iterator1 = 0;

                    // "Type"(TY) variables are used to get "data about data" so info on the full name 
                    // of a C# type or basically the location of where that its' defined can be gotten.
                    // I don't know specifics but C# can somehow use the information specified in the TY
                    // declaration to create instances of that type weather it be built-in or created by
                    // a programmer. I guess it depends on the TY, number & order of ARGs to pick the
                    // correct constructor for that TY. 
                    Type ArgumentType;

                    // When loading a file the level object is empty. Since 
                    // or guiGame only works with LinkedLists(LL) & not strings from files we need the
                    // appropriate objects to work with & these are it. 
                    currentLevel = new LinkedList<Sprite>();

                    // ALL RIGHT! Main loop!!! Below is exmaple of what's actually being read from file
                    // guiCreator.Block
                    // Argument = 2
                    // System.Int32 680 320
                    // Anyway Peek() looks at next char to be read but does not add char to STM. Instead
                    // it returns a "int" val that REP of the next char to read(refer to ANSCII table)
                    while (SR.Peek() != -1)
                    {

                        // Finding how many ARGs the SPR derived object needs from the STR vals in file.
                        // Then allocating ARY to store them
                        ArgumentLength = Convert.ToInt32(FileLine[3]);
                        ArgumentValues = new object[ArgumentLength];

                        // Reads & converts ARGs to proper types for use by derived SPR object(DSPROBJ)
                        while (Iterator1 < ArgumentLength)
                        {
                            // The first line of ARGs. All values of same type. Refer above 4 "Split()"
                            // FileLine[0] now has a STR REP of a type, which will be used later. 
                            FileLine = SR.ReadLine().Split(new char[] { ' ' });
                            ArgumentType = Type.GetType(FileLine[0]);

                            // Iterates through each string element in "FileLine"
                            foreach (string S in FileLine)
                                if (S != FileLine[0])
                                {
                                    // This converts a STR REP of a value to a type IF it's possible.
                                    // Won't work if attempted on value not of that type. 
                                    ArgumentValues[Iterator1] = Convert.ChangeType(S, ArgumentType);

                                    ++Iterator1;    // So next value is stored in a different element
                                }
                        }

                        Sprite SpriteInstance = (Sprite)Activator.CreateInstance
                                                (Type.GetType(SpriteTypeData), ArgumentValues);
                        SpriteInstance.LoadContent(this.Content);

                        // The Arguments are the last items read in a cycle of this loop. This makes 
                        // sure "FileLine" can be stored with some data from the file
                        if (SR.Peek() != -1)
                        {
                            SpriteTypeData = SR.ReadLine();
                            FileLine = SR.ReadLine().Split(new char[] { ' ' });
                        }

                        Iterator1 = 0;  // Reset ARG iterator

                        // Stores 1 piece of the level. 
                        currentLevel.AddLast(SpriteInstance);
                    }
                    SR.Close();
                    FS.Close();
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
