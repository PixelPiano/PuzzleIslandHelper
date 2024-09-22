using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/LoneEye")]
    [Tracked]
    public class LoneEye : Entity
    {
        public Sprite Sprite;
        public Image Sclera;

        public Vector2 Origin = Vector2.One * 3;
        public float Rotation;
        public float RotationRate;
        private float targetRotationRate;
        public EntityID id;
        public LoneEye(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            this.id = id;
            Add(Sprite = new Sprite(GFX.Game, "characters/PuzzleIslandHelper/Calidus/"));
            Sprite.AddLoop("idle", "eyeFront", 0.1f);
            Sprite.Add("end", "flashEnd", 0.1f, "idle");
            Sprite.Add("start", "flashStart", 0.1f, "start");
            Sprite.Play("idle");
            Sprite.OnChange = (string s1, string s2) =>
            {
                if(s1 == "start" && s2 == "end")
                {
                    Sclera.Visible = true;
                    Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn,1,true);
                    tween.OnUpdate = (Tween t) =>
                    {
                        Sclera.Color = Color.Green * (1 - t.Eased);
                    };
                    tween.OnComplete = (Tween t) =>
                    {
                        Sclera.Color = Color.Green;
                        Sclera.Visible = false;
                    };
                    Add(tween);
                }
            };
            Add(Sclera = new Image(GFX.Game["characters/PuzzleIslandHelper/calidus/scleraFlash"]));
            Sclera.Color = Color.Green;
            Sclera.Visible = false;

            Sprite.Origin = Sclera.Origin = Origin;
            Sprite.Position = Sclera.Position = Origin;
            Collider = new Hitbox(5, 5);
            Position += Vector2.One;
        }
        public override void Update()
        {
            base.Update();
            RotationRate = Calc.Approach(RotationRate, targetRotationRate, 5f * Engine.DeltaTime);
            Rotation += RotationRate;
            Rotation %= 360;
            Sprite.Rotation = Sclera.Rotation = Rotation;
        }
        public IEnumerator StartFloating(float dist, float duration, float targetRotationRate)
        {
            this.targetRotationRate = targetRotationRate;
            Vector2 from = Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
            {
                Position.Y = Calc.LerpClamp(from.Y, from.Y - dist, Ease.SineInOut(i));
                yield return null;
            }
            Position.Y = from.Y - dist;
        }
        public IEnumerator Absorb(Vector2 to, float duration, Color? flash)
        {
            if (Scene is not Level level) yield break;
            Vector2 from = Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
            {
                Position = Vector2.Lerp(from, to, Ease.CubeIn(i));
                yield return null;
            }
            if (flash != null)
            {
                level.Flash(flash.Value, true);
            }
            RemoveSelf();
        }
    }
}