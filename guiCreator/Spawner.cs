using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace guiCreator
{
    public class Spawner : Sprite
    {
        string ASSETNAME = "Spawner";
        string sType;
        object[] sArgs;
        int elapsed = 0;
        int spawnDelay = 100;

        int DemonCount = 0;        // The Number of demons comming from This Spawner!
        bool DoneSpawning = false; // false if enemies are still being spawned true for not

        public Spawner(int startX, int startY, int delay, string type, int direction) : base(startX, startY)
        {
            spawnDelay = delay;
            sType = type;
            sArgs = new object[] { startX, startY, direction };
        }

        public Spawner(int startX, int startY, int delay, string type, int direction, string ASSETFILE)
            : base(startX, startY)
        {
            // The new graphic displayed is this
            ASSETNAME = ASSETFILE; 
            
            spawnDelay = delay;
            sType = type;
            sArgs = new object[] { startX, startY, direction };
        }

        public override void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, ASSETNAME);
        }

        public override LinkedList<Sprite> Update(GameTime theGameTime, LinkedList<Sprite> level, ContentManager theContentManager)
        {
            for (int i = 0; i < 10; i++)
            {
                 elapsed++;

                 // Keeps on Spawning till no more need to be spawned. 
                 if (DemonCount > 0 && !(DoneSpawning))
                 {
                     if (elapsed >= spawnDelay)
                     {
                         elapsed = 0;
                         Sprite a = (Sprite)Activator.CreateInstance(Type.GetType(sType), sArgs);
                         a.LoadContent(theContentManager);
                         level.AddLast(a);
                     }
                     --DemonCount;
                 }
                 else
                     DoneSpawning = true;   // We're done spawning now!

                 base.Update(theGameTime, level, theContentManager);
            }

            return level;
        }
        
        // Sets & gets the number of enemies to spawn out. 
        public int ENEMYNUMBER
        {
            get { return DemonCount; }
            set { DemonCount = value; }
        }

        // To know if the spawner is done or not.
        public bool SPAWNSTATE
        {
            get { return DoneSpawning; }
            set { DoneSpawning = value; }
        }
    }
}
