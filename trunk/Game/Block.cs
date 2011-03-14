using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace guiCreator
{
    public class Block : Sprite
    {
        const string ASSETNAME = "Block";

        public Block(int startX, int startY) : base(startX, startY) { }

        public override void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, ASSETNAME);
        }
    }
}
