using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;


namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/MonitorDecalGroup")]
    [Tracked]
    public class MonitorDecalGroup : Entity
    {
        private Sprite CustomDecal;
        private string customDecalPath;
        private Level level;
        private string GroupID;
        private bool UsesDecal;
        private bool ScaleDecal;
        private float Amplitude;
        private int[] Spaces = new int[8];
        private List<MonitorDecal> MonitorGroup = new();
        private VirtualRenderTarget Target = VirtualContent.CreateRenderTarget("MonitorDecal", 320, 180);
        private VirtualRenderTarget Target2 = VirtualContent.CreateRenderTarget("MonitorDecal2", 320, 180);
        private VirtualRenderTarget Mask = VirtualContent.CreateRenderTarget("MonitorDecalMask", 320, 180);
        public MonitorDecalGroup(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            customDecalPath = data.Attr("groupDecal");
            UsesDecal = !string.IsNullOrEmpty(customDecalPath);
            ScaleDecal = data.Bool("scaleDecal", false);
            GroupID = data.Attr("groupId");

            if (UsesDecal)
            {
                CustomDecal = new Sprite(GFX.Game, customDecalPath);
                CustomDecal.AddLoop("idle", "", 0.1f);
                CustomDecal.Play("idle");
                CustomDecal.Visible = false;

                Add(CustomDecal);
            }
            Depth = 9001;
            Add(new BeforeRenderHook(BeforeRender));
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = scene as Level;
            for(int i = 0; i<8; i++)
            {
                Spaces[i] = Calc.Random.Range(1, 5);
            }
            foreach (MonitorDecal md in level.Tracker.GetEntities<MonitorDecal>())
            {
                if (md.GroupID == GroupID)
                {
                    MonitorGroup.Add(md);
                }
            }
            if (MonitorGroup.Count == 0)
            {
                RemoveSelf();
            }
            Tween tween = Tween.Create(Tween.TweenMode.Looping, Ease.Linear, 3, true);
            tween.OnUpdate = (Tween t) => { Amplitude = t.Eased; };
            Add(tween);
            Vector2 topLeft = MonitorGroup[0].Position;
            Vector2 bottomRight = MonitorGroup[0].Position + new Vector2(MonitorGroup[0].Width, MonitorGroup[0].Height);
            for (int i = 0; i < MonitorGroup.Count; i++)
            {
                if (MonitorGroup[i].Position.X < topLeft.X)
                {
                    topLeft.X = MonitorGroup[i].Position.X;
                }
                if (MonitorGroup[i].Position.Y < topLeft.Y)
                {
                    topLeft.Y = MonitorGroup[i].Position.Y;
                }
                if (MonitorGroup[i].Position.X + MonitorGroup[i].Width > bottomRight.X)
                {
                    bottomRight.X = MonitorGroup[i].Position.X + MonitorGroup[i].Width;
                }
                if (MonitorGroup[i].Position.Y + MonitorGroup[i].Height > bottomRight.Y)
                {
                    bottomRight.Y = MonitorGroup[i].Position.Y + MonitorGroup[i].Height;
                }
                MonitorGroup[i].FromController = true;
            }
            Position = topLeft;
            float width = bottomRight.X - topLeft.X;
            float height = bottomRight.Y - topLeft.Y;
            Collider = new Hitbox(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);

            if (ScaleDecal)
            {
                CustomDecal.Scale = new Vector2(width / CustomDecal.Width, height / CustomDecal.Height);
                //CustomDecal.Offset += new Vector2(((CustomDecal.Width * CustomDecal.TextScale.X) - CustomDecal.Width)/4, ((CustomDecal.Height * CustomDecal.TextScale.Y) - CustomDecal.Height)/4);

            }
        }
        private void BeforeRender()
        {
            ShaderFX.MonitorDecal.ApplyStandardParameters(level);
            ShaderFX.MonitorDecal.Parameters["Random"]?.SetValue(Calc.Random.Range(0, 1f));
            ShaderFX.MonitorDecal.Parameters["Amplitude"]?.SetValue(Amplitude);
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Mask);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Mask.SetRenderMask(DrawInsides, level.Camera.Matrix);
            Target.ApplyDraw(DrawScreen, level.Camera.Matrix, true);
            Target2.ApplyDraw(DrawScreen, level.Camera.Matrix, true, ShaderFX.MonitorDecal);

            Target2.ApplyMask(Mask);
            Target.ApplyMask(Mask);
        }
        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);
            Draw.SpriteBatch.Draw(Target2, level.Camera.Position, Color.White);
            foreach (MonitorDecal md in MonitorGroup)
            {
                md.Rays.Render();
                md.Border.Render();
                md.Buttons.Render();
            }

        }
        private void DrawInsides()
        {
            foreach (MonitorDecal md in MonitorGroup)
            {
                md.Monitor.Render();
            }
        }
        private void DrawScreen()
        {
            foreach (MonitorDecal md in MonitorGroup)
            {
                Draw.Rect(md.Collider, md.ScreenColor);
            }
            if (UsesDecal)
            {
                CustomDecal.Render();
            }

        }
        public override void DebugRender(Camera camera)
        {
            if (UsesDecal)
            {
                CustomDecal.Render();
            }
            base.DebugRender(camera);
        }
    }
}