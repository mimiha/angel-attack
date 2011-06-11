using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics; 
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
 * 02. Cleaning up comments & adding some to "LoadContent()"
 * 03. Added fields for use with spawner states
 * 04. Made a "LoadContent()" standalone to test spawner things
 * 05. Editted "Update()" to detect enemies 
 * 06. "LoadContent()" counts how many spawners are in level
 * 
*/
/*------------------------------EMMANUEL'S PROBLEMS----------------------------
 * 01. Switching window control between level results in major slow down
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

        // Track non-active & total Spawners
        int DoneSpawner = 0;
        int TotalSpawner = 0;

        bool DrawComplete = false;
        // Iterates through the "AllLevel" file to get a levelname
        int ActiveLevel = 0;

        // Name of level, used by "LoadContent()"
        string[] LevelNames; 

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
                string fileName = "test";
                if (File.Exists(fileName))
                {
                    /*----------FUNC KEY---------
                     * STM = Stream
                     * ARG = Argument
                     * ARY = Array
                     * STR = string
                     * LL  = LinkedList<>
                     * SPR = Sprite
                     * REP = Representate/Representation
                     * DRSPROBJ = Derived Sprite Object
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

                        // Tells me how many Spawners are in level. 
                        if (SpriteTypeData == "guiCreator.Spawner")
                            ++TotalSpawner;

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
                        // The Sprite object now exists! ARGs assigned to a DRSPROBJ & drawn to screen.
                        Sprite SpriteInstance = (Sprite)Activator.CreateInstance
                                                (Type.GetType(SpriteTypeData), ArgumentValues);
                        SpriteInstance.LoadContent(this.Content);

                        // The Arguments are the last items read in loop cycle. This fetches the next
                        // DRSPROBJ if the there is still data to read from file.
                        if (SR.Peek() != -1)
                        {
                            SpriteTypeData = SR.ReadLine();
                            FileLine = SR.ReadLine().Split(new char[] { ' ' });
                        }

                        Iterator1 = 0;  // Reset ARG iterator

                        // Stores 1 piece of the level. 
                        currentLevel.AddLast(SpriteInstance);
                    }
                    // Closing up the streams when file is done being read from. 
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
                LoadLevelList();
                loadGui(LevelNames[ActiveLevel]);
                //LevelNames = new string[ActiveLevel];
                //this.Exit();
            }
        }

        // Loads a list of levels
        public void LoadLevelList()
        {
            // Load only when this file exists
            if (File.Exists("AllLevels"))
            {
                // Streams read lines from the "AllLevels" file
                FileStream FS = new FileStream("AllLevels", FileMode.Open, FileAccess.Read);
                StreamReader SR = new StreamReader(FS);

                string TextLine; // Keeps 1 line of text from file

                int Iterator = 0; // Iterates through LevelNames

                // Get the count of levels to load & use to initialize array
                TextLine = SR.ReadLine();
                LevelNames = new string[Convert.ToInt32(TextLine)];

                // Only do when there is data is to read!
                while (SR.Peek() != -1)
                {
                    // Just Adding names of all levels from "AllLevel" file
                    TextLine = SR.ReadLine();
                    LevelNames[Iterator] = TextLine.ToString();
                    ++Iterator;
                }
                // Gotta close my streams
                SR.Close();
                FS.Close();
            }
        }

        // This loads a level from the list
        public void loadGui(string fileName)
        {
            TotalSpawner = 0; // Initialize to default
            // Anything with = between it is an example of what val might be. 
            // ## = Numbers, ?? = Unknown values, TT = TypeName
            if (File.Exists(fileName))
            {
                /*Opens & ena-*/
                FileStream FS = new FileStream(fileName, FileMode.Open,
                    /*bles reading*/FileAccess.Read); // to file
                /*Reader is*/
                StreamReader SR = new StreamReader(FS); // Connected to this file

                /*STD = */
                string SpriteTypeData = SR.ReadLine(); /*guiCreator.Block*/

                // If SR.ReadLine() spits out "Cats love catfish" the below Split() is told to 
                // make FileLine[] = {"Cats", "love", "catfish"}. ReadLine() returns a STR
                string[] FileLine;

                if (SR.Peek() != -1) /*See if Data exists*/
                    FileLine = SR.ReadLine().Split(new char[] { ' ' });
                else
                    FileLine = new string[5];
                object[] ArgumentValues;                    // Stores SPROBJ ARG values

                // Needed for loop control, ARY initialization & ARY iteration 
                int ArgumentLength = 0;
                int Iterator1 = 0;

                /*Stores info*/
                Type ArgumentType; // needed to convert ARGs vals to their types

                currentLevel = new LinkedList<Sprite>(); // Cleans currentLevel

                // ALL RIGHT! Main loop!!! Below an example of what's read from file
                // guiCreator.Block
                // Argument Length = 2
                // System.Int32 680 320
                while (SR.Peek() != -1) // Check for text to read from file
                {
                    // Then allocating ARY to store them
                    /*FL[3] = 3 type*/
                    ArgumentLength = Convert.ToInt32(FileLine[3]); /*info uneeded*/
                    /*AV array size*/
                    ArgumentValues = new object[ArgumentLength]; /* is 3 now*/

                    if (SpriteTypeData == "guiCreator.Spawner")
                        ++TotalSpawner; // Count up sprites

                    /*Read & convert*/
                    while (Iterator1 < ArgumentLength) //Args to create sprite obj
                    {
                        /*System32.Int32 ## ##*/
                        FileLine = SR.ReadLine().Split(new char[] { ' ' });
                        /*FL[0]=System32.Int32*/
                        ArgumentType = Type.GetType(FileLine[0]); /*Make type name*/

                        foreach (string S in FileLine) // Iterate string elements
                            if (S != FileLine[0]) // No Convert on 1st element
                            {
                                /*AV[0] = TT*/
                                ArgumentValues[Iterator1] = Convert.ChangeType(S, ArgumentType);
                                /*TT is System.Int32*/
                                ++Iterator1;
                            }
                    }

                    // The Sprite object now exists! ARGs assigned to a DRSPROBJ & drawn to screen.
                    Sprite SpriteInstance = (Sprite)Activator.CreateInstance
                                            (Type.GetType(SpriteTypeData), ArgumentValues);
                    SpriteInstance.LoadContent(this.Content);

                    if (SR.Peek() != -1) // Check existence of more data ahead
                    {
                        /*ARGs done, Sprite*/
                        SpriteTypeData = SR.ReadLine(); /*typename is next*/
                        FileLine = SR.ReadLine().Split(new char[] { ' ' });
                    }

                    Iterator1 = 0;  // Reset ARG iterator

                    currentLevel.AddLast(SpriteInstance); // Add sprite data
                }
                SR.Close(); // Close Streamreader
                FS.Close(); // Close FileReader
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

            // Any enemies to fight?
            bool DemonExists = false; 

            while (n != null)
            {
                if ((n.Value.GetType() == typeof(Protectee)) ||
                    (n.Value.GetType() == typeof(Spawner)))
                {
                    baseExists = true;

                    // Add to number of spawners done spawning
                    if (n.Value.DoneSpawning == true)
                        ++DoneSpawner; 
                }
                else
                if(n.Value.GetType() == typeof (LesserDemon))
                    // Enemies do exist in this update cycle
                    DemonExists = true; 

                currentLevel = n.Value.Update(gameTime, currentLevel, this.Content);
                n = n.Next;
            }

            if (!baseExists && (DrawComplete == true))
            {
                MessageBox(new IntPtr(0), "Game Over!", "Angel Attack", 0);
                this.Exit();
            }

            // When all Spawners are done & no more enemies to beat 
            // you've won the level!
            
            if ((DoneSpawner == TotalSpawner) && (DemonExists == false) &&
                (DrawComplete == true))
            {
                if(ActiveLevel > 0)
                MessageBox(new IntPtr(0), "You have destroyed your enemies! ",
                           "Angel Attack", 0);
                
                if (ActiveLevel != LevelNames.Count())//Load if more levels exist
                {
                    loadGui(LevelNames[ActiveLevel]);
                    ++ActiveLevel;
                }
                else
                {
                    MessageBox(new IntPtr(0), "Exiting now! ",
                           "Angel Attack", 0);
                    this.Exit();
                }
                DrawComplete = false; 
            }
            
            DoneSpawner = 0;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
           
            //spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Begin();

            foreach (Sprite n in currentLevel)
            {
                n.Draw(this.spriteBatch);
            }
            
            spriteBatch.End();
            DrawComplete = true; 

            base.Draw(gameTime);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint MessageBox(IntPtr hWnd, String text, String caption, uint type);
    }
}
