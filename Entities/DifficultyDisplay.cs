using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

// PuzzleIslandHelper.DifficultyDisplay
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DifficultyDisplay")]
    public class DifficultyDisplay : Entity
    {
        private float opacity;
        private MTexture pip = GFX.Game["objects/PuzzleIslandHelper/difficultyDisplay/pip"];
        private int difficultyLevel = 1;
        private float playerOver = 1;
        private readonly float spacing = 10;
        private bool inRoutine = false;
        private float transitionOpacity = 1;
        private bool waitingForPlayerLeave = false;
        private Rectangle bounds;
        private Camera camera;
        private Player player;
        private TransitionListener listener = new TransitionListener();
        public bool OnInRoutine = false;
        public bool OnOutRoutine = false;
        public DifficultyDisplay(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Tag = TagsExt.SubHUD;
            difficultyLevel = data.Int("difficultyLevel", 1);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Visible = true;
            if (!OnOutRoutine && !OnInRoutine)
            {
                Add(new Coroutine(ColorTween(), true));
            }
            
        }
        public override void Update()
        {
            base.Update();
            player = Scene.Tracker.GetEntity<Player>();
            camera = SceneAs<Level>().Camera;
            bounds = new Rectangle((int)camera.Left, (int)camera.Top, (pip.Width / 8) * difficultyLevel, (pip.Height / 8) * 2);

            if (player.CollideRect(bounds) && !inRoutine && !waitingForPlayerLeave)
            {
                Add(new Coroutine(PlayerOver(), true));
            }
            else
            {
                if (!inRoutine && waitingForPlayerLeave)
                {
                    Add(new Coroutine(WaitForLeave(), true));
                }
            }
        }
        private IEnumerator WaitForLeave()
        {
            inRoutine = true;
            while (player.CollideRect(bounds))
            {
                yield return null;
            }
            for (float i = 0; i < 1; i += 0.05f)
            {
                playerOver = Calc.LerpClamp(0.5f, 1, i);
                yield return null;
            }
            inRoutine = false;
            waitingForPlayerLeave = false;
        }
        public override void Render()
        {
            base.Render();
            pip.ScaleFix = 0.4f;
            if (!OnInRoutine && !OnOutRoutine)
            {
                for (int i = 0; i < difficultyLevel; i++)
                {
                    pip.Draw(new Vector2(20 + (i * (spacing + (pip.ScaleFix * pip.Width))), 10), Vector2.Zero, Color.White * playerOver * transitionOpacity);
                    pip.Draw(new Vector2(20 + (i * (spacing + (pip.ScaleFix * pip.Width))), 10), Vector2.Zero, Color.Red * opacity * transitionOpacity);
                }
            }

        }
        private IEnumerator PlayerOver()
        {
            inRoutine = true;
            for (float i = 0; i < 1; i += 0.05f)
            {
                playerOver = Calc.LerpClamp(1, 0.5f, i);
                yield return null;
            }
            waitingForPlayerLeave = true;
            inRoutine = false;
        }
        private IEnumerator ColorTween()
        {
            for (int j = 0; j < 4; j++)
            {
                for (float i = 0; i < 1; i += 0.1f)
                {
                    opacity = Calc.LerpClamp(1, 0, i);
                    yield return null;
                }
                for (float i = 0; i < 1; i += 0.1f)
                {
                    opacity = Calc.LerpClamp(0, 1, i);
                    yield return null;
                }
            }
            for (float i = 0; i < 1; i += 0.1f)
            {
                opacity = Calc.LerpClamp(1, 0, i);
                yield return null;
            }
            opacity = 0;
            yield return null;
        }
    }
}
