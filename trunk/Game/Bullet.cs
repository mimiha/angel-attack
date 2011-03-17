using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace guiCreator
{
    public class Bullet : Sprite
    {
        const string ASSETNAME = "Grenadier/Bullet";
        const float SPEED = 100.0f;
        int sDirection, startingX, startingY;
        float bullet_range;
        bool doneAttacking = false;

        // attack is dependant on grenadier's attack.
        // it is pushed over from the grenadier in the constructor.
        float attack;

        // Bullet constructor.
        public Bullet(int startX, int startY, int direction, float dmg, float range) : base(startX, startY)
        {
            startingX = startX;
            startingY = startY;
            sDirection = direction;
            attack = dmg;
            bullet_range = range;
        }


        public override void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, ASSETNAME);
        }


        public override LinkedList<Sprite> Update(GameTime theGameTime, LinkedList<Sprite> level, ContentManager theContentManager)
        {
            float elapsed = (float)theGameTime.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < 10; i++)
            {
                createBounds();

                updateMove(elapsed);

                handleCollisions(level);

                base.Update(theGameTime, level, theContentManager);
            }

            return level;
        }

        public void updateMove(float elapsed)
        {
            Vector2 velocity = Vector2.Zero;
            velocity.X = SPEED * sDirection;
            Position += velocity * elapsed;
        }

        public LinkedList<Sprite> handleCollisions(LinkedList<Sprite> level)
        {
            if (!doneAttacking)
            {
                LinkedListNode<Sprite> n = level.First;
                while (n != null)
                {
                    // Bullet runs into an enemy.
                    if (n.Value.GetType().ToString() == typeof(LesserDemon).ToString())
                    {
                        if (IntersectPixels(sBounds, textureData, n.Value.sBounds, n.Value.textureData))
                        {
                            doneAttacking = true;
                            level.Remove(this); //delete bullet
                            if (n.Value.takeDamage( attack ))
                            {
                                //delete enemy
                                level.Remove(n);
                            }
                            break;
                        }
                    }
                    // bullet runs into a block.
                    if ((n.Value.GetType().ToString() == typeof(Block).ToString()))
                    {
                        if (IntersectPixels(sBounds, textureData, n.Value.sBounds, n.Value.textureData))
                        {
                            //delete bullet
                            level.Remove(this);
                            break;
                        }
                    }

                    //bullet is too far out.
                    if (sDirection == 1) //going right
                    {
                        if (Position.X > startingX + bullet_range)
                        {
                            level.Remove(this);
                            break;
                        }
                    }
                    else if (sDirection == -1) // going left
                    {
                        if (Position.X < startingX - bullet_range)
                        {
                            level.Remove(this);
                            break;
                        }
                    }

                    n = n.Next;
                }
            }

            return level;
        }

        public override void Draw(SpriteBatch theSpriteBatch)
        {
            if (sDirection == (1))
            {
                theSpriteBatch.Draw(mSpriteTexture, drawPosition,
                    new Rectangle(0, 0, mSpriteTexture.Width, mSpriteTexture.Height),
                    Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0);
            }
            else
            {
                theSpriteBatch.Draw(mSpriteTexture, drawPosition,
                new Rectangle(0, 0, mSpriteTexture.Width, mSpriteTexture.Height),
                Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.FlipHorizontally, 0);
            }
        }

    }
}
