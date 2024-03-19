using Celeste.Mod.CommunalHelper;
using FrostHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    public class FountainMove : CutsceneEntity
    {
        public FountainBlock Block;
        public int NeededGenerators;
        public FountainMove(int value) : base()
        {
            NeededGenerators = value;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            PianoModule.Session.ForceFountainOpen = false;
            Block = (scene as Level).Tracker.GetEntity<FountainBlock>();
            if (Block is null) RemoveSelf();
        }
        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene()));
        }
        private IEnumerator Cutscene()
        {
            if (Block is null || PianoModule.Session.OpenedFountain) yield break;
            while (!PianoModule.Session.ForceFountainOpen && (!PianoModule.Session.FountainCanOpen || !InSafeLocation(Level))) yield return null;
            Player player = Level.GetPlayer();
            if (player is null) yield break;
            player.StateMachine.State = Player.StDummy;
            Camera camera = Level.Camera;
            Vector2 target = Level.Marker("fountainCamera") - new Vector2(160, 90);
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1.5f)
            {
                camera.Approach(target, i);
                yield return null;
            }
            yield return 0.5f;
            yield return Block.OpenPassage();
            yield return null;
            player.StateMachine.State = Player.StNormal;
            PianoModule.Session.ForceFountainOpen = false;
            EndCutscene(Level);
        }

        private bool InSafeLocation(Level level)
        {
            Camera camera = level.Camera;
            Player player = level.GetPlayer();
            if (camera.GetBounds().Contains(Block.Center.ToPoint()))
            {
                if (player != null && player.OnGround())
                {
                    return true;
                }
            }
            return false;
        }
        public override void OnEnd(Level level)
        {

        }
    }
}
