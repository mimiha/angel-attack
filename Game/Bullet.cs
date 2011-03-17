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

        // stats are dependant on grenadier
        // they are pushed over from the grenadier in the constructor.
        float attack, critChance, critMod;
        int attackMod;

        // Bullet constructor.
        public Bullet(int startX, int startY, int direction, float dmg, float range, float crit, float critDmg, int mod) : base(startX, startY)
        {
            startingX = startX;
            startingY = startY;
            sDirection = direction;
            attack = dmg;
            critChance = crit;
            critMod = critDmg;
            attackMod = mod;
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
                            if (n.Value.takeDamage(damageCalculation(attack) ))
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

        public override float damageCalculation(float pureAttk)
        {
            // the modifier is BEFORE the critical damage is applied
            Random mod = new Random();
            float rand = mod.Next(-attackMod, attackMod);
            rand /= 10; //we divide it to get a float, what we wanted
            float modDmg = pureAttk * rand;
            pureAttk += modDmg;
            if (criticalChance(critChance) == true)
            { //critical hit! it's super effective!
                pureAttk *= CRITICAL_DAMAGE;    //add critical damage bonus
                if (critMod > 0) //if we have crit modifiers, add the damage
                {
                    // critMod damage is in %, so 2 = .2% 
                    float tempMod = pureAttk * critMod / 10;
                    pureAttk += tempMod;
                }
            }
            return pureAttk;
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
