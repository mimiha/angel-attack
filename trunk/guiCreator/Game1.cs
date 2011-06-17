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

/*------------------------------EMMANUEL'S EDITS--------------------------------
 * 01. Added "LinkManage" value to "guiMode" enum
 * 02. Add F4 key functionality for use with "LinkManage" in "guiMode" enum.
 * 03. Beautified KeyState.IsKeyDown(Keys.F12) in "UpdateGui()"
 * 04. "GuiLinkManage.cs" check it out
 * 05. Optimized save & load functions for level
 * 06. Optimized "UpdateGui()" now you can click & drag to create levels. Try it out!
 * 07. Added "EditGuiObject" stuff to "Draw()", "UpdateGui()"
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
        Sprite BackGround; 

        Text header,text;

        // So Same Sprite doesn't keep being drawn in same location
        Vector2 PrevPos = new Vector2();
        Vector2 OuterPos = new Vector2(); 

        // Background Names
        string BackGroundName = "BackGround1";

        // x = 25; y = 19
        string[][] InterfaceLabels;
        
        public struct LevelData
        {
            public LinkedList<Sprite> data;
            public LinkedList<int> numArgs;
            // public LinkedList<LinkedList<object>> args;
            public LinkedList<object[]> args;
        }
        LevelData level = new LevelData();

        LinkedList<object> PlayerGui = new LinkedList<object>(); 

        // Tracks levels that have been saved so far. 
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
            Grenadier,
            Espion,
            LesserDemonLeft,
            LesserDemonRight,
            SpawnerLeft,
            SpawnerRight,
            SpawnerPlayer,
            Protectee, 
            CurrentSpawner, 
            NewSpawner, 
            InsertBackGround, 
            PlayerSpawn
        }

        //Sets default object in GUI mode to be the block
        guiObject currentGuiObject = guiObject.Block;

        enum guiMode
        {
            Edit,
            Save,
            SaveLevels, 
            LoadLevels,
            Load,
            Test, 
            LinkListManage, 
            EditGuiObject, 
            PlayCreatedLevels,
            EditBackGround, 
            EditInterface
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
            level.args = new LinkedList<object[]>(); 
            mSprite = new Sprite(10, 10);
            header = new Text(10, 10);
            text = new Text(100, 10);
            BackGround = new Sprite(0, 0);

            // Loads LevelList
            LoadLevelList(); 

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
            BackGround.LoadContent(this.Content, "BackGrounds/" + BackGroundName);
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
        
        private void FunctionKeys()
        {
            keyState = Keyboard.GetState();
            // Checks if keys are pressed.
            if (keyState.IsKeyDown(Keys.F1) == true)
            {
                currentGuiMode = guiMode.Edit;
                camera = new Vector2(0, 0);
                hFileName = "";
                fileName = "";
                level.data = new LinkedList<Sprite>();
                level.numArgs = new LinkedList<int>();
                level.args = new LinkedList<object[]>();
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
            // So yeah I don't know about what I'm supposed to do with this LinkedList(LL)
            // Might spew out a bunch of LL data. Please add by comments what exactly needs
            // to be done
            if (keyState.IsKeyDown(Keys.F4) == true)
            {
                currentGuiMode = guiMode.EditBackGround;
                BackGroundName = ""; 
            }
            // EditGuiObject entered by F5
            if (keyState.IsKeyDown(Keys.F5) == true)
            {
                // Reset output
                hFileName = "In EditGuiObject mode now";
                fileName = "";
                currentGuiMode = guiMode.EditGuiObject;
            }
            // PlayCreatedLevels enterted by F6
            if (keyState.IsKeyDown(Keys.F6) == true)
            {
                // Prompt for user input
                hFileName = "Press Enter to play through all created levels";
                currentGuiMode = guiMode.PlayCreatedLevels;
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
                    "Special Keys / Functions\n" +
                    "====================\n" +
                    "F1 = New Level\n" +
                    "F2 = Load Level\n" +
                    "F3 = Save Level\n" +
                    "F4 = ChangeBackGround\n" +
                    "F5 = Edit Gui Object\n" +
                    "F6 = Play Through all Created Levels\n" +
                    "F10 = Test Level\n" +
                    "F12 = Help\n" +
                    "Delete = Delete Mode\n\n" +
                    "Blocks\n" +
                    "====================\n" +
                    "Q = Regular Block\n" +
                    "W = Wall\n" +
                    "E = Protectee\n\n" +
                    "Enemies / Players\n" +
                    "====================\n" +
                    "A = Grenadier\n" +
                    "S = Espion\n" +
                    "D = Average Joe (moving left)\n" +
                    "F = Average Joe (moving right)\n\n" +
                    "Spawners\n" +
                    "====================\n" +
                    "Z = Average Joe Spawner (moving left)\n" +
                    "X = Average Joe Spawner (moving right)"
                    , "Help", 0);
            }
        }
        private void ChangeSpriteImage()
        {
            keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Delete) == true)
            {
                // Reseting Position
                PrevPos = new Vector2(0, 0);
                mSprite.LoadContent(this.Content, "Delete");
                currentGuiObject = guiObject.Delete;
            }
            if (keyState.IsKeyDown(Keys.Q) == true)
            {
                // Reseting Position
                PrevPos = new Vector2(0, 0);
                mSprite.LoadContent(this.Content, "Block");
                currentGuiObject = guiObject.Block;
            }
            if (keyState.IsKeyDown(Keys.W) == true)
            {
                // Reseting Position
                PrevPos = new Vector2(0, 0);
                mSprite.LoadContent(this.Content, "Wall");
                currentGuiObject = guiObject.Wall;
            }
            if (keyState.IsKeyDown(Keys.A) == true)
            {
                // Reseting Position
                PrevPos = new Vector2(0, 0);
                mSprite.LoadContent(this.Content, "Grenadier/Stand0");
                currentGuiObject = guiObject.Grenadier;
            }
            if (keyState.IsKeyDown(Keys.S) == true)
            {
                // Reseting Position
                PrevPos = new Vector2(0, 0);
                mSprite.LoadContent(this.Content, "Espion/Stand0");
                currentGuiObject = guiObject.Espion;
            }
            if (keyState.IsKeyDown(Keys.D) == true)
            {
                mSprite.LoadContent(this.Content, "LesserDemon/run0");
                currentGuiObject = guiObject.LesserDemonLeft;
            }
            if (keyState.IsKeyDown(Keys.F) == true)
            {
                mSprite.LoadContent(this.Content, "LesserDemon/run0");
                currentGuiObject = guiObject.LesserDemonRight;
            }
            if (keyState.IsKeyDown(Keys.Z) == true)
            {
                // Reseting Position
                PrevPos = new Vector2(0, 0);
                mSprite.LoadContent(this.Content, "Spawner_Left");
                currentGuiObject = guiObject.SpawnerLeft;
            }
            if (keyState.IsKeyDown(Keys.X) == true)
            {
                // Reseting Position
                PrevPos = new Vector2(0, 0);
                mSprite.LoadContent(this.Content, "Spawner_Right");
                currentGuiObject = guiObject.SpawnerRight;
            }
            if (keyState.IsKeyDown(Keys.C) == true)
            {
                PrevPos = new Vector2(0, 0); 
                mSprite.LoadContent(this.Content, "Spawner_Player"); 
                currentGuiObject = guiObject.PlayerSpawn; 
            }
            if (keyState.IsKeyDown(Keys.E) == true)
            {
                // Reseting Position
                PrevPos = new Vector2(0, 0);
                mSprite.LoadContent(this.Content, "Protectee");
                currentGuiObject = guiObject.Protectee;
            }
        }
        private void LeftButtonActs(Vector2 pos)
        {
            if (currentGuiObject == guiObject.Block && (PrevPos != pos))
            {
                Debug.WriteLine("PrevPos values " + PrevPos.X.ToString() + PrevPos.Y.ToString());
                Debug.WriteLine("OuterPos values " + OuterPos.X.ToString() + OuterPos.Y.ToString());
                Block c = new Block((int)pos.X, (int)pos.Y);
                c.LoadContent(this.Content);
                level.data.AddLast(c);
                level.numArgs.AddLast(2);
                object[] d = new object[] { (int)pos.X, (int)pos.Y };
                level.args.AddLast(d);
                if (level.data.Contains(c))
                    Debug.WriteLine("Successful!");
            }

            if (currentGuiObject == guiObject.Wall && (PrevPos != pos))
            {
                Wall c = new Wall((int)pos.X, (int)pos.Y);
                c.LoadContent(this.Content);
                level.data.AddLast(c);
                level.numArgs.AddLast(2);
                object[] d = new object[] { (int)pos.X, (int)pos.Y };
                level.args.AddLast(d);
            }

            if (currentGuiObject == guiObject.Grenadier && (PrevPos != pos))
            {
                Grenadier c = new Grenadier((int)pos.X, (int)pos.Y, 512, 384);
                c.LoadContent(this.Content);
                level.data.AddLast(c);
                level.numArgs.AddLast(4);
                object[] d = new object[] { (int)pos.X, (int)pos.Y, 512, 384 };
                level.args.AddLast(d);
            }

            if (currentGuiObject == guiObject.Espion && (PrevPos != pos))
            {
                Espion c = new Espion((int)pos.X, (int)pos.Y, 512, 384);
                c.LoadContent(this.Content);
                level.data.AddLast(c);
                level.numArgs.AddLast(4);
                object[] d = new object[] { (int)pos.X, (int)pos.Y, 512, 384};
                level.args.AddLast(d);
            }

            if (currentGuiObject == guiObject.LesserDemonLeft && (PrevPos != pos))
            {
                LesserDemon c = new LesserDemon((int)pos.X, (int)pos.Y, -1);
                c.LoadContent(this.Content);
                level.data.AddLast(c);
                level.numArgs.AddLast(3);
                //LinkedList<object> d = new LinkedList<object>();
                /**
                d.AddLast((int)pos.X);
                d.AddLast((int)pos.Y);
                d.AddLast((int)-1);/**/
                object[] d = new object[] { (int)pos.X, (int)pos.Y, (int)-1 };
                level.args.AddLast(d);
            }
            if (currentGuiObject == guiObject.LesserDemonRight && (PrevPos != pos))
            {
                LesserDemon c = new LesserDemon((int)pos.X, (int)pos.Y, 1);
                c.LoadContent(this.Content);
                level.data.AddLast(c);
                level.numArgs.AddLast(3);
                //LinkedList<object> d = new LinkedList<object>();
                /**
                d.AddLast((int)pos.X);
                d.AddLast((int)pos.Y);
                d.AddLast((int)1);
                /**/
                object[] d = new object[] { (int)pos.X, (int)pos.Y, (int)1 };
                level.args.AddLast(d);
            }

            if (currentGuiObject == guiObject.SpawnerLeft && (PrevPos != pos))
            {
                Spawner c = new Spawner((int)pos.X, (int)pos.Y, 5000, "guiCreator.LesserDemon", -1, "Spawner_Left");
                c.LoadContent(this.Content);
                level.data.AddLast(c);
                level.numArgs.AddLast(5);
                object[] d = new object[] { (int)pos.X, (int)pos.Y, (int)5000, "guiCreator.LesserDemon", (int)-1 };
                level.args.AddLast(d);
            }
            if (currentGuiObject == guiObject.SpawnerRight && (PrevPos != pos))
            {
                Spawner c = new Spawner((int)pos.X, (int)pos.Y, 5000, "guiCreator.LesserDemon", 1, "Spawner_Right");
                c.LoadContent(this.Content);
                level.data.AddLast(c);
                level.numArgs.AddLast(5);
                object[] d = new object[] { (int)pos.X, (int)pos.Y, (int)5000, "guiCreator.LesserDemon", (int)1 };
                level.args.AddLast(d);
            }

            // Player Spawning Point
            if (currentGuiObject == guiObject.PlayerSpawn && (PrevPos != pos))
            {
                Spawner c = new Spawner((int)pos.X, (int)pos.Y, true, "Spawner_Player");
                c.LoadContent(this.Content);
                level.data.AddLast(c);
                level.numArgs.AddLast(4);
                object[] d = new object[] { (int)pos.X, (int)pos.Y, (bool)true, (string)"Spawner_Player"};
                level.args.AddLast(d); 
            }

            if (currentGuiObject == guiObject.Protectee && (PrevPos != pos))
            {
                Protectee c = new Protectee((int)pos.X, (int)pos.Y);
                c.LoadContent(this.Content);
                level.data.AddLast(c);
                level.numArgs.AddLast(2);
                object[] d = new object[] { (int)pos.X, (int)pos.Y };
                level.args.AddLast(d);
            }

            // Inserts a background into level
            if (currentGuiObject == guiObject.InsertBackGround)
            {
                BackGround = new Sprite((int)0, (int)0);
                BackGround.LoadContent(this.Content);
            }

            if (currentGuiObject == guiObject.Delete)
            {
                int SpriteIndex = 0; 
                mSprite = level.data.LastOrDefault(LevelY => LevelY.Position == pos);

                if (mSprite != null)
                    foreach (Sprite SpriteObject in level.data)
                    {
                        // When current index is not correct increase it
                        if (SpriteObject != mSprite)
                            ++SpriteIndex;
                        else
                        {
                            if (PrevPos != pos)
                            {
                                level.data.Remove(level.data.ElementAt(SpriteIndex));
                                level.args.Remove(level.args.ElementAt(SpriteIndex));
                                level.numArgs.Remove(level.numArgs.ElementAt(SpriteIndex));
                            }
                            mSprite = new Sprite((int)pos.X, (int)pos.Y);
                            mSprite.LoadContent(this.Content, "Delete");
                            break;
                        }
                    }
                else
                {
                    mSprite = new Sprite((int)pos.X, (int)pos.Y); 
                    mSprite.LoadContent(this.Content, "Delete");
                }

                /*LinkedListNode<Sprite> n = level.data.First;
                LinkedListNode<int> m = level.numArgs.First;
                LinkedListNode<object[]> o = level.args.First;
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
                }*/
            }
        }
        private void EditGuiActs(Vector2 pos)
        {
            // Setup to editting Spawner
            if (currentGuiMode == guiMode.EditGuiObject)
            {
                // Try & get a Sprite Object & test for a Spawner type
                mSprite = level.data.LastOrDefault(LevelY => LevelY.drawPosition == pos);
                if ((mSprite != null) &&
                    (mSprite.GetType() == typeof(Spawner)))
                {
                    // Load A Spawn Texture & Display its' enemycount
                    mSprite.LoadContent(this.Content);
                    Spawner SpawnCopyEnemyCount = (Spawner)mSprite;

                    if (currentGuiObject != guiObject.NewSpawner)
                        hFileName = "The type is a Spawner! Set enemy Count?[Y\\N]\n" +
                                    "enemy count " + SpawnCopyEnemyCount.ENEMYNUMBER;

                    // When ready to change the 
                    if (keyState.IsKeyDown(Keys.Y) == true)
                    {
                        hFileName = "";
                        currentGuiObject = guiObject.NewSpawner;
                    }
                }

                else
                {
                    mSprite = new Sprite((int)pos.X, (int)pos.Y);
                    mSprite.drawPosition = pos;
                    mSprite.LoadContent(this.Content, "SetSpawn");
                    hFileName = "It's not a Spawner! ";
                }

                //currentGuiObject = guiObject.CurrentSpawner;
            }

            if (currentGuiObject == guiObject.NewSpawner)
            {
                if ((fileName.ToUpper() == "Y") ||
                   (fileName.ToLower() == "y"))
                    fileName = fileName.Remove(fileName.Length - 1, 1);
                updateText();
                if (keyState.IsKeyDown(Keys.Enter) == true)
                    hFileName = "Done!";
            }
        }
        /*private void InterfaceActs(Vector2 pos)
        {
            // Make
            if (currentGuiMode == guiMode.EditInterface)
            { }
        }*/

        private void updateGui()
        {
            // Camera moving with arrow keys in GUI mode.
            KeyboardState keyState = Keyboard.GetState();
            
            // Provides different things based on F-key=============================
            FunctionKeys(); 
            //======================================================================

            if ((keyState.IsKeyDown(Keys.Up) == true))
                camera.Y -= cameraDist;
            if ((keyState.IsKeyDown(Keys.Down) == true))
                camera.Y += cameraDist;
            if ((keyState.IsKeyDown(Keys.Left) == true))
                camera.X -= cameraDist;
            if ((keyState.IsKeyDown(Keys.Right) == true))
                camera.X += cameraDist;

            if (keyState.IsKeyDown(Keys.Escape) == true)
                this.Exit();

            if ((currentGuiMode == guiMode.Save) || 
                (currentGuiMode == guiMode.Load) || 
                (currentGuiMode == guiMode.Test))
                updateText();

            if ((currentGuiMode == guiMode.PlayCreatedLevels))
            {
                updateText();
                if (fileName.Length > 1)
                    fileName = fileName.Remove(fileName.Length-2, 2);
            }
            else
            {
                MouseState state = Mouse.GetState();
                Vector2 pos = new Vector2(state.X, state.Y);
                pos = handleMouse(pos);
                if (currentGuiMode != guiMode.EditGuiObject)
                    mSprite.drawPosition = pos;
                OuterPos = pos += camera;

                // Changes image guiCreator displays==================================
                ChangeSpriteImage();
                //====================================================================

                // Finds Spawners and sets their spawn count==========================
                EditGuiActs(pos);
                //====================================================================

                if ((state.LeftButton == ButtonState.Pressed))
                {
                    // Mainly inserts object & deletes them===========================
                    LeftButtonActs(pos);
                    //================================================================

                    // Setting past cursor position to stop redundant "level" modifying
                    PrevPos = pos; 
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
                            mSprite.LoadContent(this.Content, "Block"); 
                            // Writes names of all levels to a file
                            // See if same value exist & copy it
                            if (!LevelNames.Contains(fileName))
                            {
                                SaveLevelList(fileName);
                            }
                        }
                        if (currentGuiMode == guiMode.Load)
                        {
                            loadGui(fileName);
                            mSprite.LoadContent(this.Content, "Block"); 
                        }
                        if (currentGuiMode == guiMode.Test)
                        {
                            if (!fileName.Equals("") && fileName != null)
                            {
                                Process.Start("Game.exe", fileName);
                            }
                        }
                        // Tells "Game.exe" to open "AllLevels"
                        if (currentGuiMode == guiMode.PlayCreatedLevels)
                            Process.Start("Game.exe", "AllLevels"); 

                        if (currentGuiMode == guiMode.EditGuiObject)
                        {
                                // code to do things with spawner's data
                            EditEnemySpawn(fileName);
                            mSprite.LoadContent(this.Content, "Block"); 
                            
                        }
                        //hFileName = "";
                        //fileName = "";
                        currentGuiMode = guiMode.Edit;
                        currentGuiObject = guiObject.Block; 
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

                // Make Line for Background to load
                SWA.WriteLine("BackGround title is " + BackGroundName); 

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
            // STMs for File & Reading. FileSTM opens the "fileName" file located here
            // "angelattack\guiCreator\bin\x86\Debug" And ReadSTM reads from that file.
            FileStream FS = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader SR = new StreamReader(FS);

            if (File.Exists(fileName) && SR.Peek() != -1)
            {
                camera = new Vector2(0, 0);
                level.data = new LinkedList<Sprite>();
                level.numArgs = new LinkedList<int>();
                level.args = new LinkedList<object[]>();
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

                string SpriteTypeData = "";
                string[] FileLine; 

                if (SR.Peek() != -1)
                SpriteTypeData = SR.ReadLine();      // Gets the SPROBJ for the ARGs

                // Skip the Background title for now
                if (SR.Peek() != -1 && SpriteTypeData.Contains("BackGround"))
                    SpriteTypeData = SR.ReadLine(); 

                // If SR.ReadLine() spits out "Cats love catfish" the below Split() is told to 
                // make FileLine[] = {"Cats", "love", "catfish"}. ReadLine() returns a STR
                if (SR.Peek() != -1)
                    FileLine = SR.ReadLine().Split(new char[] { ' ' });
                else
                    FileLine = new string[3]; 

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
        
        // Saves a List of levels
        public void SaveLevelList(string fileName)
        {
            // The count of levels to play
            if(LevelNames.Count > 0)
                LevelNames.RemoveFirst();

            int ArrayCount = LevelNames.Count + 1; // Count of Levels created
            LevelNames.AddFirst(ArrayCount.ToString()); 

            // Adding new node
            LevelNames.AddLast(fileName); 

            // New streams one to write too & one to do writing
            FileStream FSA = new FileStream("AllLevels", FileMode.Create, FileAccess.Write);
            StreamWriter SWA = new StreamWriter(FSA);

            // Iterate through LevelName's Nodes
            LinkedListNode<string> LevelNameSingle = LevelNames.First; 

            // Keep Writing till there's no more levels!
            while (LevelNameSingle != null)
            {
                SWA.WriteLine(LevelNameSingle.Value);
                LevelNameSingle = LevelNameSingle.Next; 
            }
            // Gotta close the streams
            SWA.Close();
            FSA.Close(); 
        }
        public void LoadLevelList()
        {
            // Load only when this file exists
            if (File.Exists("AllLevels"))
            {
                // Streams read lines from the "AllLevels" file
                FileStream FS = new FileStream("AllLevels", FileMode.Open, FileAccess.Read);
                StreamReader SR = new StreamReader(FS);

                // Keeps 1 line of text from file
                string TextLine;

                // Only do when there is data is to read!
                while (SR.Peek() != -1)
                {
                    // Just Adding names of all levels from "AllLevel" file
                    TextLine = SR.ReadLine();
                    LevelNames.AddLast(TextLine);
                }
                // Gotta close my streams
                SR.Close();
                FS.Close();
            }
        }

        // Edits enemy count of a spawner
        public void EditEnemySpawn(string EnemyCount)
        {
            // Get copy of Spawner
            Spawner SpawnCopy = (Spawner)mSprite; 
            SpawnCopy.LoadContent(this.Content, "Spawner"); 

            int NumeralEnemyCount;  // Stores string val as int
            int SpriteIndex = 0;    // Where Spawn mSprite is located in List
            object[] Arguments;     // Arguments to Spawner

            // Finding where mSprite is located in list
            foreach (Sprite SpriteObject in level.data)
            {
                // When current index is not correct increase it
                if (SpriteObject != mSprite)
                    ++SpriteIndex;
                else
                {
                    level.data.Remove(level.data.ElementAt(SpriteIndex));
                    level.args.Remove(level.args.ElementAt(SpriteIndex));
                    level.numArgs.Remove(level.numArgs.ElementAt(SpriteIndex));
                    break;
                }

            }

            // Knowing the Index I can use this to remove from LevelData struct
            Debug.WriteLine("Sprite Index is " + SpriteIndex);
            Debug.WriteLine("Size of level.data is " + level.data.Count); 
            

            // Initialize count to spawner copy and add new & remove old spawn.
            if (Int32.TryParse(EnemyCount, out NumeralEnemyCount))
            {
                // Updating LevelData struct
                Spawner NewSpawner = new Spawner((int)SpawnCopy.sArgs[0],
                  (int)SpawnCopy.sArgs[1], SpawnCopy.spawnDelay, SpawnCopy.sType,
                  (int)SpawnCopy.sArgs[2], SpawnCopy.AssetName, NumeralEnemyCount);

                NewSpawner.LoadContent(this.Content, "Spawner"); 

                Arguments = new object[]{SpawnCopy.sArgs[0], SpawnCopy.sArgs[1], 
                                         SpawnCopy.spawnDelay, SpawnCopy.sType, 
                                         SpawnCopy.sArgs[2], SpawnCopy.AssetName,
                                         NumeralEnemyCount};
                
                level.args.AddLast(Arguments);
                level.data.AddLast(NewSpawner);
                level.numArgs.AddLast(7); 

                hFileName = "Successful!";
            }
            else 
                hFileName = "It Failed! :(";

            mSprite.LoadContent(this.Content, "Block"); 
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
            BackGround.Draw(this.spriteBatch, camera);    
            foreach (Sprite n in level.data)
            {
                n.Draw(this.spriteBatch, camera);
            }
            if(currentGuiMode==guiMode.Edit)
            {
                mSprite.Draw(this.spriteBatch);
            }
            if ((currentGuiMode == guiMode.Load) || 
                (currentGuiMode == guiMode.Save) || 
                (currentGuiMode == guiMode.Test) ||
                (currentGuiMode == guiMode.PlayCreatedLevels))
            {
                header.DrawText(this.spriteBatch, hFileName);
                text.DrawText(this.spriteBatch, fileName);
            }
            if (currentGuiMode == guiMode.EditGuiObject)
            {
                mSprite.Draw(this.spriteBatch);
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
