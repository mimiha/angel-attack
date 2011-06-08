using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
/*------------------------------EMMANUEL'S EDITS--------------------------------
 * 01. Added "SetX" & "SetY" modifiers
 */
namespace guiCreator
{
    public class Text : Sprite
    {
        public SpriteFont mSpriteFont;

        public Text(int startX, int startY) : base(startX, startY) { }

        public void LoadContent(ContentManager theContentManager, string theAssetName)
        {
            mSpriteFont = theContentManager.Load<SpriteFont>(theAssetName);
            AssetName = theAssetName;
        }

        //Draw the sprite to the screen
        public void DrawText(SpriteBatch theSpriteBatch, string text)
        {
            theSpriteBatch.DrawString(mSpriteFont, text, Position, Color.Black,
        0, new Vector2(0,0), 1.0f, SpriteEffects.None, 0.5f);
        }

        // Accessor methods for specifically for Sprite.Position
        public float XVAL
        {
            get { return Position.X; }
            set { Position.X = value; }
        }
        public float YVAL
        {
            get { return Position.Y; }
            set { Position.Y = value; }
        }
    }
}
