using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Reflection;
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
using System.Runtime.InteropServices;
using System.Collections.Generic; 

/*------------------------------EMMANUEL'S EDITS--------------------------------
 * 01. Added "LinkManage" value to "guiMode" enum
 * 02. Add F4 key functionality for use with "LinkManage" in "guiMode" enum.
 * 03. Beautified KeyState.IsKeyDown(Keys.F12) in "UpdateGui()"
 * 04. "GuiLinkManage.cs" check it out
 * 05. Optimized save & load functions for level
 * 06. Optimized "UpdateGui()" now you can click & drag to create levels. Try it out!
 */
/*-----------------------------EMMANUEL'S NOTES---------------------------------
 * POD = Plain Old Data
 */
namespace guiCreator
{
    public class GuiGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Vector2 camera;
        //Speed that camera moves when in GUI mode, with arrow keys.
        //DO NOT CHANGE THIS.
        const int cameraDist = 40;
        Sprite mSprite;
        Text header,text;

        Text[] SpriteTextLocation;   // Locations where sprite information will be written
        string[] SpriteInformation;  // Sprite information is written to these!

        // So Same Sprite doesn't keep being drawn in same location
        Vector2 PrevPos = new Vector2();

        public struct LevelData
        {
            public LinkedList<Sprite> data;
            public LinkedList<int> numArgs;
            // public LinkedList<LinkedList<object>> args;
            public LinkedList<object[]> args;
        }

        LevelData level = new LevelData();

        // This keeps track of all the levels that have been created!
        LinkedList<string> LevelNames = new LinkedList<string>(); 

        // Mouse/Keyboard input
        MouseState mouseState = Mouse.GetState();
        KeyboardState keyState = Keyboard.GetState();
        KeyboardState currentKeyboardState = Keyboard.GetState();
        KeyboardState oldKeyboardState = Keyboard.GetState();

        // strings for file name input
        string hFileName = "";
        string fileName = "";

        enum guiObject
        {
            Delete,
            Block,
            Wall,
            Player,
            AverageJoeLeft,
            AverageJoeRight,
            SpawnerLeft,
            SpawnerRight,
            SetSpawner,
            ObjectInfo,
            Protectee
        }

        //Sets default object in GUI mode to be the block
        guiObject currentGuiObject = guiObject.Block;

        enum guiMode
        {
            Edit,
            Save,
            Load,
            Test, 
            LinkListManage
        }

        // sets default mode to editing in GUI mode
        guiMode currentGuiMode = guiMode.Edit;
    
