using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace guiCreator
{
    public class Grenadier : Sprite
    {
        // The graphic to use to draw the sprite
        const string ASSETNAME = "Grenadier/Stand0";

        // not reloading now
        bool reloading = false;

        // Text to display number of bullets
        Text hudBullets;
        Texture2D reloadBackground;
        Texture2D reloadFill;

        // General variables for movement of the sprite
        Vector2 velocity;
        int sDirection = -1;
        const float ACCELERATION = 0.5f;
        const float SPEED_MAX = 12.0f;
        
        // Variables used for gravity and jumping (general physics)
        const float GRAVITY_ACCELERATION = 0.5f;
        const float GRAVITY_MAX = 25.0f;
        const float JUMP_ACCELERATION = -0.5f;
        const float JUMP_MAX = -25.0f;
        const int JUMP_FRAMES = 400;

        // Attacking variables for the sprite
        bool attacking = false;        
        int numBullets = 30;
        //reloadtime = numBullets*BASE_RELOAD
        int reloadTime;
        int elapsedAttackAnimation = 200;
        //miliseconds per bullet to reload
        const int BASE_RELOAD = 125;
        const float ATTACK_DAMAGE = 5;
        const int BULLETS_MAX = 30;
        const int RELOAD_LENGTH = BULLETS_MAX*BASE_RELOAD;
        const int ATTACK_ANIMATION_LENGTH = 200;

        // Animation variables
        string animation;
        bool animationIsLooping;
        const float FRAME_TIME = 0.7f;
        float time;
        int frameIndex;
        int frameCount;
       

        int numJumpFrames;

        enum State
        {
            Walking,
            Jumping,
            Attacking
        }
        State mCurrentState = State.Walking;

        KeyboardState mPreviousKeyboardState;

        // Start collision detection/response variables

        LinkedList<Sprite> collidingObjects = new LinkedList<Sprite>();

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

        public Grenadier(int startX, int startY, int screenPosX, int screenPosY) : base(startX, startY)
        {
            screenPos = new Vector2(screenPosX, screenPosY);
            hudBullets = new Text(10, 10);
        }

        public override void LoadContent(ContentManager theContentManager)
        {
            velocity = Vector2.Zero;
            base.LoadContent(theContentManager, ASSETNAME);
            PlayAnimation("Grenadier/Stand", 16, true);
            hudBullets.LoadContent(theContentManager, "Font");
            reloadBackground = theContentManager.Load<Texture2D>("UI/ReloadBackground");
            reloadFill = theContentManager.Load<Texture2D>("UI/ReloadFill");
        }

        public override LinkedList<Sprite> Update(GameTime theGameTime, LinkedList<Sprite> level, ContentManager theContentManager)
        {
            float elapsed = (float)theGameTime.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < 10; i++)
            {
                base.Update(theGameTime, level, theContentManager);

                handleInput(level);

                updateAnimation(theGameTime, theContentManager);

                level = updateAttack(theGameTime, level, theContentManager);

                applyPhysics(elapsed, level);
            }
            return level;
        }



        public void handleInput(LinkedList<Sprite> level)
        {
            KeyboardState aCurrentKeyboardState = Keyboard.GetState();

            //moving left and right
            if (aCurrentKeyboardState.IsKeyDown(Keys.Left) == true)
            {
                velocity.X -= ACCELERATION;
                sDirection = -1;
            }
            else if (aCurrentKeyboardState.IsKeyDown(Keys.Right) == true)
            {
                velocity.X += ACCELERATION;
                sDirection = 1;
            }
            else
            {
                if (velocity.X > 0)
                {
                    velocity.X -= ACCELERATION;
                }
                if (velocity.X < 0)
                {
                    velocity.X += ACCELERATION;
                }
            }
            if ((velocity.X > SPEED_MAX))
            {
                velocity.X = SPEED_MAX;
            }
            if ((velocity.X < -SPEED_MAX))
            {
                velocity.X = -SPEED_MAX;
            }

            //jumping
            if (aCurrentKeyboardState.IsKeyDown(Keys.Up) == true)
            {
                if (isOnGround())
                {
                    mCurrentState = State.Jumping;
                    numJumpFrames = 0;
                }
            }

            if ((mPreviousKeyboardState.IsKeyDown(Keys.Space) == true)&&(aCurrentKeyboardState.IsKeyUp(Keys.Space)==true))
            {
                attacking = true;
            }

            //reloading
            if ((mPreviousKeyboardState.IsKeyDown(Keys.LeftAlt) == true) && (aCurrentKeyboardState.IsKeyUp(Keys.LeftAlt) == true))
            {
                if (reloading==false)
                {
                    reloadTime = (BULLETS_MAX - numBullets) * BASE_RELOAD;
                    reloading = true;
                }
            }

            if ((mPreviousKeyboardState.IsKeyDown(Keys.LeftControl) == true) && (aCurrentKeyboardState.IsKeyUp(Keys.LeftControl) == true))
            {
                foreach (Sprite n in collidingObjects)
                {
                    if (n.GetType().ToString() == typeof(Wall).ToString())
                    {
                        if(IntersectPixels(sBounds, textureData, n.sBounds, n.textureData))
                        {
                            n.takeDamage(-1);
                        }
                    }
                }
            }

            if ((velocity.X < 0) || (velocity.X > 0))
            {
                if (elapsedAttackAnimation < ATTACK_ANIMATION_LENGTH)
                {
                    PlayAnimation("Grenadier/runshoot", 14, true);
                }
                else
                {
                    PlayAnimation("Grenadier/run", 15, true);
                }
            }
            else
            {
                if (elapsedAttackAnimation < ATTACK_ANIMATION_LENGTH)
                {
                    PlayAnimation("Grenadier/shoot", 1, false);
                }
                else
                {
                    PlayAnimation("Grenadier/stand", 16, true);
                }
            }

            mPreviousKeyboardState = aCurrentKeyboardState;
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
            if (anim==animation)
                return;

            // Start the new animation.
            animation = anim;
            frameCount = fCount;
            animationIsLooping = loop;
            this.frameIndex = 0;
            this.time = 0.0f;
        }

        public LinkedList<Sprite> updateAttack(GameTime theGameTime, LinkedList<Sprite> level, ContentManager theContentManager)
        {
            //can't attack while loading
            if (reloading == false)
            {
                if (numBullets > 0)
                {
                    if (attacking)
                    {
                        elapsedAttackAnimation = 0;
                        Bullet a;
                        if ((velocity.X > 0) || (velocity.X < 0))
                            a = new Bullet((int)Position.X, (int)(Position.Y + 63), sDirection);
                        else
                            a = new Bullet((int)Position.X, (int)(Position.Y + 50), sDirection);
                        a.LoadContent(theContentManager);
                        level.AddLast(a);

                        numBullets--;
                    }
                }
                //else no bullets, so we auto-reload:
                else
                {
                    reloadTime = (BULLETS_MAX - numBullets) * BASE_RELOAD;
                    reloading = true;
                }
            }
            //we're still reloading
            else if (reloading == true && reloadTime!=0)
            {
                reloadTime--;
            }
            //we're done!
            else if (reloading == true && reloadTime >= 0)
            {
                numBullets = 30;
                reloading = false;
            }


            if (elapsedAttackAnimation < ATTACK_ANIMATION_LENGTH)
            {
                elapsedAttackAnimation++;
                //mSpriteTexture = theContentManager.Load<Texture2D>("Grenadier");
            }
            else
            {
                //mSpriteTexture = theContentManager.Load<Texture2D>("Grenadier");
            }
            attacking = false;

            return level;
        }






        public void applyPhysics(float elapsed, LinkedList<Sprite> level)
        {
            getCollisions(level);

            if ((mCurrentState == State.Jumping) && (numJumpFrames < JUMP_FRAMES))
            {
                if (numJumpFrames >= 1)
                {
                    if (isOnGround())
                    {
                        numJumpFrames = JUMP_FRAMES;
                    }
                }
                velocity.Y += JUMP_ACCELERATION;
                if (velocity.Y < JUMP_MAX)
                    velocity.Y = JUMP_MAX;
                numJumpFrames++;
            }
            else
            {
                mCurrentState = State.Walking;
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
            }

            Position += velocity * elapsed;

            handleCollisions(level);
        }

        // taken from http://go.colorize.net/xna/2d_collision_response_xna/index.html
        public void handleCollisions(LinkedList<Sprite> level)
        {
            if (isColliding())
            {
                LinkedList<CorrectionVector2> corrections = new LinkedList<CorrectionVector2>();
                foreach (Sprite n in collidingObjects)
                {
                    if ((n.GetType().ToString() == typeof(Block).ToString()))
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

        public override void createBounds()
        {
            sBounds = new Rectangle((int)Position.X, (int)Position.Y, mSpriteTexture.Width, mSpriteTexture.Height);

            boundsTop = new Rectangle((int)(Position.X + 10), (int)(Position.Y - 5), (mSpriteTexture.Width - 20), 5);
            boundsBottom = new Rectangle((int)(Position.X + 10), (int)(Position.Y + mSpriteTexture.Height), (mSpriteTexture.Width - 20), 5);
            boundsLeft = new Rectangle((int)(Position.X - 5), (int)Position.Y, 5, mSpriteTexture.Height);
            boundsRight = new Rectangle((int)(Position.X + mSpriteTexture.Height), (int)Position.Y, 5, mSpriteTexture.Height);
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
                if ((n.GetType().ToString() == typeof(Block).ToString()))
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
            //ret.Y--;
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




        public override void Draw(SpriteBatch theSpriteBatch)
        {
            string sBullets = "Ammo: " + numBullets + "/" +BULLETS_MAX;
            hudBullets.DrawText(theSpriteBatch, sBullets);

            //draw the ammo reload bar if we're reloading
            if (reloading==true)
            {
                Vector2 pos = new Vector2(10, 35);
                float width = 40;
                float height = 10;
                theSpriteBatch.Draw(reloadBackground, pos,
                    new Rectangle(0, 0, (int)width, (int)height),
                    Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0);

                pos += new Vector2(2, 2);
                width = 36 * ((float)reloadTime / (float)RELOAD_LENGTH);
                height = 6;
                theSpriteBatch.Draw(reloadFill, pos,
                    new Rectangle(0, 0,  (int)width, (int)height),
                    Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0);
            }

            if (sDirection == (-1))
            {
                theSpriteBatch.Draw(mSpriteTexture, screenPos,
                    new Rectangle(0, 0, mSpriteTexture.Width, mSpriteTexture.Height),
                    Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0);
            }
            else
            {
                theSpriteBatch.Draw(mSpriteTexture, screenPos,
                new Rectangle(0, 0, mSpriteTexture.Width, mSpriteTexture.Height),
                Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.FlipHorizontally, 0);
            }
        }


    }
}
