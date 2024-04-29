using Microsoft.Xna.Framework;
using Monocle;

// PuzzleIslandHelper.TransitionEvent
namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions
{
    [TrackedAs(typeof(ShaderOverlay))]
    public class DuelView : ShaderOverlay
    {
        public int MaxSize = 80;
        public int StartSize = 20;
        public Vector2 GlobalBoxCenter;
        public Vector2 Offset = new Vector2(80, 20);

        public DuelView() : base("PuzzleIslandHelper/Shaders/duelView", "", false, 0)
        {
            Depth = -20001;
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player)
            {
                return;
            }
            GlobalBoxCenter = player.Center;
        }
        public Rectangle GetSource()
        {
            return new Rectangle((int)GlobalBoxCenter.X, (int)GlobalBoxCenter.Y, MaxSize, MaxSize);
        }
        public Vector2[] GetBoxPositions()
        {
            if (Scene is not Level level) return null;
            Vector2[] result = new Vector2[2];
            Vector2 maxSize = Vector2.One * MaxSize;
            Vector2 startSize = Vector2.One * StartSize;
            Vector2 boxSize = startSize + (maxSize - startSize) * Amplitude;
            Vector2 offset = Offset;
            Vector2 puv = GlobalBoxCenter;

            Vector2 targetA = new Vector2(160 - boxSize.X - offset.X, offset.Y);
            Vector2 targetB = new Vector2(160 + offset.X, offset.Y);
            Vector2 basePos = puv - boxSize / 2;
            result[0] = basePos + (targetA - basePos);
            result[1] = basePos + (targetB - basePos);
            return result;
        }
        public Vector2 uvToWorld(Vector2 uv)
        {
            if (Scene is not Level level) return Vector2.Zero;
            return uv * Effect.Parameters["Dimensions"].GetValueVector2() + level.Camera.Position;
        }
        public override void ApplyParameters(bool identity)
        {
            base.ApplyParameters(identity);
            if (Scene is not Level level) return;
            Effect.Parameters["BoxCenter"]?.SetValue(GlobalBoxCenter);
            Effect.Parameters["StartSize"]?.SetValue(StartSize);
            Effect.Parameters["MaxSize"]?.SetValue(MaxSize);
            Effect.Parameters["CurveOffset"]?.SetValue(level.LevelOffset + Offset);
        }
    }
}
