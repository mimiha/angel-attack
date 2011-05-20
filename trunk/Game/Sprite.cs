using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace guiCreator
{
    public class Sprite
    {
        //The current position of the Sprite
        public Vector2 Position = new Vector2(0, 0);
        public Vector2 drawPosition;
        public Vector2 screenPos;

        //The texture object used when drawing the sprite
        public Texture2D mSpriteTexture;

        //The texture data of the Sprite
        public Color[] textureData;

        //The asset name for the Sprite's Texture
        public string AssetName;

        //The Size of the Sprite (with scale applied)
        public Rectangle Size;

        //The boundaries of the Sprite
        public Rectangle sBounds;
        public Rectangle boundsTop;
        public Rectangle boundsBottom;
        public Rectangle boundsLeft;
        public Rectangle boundsRight;
        public Rectangle backBounds;
        public Rectangle frontBounds;

        //The amount to increase/decrease the size of the original sprite. 
        public float mScale = 1.0f;

        public const float CRITICAL_DAMAGE = 1.5f;  //the x amount of how much a critical does.


        public enum effectName
        {
            DEFAULT,
            DASH,
            E_SLICE,
            BACKDASH,
            CRITICAL,
        }

        public Sprite(int startX, int startY)
        {
            Position = new Vector2(startX, startY);
            drawPosition = Position;
        }

        public virtual void LoadContent(ContentManager theContentManager) { }




        //Load the texture for the sprite using the Content Pipeline
        public void LoadContent(ContentManager theContentManager, string theAssetName)
        {
            mSpriteTexture = theContentManager.Load<Texture2D>(theAssetName);
            AssetName = theAssetName;
            textureData = new Color[mSpriteTexture.Width * mSpriteTexture.Height];
            mSpriteTexture.GetData(textureData);
            createBounds();
            Size = new Rectangle(0, 0, (int)(mSpriteTexture.Width * Scale), (int)(mSpriteTexture.Height * Scale));
        }




        //When the scale is modified throught he property, the Size of the 
        //sprite is recalculated with the new scale applied.
        public float Scale
        {
            get { return mScale; }
            set
            {
                mScale = value;
                //Recalculate the Size of the Sprite with the new scale
                Size = new Rectangle(0, 0, (int)(mSpriteTexture.Width * Scale), (int)(mSpriteTexture.Height * Scale));
            }
        }



        public virtual LinkedList<Sprite> Update(GameTime theGameTime, LinkedList<Sprite> level, ContentManager theContentManager)
        {
            createBounds();
            bool foundPlayer = false;
            Vector2 screenPos = new Vector2();
            Vector2 playerPos = new Vector2();
            foreach (Sprite n in level)
            {
                if (n.GetType().ToString() == typeof(Grenadier).ToString() ||
                    n.GetType().ToString() == typeof(Espion).ToString() )
                {
                    foundPlayer = true;
                    screenPos = n.screenPos;
                    playerPos = n.Position;
                }
            }
            if (foundPlayer && (this.GetType().ToString() != typeof(Grenadier).ToString()) ||
                foundPlayer && (this.GetType().ToString() != typeof(Espion).ToString()) )
            {
                drawPosition = Position + screenPos - playerPos;
            }
            else
            {
                drawPosition = Position;
            }
            return level; 
        }



        //Draw the sprite to the screen
        public virtual void Draw(SpriteBatch theSpriteBatch)
        {
            theSpriteBatch.Draw(mSpriteTexture, drawPosition,
                new Rectangle(0, 0, mSpriteTexture.Width, mSpriteTexture.Height),
                Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }

        public void Draw(SpriteBatch theSpriteBatch, Vector2 camera)
        {
            Vector2 pos = Position - camera;
            theSpriteBatch.Draw(mSpriteTexture, pos,
                new Rectangle(0, 0, mSpriteTexture.Width, mSpriteTexture.Height),
                Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }




        public static bool IntersectBounds(Rectangle rectangleA, Rectangle rectangleB)
        {
            if (rectangleA.Intersects(rectangleB))
            {
                return true;
            }
            return false;
        }



        public static bool IntersectPixels(Rectangle rectangleA, Color[] dataA,
                                   Rectangle rectangleB, Color[] dataB)
        {
            // Find the bounds of the rectangle intersection
            int top = Math.Max(rectangleA.Top, rectangleB.Top);
            int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
            int left = Math.Max(rectangleA.Left, rectangleB.Left);
            int right = Math.Min(rectangleA.Right, rectangleB.Right);

            // Check every point within the intersection bounds
            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    // Get the color of both pixels at this point
                    Color colorA = dataA[(x - rectangleA.Left) +
                                         (y - rectangleA.Top) * rectangleA.Width];
                    Color colorB = dataB[(x - rectangleB.Left) +
                                         (y - rectangleB.Top) * rectangleB.Width];

                    // If both pixels are not completely transparent,
                    if (colorA.A != 0 && colorB.A != 0)
                    {
                        // then an intersection has been found
                        return true;
                    }
                }
            }

            // No intersection found
            return false;
        }    
            


        public virtual void createBounds()
        {
            sBounds = new Rectangle((int)Position.X, (int)Position.Y, mSpriteTexture.Width, mSpriteTexture.Height);

            boundsTop = new Rectangle((int)(Position.X), (int)(Position.Y - 5), mSpriteTexture.Width, 5);
            boundsBottom = new Rectangle((int)(Position.X), (int)(Position.Y + mSpriteTexture.Height), mSpriteTexture.Width, 5);
            boundsLeft = new Rectangle((int)(Position.X - 5), (int)Position.Y, 5, mSpriteTexture.Height);
            boundsRight = new Rectangle((int)(Position.X + mSpriteTexture.Height), (int)Position.Y, 5, mSpriteTexture.Height);
        }

        //Parameter: getDirection()
        public virtual void createBackBounds(bool dir)
        {
            int half = (mSpriteTexture.Width / 2);

            if (dir == true)    //facing left
                backBounds = new Rectangle((int)(Position.X+half), (int)Position.Y, half, mSpriteTexture.Height);
            else
                backBounds = new Rectangle((int)(Position.X), (int)Position.Y, half, mSpriteTexture.Height);
        }

        //Parameter: getDirection()
        public virtual void createFrontBounds(bool dir)
        {
            int half = (mSpriteTexture.Width / 2);

            if (dir == true)    //facing left
                frontBounds = new Rectangle((int)(Position.X), (int)Position.Y, half, mSpriteTexture.Height);
            else
                frontBounds = new Rectangle((int)(Position.X + half), (int)Position.Y, half, mSpriteTexture.Height);
        }

        //======================================
        // Virtual methods common to other objects.
        // Must be overridden in classes of those who use them.
        //======================================

        // calculating a critical hit
        public virtual bool criticalChance(float critChance) { return false; }

        // calculating base damage with no defense reduction
        public virtual float damageCalculation(float pureAttk) { return pureAttk; }

        // damage-versus-defense calculation
        public virtual bool takeDamage(float damageAmount) { return false; }

        // getting the direction of the sprite;
        // true = left, false = right
        public virtual bool getDirection() { return false; } 

    }
}
