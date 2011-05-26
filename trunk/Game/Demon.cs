using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace guiCreator
{
    public class LesserDemon : Sprite
    {
        const string ASSETNAME = "AverageJoe/aj_run0";
        Vector2 velocity;
        int sDirection;
        Texture2D healthGreen;
        Texture2D healthRed;
        //Texture2D _DEBUG;   //DEBUG BOUNDING BOXES


        int elapsedAttack = 0;
        int elapsedAttackAnimation = 60;
        const float ACCELERATION = 0.5f;
        const float SPEED_MAX = 8.0f;
        const float GRAVITY_ACCELERATION = 0.5f;
        const float GRAVITY_MAX = 25.0f;


        //= Base Stats
        //======================================
        const float MAX_HEALTH = 50;
        float health = MAX_HEALTH;              // current health of monster
        const float BASE_ATTACK_DAMAGE = 12,    // Max damage a monster does
            ATTACK_MOD = 0.7f,                    // Attack modifier (.7%)
            ATTACK_ANIMATION_LENGTH = 300,      // Max amount of miliseconds an attack is in animation
            ATTACK_DELAY = 500,                 // delay before next attack
            CRITCHANCE = 3,                     // chance of crit (%)
            DEFENSE = 5;                       // damage reduced (%) from basic attacks


        // Animation variables
        string animation;
        bool animationIsLooping;
        const float FRAME_TIME = 0.7f;
        float time;
        int frameIndex;
        int frameCount;

        // Start collision detection/response variables

        public struct CorrectionVector2
        {
            public DirectionX DirectionX;
            public DirectionY DirectionY;
            public float X;
            public float Y;
        }

        public enum DirectionX
        {
            Left = -1,
            None = 0,
            Right = 1
        }

        public enum DirectionY
        {
            Up = -1,
            None = 0,
            Down = 1
        }

        // End Collision detection/response variables

        LinkedList<Sprite> collidingObjects = new LinkedList<Sprite>();

        public LesserDemon(int startX, int startY, int direction) : base(startX, startY) 
        {
            sDirection = direction;

        }

        public override void LoadContent(ContentManager theContentManager)
        {
            velocity = Vector2.Zero;
            base.LoadContent(theContentManager, ASSETNAME);
            healthGreen = theContentManager.Load<Texture2D>("UI/HealthGreen");
            healthRed = theContentManager.Load<Texture2D>("UI/HealthRed");
            //_DEBUG = theContentManager.Load<Texture2D>("black");
        }

        public override LinkedList<Sprite> Update(GameTime theGameTime, LinkedList<Sprite> level, ContentManager theContentManager)
        {
            float elapsed = (float)theGameTime.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < 10; i++)
            {
                createBounds();

                level = updateAttack(theGameTime, level, theContentManager);

                updateAnimation(theGameTime, theContentManager);

                applyPhysics(elapsed, level);

                base.Update(theGameTime, level, theContentManager);
            }
            return level;
        }

        public void applyPhysics(float elapsed, LinkedList<Sprite> level)
        {
            getCollisions(level);

            if (isOnGround())
            {
                if (!isCollidingWithDefenses())
                {
                    velocity.X = SPEED_MAX * sDirection;
                }
                else
                {
                    velocity.X = 0;
                }
            }
            else
            {
                velocity.X = 0;
            }
            if ((velocity.X > SPEED_MAX))
            {
                velocity.X = SPEED_MAX;
            }
            if ((velocity.X < -SPEED_MAX))
            {
                velocity.X = -SPEED_MAX;
            }


            if (!isOnGround())
            {
                velocity.Y += GRAVITY_ACCELERATION;
            }
            else
            {
                velocity.Y = 0;
            }
            if (velocity.Y > GRAVITY_MAX)
            {
                velocity.Y = GRAVITY_MAX;
            }

            bool enemyInFront = false;
            foreach (Sprite n in collidingObjects)
            {
                if ((n.GetType().ToString() == typeof(Wall).ToString()) || (n.GetType().ToString() == typeof(Protectee).ToString()))
                {
                    if (IntersectBounds(boundsLeft, n.sBounds) || IntersectBounds(boundsRight, n.sBounds))
                    {
                        enemyInFront = true;
                    }
                }
            }
            if (!enemyInFront)
            {
                Position += velocity * elapsed;
            }

            handleCollisions(level);
        }

        public LinkedList<Sprite> updateAttack(GameTime theGameTime, LinkedList<Sprite> level, ContentManager theContentManager)
        {
            if (elapsedAttack == ATTACK_DELAY)
            {
                foreach (Sprite n in collidingObjects)
                {
                    if ((n.GetType().ToString() == typeof(Wall).ToString()) || (n.GetType().ToString() == typeof(Protectee).ToString()))
                    {
                        if (IntersectBounds(boundsLeft, n.sBounds) || IntersectBounds(boundsBottom, n.sBounds) || IntersectBounds(boundsRight, n.sBounds))
                        {
                            elapsedAttackAnimation = 0;
                            if (n.takeDamage(BASE_ATTACK_DAMAGE))
                            {
                                level.Remove(n);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                elapsedAttack++;
            }
            if (elapsedAttackAnimation < ATTACK_ANIMATION_LENGTH)
            {
                elapsedAttack = 0;
                elapsedAttackAnimation++;
                PlayAnimation("AverageJoe/aj_atk", 7, true);
            }
            else
            {
                PlayAnimation("AverageJoe/aj_run", 15, true);
            }
            return level;
        }

        public void updateAnimation(GameTime theGameTime, ContentManager theContentManager)
        {
            if (animation == null)
                throw new NotSupportedException("No animation is currently playing.");

            // Process passing time.
            time += (float)theGameTime.ElapsedGameTime.TotalSeconds;
            while (time > FRAME_TIME)
            {
                time -= FRAME_TIME;

                // Advance the frame index; looping or clamping as appropriate.
                if (animationIsLooping)
                {
                    frameIndex = (frameIndex + 1) % frameCount;
                }
                else
                {
                    frameIndex = Math.Min(frameIndex + 1, frameCount - 1);
                }
            }
            mSpriteTexture = theContentManager.Load<Texture2D>(animation + frameIndex);
        }

        public void PlayAnimation(string anim, int fCount, bool loop)
        {
            // If this animation is already running, do not restart it.
            if (anim == animation)
                return;

            // Start the new animation.
            animation = anim;
            frameCount = fCount;
            animationIsLooping = loop;
            this.frameIndex = 0;
            this.time = 0.0f;
        }

        // taken from http://go.colorize.net/xna/2d_collision_response_xna/index.html
        public void handleCollisions(LinkedList<Sprite> level)
        {
            if (isColliding())
            {
                LinkedList<CorrectionVector2> corrections = new LinkedList<CorrectionVector2>();
                foreach (Sprite n in collidingObjects)
                {
                    if ((n.GetType().ToString() == typeof(Block).ToString()) || (n.GetType().ToString() == typeof(Wall).ToString()) || (n.GetType().ToString() == typeof(Protectee).ToString()))
                    {
                        if (IntersectPixels(sBounds, textureData, n.sBounds, n.textureData))
                        {
                            corrections.AddLast(getCorrectionVector(n));
                        }
                    }
                }

                int horizontalSum = 0;
                int verticalSum = 0;

                foreach (CorrectionVector2 n in corrections)
                {
                    if (n.DirectionX == DirectionX.Left)
                        horizontalSum--;
                    if (n.DirectionX == DirectionX.Right)
                        horizontalSum++;
                    if (n.DirectionY == DirectionY.Up)
                        verticalSum--;
                    if (n.DirectionY == DirectionY.Down)
                        verticalSum++;
                }

                DirectionX directionX = DirectionX.None;
                DirectionY directionY = DirectionY.None;


                if (horizontalSum <= -1)
                    directionX = DirectionX.Left;
                else if (horizontalSum >= 1)
                    directionX = DirectionX.Right;
                else
                    directionX = DirectionX.None; // if they cancel each other out, i.e 2 Left and 2 Right


                if (verticalSum <= (float)DirectionY.Up)
                    directionY = DirectionY.Up;
                else if (verticalSum >= (float)DirectionY.Down)
                    directionY = DirectionY.Down;
                else
                    directionY = DirectionY.None; // if they cancel each other out, i.e 1 Up and 1 Down

                CorrectionVector2 smallestCorrectionY = getSmallestCorrectionY(directionY, corrections);
                CorrectionVector2 smallestCorrectionX = getSmallestCorrectionX(directionX, corrections);

                if (Math.Abs(verticalSum) > Math.Abs(horizontalSum)) // start with Y, if collision = then try X
                {
                    correctCollision(smallestCorrectionY, false);
                    createBounds();
                    if (isColliding())
                        correctCollision(smallestCorrectionX, true);
                    else
                        directionX = DirectionX.None;
                }
                else if (Math.Abs(horizontalSum) > Math.Abs(verticalSum)) // start with X, if collision = then try Y
                {
                    correctCollision(smallestCorrectionX, true);
                    createBounds();
                    if (isColliding())
                        correctCollision(smallestCorrectionY, false);
                    else
                        directionY = DirectionY.None;
                }
                else
                {
                    if (smallestCorrectionX.X > smallestCorrectionY.Y) // start with Y
                    {
                        correctCollision(smallestCorrectionY, false);
                        createBounds();
                        if (isColliding())
                            correctCollision(smallestCorrectionX, true);
                        else
                            directionX = DirectionX.None;
                    }
                    else // start with X
                    {
                        correctCollision(smallestCorrectionX, true);
                        createBounds();
                        if (isColliding())
                            correctCollision(smallestCorrectionY, false);
                        else
                            directionY = DirectionY.None;
                    }
                }
            }
        }

        public void getCollisions(LinkedList<Sprite> level)
        {
            collidingObjects = new LinkedList<Sprite>();
            Rectangle collisionArea = new Rectangle(sBounds.X - 5, sBounds.Y - 5, sBounds.Width + 10, sBounds.Height + 10);
            foreach (Sprite n in level)
            {
                if (IntersectBounds(collisionArea, n.sBounds))
                {
                    collidingObjects.AddLast(n);
                }
            }
        }

        public bool isColliding()
        {
            foreach (Sprite n in collidingObjects)
            {
                if ((n.GetType().ToString() == typeof(Block).ToString()) || (n.GetType().ToString() == typeof(Wall).ToString()) || (n.GetType().ToString() == typeof(Protectee).ToString()))
                {
                    if (IntersectPixels(sBounds, textureData, n.sBounds, n.textureData))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool isOnGround()
        {
            foreach (Sprite n in collidingObjects)
            {
                if (n.GetType().ToString() == typeof(Block).ToString())
                {
                    if (IntersectBounds(boundsBottom, n.sBounds))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool isCollidingWithDefenses()
        {
            foreach (Sprite n in collidingObjects)
            {
                if (n.GetType().ToString() == typeof(Wall).ToString() || (n.GetType().ToString() == typeof(Protectee).ToString()))
                {
                    if (IntersectBounds(boundsBottom, n.sBounds))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public CorrectionVector2 getCorrectionVector(Sprite target)
        {
            CorrectionVector2 ret = new CorrectionVector2();

            float x1 = Math.Abs(sBounds.Right - target.sBounds.Left);
            float x2 = Math.Abs(sBounds.Left - target.sBounds.Right);
            float y1 = Math.Abs(sBounds.Bottom - target.sBounds.Top);
            float y2 = Math.Abs(sBounds.Top - target.sBounds.Bottom);

            // calculate displacement along X-axis
            if (x1 < x2)
            {
                ret.X = x1;
                ret.DirectionX = DirectionX.Left;
            }
            else if (x1 > x2)
            {
                ret.X = x2;
                ret.DirectionX = DirectionX.Right;
            }

            // calculate displacement along Y-axis
            if (y1 < y2)
            {
                ret.Y = y1;
                ret.DirectionY = DirectionY.Up;
            }
            else if (y1 > y2)
            {
                ret.Y = y2;
                ret.DirectionY = DirectionY.Down;
            }
            return ret;
        }

        public void correctCollision(CorrectionVector2 correction, bool correctHorizontal)
        {
            if (correctHorizontal) // horizontal
            {
                if (correction.DirectionX == DirectionX.Left)
                    Position.X += correction.X * (-1);
                if (correction.DirectionX == DirectionX.Right)
                    Position.X += correction.X * 1;
                if (correction.DirectionX == DirectionX.None)
                    Position.X += correction.X * 0;
            }
            else // vertical
            {
                if (correction.DirectionY == DirectionY.Down)
                    Position.Y += correction.Y * 1;
                if (correction.DirectionY == DirectionY.Up)
                    Position.Y += correction.Y * (-1);
                if (correction.DirectionY == DirectionY.None)
                    Position.Y += correction.Y * 0;


            }
        }

        private CorrectionVector2 getSmallestCorrectionX(DirectionX directionX, LinkedList<CorrectionVector2> corrections)
        {
            CorrectionVector2 smallest = new CorrectionVector2();
            smallest.X = int.MaxValue;

            foreach (CorrectionVector2 correction in corrections)
            {
                if (correction.DirectionX == directionX && correction.X < smallest.X)
                    smallest = correction;
            }

            return smallest;
        }

        private CorrectionVector2 getSmallestCorrectionY(DirectionY directionY, LinkedList<CorrectionVector2> corrections)
        {
            CorrectionVector2 smallest = new CorrectionVector2();
            smallest.Y = int.MaxValue;

            foreach (CorrectionVector2 correction in corrections)
            {
                if (correction.DirectionY == directionY && correction.Y < smallest.Y)
                    smallest = correction;
            }

            return smallest;
        }



        // Creates a bounding box for attack (Space)
        public override void createHitBox(bool dir)
        {
            int half = (mSpriteTexture.Width / 2);

            if (sDirection != (-1)) //facing left
                rightHitBox = new Rectangle((int)(Position.X + half), (int)Position.Y, (half + 45), mSpriteTexture.Height);
        }




        //=======================================================
        //= Overwritten functions from virtual methods in sprite.
        //=======================================================

        public override bool takeDamage(float baseDamage)
        {
            // damageAmount is the damage a player does 
            // AFTER the modifier is applied.
            float totalDamage = baseDamage;
            baseDamage*=DEFENSE/100;
            totalDamage -= baseDamage;
            health -= totalDamage;
            if (health <= 0)
            {
                return true;
            }
            return false;
        }



        // Calculating a critical hit.
        public override bool criticalChance(float critChance)
        {
            Random chance = new Random();
            int rand = chance.Next(101);
            if (critChance > rand)
                return true; //scored a crit
            else
                return false;
        }


        // Getting direction.
        // true = left; false = right
        public override bool getDirection()
        {
            if (sDirection == (-1))
                return true;
            else return false;
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

            // _DEBUG
            //theSpriteBatch.Draw(_DEBUG, drawPosition, rightHitBox,
            //    Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0);


            // drawing the directions
            if (sDirection == (-1))
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


            //base.Draw(theSpriteBatch);
        }
    }
}
