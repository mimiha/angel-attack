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

        Sprite BackGround; 
        // Track non-active & total Spawners
        int DoneSpawner = 0;
        int TotalSpawner = 0;

        bool DrawComplete = false;
        bool SingleLoad = false; 

        // Iterates through the "AllLevel" file to get a levelname
        int ActiveLevel = 0;

        // Draw() flags
        bool NoDemonsShown = true; // draws no demons on screen
        bool NoProtectorShown = true; // draws no protector on screen

        // Text displaying
        Text PromptDisplay; // Area to display prompts for user
        Text[] LabelDisplay;// Labels for character choices
        string PlayerPrompt;// Message to player to do something!
        string[] LabelNames;// Label Names

        // If level enables player choice in picking character
        bool PlayerPick = false;

        Sprite[] Characters = new Sprite[2];
        Vector2 CharacterLocation; 

        // Name of level, used by "LoadContent()"
        string[] LevelNames;

        SoundMgr sound = new SoundMgr(); // Enables sound playing
        
        // Backgrounds need to be loaded through guiEditor. 
        // Dependant on weather the Content has been loaded or not for background
        // probably a linkedlist of sprites
        // Or Cycles through screen creating a level from different background
        // parts. 
        // when creating background and using sprite.Draw it will not change 
        // if the camera is not sometih
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
            BackGround = new Sprite(0, 0);

            // Initializing Sprite draw locations
            Characters[0] = new Sprite((int)(1024 * .25 - 40), 50);
            Characters[1] = new Sprite((int)(1024 * .75 - 40), 50);

            // Initializing Text Draw Locations
            PromptDisplay = new Text(10, 10);
            LabelDisplay = new Text[2];
            LabelDisplay[0] = new Text((int)(1024 * .25 - 40), 40);
            LabelDisplay[1] = new Text((int)(1024 * .75 - 40), 40); 

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
            BackGround.LoadContent(this.Content, "BackGround1");

            sound.PlayBGM(this.Content, "bgm");

            // TODO: use this.Content to load your game content here
            string[] commandArgs = Environment.GetCommandLineArgs();
            if (commandArgs.Length > 1)
            {
                string fileName = commandArgs[1];
                if (File.Exists(fileName) && fileName != "AllLevels" )
                {
                    // Loads a level================================================
                    loadGui(fileName); 
                    //==============================================================

                    // Stops going through mulitple levels
                    SingleLoad = true; 
                }
                else
                {
                    LoadLevelList();
                    SingleLoad = false; 
                    //this.Exit();
                }
            }
            else
            {
                LoadLevelList();
                //loadGui("test2");
                SingleLoad = false; 
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

                // Skip the Background title for now
                if (SpriteTypeData.Contains("BackGround"))
                    SpriteTypeData = SR.ReadLine(); 

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
                        if (ArgumentValues.Length == 6 || 
                            ArgumentValues.Length == 5 ||
                           ArgumentValues.Length == 7)
                            ++TotalSpawner; // Count up sprites
                        else if(ArgumentValues.Length == 4)
                            PlayerPick = true; // Sprite is player SpawnPoint

                    /*Read & convert time!!======================================*/
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
                    /*=============================================================*/

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

                    // Getting information on where to load sprite & reset flag
                    if (PlayerPick && SpriteInstance.GetType() == typeof(Spawner) && 
                        CharacterLocation.X == 0 && CharacterLocation.Y == 0)
                    {
                        CharacterLocation = SpriteInstance.drawPosition;
                    }

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
            if(sound.sndCtrl != null)
            if(sound.sndCtrl.State == SoundState.Stopped)
                sound.PlayBGM(this.Content, "bgm");

            if (!PlayerPick)
            {
                // MainGame Function!===================================================
                GuiGame(gameTime);
                //======================================================================
                base.Update(gameTime); 
            }
            else
            {
                // Do something here! for player choice!
                ChooseCharacter();
            }
            
            base.Update(gameTime);
        }

        // Game players will actually play. 
        public void GuiGame(GameTime gameTime) 
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
                if (n.Value.GetType() == typeof(Protectee))
                {
                    baseExists = true;
                }
                if (n.Value.GetType() == typeof(Spawner))
                    // Add to number of spawners done spawning
                    if (n.Value.DoneSpawning == true)
                        ++DoneSpawner;
                    /*else /*if (n.Value.PlayerSpawned == true)
                        baseExists = true; */

                if (n.Value.GetType() == typeof(LesserDemon))
                    // Enemies do exist in this update cycle
                    DemonExists = true;

                currentLevel = n.Value.Update(gameTime, currentLevel, this.Content);
                n = n.Next;
            } 

            Debug.WriteLine("The number done DrawComplete is " + DrawComplete);
            Debug.WriteLine("The total Spawners are " + TotalSpawner);
            Debug.WriteLine("AverageJoes existence is " + DemonExists);
            Debug.WriteLine("The number done spawner is " + DoneSpawner); 
            Debug.WriteLine("The NoDemonsShown is " + NoDemonsShown.ToString()); 

            if (((!baseExists) || (baseExists)) && (DrawComplete) && (SingleLoad) && 
                (!DemonExists) && (DoneSpawner == TotalSpawner))
            {
                MessageBox(new IntPtr(0), "Game Over!", "Angel Attack", 0);
                this.Exit();
            }

            // When all Spawners are done & no more enemies to beat 
            // you've won the level!

            if (((DoneSpawner == TotalSpawner) && (DemonExists == false) &&
                (DrawComplete == true) && (SingleLoad == false)) ||
                ((!baseExists) && (!SingleLoad)))
            {
                if (ActiveLevel > 0 && baseExists)
                    MessageBox(new IntPtr(0), "You have destroyed your enemies! ",
                               "Angel Attack", 0);

                if (!baseExists && currentLevel.Count() > 0)
                    MessageBox(new IntPtr(0), "You've Lost! ", "Angel Attack", 0);
                if (ActiveLevel != LevelNames.Count())//Load if more levels exist
                {
                    loadGui(LevelNames[ActiveLevel]);
                    if (PlayerPick)
                        ChooseCharacter(); 
                }
                else
                {
                    MessageBox(new IntPtr(0), "You win!! ", "Angel Attack", 0);
                    this.Exit();
                }
                ++ActiveLevel;
            }

            DrawComplete = false;
            DoneSpawner = 0;
        }

        // Updates current level with the sprite
        private void updateText()
        {
            oldKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            Keys[] pressedKeys;
            pressedKeys = currentKeyboardState.GetPressedKeys();

            foreach (Keys key in pressedKeys)
            {
                if (oldKeyboardState.IsKeyUp(key))
                {
                    if (key == Keys.D0)
                    { /* Character choice action*/}
                    else if (key == Keys.D1)
                    {
                        // Create Espion instance & add to level
                        Grenadier NewGrenadier = new Grenadier((int)CharacterLocation.X, 
                            (int)CharacterLocation.Y, 512, 384);
                        NewGrenadier.LoadContent(this.Content); 
                        currentLevel.AddLast(NewGrenadier);

                        PlayerPick = false; // Resets to get on with game 
                    }
                    else if (key == Keys.D2)
                    {
                        Espion NewEspion = new Espion((int)CharacterLocation.X, 
                            (int)CharacterLocation.Y, 512, 384);
                        NewEspion.LoadContent(this.Content); 
                        currentLevel.AddLast(NewEspion);

                        PlayerPick = false; // Resets to get on with game 
                    }
                    else if (key == Keys.D3)
                    { /* Character choice action*/}
                    else if (key == Keys.D4)
                    { /* Character choice action*/}
                    else if (key == Keys.D5)
                    { /* Character choice action*/}
                    else if (key == Keys.D6)
                    { /* Character choice action*/}
                    else if (key == Keys.D7)
                    { /* Character choice action*/}
                    else if (key == Keys.D8)
                    { /* Character choice action*/}
                    else if (key == Keys.D9)
                    { /* Character choice action*/}
                    else

                    {
                        PlayerPrompt = "Your input was wrong try again!";
                        updateText();
                    }
                }
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            int DemonCount = 0;

            //spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Begin();
            if (!PlayerPick)
            {
                //BackGround.Draw(this.spriteBatch);
                foreach (Sprite n in currentLevel)
                {
                    n.Draw(this.spriteBatch);
                    if (n.GetType() == typeof(LesserDemon))
                        ++DemonCount;
                }
            }
            else
            {
                foreach (Sprite n in Characters)
                {
                    n.Draw(this.spriteBatch);
                }
                PromptDisplay.DrawText(this.spriteBatch, PlayerPrompt);
                LabelDisplay[0].DrawText(this.spriteBatch, LabelNames[0]);
                LabelDisplay[1].DrawText(this.spriteBatch, LabelNames[1]); 
            }

            spriteBatch.End();
            if (DemonCount > 0)
                NoDemonsShown = false;
            else
                NoDemonsShown = true; 

            DrawComplete = true; 

            base.Draw(gameTime);
        }

        // Initializes character images & provides choics for player
        private void ChooseCharacter()
        {
            // Initialize Sprites to an image
            Characters[0].LoadContent(this.Content, "Grenadier/Stand0");
            Characters[1].LoadContent(this.Content, "Espion/Stand0"); 

            // Create User prompt messages
            PlayerPrompt = "Choose the number above the character to pick them";

            // Get sprites for text to draw
            PromptDisplay.LoadContent(this.Content, "Font");
            LabelDisplay[0].LoadContent(this.Content, "Font");
            LabelDisplay[1].LoadContent(this.Content, "Font");

            LabelNames = new string[2] { "1", "2" }; // Create Labels

            updateText(); // Updates level with new character!

        }
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint MessageBox(IntPtr hWnd, String text, String caption, uint type);
    }
}
