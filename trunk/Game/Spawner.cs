using System;
using System.Collections.Generic;
using System.Diagnostics; 
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
/*------------------------------EMMANUEL'S EDITS--------------------------------
 * 01. Added "DemonCount" field
 * 02. Edited "Update()" to make Spawn a number based on a condition
 * 03. 2nd constructor for Spawner created
 */
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

        public bool DemonSpawner = true; 
        public Spawner(int startX, int startY, int delay, string type, int direction) : base(startX, startY)
        { 
            spawnDelay = delay;
            sType = type;
            sArgs = new object[] { startX, startY, direction };
        }
 
        // New Constructor initializing number of enemies spawned
        public Spawner(int startX, int startY, int delay, string type, int direction, int DemonNumber)
            : base(startX, startY)
        {
            spawnDelay = delay;
            sType = type;
            sArgs = new object[] { startX, startY, direction };
            DemonCount = DemonNumber; 
        }

        // My Spawner initializer
        public Spawner(int startX, int startY, int delay, string type,
                       int direction, string ASSETFILE, int EnemyNumber)
            : base(startX, startY)
        {
            // The new graphic displayed is this
            ASSETNAME = ASSETFILE;
            DemonCount = EnemyNumber;
            spawnDelay = delay;
            sType = type;
            sArgs = new object[] { startX, startY, direction };
        }

        public override void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, ASSETNAME);
        }

        // This will Spawn whatever the player picks
        public Spawner(int startX, int startY, int screenPosX, int screenPosY,
                       string type, int direction, string ASSETFILE, bool SpawnType)
            : base(startX, startY)
        {
            DemonSpawner = SpawnType;
            ASSETNAME = ASSETFILE;
            sType = type; 
            sArgs = new object[] { startX, startY, 512, 384 };  
        }

        // Blank Spawner for player
        public Spawner(int startX, int startY, bool PlayerSpawning, string ASSETFILE)
            : base(startX, startY)
        {
            PlayerSpawned = PlayerSpawning;
            ASSETNAME = ASSETFILE;
        }

        public override LinkedList<Sprite> Update(GameTime theGameTime, LinkedList<Sprite> level, ContentManager theContentManager)
        {
            for (int i = 0; i < 10; i++)
            {
                 elapsed++;

                 if (!PlayerSpawned)
                     // Keeps on Spawning till no more need to be spawned. 
                     if (DemonCount > 0 && !(DoneSpawning))
                     {
                         if (elapsed >= spawnDelay)
                         {
                             elapsed = 0;
                             Sprite a = (Sprite)Activator.CreateInstance(Type.GetType(sType), sArgs);
                             a.LoadContent(theContentManager);
                             level.AddLast(a);
                             --DemonCount; // Reduce count
                         }
                     }
                     else
                     {
                         DoneSpawning = true;   // We're done spawning now!
                     }
                 else
                 {
                     spawnDelay = 5000;
                     if (elapsed >= spawnDelay)
                         level.Remove(this);
                 }

                 base.Update(theGameTime, level, theContentManager);
            }

            return level;
        }

        // Sets & gets the number of enemies to spawn out. 
        public int ENEMYNUMBER{
            get { return DemonCount;  }
            set { DemonCount = value; }
        }

        // To know if the spawner is done or not.

    }
    
}
