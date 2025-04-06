using Celeste.Mod.Registry.DecalRegistryHandlers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;


namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public class DecalOutline
    {
        public class Outline : Component
        {
            public Color Color = Color.Black;
            public int Thickness = 1;
            public Decal Decal => (Decal)base.Entity;
            public Outline() : base(true, true)
            {
            }
            public Outline(Color color) : base(true, true)
            {
                Color = color;
            }
            public Outline(int thickness) : base(true, true)
            {
                Thickness = thickness;
            }
            public Outline(Color color, int thickness) : base(true, true)
            {
                Color = color;
                Thickness = thickness;
            }
            public override void Render()
            {
                MTexture frame = Decal.textures[(int)Decal.frame];
                DrawOutline(frame, Thickness, Color);
            }
            public void DrawOutline(MTexture texture, int thickness, Color outlineColor)
            {
                float scaleFix = texture.ScaleFix;
                Vector2 scale = Decal.scale * scaleFix;
                Vector2 origin = (texture.Center - texture.DrawOffset) / texture.ScaleFix;
                Rectangle clipRect = texture.ClipRect;
                Vector2 origin2 = (origin - texture.DrawOffset) / scaleFix;
                for (int i = -thickness; i <= thickness; i++)
                {
                    for (int j = -thickness; j <= thickness; j++)
                    {
                        if (i != 0 || j != 0)
                        {
                            Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, Decal.Position + new Vector2(i, j), clipRect, outlineColor, Decal.Rotation, origin2, scale, SpriteEffects.None, 0f);
                        }
                    }
                }

                Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, Decal.Position, clipRect, Decal.Color, Decal.Rotation, origin2, scale, SpriteEffects.None, 0f);
            }
        }

        internal sealed class OutlineHandler : DecalRegistryHandler
        {
            public override string Name => "outline";
            private Color color = Color.Black;
            private int thickness = 1;
            public override void ApplyTo(Decal decal) => MakeOutlined(decal);
            public void MakeOutlined(Decal decal) => decal.Add(decal.image = (Outline)new(color, thickness));
            public override void Parse(XmlAttributeCollection xml)
            {
                color = GetHexColor(xml, "color", Color.Black);
                thickness = Get(xml, "thickness", 1);
            }
        }
        [OnLoad]
        public static void Load()
        {
            DecalRegistry.AddPropertyHandler<OutlineHandler>();
        }
    }
}