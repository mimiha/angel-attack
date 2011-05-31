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
        const string ASSETNAME = "Spawner";
        string sType;
        object[] sArgs;
        int elapsed = 0;
        int spawnDelay = 100;
        
        int DemonCount = 1;        // The Number of demons comming from This Spawner!

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
                         --DemonCount; // Reduce count
                     }

                 }
                 else
                 {
                     DoneSpawning = true;   // We're done spawning now!
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
