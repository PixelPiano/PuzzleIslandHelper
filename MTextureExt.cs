using Celeste.Mod.Core;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper
{
    /// <summary>
    /// Adds additional outline drawing methods to the <see cref="MTexture"/> class. <list type="bullet">
    /// <item>"Lazy" methods draw the image in each corner.</item>
    /// <item> All other methods draw the image incrementally from the center to the corners.</item>
    /// </list>
    /// </summary>
    public static class MTextureExt
    {
        public static void DrawOutline(this MTexture tex, Vector2 position, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = -tex.DrawOffset / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, 0f, origin, scaleFix, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, Color.White, 0f, origin, scaleFix, SpriteEffects.None, 0f);
        }
        public static void DrawOutline(this MTexture tex, Vector2 position, Vector2 origin, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin2 = (origin - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, 0f, origin2, scaleFix, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, Color.White, 0f, origin2, scaleFix, SpriteEffects.None, 0f);
        }
        public static void DrawOutline(this MTexture tex, Vector2 position, Vector2 origin, Color color, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin2 = (origin - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, 0f, origin2, scaleFix, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin2, scaleFix, SpriteEffects.None, 0f);
        }
        public static void DrawOutline(this MTexture tex, Vector2 position, Vector2 origin, Color color, float scale, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin2 = (origin - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, 0f, origin2, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin2, scale, SpriteEffects.None, 0f);
        }
        public static void DrawOutline(this MTexture tex, Vector2 position, Vector2 origin, Color color, float scale, float rotation, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin2 = (origin - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, rotation, origin2, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin2, scale, SpriteEffects.None, 0f);
        }
        public static void DrawOutline(this MTexture tex, Vector2 position, Vector2 origin, Color color, float scale, float rotation, SpriteEffects flip, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin2 = (origin - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, rotation, origin2, scale, flip, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin2, scale, flip, 0f);
        }
        public static void DrawOutline(this MTexture tex, Vector2 position, Vector2 origin, Color color, Vector2 scale, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin2 = (origin - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, 0f, origin2, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin2, scale, SpriteEffects.None, 0f);
        }
        public static void DrawOutline(this MTexture tex, Vector2 position, Vector2 origin, Color color, Vector2 scale, float rotation, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin2 = (origin - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, rotation, origin2, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin2, scale, SpriteEffects.None, 0f);
        }
        public static void DrawOutline(this MTexture tex, Vector2 position, Vector2 origin, Color color, Vector2 scale, float rotation, SpriteEffects flip, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin2 = (origin - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, rotation, origin2, scale, flip, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin2, scale, flip, 0f);
        }
        public static void DrawOutlineCentered(this MTexture tex, Vector2 position, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (tex.Center - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, 0f, origin, scaleFix, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, Color.White, 0f, origin, scaleFix, SpriteEffects.None, 0f);
        }
        public static void DrawOutlineCentered(this MTexture tex, Vector2 position, Color color, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (tex.Center - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, 0f, origin, scaleFix, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin, scaleFix, SpriteEffects.None, 0f);
        }
        public static void DrawOutlineCentered(this MTexture tex, Vector2 position, Color color, float scale, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (tex.Center - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, 0f, origin, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }
        public static void DrawOutlineCentered(this MTexture tex, Vector2 position, Color color, float scale, float rotation, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (tex.Center - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, rotation, origin, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
        public static void DrawOutlineCentered(this MTexture tex, Vector2 position, Color color, float scale, float rotation, SpriteEffects flip, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (tex.Center - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, rotation, origin, scale, flip, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin, scale, flip, 0f);
        }
        public static void DrawOutlineCentered(this MTexture tex, Vector2 position, Color color, Vector2 scale, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (tex.Center - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, 0f, origin, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }
        public static void DrawOutlineCentered(this MTexture tex, Vector2 position, Color color, Vector2 scale, float rotation, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (tex.Center - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, rotation, origin, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
        public static void DrawOutlineCentered(this MTexture tex, Vector2 position, Color color, Vector2 scale, float rotation, SpriteEffects flip, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (tex.Center - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, rotation, origin, scale, flip, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin, scale, flip, 0f);
        }
        public static void DrawOutlineJustified(this MTexture tex, Vector2 position, Vector2 justify, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (new Vector2((float)tex.Width * justify.X, (float)tex.Height * justify.Y) - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, 0f, origin, scaleFix, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, Color.White, 0f, origin, scaleFix, SpriteEffects.None, 0f);
        }
        public static void DrawOutlineJustified(this MTexture tex, Vector2 position, Vector2 justify, Color color, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (new Vector2((float)tex.Width * justify.X, (float)tex.Height * justify.Y) - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, 0f, origin, scaleFix, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin, scaleFix, SpriteEffects.None, 0f);
        }
        public static void DrawOutlineJustified(this MTexture tex, Vector2 position, Vector2 justify, Color color, float scale, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (new Vector2((float)tex.Width * justify.X, (float)tex.Height * justify.Y) - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, 0f, origin, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }
        public static void DrawOutlineJustified(this MTexture tex, Vector2 position, Vector2 justify, Color color, float scale, float rotation, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (new Vector2((float)tex.Width * justify.X, (float)tex.Height * justify.Y) - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, rotation, origin, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
        public static void DrawOutlineJustified(this MTexture tex, Vector2 position, Vector2 justify, Color color, float scale, float rotation, SpriteEffects flip, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (new Vector2((float)tex.Width * justify.X, (float)tex.Height * justify.Y) - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, rotation, origin, scale, flip, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin, scale, flip, 0f);
        }
        public static void DrawOutlineJustified(this MTexture tex, Vector2 position, Vector2 justify, Color color, Vector2 scale, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (new Vector2((float)tex.Width * justify.X, (float)tex.Height * justify.Y) - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, 0f, origin, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }
        public static void DrawOutlineJustified(this MTexture tex, Vector2 position, Vector2 justify, Color color, Vector2 scale, float rotation, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (new Vector2((float)tex.Width * justify.X, (float)tex.Height * justify.Y) - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, rotation, origin, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
        public static void DrawOutlineJustified(this MTexture tex, Vector2 position, Vector2 justify, Color color, Vector2 scale, float rotation, SpriteEffects flip, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (new Vector2((float)tex.Width * justify.X, (float)tex.Height * justify.Y) - tex.DrawOffset) / scaleFix;
            for (float i = -outlineSize; i <= outlineSize; i++)
            {
                for (float j = -outlineSize; j <= outlineSize; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j), clipRect, outline, rotation, origin, scale, flip, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin, scale, flip, 0f);
        }
        public static void DrawLazyOutline(this MTexture tex, Vector2 position, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = -tex.DrawOffset / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, 0f, origin, scaleFix, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, Color.White, 0f, origin, scaleFix, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutline(this MTexture tex, Vector2 position, Vector2 origin, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin2 = (origin - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, 0f, origin2, scaleFix, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, Color.White, 0f, origin2, scaleFix, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutline(this MTexture tex, Vector2 position, Vector2 origin, Color color, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin2 = (origin - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, 0f, origin2, scaleFix, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin2, scaleFix, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutline(this MTexture tex, Vector2 position, Vector2 origin, Color color, float scale, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin2 = (origin - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, 0f, origin2, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin2, scale, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutline(this MTexture tex, Vector2 position, Vector2 origin, Color color, float scale, float rotation, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin2 = (origin - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, rotation, origin2, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin2, scale, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutline(this MTexture tex, Vector2 position, Vector2 origin, Color color, float scale, float rotation, SpriteEffects flip, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin2 = (origin - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, rotation, origin2, scale, flip, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin2, scale, flip, 0f);
        }
        public static void DrawLazyOutline(this MTexture tex, Vector2 position, Vector2 origin, Color color, Vector2 scale, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin2 = (origin - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, 0f, origin2, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin2, scale, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutline(this MTexture tex, Vector2 position, Vector2 origin, Color color, Vector2 scale, float rotation, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin2 = (origin - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, rotation, origin2, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin2, scale, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutline(this MTexture tex, Vector2 position, Vector2 origin, Color color, Vector2 scale, float rotation, SpriteEffects flip, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin2 = (origin - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, rotation, origin2, scale, flip, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin2, scale, flip, 0f);
        }
        public static void DrawLazyOutlineCentered(this MTexture tex, Vector2 position, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (tex.Center - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, 0f, origin, scaleFix, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, Color.White, 0f, origin, scaleFix, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutlineCentered(this MTexture tex, Vector2 position, Color color, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (tex.Center - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, 0f, origin, scaleFix, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin, scaleFix, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutlineCentered(this MTexture tex, Vector2 position, Color color, float scale, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (tex.Center - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, 0f, origin, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutlineCentered(this MTexture tex, Vector2 position, Color color, float scale, float rotation, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (tex.Center - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, rotation, origin, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutlineCentered(this MTexture tex, Vector2 position, Color color, float scale, float rotation, SpriteEffects flip, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (tex.Center - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, rotation, origin, scale, flip, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin, scale, flip, 0f);
        }
        public static void DrawLazyOutlineCentered(this MTexture tex, Vector2 position, Color color, Vector2 scale, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (tex.Center - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, 0f, origin, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutlineCentered(this MTexture tex, Vector2 position, Color color, Vector2 scale, float rotation, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (tex.Center - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, rotation, origin, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutlineCentered(this MTexture tex, Vector2 position, Color color, Vector2 scale, float rotation, SpriteEffects flip, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (tex.Center - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, rotation, origin, scale, flip, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin, scale, flip, 0f);
        }
        public static void DrawLazyOutlineJustified(this MTexture tex, Vector2 position, Vector2 justify, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (new Vector2((float)tex.Width * justify.X, (float)tex.Height * justify.Y) - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, 0f, origin, scaleFix, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, Color.White, 0f, origin, scaleFix, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutlineJustified(this MTexture tex, Vector2 position, Vector2 justify, Color color, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (new Vector2((float)tex.Width * justify.X, (float)tex.Height * justify.Y) - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, 0f, origin, scaleFix, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin, scaleFix, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutlineJustified(this MTexture tex, Vector2 position, Vector2 justify, Color color, float scale, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (new Vector2((float)tex.Width * justify.X, (float)tex.Height * justify.Y) - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, 0f, origin, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutlineJustified(this MTexture tex, Vector2 position, Vector2 justify, Color color, float scale, float rotation, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (new Vector2((float)tex.Width * justify.X, (float)tex.Height * justify.Y) - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, rotation, origin, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutlineJustified(this MTexture tex, Vector2 position, Vector2 justify, Color color, float scale, float rotation, SpriteEffects flip, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (new Vector2((float)tex.Width * justify.X, (float)tex.Height * justify.Y) - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, rotation, origin, scale, flip, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin, scale, flip, 0f);
        }
        public static void DrawLazyOutlineJustified(this MTexture tex, Vector2 position, Vector2 justify, Color color, Vector2 scale, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (new Vector2((float)tex.Width * justify.X, (float)tex.Height * justify.Y) - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, 0f, origin, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutlineJustified(this MTexture tex, Vector2 position, Vector2 justify, Color color, Vector2 scale, float rotation, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (new Vector2((float)tex.Width * justify.X, (float)tex.Height * justify.Y) - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, rotation, origin, scale, SpriteEffects.None, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
        public static void DrawLazyOutlineJustified(this MTexture tex, Vector2 position, Vector2 justify, Color color, Vector2 scale, float rotation, SpriteEffects flip, Color outline, float outlineSize = 1)
        {
            float scaleFix = tex.ScaleFix;
            scale *= scaleFix;
            Rectangle clipRect = tex.ClipRect;
            Vector2 origin = (new Vector2((float)tex.Width * justify.X, (float)tex.Height * justify.Y) - tex.DrawOffset) / scaleFix;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position + new Vector2(i, j) * outlineSize, clipRect, outline, rotation, origin, scale, flip, 0f);
                    }
                }
            }

            Draw.SpriteBatch.Draw(tex.Texture.Texture_Safe, position, clipRect, color, rotation, origin, scale, flip, 0f);
        }

    }
}
