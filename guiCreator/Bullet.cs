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
        int sDirection;
        const float ATTACK_DAMAGE = 20.0f;
        bool doneAttacking = false;

        public Bullet(int startX, int startY, int direction) : base(startX, startY)
        {
            sDirection = direction;
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
                    if (n.Value.GetType().ToString() == typeof(LesserDemon).ToString())
                    {
                        if (IntersectPixels(sBounds, textureData, n.Value.sBounds, n.Value.textureData))
                        {
                            doneAttacking = true;
                            level.Remove(this);
                            if (n.Value.takeDamage(ATTACK_DAMAGE))
                            {
                                level.Remove(n);
                            }
                            break;
                        }
                    }
                    if ((n.Value.GetType().ToString() == typeof(Block).ToString()))
                    {
                        if (IntersectPixels(sBounds, textureData, n.Value.sBounds, n.Value.textureData))
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