        /*
         * The Gamer Services functionality must be initialized before you call this method. 
         * The easiest way to do that is to add a GamerServicesComponent to the Game.Components collection in the constructor of your Game class.
        */
        public GuiGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            //graphics.IsFullScreen = true;
            this.IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler(Window_ClientSizeChanged);
            camera = new Vector2(0, 0);
            Content.RootDirectory = "Content";
        }

        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        protected override void Initialize()
        {
            level.data = new LinkedList<Sprite>();
            level.numArgs = new LinkedList<int>();
            //level.args = new LinkedList<LinkedList<object>>();
            level.args = new LinkedList<object[]>(); 
            mSprite = new Sprite(10, 10);
            header = new Text(10, 10);
            text = new Text(100, 10);

            base.Initialize();
        }

        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            mSprite.LoadContent(this.Content, "Block");
            header.LoadContent(this.Content, "Font");
            text.LoadContent(this.Content, "Font");
        }


        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            updateGui();

            base.Update(gameTime);
        }

        private void updateGui()
        {
            // Camera moving with arrow keys in GUI mode.
            KeyboardState keyState = Keyboard.GetState();
            if ((keyState.IsKeyDown(Keys.Up) == true))
            {
                camera.Y -= cameraDist;
            }
            if ((keyState.IsKeyDown(Keys.Down) == true))
            {
                camera.Y += cameraDist;
            }
            if ((keyState.IsKeyDown(Keys.Left) == true))
            {
                camera.X -= cameraDist;
            }
            if ((keyState.IsKeyDown(Keys.Right) == true))
            {
                camera.X += cameraDist;
            }

            // Checks if keys are pressed.
            if (keyState.IsKeyDown(Keys.F1) == true)
            {
                currentGuiMode = guiMode.Edit;
                camera = new Vector2(0, 0);
                hFileName = "";
                fileName = "";
                level.data = new LinkedList<Sprite>();
                level.numArgs = new LinkedList<int>();
                level.args = new LinkedList<LinkedList<object>>();
            }
            if (keyState.IsKeyDown(Keys.F2) == true)
            {
                currentGuiMode = guiMode.Load;
                hFileName = "Load File: ";
                fileName = "";
            }
            if (keyState.IsKeyDown(Keys.F3) == true)
            {
                currentGuiMode = guiMode.Save;
                hFileName = "Save As: ";
                fileName = "";
            }
            if (keyState.IsKeyDown(Keys.F4) == true)
            {
                currentGuiMode = guiMode.LinkManage;
            }
            if (keyState.IsKeyDown(Keys.F10) == true)
            {
                currentGuiMode = guiMode.Test;
                hFileName = "Test Level: ";
                fileName = "";
            }
            if (keyState.IsKeyDown(Keys.F12) == true)
            {
                MessageBox(new IntPtr(0),
                    "Special Keys / Functions\n====================\n" +
                    "F1 = New Level\n" +
                    "F2 = Load Level\n" +
                    "F3 = Save Level\n" +
                    "F10 = Test Level\n" +
                    "F12 = Help\n" +
                    "Delete = Delete Mode\n" +
                    "\nBlocks\n====================\n" +
                    "Q = Regular Block\n" +
                    "W = Wall\n" +
                    "E = Protectee\n\n" +
                    "Enemies / Players\n" +
                    "====================\n" +
                    "A = Grenadier\n" +
                    "S = Average Joe (moving left)\n" +
                    "D = Average Joe (moving right)\n" +
                    "\nSpawners\n" +
                    "====================\n" +
                    "Z = Average Joe Spawner (moving left)\n" +
                    "X = Average Joe Spawner (moving right)", "Help", 0);
            }
            if (keyState.IsKeyDown(Keys.Escape) == true)
            {
                this.Exit();
            }
            if ((currentGuiMode == guiMode.Save) || (currentGuiMode == guiMode.Load) || (currentGuiMode == guiMode.Test))
            {
                updateText();
            }
            else
            {
                MouseState state = Mouse.GetState();
                Vector2 pos = new Vector2(state.X, state.Y);
                pos = handleMouse(pos);
                mSprite.drawPosition = pos;
                pos += camera;
                if (keyState.IsKeyDown(Keys.Delete) == true)
                {
                    mSprite.LoadContent(this.Content, "Delete");
                    currentGuiObject = guiObject.Delete;
                }
                if (keyState.IsKeyDown(Keys.Q) == true)
                {
                    mSprite.LoadContent(this.Content, "Block");
                    currentGuiObject = guiObject.Block;
                }
                if (keyState.IsKeyDown(Keys.W) == true)
                {
                    mSprite.LoadContent(this.Content, "Wall");
                    currentGuiObject = guiObject.Wall;
                }
                if (keyState.IsKeyDown(Keys.A) == true)
                {
                    mSprite.LoadContent(this.Content, "Grenadier/Stand0");
                    currentGuiObject = guiObject.Player;
                }
                if (keyState.IsKeyDown(Keys.S) == true)
                {
                    mSprite.LoadContent(this.Content, "AverageJoe/aj_run0");
                    currentGuiObject = guiObject.AverageJoeLeft;
                }
                if (keyState.IsKeyDown(Keys.D) == true)
                {
                    mSprite.LoadContent(this.Content, "AverageJoe/aj_run0");
                    currentGuiObject = guiObject.AverageJoeRight;
                }
                if (keyState.IsKeyDown(Keys.Z) == true)
                {
                    mSprite.LoadContent(this.Content, "Spawner");
                    currentGuiObject = guiObject.SpawnerLeft;
                }
                if (keyState.IsKeyDown(Keys.X) == true)
                {
                    mSprite.LoadContent(this.Content, "Spawner");
                    currentGuiObject = guiObject.SpawnerRight;
                }
                if (keyState.IsKeyDown(Keys.E) == true)
                {
                    mSprite.LoadContent(this.Content, "Protectee");
                    currentGuiObject = guiObject.Protectee;
                }
                if ((mouseState.LeftButton == ButtonState.Released) && (state.LeftButton == ButtonState.Pressed))
                {
                    if (currentGuiObject == guiObject.Delete)
                    {
                        LinkedListNode<Sprite> n = level.data.First;
                        LinkedListNode<int> m = level.numArgs.First;
                        LinkedListNode<LinkedList<object>> o = level.args.First;
                        while (n != null)
                        {
                            if (((pos.X + 20) > n.Value.Position.X) && ((pos.X + 20) < (n.Value.Position.X + n.Value.Size.Right)) && ((pos.Y + 20) > n.Value.Position.Y) && ((pos.Y + 20) < (n.Value.Position.Y + n.Value.Size.Bottom)))
                            {
                                level.data.Remove(n);
                                level.numArgs.Remove(m);
                                level.args.Remove(o);
                                break;
                            }
                            else
                            {
                                n = n.Next;
                                m = m.Next;
                                o = o.Next;
                            }
                        }
                    }
                    if (currentGuiObject == guiObject.Block)
                    {
                        Block c = new Block((int)pos.X, (int)pos.Y);
                        c.LoadContent(this.Content);
                        level.data.AddLast(c);
                        // level.numArgs.AddLast(2);
                        /**
                        LinkedList<object> d = new LinkedList<object>();
                        d.AddLast((int)pos.X);
                        d.AddLast((int)pos.Y);/**/

                        object[] d = new object[] { (int)pos.X, (int)pos.Y };
                    }
                    if (currentGuiObject == guiObject.Wall)
                    {
                        Wall c = new Wall((int)pos.X, (int)pos.Y);
                        c.LoadContent(this.Content);
                        level.data.AddLast(c);
                        level.numArgs.AddLast(2);
                        LinkedList<object> d = new LinkedList<object>();
                        /**
                        d.AddLast((int)pos.X);
                        d.AddLast((int)pos.Y); /**/
                        object[] d = new object[] { (int)pos.X, (int)pos.Y };
                        level.args.AddLast(d);
                    }
                    if (currentGuiObject == guiObject.Player)
                    {
                        Grenadier c = new Grenadier((int)pos.X, (int)pos.Y, 512, 384);
                        c.LoadContent(this.Content);
                        level.data.AddLast(c);
                        level.numArgs.AddLast(4);
                        LinkedList<object> d = new LinkedList<object>();
                        /**
                        d.AddLast((int)pos.X);
                        d.AddLast((int)pos.Y);
                        d.AddLast(512);
                        d.AddLast(384);/**/
                        object[] d = new object[] { (int)pos.X, (int)pos.Y, 512, 384 };
                        level.args.AddLast(d);
                    }
                    if (currentGuiObject == guiObject.AverageJoeLeft)
                    {
                        LesserDemon c = new LesserDemon((int)pos.X, (int)pos.Y, -1);
                        c.LoadContent(this.Content);
                        level.data.AddLast(c);
                        level.numArgs.AddLast(3);
                        LinkedList<object> d = new LinkedList<object>();
                        /**
                        d.AddLast((int)pos.X);
                        d.AddLast((int)pos.Y);
                        d.AddLast((int)-1);/**/
                        object[] d = new object[] { (int)pos.X, (int)pos.Y, (int)-1 };
                        level.args.AddLast(d);
                    }
                    if (currentGuiObject == guiObject.AverageJoeRight)
                    {
                        LesserDemon c = new LesserDemon((int)pos.X, (int)pos.Y, 1);
                        c.LoadContent(this.Content);
                        level.data.AddLast(c);
                        level.numArgs.AddLast(3);
                        LinkedList<object> d = new LinkedList<object>();
                        /**
                        d.AddLast((int)pos.X);
                        d.AddLast((int)pos.Y);
                        d.AddLast((int)1);
                        /**/
                        object[] d = new object[] { (int)pos.X, (int)pos.Y, (int)1 };
                        level.args.AddLast(d);
                    }
                    if (currentGuiObject == guiObject.SpawnerLeft)
                    {
                        Spawner c = new Spawner((int)pos.X, (int)pos.Y, 5000, "guiCreator.LesserDemon", -1);
                        c.LoadContent(this.Content);
                        level.data.AddLast(c);
                        level.numArgs.AddLast(5);
                        LinkedList<object> d = new LinkedList<object>();
                        /**
                        d.AddLast((int)pos.X);
                        d.AddLast((int)pos.Y);
                        d.AddLast((int)5000);
                        d.AddLast("guiCreator.LesserDemon");
                        d.AddLast((int)-1);
                        /**/
                        object[] d = new object[] { (int)pos.X, (int)pos.Y, (int)5000, (int)-1 };
                        level.args.AddLast(d);
                    }
                    if (currentGuiObject == guiObject.SpawnerRight)
                    {
                        Spawner c = new Spawner((int)pos.X, (int)pos.Y, 5000, "guiCreator.LesserDemon", 1);
                        c.LoadContent(this.Content);
                        level.data.AddLast(c);
                        level.numArgs.AddLast(5);
                        LinkedList<object> d = new LinkedList<object>();
                        /**
                        d.AddLast((int)pos.X);
                        d.AddLast((int)pos.Y);
                        d.AddLast((int)5000);
                        d.AddLast("guiCreator.LesserDemon");
                        d.AddLast((int)1);
                        /**/
                        object[] d = new object[] { (int)pos.X, (int)pos.Y, (int)5000, "guiCreator.LesserDemon" };
                        level.args.AddLast(d);
                    }
                    if (currentGuiObject == guiObject.Protectee)
                    {
                        Protectee c = new Protectee((int)pos.X, (int)pos.Y);
                        c.LoadContent(this.Content);
                        level.data.AddLast(c);
                        level.numArgs.AddLast(2);
                        LinkedList<object> d = new LinkedList<object>();
                        /**
                        d.AddLast((int)pos.X);
                        d.AddLast((int)pos.Y);
                        /**/
                        object[] d = new object[] { (int)pos.X, (int)pos.Y };
                        level.args.AddLast(d);
                    }
                }
                mouseState = state;
            }
        }

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
                    if (key == Keys.Back) // overflows
                    {
                        if (fileName.Length > 0)
                            fileName = fileName.Remove(fileName.Length - 1, 1);
                    }
                    else if (key == Keys.Space)
                        fileName = fileName.Insert(fileName.Length, " ");
                    else if (key == Keys.Enter)
                    {
                        if (currentGuiMode == guiMode.Save)
                        {
                            saveGui(fileName);
                        }
                        if (currentGuiMode == guiMode.Load)
                        {
                            loadGui(fileName);
                        }
                        if (currentGuiMode == guiMode.Test)
                        {
                            if (!fileName.Equals("") && fileName != null)
                            {
                                Process.Start("Game.exe", fileName);
                            }
                        }
                        hFileName = "";
                        fileName = "";
                        currentGuiMode = guiMode.Edit;
                    }
                    else if (key == Keys.D0)
                        fileName += "0";
                    else if (key == Keys.D1)
                        fileName += "1";
                    else if (key == Keys.D2)
                        fileName += "2";
                    else if (key == Keys.D3)
                        fileName += "3";
                    else if (key == Keys.D4)
                        fileName += "4";
                    else if (key == Keys.D5)
                        fileName += "5";
                    else if (key == Keys.D6)
                        fileName += "6";
                    else if (key == Keys.D7)
                        fileName += "7";
                    else if (key == Keys.D8)
                        fileName += "8";
                    else if (key == Keys.D9)
                        fileName += "9";
                    else
                        fileName += key.ToString().ToLower();
                }
            }
        }
        public void saveGui(string fileName)
        {
            if ((fileName != null)&&(!fileName.Equals("")))
            {
                /* ---------------------FUNCTION COMMENT KEY---------------------
                 * LL  = LinkedList
                 * NOD = Node/Link
                 * LLN = LinkedListNode
                 * ARG = Argument
                 * WRT = Write
                 * SPROBJ = Sprite Object
                 */

                // Space for keeping a line of text to be written to file
                string StoreFileLine = "";

                // Create a new file that's writable & object to write(WRT) to file. 
                FileStream FSA = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                StreamWriter SWA = new StreamWriter(FSA);

                int Iterator2 = 0;          // Iterates through array of Argument(ARG) values

                // A Single Node/Link(NOD) that's first NOD in LinkedList(LL) assigned to LLNodes(LLN)
                LinkedListNode<Sprite> SpriteObjectSingle = level.data.First; 
                LinkedListNode<int> ArgumentLengthSingle = level.numArgs.First;
                LinkedListNode<object[]> ArgumentValueSingle = level.args.First;

                // Example of what a guiCreator.Block looks like in file
                // guiCreator.Block
                // Argument Length = 2
                // System.Int32 440 320
                // File lists different ARGs depending on Sprite to be saved, but overall format
                // will be like the above
                while (ArgumentLengthSingle != null)
                {
                    // Do if new set of ARG needs to written from LL
                    if (Iterator2 == 0)
                    {
                        // Write the Sprite Object(SPROBJ) full typename belonging to the ARGs
                        SWA.WriteLine(SpriteObjectSingle.Value.GetType().ToString()); 

                        // Create a line declaring number of arguments & write it
                        StoreFileLine = "Argument Length = " +
                                        ArgumentLengthSingle.Value.ToString();
                        SWA.WriteLine(StoreFileLine);

                        // Create string holding 1st ARG type & it's value seperated by a space
                        // Line StoreFileLine value might be: System.Int32 340
                        StoreFileLine = ArgumentValueSingle.Value[Iterator2].GetType().ToString() +
                                        " " + ArgumentValueSingle.Value[Iterator2].ToString();
                        ++Iterator2;
                    }
                    else
                        // Do if all ARG in array are done with first
                        if (Iterator2 >= ArgumentLengthSingle.Value)
                        {
                            // Write last Line of ARG type & values, then goto next NODs in LL
                            SWA.WriteLine(StoreFileLine);
                            SpriteObjectSingle = SpriteObjectSingle.Next;
                            ArgumentValueSingle = ArgumentValueSingle.Next;
                            ArgumentLengthSingle = ArgumentLengthSingle.Next;

                            Iterator2 = 0; // Reset ARG iterator
                        }
                        else
                            // Keep adding values on same line when ARG types are same
                            if (ArgumentValueSingle.Value[Iterator2 - 1].GetType() ==
                               ArgumentValueSingle.Value[Iterator2].GetType())
                            {
                                // Nothing to WRT because type is same
                                StoreFileLine = StoreFileLine + " " +
                                                ArgumentValueSingle.Value[Iterator2].ToString();

                                ++Iterator2; // Gotta get next ARG value
                            }
                            else
                            // Types aren't the same between eachother
                            {
                                // WRT line of previous ARG type & values, then get new ARG value & type
                                // and WRT values as strings to a new line in file
                                SWA.WriteLine(StoreFileLine);
                                StoreFileLine = ArgumentValueSingle.Value[Iterator2].GetType().ToString() +
                                                " " + ArgumentValueSingle.Value[Iterator2].ToString();

                                ++Iterator2; // Gotta get next ARG value
                            }
                }
                // Closing File & Write streams
                SWA.Close();
                FSA.Close(); 
            }
        }

        public void loadGui(string fileName)
        {
            if (File.Exists(fileName))
            {
                camera = new Vector2(0, 0);
                level.data = new LinkedList<Sprite>();
                level.numArgs = new LinkedList<int>();
                level.args = new LinkedList<object[]>();
                
                /*FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
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
                    //level.args.AddLast(llArgs);
                    Sprite a = (Sprite)Activator.CreateInstance(Type.GetType(type), args);
                    a.LoadContent(this.Content);
                    level.data.AddLast(a);
                    //level.numArgs.AddLast(numArgs);
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
                LinkedList<Sprite> SpriteObject = new LinkedList<Sprite>();
                LinkedList<int> ValueLength = new LinkedList<int>();
                LinkedList<object[]> ArgumentList = new LinkedList<object[]>();

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
                    level.data.AddLast(SpriteInstance); 
                    level.numArgs.AddLast(ArgumentLength);
                    level.args.AddLast(ArgumentValues);
                }
                SR.Close();
                FS.Close();
            }
        }

        public Vector2 handleMouse(Vector2 pos)
        {
            int roundX = (int)pos.X / 40;
            int roundY = (int)pos.Y / 40;
            pos.X = roundX * 40;
            pos.Y = roundY * 40;
            return pos;
        }

        public void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            // Make changes to handle the new window size.            
            graphics.PreferredBackBufferWidth = Window.ClientBounds.Left;
            graphics.PreferredBackBufferHeight = Window.ClientBounds.Bottom;
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
            foreach (Sprite n in level.data)
            {
                n.Draw(this.spriteBatch, camera);
            }
            if(currentGuiMode==guiMode.Edit)
            {
                mSprite.Draw(this.spriteBatch);
                if (currentGuiObject == guiObject.ObjectInfo)
                {
                    // Loop testing multiple lines of output
                    int LocationY = 100;
                    int Iterator = 0;
                    if(SpriteTextLocation != null)
                    foreach (Text Label in SpriteTextLocation)
                    {
                        SpriteTextLocation[Iterator] = new Text(20, LocationY);
                        SpriteTextLocation[Iterator].LoadContent(this.Content, "Font");
                        SpriteTextLocation[Iterator].DrawText(this.spriteBatch,
                            SpriteInformation[Iterator]);
                        LocationY = LocationY + 20;
                        Iterator++; 
                    }
                }
            }
            if ((currentGuiMode == guiMode.Load) || (currentGuiMode == guiMode.Save) || (currentGuiMode == guiMode.Test))
            {
                header.DrawText(this.spriteBatch, hFileName);
                text.DrawText(this.spriteBatch, fileName);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint MessageBox(IntPtr hWnd, String text, String caption, uint type);

    }
}
