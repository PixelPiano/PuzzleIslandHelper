using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [CustomEntity("PuzzleIslandHelper/AudienceMember")]
    [Tracked]
    public class AudienceMember : Entity
    {
        public Sprite Sprite;
        public bool AffectedByLevelLighting;
        public FlagData Flag;
        private bool prevState;
        private readonly string[] types = ["angry", "ok", "stapler", "uwu"];
        private string faceType;
        public bool Dead;
        public AudienceMember(EntityData data, Vector2 offset) : this(data.Position + offset, data.Flag("flag", "inverted"), data.Bool("randomFace") ? null : data.Attr("faceType"), data.Bool("usesLighting")) { }
        public AudienceMember(Vector2 position, FlagData flag, string faceType = null, bool affectedByLevelLighting = false) : base(position)
        {
            this.faceType = faceType;
            Flag = flag;
            Depth = 4;
            Tag |= Tags.TransitionUpdate;
            AffectedByLevelLighting = affectedByLevelLighting;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!Flag.GetState(scene))
            {
                Dead = true;
            }
            else
            {
                if (string.IsNullOrEmpty(faceType))
                {
                    Calc.PushRandom((int)(X * Y));
                    faceType = types.Random();
                    Calc.PopRandom();
                }
                Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/gameshow/audience/" + faceType + "/");
                Sprite.AddLoop("idle", faceType + "Face", 0.1f);
                Sprite.AddLoop("cheer", faceType + "Laugh", 0.1f);
                Sprite.AddLoop("die", faceType + "Die", 0.1f);

                Add(Sprite);
                Sprite.Position -= new Vector2(Sprite.Width / 2, Sprite.Height / 2);
                Collider = Sprite.Collider();
                Idle();
            }
        }
        public override void Update()
        {
            bool state = Flag.State;
            if (prevState != state)
            {
                if (!state) Die();
                else Idle();
            }
            if (AffectedByLevelLighting)
            {
                Sprite.Color = Color.Lerp(Color.White, Color.Black, SceneAs<Level>().Lighting.Alpha);
            }
            base.Update();
            prevState = Flag.State;
        }
        public void Die()
        {
            if (!Dead)
            {
                Add(new Coroutine(dieRoutine()));
            }
        }
        public override void Render()
        {
            if (!Dead)
            {
                base.Render();
            }

        }
        private IEnumerator dieRoutine()
        {
            Sprite.Play("die");
            for (int i = 0; i < 4; i++)
            {
                Dead = true;
                yield return 0.1f;
                Dead = false;
                yield return 0.1f;
            }
            Dead = true;
            yield return null;
        }
        public void Cheer()
        {
            Sprite.Play("cheer");
        }
        public void Idle()
        {
            Dead = false;
            Sprite.Play("idle", false, true);
        }
        public IEnumerator CheerRoutine(float duration)
        {
            Cheer();
            yield return duration;
            Idle();
        }
    }
}
