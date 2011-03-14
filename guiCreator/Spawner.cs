using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace guiCreator
{
    public class Spawner : Sprite
    {
        const string ASSETNAME = "Spawner";
        string sType;
        object[] sArgs;
        int elapsed = 0;
        int spawnDelay = 100;

        public Spawner(int startX, int startY, int delay, string type, int direction) : base(startX, startY)
        {
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
                 if(elapsed>=spawnDelay)
                 {
                     elapsed = 0;
                     Sprite a = (Sprite)Activator.CreateInstance(Type.GetType(sType), sArgs);
                     a.LoadContent(theContentManager);
                     level.AddLast(a);
                 }
                 base.Update(theGameTime, level, theContentManager);
            }

            return level;
        }
    }
}
