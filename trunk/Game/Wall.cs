using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace guiCreator
{
    public class Wall : Sprite
    {
        const string ASSETNAME = "Wall";
        float health = 100;
        const float MAX_HEALTH = 100;
        Texture2D healthGreen;
        Texture2D healthRed;

        public Wall(int startX, int startY) : base(startX, startY) { }

        public override void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, ASSETNAME);
            healthGreen = theContentManager.Load<Texture2D>("UI/HealthGreen");
            healthRed = theContentManager.Load<Texture2D>("UI/HealthRed");
        }

        public override bool takeDamage(float damageAmount)
        {
            health -= damageAmount;
            if (health > MAX_HEALTH)
            {
                health = MAX_HEALTH;
            }
            if (health <= 0)
            {
                return true;
            }
            return false;
        }

        public override void Draw(SpriteBatch theSpriteBatch)
        {
            Vector2 pos = drawPosition;
            float width = 40;
            float height = 10;

            pos.Y -= 15;
            theSpriteBatch.Draw(healthRed, pos,
               new Rectangle(0, 0, (int)width, (int)height),
               Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0);

            width = 40 * (health / MAX_HEALTH);
            theSpriteBatch.Draw(healthGreen, pos,
               new Rectangle(0, 0, (int)width, (int)height),
               Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0);
        
            base.Draw(theSpriteBatch);
        }
    }
}
