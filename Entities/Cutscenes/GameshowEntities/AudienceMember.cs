using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [CustomEntity("PuzzleIslandHelper/AudienceMember")]
    [Tracked]
    public class AudienceMember : Entity
    {
        public Sprite Sprite;
        public AudienceMember(Vector2 position, string faceType) : base(position)
        {
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/gameshow/audience/" + faceType + "/");
            Sprite.AddLoop("idle", faceType + "Face", 0.1f);
            Sprite.AddLoop("cheer", faceType + "Laugh", 0.1f);
            Sprite.AddLoop("die", faceType + "Die", 0.1f);
            Add(Sprite);
            Sprite.Position -= new Vector2(Sprite.Width / 2, Sprite.Height / 2);
            Sprite.Play("idle");
            Depth = 4;
            Tag |= Tags.TransitionUpdate;
        }
        public override void Update()
        {
            if (!SceneAs<Level>().Session.Level.Contains("Gameshow"))
            {
                Sprite.Color = Color.Lerp(Color.White, Color.Black, SceneAs<Level>().Lighting.Alpha);
            }
            else
            {
                Sprite.Color = Color.White;
            }
            base.Update();
        }
        public void Die()
        {
            Add(new Coroutine(dieRoutine()));
        }
        private IEnumerator dieRoutine()
        {
            Sprite.Play("die");
            for (int i = 0; i < 4; i++)
            {
                Visible = true;
                yield return Engine.DeltaTime * 2;
                Visible = false;
                yield return Engine.DeltaTime * 2;
            }
            RemoveSelf();
            yield return null;
        }
        public void Cheer()
        {
            Sprite.Play("cheer");
        }
        public void StopCheering()
        {
            Sprite.Play("idle");
        }
        public IEnumerator CheerRoutine(float duration)
        {
            Cheer();
            yield return duration;
            StopCheering();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if ((scene as Level).Session.GetFlag("FacesWiped"))
            {
                RemoveSelf();
            }
        }
        public AudienceMember(EntityData data, Vector2 offset) : this(data.Position + offset, data.Attr("faceType"))
        {

        }
    }
}
