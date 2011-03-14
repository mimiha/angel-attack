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

        public struct LevelData
        {
            public LinkedList<Sprite> data;
            public LinkedList<int> numArgs;
            public LinkedList<LinkedList<object>> args;
        }
        LevelData level = new LevelData();

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
            Protectee
        }

        //Sets default object in GUI mode to be the block
        guiObject currentGuiObject = guiObject.Block;

        enum guiMode
        {
            Edit,
            Save,
            Load,
            Test
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
            level.args = new LinkedList<LinkedList<object>>();
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
            if (keyState.IsKeyDown(Keys.F10) == true)
            {
                currentGuiMode = guiMode.Test;
                hFileName = "Test Level: ";
                fileName = "";
            }
            if (keyState.IsKeyDown(Keys.F12) == true)
            {
                MessageBox(new IntPtr(0),
                    "Special Keys / Functions\n====================\nF1 = New Level\nF2 = Load Level\nF3 = Save Level\nF10 = Test Level\nF12 = Help\nDelete = Delete Mode\n\nBlocks\n====================\nQ = Regular Block\nW = Wall\nE = Protectee\n\nEnemies / Players\n====================\nA = Grenadier\nS = Average Joe (moving left)\nD = Average Joe (moving right)\n\nSpawners\n====================\nZ = Average Joe Spawner (moving left)\nX = Average Joe Spawner (moving right)"
                    , "Help", 0);
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
                        level.numArgs.AddLast(2);
                        LinkedList<object> d = new LinkedList<object>();
                        d.AddLast((int)pos.X);
                        d.AddLast((int)pos.Y);
                        level.args.AddLast(d);
                    }
                    if (currentGuiObject == guiObject.Wall)
                    {
                        Wall c = new Wall((int)pos.X, (int)pos.Y);
                        c.LoadContent(this.Content);
                        level.data.AddLast(c);
                        level.numArgs.AddLast(2);
                        LinkedList<object> d = new LinkedList<object>();
                        d.AddLast((int)pos.X);
                        d.AddLast((int)pos.Y);
                        level.args.AddLast(d);
                    }
                    if (currentGuiObject == guiObject.Player)
                    {
                        Grenadier c = new Grenadier((int)pos.X, (int)pos.Y, 512, 384);
                        c.LoadContent(this.Content);
                        level.data.AddLast(c);
                        level.numArgs.AddLast(4);
                        LinkedList<object> d = new LinkedList<object>();
                        d.AddLast((int)pos.X);
                        d.AddLast((int)pos.Y);
                        d.AddLast(512);
                        d.AddLast(384);
                        level.args.AddLast(d);
                    }
                    if (currentGuiObject == guiObject.AverageJoeLeft)
                    {
                        AverageJoe c = new AverageJoe((int)pos.X, (int)pos.Y, -1);
                        c.LoadContent(this.Content);
                        level.data.AddLast(c);
                        level.numArgs.AddLast(3);
                        LinkedList<object> d = new LinkedList<object>();
                        d.AddLast((int)pos.X);
                        d.AddLast((int)pos.Y);
                        d.AddLast((int)-1);
                        level.args.AddLast(d);
                    }
                    if (currentGuiObject == guiObject.AverageJoeRight)
                    {
                        AverageJoe c = new AverageJoe((int)pos.X, (int)pos.Y, 1);
                        c.LoadContent(this.Content);
                        level.data.AddLast(c);
                        level.numArgs.AddLast(3);
                        LinkedList<object> d = new LinkedList<object>();
                        d.AddLast((int)pos.X);
                        d.AddLast((int)pos.Y);
                        d.AddLast((int)1);
                        level.args.AddLast(d);
                    }
                    if (currentGuiObject == guiObject.SpawnerLeft)
                    {
                        Spawner c = new Spawner((int)pos.X, (int)pos.Y, 5000, "guiCreator.AverageJoe", -1);
                        c.LoadContent(this.Content);
                        level.data.AddLast(c);
                        level.numArgs.AddLast(5);
                        LinkedList<object> d = new LinkedList<object>();
                        d.AddLast((int)pos.X);
                        d.AddLast((int)pos.Y);
                        d.AddLast((int)5000);
                        d.AddLast("guiCreator.AverageJoe");
                        d.AddLast((int)-1);
                        level.args.AddLast(d);
                    }
                    if (currentGuiObject == guiObject.SpawnerRight)
                    {
                        Spawner c = new Spawner((int)pos.X, (int)pos.Y, 5000, "guiCreator.AverageJoe", 1);
                        c.LoadContent(this.Content);
                        level.data.AddLast(c);
                        level.numArgs.AddLast(5);
                        LinkedList<object> d = new LinkedList<object>();
                        d.AddLast((int)pos.X);
                        d.AddLast((int)pos.Y);
                        d.AddLast((int)5000);
                        d.AddLast("guiCreator.AverageJoe");
                        d.AddLast((int)1);
                        level.args.AddLast(d);
                    }
                    if (currentGuiObject == guiObject.Protectee)
                    {
                        Protectee c = new Protectee((int)pos.X, (int)pos.Y);
                        c.LoadContent(this.Content);
                        level.data.AddLast(c);
                        level.numArgs.AddLast(2);
                        LinkedList<object> d = new LinkedList<object>();
                        d.AddLast((int)pos.X);
                        d.AddLast((int)pos.Y);
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
                FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                LinkedListNode<Sprite> n = level.data.First;
                LinkedListNode<int> m = level.numArgs.First;
                LinkedListNode<LinkedList<object>> o = level.args.First;
                while (n != null)
                {
                    sw.WriteLine(n.Value.GetType().ToString());
                    sw.WriteLine(m.Value);
                    LinkedListNode<object> p = o.Value.First;
                    while (p != null)
                    {
                        sw.WriteLine(p.Value.GetType().ToString());
                        sw.WriteLine(p.Value);

                        p = p.Next;
                    }
                    n = n.Next;
                    m = m.Next;
                    o = o.Next;
                }
                sw.Close();
                fs.Close();
            }
        }

        public void loadGui(string fileName)
        {
            if (File.Exists(fileName))
            {
                camera = new Vector2(0, 0);
                level.data = new LinkedList<Sprite>();
                level.numArgs = new LinkedList<int>();
                level.args = new LinkedList<LinkedList<object>>();
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
                    //level.args.AddLast(llArgs);
                    Sprite a = (Sprite)Activator.CreateInstance(Type.GetType(type), args);
                    a.LoadContent(this.Content);
                    level.data.AddLast(a);
                    //level.numArgs.AddLast(numArgs);
                }
                sr.Close();
                fs.Close();
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
