using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
// PuzzleIslandHelper.ComputerAccess
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ComputerAccess")]
    public class ComputerAccess : Entity
    {
        private bool moveCamera;

        private Sprite sprite;

        public Player player;

        public bool playerNear;

        private bool inFunction;

        private Entity pop;

        private Sprite popSprite;

        private bool canPop;

        private bool noPop;

        private bool canZoom;

        private bool beforeZoomSequence;
        public ComputerAccess(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Collider = new Hitbox(14, 14, -7, -6);
            inFunction = false;
            Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/access/"));
            sprite.AddLoop("float", "artifact", 0.1f);
            sprite.AddLoop("idle", "artifact", 0.1f, 3);
            sprite.Origin += new Vector2(sprite.Width / 2, sprite.Height / 2);
        }
        public IEnumerator toPlayer()
        {
            player = Scene.Tracker.GetEntity<Player>();
            Vector2 from = Position;
            Vector2 target = player.Position;
            float amount = 0.0f;
            float amount2 = 0.0f;
            Vector2 scaleFrom = sprite.Scale;
            Vector2 scaleTarget = Vector2.One;
            float rotateFrom = sprite.Rotation;
            float rotateTarget = rotateFrom + 180;
            int counter = 0;
            rotateFrom = sprite.Rotation;
            while (Position.X >= target.X + 10 && Position.Y <= target.Y - 10)
            {

                sprite.Rotation = rotateFrom + (rotateTarget * Ease.SineIn(amount2 / 4));
                if (amount2 < 0.2) { amount2 += 0.005f; }
                else if (amount2 <= 0.4) { amount2 += 0.01f; }
                else if (amount2 <= 0.6) { amount2 += 0.03f; }
                else if (amount2 < 1) { amount2 += 0.035f; }
                else { amount2 += 0.05f; }
                counter++;

                if (counter >= 90)
                {
                    moveCamera = true;
                    Position.X = from.X - (from.X - target.X - 1) * Ease.SineIn(amount);
                    Position.Y = from.Y - (from.Y - (target.Y - player.Height)) * Ease.SineIn(amount);
                    sprite.Scale = scaleFrom - (scaleFrom * Ease.SineIn(amount + 0.01f));
                    if (amount < 0.2) { amount += 0.005f; }
                    else if (amount <= 0.4) { amount += 0.01f; }
                    else if (amount <= 0.6) { amount += 0.03f; }
                    else if (amount < 1) { amount += 0.035f; }
                }
                yield return null;
            }

            sprite.Scale = Vector2.Zero;
            sprite.Rotation = rotateFrom;
            Position = player.Position;
            canPop = true;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Position += new Vector2(Width / 2, Height / 2);
            sprite.Play("float");
            SceneAs<Level>().Session.SetFlag("artifactCollect", false);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if((scene as Level).Session.GetFlag("artifactObtained"))
            {
                RemoveSelf();
            }
            moveCamera = false;
            canZoom = false;
            beforeZoomSequence = false;
            SceneAs<Level>().Session.SetFlag("nextArtifactSequence", false);
            scene.Add(pop = new Entity(new Vector2(Position.X - 4, Position.Y - 4)));
            canPop = false;
            noPop = false;
            pop.Add(popSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/access/"));
            popSprite.CenterOrigin();
            popSprite.Add("pop", "pop", 0.07f);
            pop.Depth = -10500;
        }
        public override void Update()
        {

            base.Update();
            if(Scene as Level is not null)
            {
                if (SceneAs<Level>().Session.GetFlag("obtainedArtifact"))
                {
                    return;
                }
            }
            else
            {
                return;
            }


            //Visible = false;
            Coroutine coroutine = new Coroutine(toPlayer(), true);
            player = Scene.Tracker.GetEntity<Player>();
            pop.Position = player.Position - new Vector2(player.Width * 1.5f - 1, player.Height * 2);
            Coroutine zoom = new Coroutine(ZoomZoom(100, 2, Scene as Level));



            if (!noPop && canPop)
            {
                popSprite.Play("pop");
                //TODO: Play pop sound
                noPop = true;
                SceneAs<Level>().Session.SetFlag("nextArtifactSequence", true);
            }
            if (SceneAs<Level>().Session.GetFlag("artifactCollect"))
            {
                sprite.Play("idle");
                if (!inFunction)
                {
                    //new Vector2(250, getFocus(Scene as Level).Y)
                    player.Facing = Facings.Right;
                    inFunction = true;
                    Coroutine focus = new Coroutine(ZoomFocus(new Vector2(250,120), 1.5f, Scene as Level), true);
                    Add(coroutine);
                    Add(focus);
                    //TODO: Add sounds
                }
                if (beforeZoomSequence)
                {

                }
                if (SceneAs<Level>().Session.GetFlag("youCanZoomNow") && canZoom)
                {
                    Add(zoom);
                    canZoom = false;
                }
            }
            else
            {
                sprite.Play("float");
            }
        }
        public Vector2 getFocus(Level level)
        {
            return level.ZoomFocusPoint;
        }
        public float getZoom(Level level)
        {
            return level.Zoom;
        }
        public IEnumerator ZoomFocus(Vector2 screenSpaceFocusPoint, float duration, Level level)
        {
            while (!moveCamera)
            {
                yield return null;
            }
            Vector2 fromFocus = level.ZoomFocusPoint;
            player = Scene.Tracker.GetEntity<Player>();
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
            {
                float amount = Ease.CubeIn(MathHelper.Clamp(p, 0f, 1f));
                level.ZoomFocusPoint = Vector2.Lerp(fromFocus, screenSpaceFocusPoint, amount);
                yield return null;
            }
            level.ZoomFocusPoint = screenSpaceFocusPoint;
            canZoom = true;
        }
        public IEnumerator ZoomZoom(float zoom, float duration, Level level)
        {
            float fromZoom = level.Zoom;
            player = Scene.Tracker.GetEntity<Player>();

            for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
            {
                float amount = Ease.SineInOut(MathHelper.Clamp(p, 0f, 1f));
                level.Zoom = level.ZoomTarget = MathHelper.Lerp(fromZoom, zoom, amount);
                yield return null;
            }
            level.Zoom = level.ZoomTarget;
        }
    }
}