using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics; 
namespace guiCreator
{
    public class Effect : Sprite
    {
        const string ASSETNAME = "effects/default";

        int sDirection, startingX, startingY;

        // Animation variables
        string animation;
        bool animationIsLooping;
        const float FRAME_TIME = 0.7f;
        float time;
        int frameIndex;
        int frameCount;

        //constructor, input string to say the type of effect
        public Effect(int startX, int startY, effectName name, int dir) : base(startX, startY) {
            startingX = startX;
            startingY = startY;
            sDirection = dir;

            if (name == effectName.DASH)
                PlayAnimation("effects/dash/forward", 9, false);
            if (name == effectName.E_SLICE)
                PlayAnimation("effects/melee/slice", 5, false);
            if (name == effectName.HIT_A)
                PlayAnimation("effects/melee/hitA", 18, false);
        }

        public override void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, ASSETNAME);
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


        public override LinkedList<Sprite> Update(GameTime theGameTime, LinkedList<Sprite> level, ContentManager theContentManager)
        {
            float elapsed = (float)theGameTime.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < 10; i++)
            {
                base.Update(theGameTime, level, theContentManager);

                updateAnimation(theGameTime, theContentManager);
            }
            //end of animation? Delete it!
            if (frameIndex == frameCount-1)
                level.Remove(this); 
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
