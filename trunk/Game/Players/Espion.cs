
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace guiCreator
{
    public class Espion : Sprite
    {
        // The graphic to use to draw the sprite
        const string ASSETNAME = "Espion/Stand0";

        // Status bar textures
        Texture2D status_hp;
        Texture2D status_sp;
        Texture2D status_hpmp_outline;
        Texture2D status_e; //status above health
        Texture2D wing;
        //Texture2D _DEBUG;   //DEBUG BOUNDING BOXES


        // General variables for movement of the sprite
        Vector2 velocity;
        int sDirection = -1;
        const float ACCELERATION = 0.5f;
        const float DASH_ACCELERATION = 1.0f;
        const float SPEED_MAX = 20.0f;
        const float DASH_MAX = SPEED_MAX * 1.5f;
        const int DASH_TIMEFRAME = 70;     // Time in miliseconds for a player to doubletap to dash
        int elapsedDash = 500;              // current dash timeframe
        bool dashing = false;
        bool counting = false;
        

        // Variables used for gravity and jumping (general physics)
        const float GRAVITY_ACCELERATION = 5.0f;
        const float GRAVITY_MAX = 20.0f;
        const float JUMP_ACCELERATION = -5.0f;
        const float JUMP_MAX = -25.0f;
        const int JUMP_FRAMES = 250;
        

        //= Base Stats, attacking
        //= Characters have a base attack that all basic attacks leech off of.
        //= Special attacks apply large percentage modifiers to this base attack to calculate damage.
        //======================================
        float attack = 30,      // amt of damage player does
            attackSpeed = 200,  // Max amount of miliseconds an attack is in animation
            critChance = 8,     // chance of crit (%)
            critMod = 0,        // adds amount of damage that a crit does (%)
            maxHealth = 70,    // max health available
            curHealth = 70,    // current health
            maxSpecial = 100,   // max special power available
            curSpecial = 100,   // current special
            defense = 9;       // damage reduced (%) from basic attacks
        int attackMod = 4;      // The real # (%) is divided by 10 in formula. attackMod is min/max.
        //int combo = 0;          // combo counter


        // Attacking variables for the sprite
        bool attacking = false;
        bool canAct = true;     //This is used during animations where the player can't move or attack.
        bool special_1 = false; //stealth arts: paralyzing strike
        bool special_2 = false; //stealth arts: gouging blade
        bool special_3 = false; //stealth arts: backstab
        bool facing_back = false;   //if we're facing the back of the enemy
        bool cloaked = false;
        float elapsedAttackAnimation = 200; //time during animation, MAKE SURE IT'S THE SAME AS ATTK SPEED

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

        // Constructor of Espion.
        public Espion(int startX, int startY, int screenPosX, int screenPosY) : base(startX, startY)
        {
            screenPos = new Vector2(screenPosX, screenPosY);
            PlayAnimation("Espion/Stand", 16, true);
        }

        public override void LoadContent(ContentManager theContentManager)
        {
            velocity = Vector2.Zero;
            base.LoadContent(theContentManager, ASSETNAME);
            status_hpmp_outline = theContentManager.Load<Texture2D>("UI/status_hpmp_outline");
            status_hp = theContentManager.Load<Texture2D>("UI/status_hp");
            status_sp = theContentManager.Load<Texture2D>("UI/status_sp");
            status_e = theContentManager.Load<Texture2D>("UI/status_e");
            wing = theContentManager.Load<Texture2D>("UI/wing");
            //_DEBUG = theContentManager.Load<Texture2D>("black");
        }

        public override LinkedList<Sprite> Update(GameTime theGameTime, LinkedList<Sprite> level, ContentManager theContentManager)
        {
            float elapsed = (float)theGameTime.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < 10; i++)
            {
                base.Update(theGameTime, level, theContentManager);

                handleInput(level, theContentManager);

                updateAnimation(theGameTime, theContentManager);

                level = updateAttack(theGameTime, level, theContentManager);

                applyPhysics(elapsed, level);
            }
            return level;
        }



        public void handleInput(LinkedList<Sprite> level, ContentManager theContentManager)
        {
            KeyboardState aCurrentKeyboardState = Keyboard.GetState();

            // Some dashing stuff
            if (velocity.X == 0 && dashing == true)
            {   //if we're not moving, we aren't dashing anymore.
                dashing = false;
                counting = false;
                elapsedDash = DASH_TIMEFRAME;
            }
            if (elapsedDash < DASH_TIMEFRAME && counting == true && dashing == false)  //dash is not over the timeframe
                elapsedDash++;



            if ((mPreviousKeyboardState.IsKeyDown(Keys.Left) == true) && (aCurrentKeyboardState.IsKeyUp(Keys.Left) == true) ||
                (mPreviousKeyboardState.IsKeyDown(Keys.Right) == true) && (aCurrentKeyboardState.IsKeyUp(Keys.Right) == true))
            {   // we have pressed the key and lifted it up.
                if (canAct == true && cloaked==false)
                {
                    if (elapsedDash >= DASH_TIMEFRAME)
                    { //dash is over the timeframe
                        elapsedDash = 0;    //start again 
                        counting = true;
                    }
                }
            }

            //moving left and right
            if (aCurrentKeyboardState.IsKeyDown(Keys.Left) == true && canAct == true)
            {
                if (elapsedDash < DASH_TIMEFRAME)
                {   // successfully comboed two arrow keys

                    if (dashing == false)
                    {
                        Effect dashCloud;
                        dashCloud = new Effect((int)(Position.X + 30), (int)Position.Y, effectName.DASH, sDirection);
                        dashCloud.LoadContent(theContentManager);
                        level.AddLast(dashCloud);
                    }

                    dashing = true;
                    elapsedDash = DASH_TIMEFRAME;
                    counting = false;
                    velocity.X -= DASH_ACCELERATION;
                } 
                else //not dashing
                    velocity.X -= ACCELERATION;
                sDirection = -1;
            }
            else if (aCurrentKeyboardState.IsKeyDown(Keys.Right) == true && canAct == true)
            {
                if (elapsedDash < DASH_TIMEFRAME)
                {   // successfully comboed two arrow keys

                    if (dashing == false)
                    {
                        Effect dashCloud;
                        dashCloud = new Effect((int)(Position.X - 80), (int)Position.Y, effectName.DASH, sDirection);
                        dashCloud.LoadContent(theContentManager);
                        level.AddLast(dashCloud);
                    }

                    dashing = true;
                    elapsedDash = DASH_TIMEFRAME;
                    counting = false;
                    velocity.X += DASH_ACCELERATION;
                }
                else //not dashing
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
            // speed checks
            if (dashing == true)
            {
                if ((velocity.X > DASH_MAX))
                {
                    velocity.X = DASH_MAX;
                }
                if ((velocity.X < -DASH_MAX))
                {
                    velocity.X = -DASH_MAX;
                }
            }
            else
            {
                if ((velocity.X > SPEED_MAX))
                {
                    velocity.X = SPEED_MAX;
                }
                if ((velocity.X < -SPEED_MAX))
                {
                    velocity.X = -SPEED_MAX;
                }
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
                attacking = true;


            // Stealth Arts keys
            if ((mPreviousKeyboardState.IsKeyDown(Keys.Z) == true) && (aCurrentKeyboardState.IsKeyUp(Keys.Z) == true))
            {
                // Stealth Arts: Paralyzing Strike
                if (cloaked == true && facing_back==true)
                {
                    attacking = true;
                    special_1 = true;
                }
            }

            if ((mPreviousKeyboardState.IsKeyDown(Keys.X) == true) && (aCurrentKeyboardState.IsKeyUp(Keys.X) == true))
            {
                // Stealth Arts: Gouging Blade
                if (cloaked == true && facing_back == true)
                {
                    attacking = true;
                    special_2 = true;
                }
            }

            if ((mPreviousKeyboardState.IsKeyDown(Keys.C) == true) && (aCurrentKeyboardState.IsKeyUp(Keys.C) == true))
            {
                // Stealth Arts: Backstab
                if (cloaked == true && facing_back == true)
                {
                    attacking = true;
                    special_3 = true;
                }
            }


            //=============================
            //= CLOAKING - LEFT ALT KEY ===
            //=============================
            if ((mPreviousKeyboardState.IsKeyDown(Keys.LeftAlt) == true) && (aCurrentKeyboardState.IsKeyUp(Keys.LeftAlt) == true) && canAct == true)
            {
                if (cloaked == true)
                {   // Uncloak, but make sure we're standing
                    if (mCurrentState == State.Walking)
                    {
                        //PlayAnimation("Espion/uncloak", 15, false);
                        PlayAnimation("Espion/stand", 16, true);
                        facing_back = false;
                        cloaked = false;
                    }
                }
                else
                {   // Prepare for cloak, make sure we're standing
                    if (mCurrentState == State.Walking)
                    {
                        //PlayAnimation("Espion/cloak", 15, false);
                        PlayAnimation("Espion/cloakstand", 12, true);
                        cloaked = true;
                    }
                }
                
            }



            // Healing walls
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



            if ( ((velocity.X < 0) || (velocity.X > 0)) && canAct==true)
            {
                if (elapsedAttackAnimation >= attackSpeed)
                {
                    if (cloaked)
                        PlayAnimation("Espion/run", 15, true);
                    else
                        PlayAnimation("Espion/run", 15, true);
                }
            }
            else
            {
                if (elapsedAttackAnimation >= attackSpeed)
                {
                    if (cloaked && facing_back==false)
                        PlayAnimation("Espion/cloakstand", 13, true);
                    else if (cloaked && facing_back==true)
                        PlayAnimation("Espion/cloakready", 13, true);
                    else
                        PlayAnimation("Espion/stand", 16, true);
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
            if (canAct == true)
            {
                if (attacking)
                {
                    if (cloaked == false)
                    {
                        handleAttack(level, theContentManager);
                    }
                    else
                    {
                        // PLAYER IS CLOAKED. Stealth arts activate!
                        if (special_1 == true)
                        {
                            //All conditions met. Activate Paralyzing Strike
                            cloaked = false;
                            //PlayAnimation("Espion/cloakAttack", 8, false);
                        }
                        if (special_2 == true)
                        {
                            //All conditions met. Activate Gouging Blade
                            cloaked = false;
                            //PlayAnimation("Espion/cloakAttack", 8, false);
                        }
                        if (special_3 == true)
                        {
                            //All conditions met. Activate Backstab
                            cloaked = false;
                            //PlayAnimation("Espion/cloakAttack", 8, false);
                        }

                    }
                }
            }
            if (elapsedAttackAnimation < attackSpeed)
            {
                elapsedAttackAnimation++;
                canAct = false;
            }
            else
            {
                canAct = true;
                attacking = false;
            }

            return level;
        }


        public void handleAttack(LinkedList<Sprite> level, ContentManager theContentManager)
        {
            elapsedAttackAnimation = 0;
            PlayAnimation("Espion/attack", 7, false);
            createAttackBounds();
            createBounds();

            //Plays slicing effect
            //Depending on the direction, the effect's position is modified...
            if (sDirection == -1)
            {
                Effect e_Slice;
                e_Slice = new Effect((int)(Position.X) - 35, (int)Position.Y + 30, effectName.E_SLICE, -sDirection);
                e_Slice.LoadContent(theContentManager);
                level.AddLast(e_Slice);
            }
            else
            {
                Effect e_Slice;
                e_Slice = new Effect((int)(Position.X) - 10, (int)Position.Y + 30, effectName.E_SLICE, -sDirection);
                e_Slice.LoadContent(theContentManager);
                level.AddLast(e_Slice);
            }

            foreach (Sprite n in level)
            {
                //Make sure they're demons
                if (n.GetType().ToString() == typeof(LesserDemon).ToString())
                {
                    //if facing left && monster is facing right
                    if (sDirection == -1 && n.getDirection() == false)
                    {
                        n.createHitBox(n.getDirection());
                        if (IntersectBounds(attackBounds, n.rightHitBox))
                        {
                            //Connecting hit
                            if (n.takeDamage(damageCalculation(attack)))
                            {
                                //delete enemy
                                level.Remove(n);
                            }
                            break;
                        }
                    }
                    else
                    {
                        //facing right
                        if (IntersectBounds(attackBounds, n.sBounds))
                        {
                            if (n.takeDamage(damageCalculation(attack)))
                            {
                                //delete enemy
                                level.Remove(n);
                            }
                            break;
                        }
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


        // Creates a bounding box for attack (Space)
        public void createAttackBounds()
        {
            int half = (mSpriteTexture.Width / 2);

            if (sDirection == (-1)) //facing left
                attackBounds = new Rectangle((int)(Position.X - 15), (int)Position.Y, (half - 5), (mSpriteTexture.Height - 20));
            else
                attackBounds = new Rectangle((int)(Position.X + half), (int)Position.Y, (half + 65), (mSpriteTexture.Height - 20));
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
            //First check if we're colliding with anything
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

                    //We are colliding with demons.
                    if ((n.GetType().ToString() == typeof(LesserDemon).ToString()))
                    {
                        if (IntersectPixels(sBounds, textureData, n.sBounds, n.textureData))
                        {
                            //If we're cloaked, we check if an enemy's back is to us.
                            if (cloaked == true)
                            {
                                //if we're facing the same directions
                                if (sDirection == -1 && n.getDirection() == true ||
                                    sDirection != -1 && n.getDirection() == false)
                                {
                                    //make LoS Bounds in the back of the enemy & front of player
                                    n.createBackBounds(n.getDirection());
                                    createFrontBounds(getDirection());

                                    //if we're intersecting with the back bounds 
                                    if (IntersectPixels(frontBounds, textureData, n.backBounds, n.textureData))
                                        facing_back = true;
                                    else
                                        facing_back = false;
                                }
                                else
                                    facing_back = false;
                            }
                        }
                    }
                    else
                    {
                        facing_back = false;
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
            else
            {
                facing_back = false;
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
                if ((n.GetType().ToString() == typeof(Block).ToString()))
                {
                    if (IntersectPixels(sBounds, textureData, n.sBounds, n.textureData))
                        return true;
                }

                //Are we colliding with demons?
                if ((n.GetType().ToString() == typeof(LesserDemon).ToString()))
                {
                    if (IntersectPixels(sBounds, textureData, n.sBounds, n.textureData))
                        return true;
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



        //=======================================================
        //= Overwritten functions from virtual methods in sprite.
        //=======================================================


        // Calculating a critical hit.
        public override bool criticalChance(float crittingChance)
        {
            Random chance = new Random();
            int rand = chance.Next(101);
            if (crittingChance > rand)
                return true; //scored a crit
            else
                return false;
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

        public override bool takeDamage(float baseDamage)
        {
            // damageAmount is the damage a player does 
            // AFTER the modifier is applied.
            float totalDamage = baseDamage;
            baseDamage *= defense / 100;
            totalDamage -= baseDamage;
            curHealth -= totalDamage;
            if (curHealth <= 0)
            {
                return true;
            }
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

            float width = 40;
            float height = 10;

            // wing behind health bar
             Vector2 wing_status = new Vector2(-10, 20);
            theSpriteBatch.Draw(wing, wing_status, null,
                Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0); 

            // HP/MP Bars behind outline
            Vector2 hp_bar = new Vector2(41, 53);
            height = 22;
            width = 127 * (curHealth / maxHealth);
            theSpriteBatch.Draw(status_hp, hp_bar,
               new Rectangle(0, 0, (int)width, (int)height),
               Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0);

            Vector2 sp_bar = new Vector2(61, 87);
            width = 127 * (curSpecial / maxSpecial);
            theSpriteBatch.Draw(status_sp, sp_bar,
               new Rectangle(0, 0, (int)width, (int)height),
               Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0);

            // draw the health bars' outlines
            Vector2 hpsp_outline = new Vector2(30, 47);
            theSpriteBatch.Draw(status_hpmp_outline, hpsp_outline, null,
                Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0);

            // draw the angel's name
            Vector2 angel_status = new Vector2(0, 0);
            theSpriteBatch.Draw(status_e, angel_status, null,
                Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0);

            // _DEBUG
            //theSpriteBatch.Draw(_DEBUG, screenPos, attackBounds,
            //    Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0);

            
            // drawing the directions
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
