using System;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    public class FrequencyDecalTarget
    {
        public string ID;
        public Vector2 PositionInRoom;
        public Color Color;
        public string LevelName;
        public Vector2 WorldPosition
        {
            get
            {
                if(Engine.Scene is Level level)
                {
                    if(level.Session.MapData.Get(LevelName) is LevelData data)
                    {
                        return data.Position + PositionInRoom;
                    }
                }
                return Vector2.Zero;
            }
        }
    }
    [Tracked]
    public class FrequencyDecalComponent : Component
    {
/*        [ConstantEntity("PuzzleIslandHelper/FrequencyDecalRenderer")]
        internal class Renderer : Entity
        {
            public VirtualRenderTarget Buffer;
            public Renderer() : base()
            {
                Depth = int.MinValue;
                Buffer = VirtualContent.CreateRenderTarget("frequency-decal-renderer", 320, 180);
                Add(new BeforeRenderHook(() =>
                {

                }));
            }
            public override void Render()
            {
                base.Render();
                Camera camera = SceneAs<Level>().Camera;
                Draw.SpriteBatch.Draw(Buffer, camera.Position, Color.White);
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Buffer.Dispose();
            }
        }*/
        public MTexture Texture;
        public string TargetID;
        public FrequencyDecalTarget Target;
        public bool HasTarget => Target != null;
        public Vector2[] Offsets;
        public FrequencyDecalComponent(string targetID, MTexture texture, params Vector2[] offsets) : base(true, true)
        {
            TargetID = targetID;
            Texture = texture;
            Offsets = offsets;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            if(PianoMapDataProcessor.FrequencyDecalTargets.TryGetValue(entity.Scene.GetAreaKey(), out var targets))
            {
               targets?.TryGetValue(TargetID ?? "", out Target);
            }
        }
    }
}
