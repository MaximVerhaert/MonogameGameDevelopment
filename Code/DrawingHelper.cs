using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Code
{
    public static class DrawingHelper
    {
        public static void DrawRectangleBorder(SpriteBatch spriteBatch, Rectangle rectangle, Color color, int borderThickness, GraphicsDevice graphicsDevice)
        {
            Texture2D pixel = CreateSinglePixelTexture(graphicsDevice, color);

            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, borderThickness), color);
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - borderThickness, rectangle.Width, borderThickness), color);
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, borderThickness, rectangle.Height), color);
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X + rectangle.Width - borderThickness, rectangle.Y, borderThickness, rectangle.Height), color);
        }

        private static Texture2D CreateSinglePixelTexture(GraphicsDevice graphicsDevice, Color color)
        {
            Texture2D texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { color });
            return texture;
        }
    }

}
