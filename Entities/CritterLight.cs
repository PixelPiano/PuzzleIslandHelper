using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class CritterLight : Image
    {
        public float Radius;
        public bool OnScreen;
        public VertexLight Light;
        public bool Enabled;
        public MTexture Gradient;
        public CritterLight(float radius, VertexLight light, bool enabled = false) : base(GFX.Game["objects/PuzzleIslandHelper/light"], true)
        {
            Gradient = GFX.Game["objects/PuzzleIslandHelper/lightGradient"];
            Enabled = enabled;
            Radius = radius;
            Scale = Vector2.One * (radius * 2) / new Vector2(Texture.Width, Texture.Height);
            Light = light;
        }
        public bool Colliding(Entity entity)
        {
            return Enabled && Vector2.DistanceSquared(entity.Center, Light.Center) < Radius * Radius + 16;
        }
        public override void Render()
        {

        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level)
            {
                OnScreen = false;
                return;
            }
            Vector2 pos = RenderPosition;
            Rectangle c = level.Camera.GetBounds();
            Rectangle r = new Rectangle(c.Left - 8, c.Top - 8, c.Width + 16, c.Height + 16);
            OnScreen = new Rectangle((int)pos.X, (int)pos.Y, Texture.Width, Texture.Height).Colliding(r);
        }
        public void DrawLight(Color color, Vector2 offset = default)
        {
            Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, Light.Center - Vector2.One * Radius + offset, null, color, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }
        public void DrawGradient(Color color, Vector2 offset = default)
        {
            Draw.SpriteBatch.Draw(Gradient.Texture.Texture_Safe, Light.Center - Vector2.One * Radius + offset, null, color, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }
    }
}
