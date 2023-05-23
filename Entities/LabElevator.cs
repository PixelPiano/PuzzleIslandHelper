using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Reflection.Emit;

// PuzzleIslandHelper.LabElevator
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LabElevator")]
    public class LabElevator : Solid
    {
        private int counter;

        private bool moving;

        private TalkComponent talk;

        private float jitterAmount;

        private float moveSpeed;

        private Vector2 startPosition;

        private Vector2 endPosition;

        public string flag;

        private Sprite doorSprite;

        private Image backGlass;

        private Image frontGlass;

        private Entity back;

        private Entity front;

        private SoundSource sfx = new SoundSource("event:/PianoBoy/ElevatorMusic");


        private InvisibleBarrier barrierOne;

        private InvisibleBarrier barrierTwo;

        private InvisibleBarrier barrierThree;
        public LabElevator(EntityData data, Vector2 offset)
            : base(data.Position + offset, 48, 6, false)
        {
            counter = 0;
            Add(talk = new TalkComponent(new Rectangle(0, -8, 48, 8), new Vector2(23.5f, -10f), Interact));
            talk.PlayerMustBeFacing = false;
            flag = data.Attr("flag");
            startPosition = Position;
            endPosition = Position + new Vector2(0f, data.Float("endPosition"));
            moveSpeed = data.Float("moveSpeed");
            jitterAmount = data.Float("jitterAmount");
            Add(sfx);
            sfx.Pause();
            Add(doorSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/labElevator/"));
            doorSprite.AddLoop("idle", "idle", 0.1f);
            doorSprite.Rate = 1.5f;
            /*Collider = new ColliderList(new Hitbox(48, 8, 0, 0), new Hitbox(1, 10, 0, -27), new Hitbox(1, 10, 47, -27),
                                        new Hitbox(2, 4, 1, -30), new Hitbox(2, 4, 45, -30),
                                        new Hitbox(2, 2, 3, -32), new Hitbox(4,3,5,-35),
                                        new Hitbox(4, 3, 9, -37), new Hitbox(22, 4, 13, -40),
                                        new Hitbox(2, 2, 43, -32), new Hitbox(4, 3, 39, -35),
                                        new Hitbox(4, 3, 35, -37)); //how to summon a demon*/
            Collider = new Hitbox(48, 8, 0, 0);


            Depth = -10500;
            Add(new LightOcclude());
        }
        private void Interact(Player player)
        {
            Coroutine move = new Coroutine(MoveElevator());
            move.RemoveOnComplete = true;
            Add(move);
        }
        public void JitterPlatforms(bool state)
        {
            counter++;
            if (counter == 5)
            {
                counter = 1;
            }
            //if (state)
            if (counter == 1)
            {
                MoveToX(Position.X + jitterAmount);
            }
            //else
            if (counter == 3)
            {
                MoveToX(Position.X - jitterAmount);
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (SceneAs<Level>().Session.GetFlag(flag))
            {
                MoveToY(startPosition.Y);
            }
            else
            {
                MoveToY(endPosition.Y);
            }
            SceneAs<Level>().Add(barrierOne = new InvisibleBarrier(new Vector2(Position.X, Position.Y - 30), 5, 13));
            SceneAs<Level>().Add(barrierTwo = new InvisibleBarrier(new Vector2(Position.X + 43, Position.Y - 30), 5, 13));
            SceneAs<Level>().Add(barrierThree = new InvisibleBarrier(new Vector2(Position.X, Position.Y - 40), 48, 10));
        }
        public override void Update()
        {
            base.Update();
            talk.Enabled = !moving;
            if (backGlass != null)
            {
                back.Position = Position;
            }
            if (frontGlass != null)
            {
                front.Position = Position;
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.Add(back = new Entity(Position));
            scene.Add(front = new Entity(Position));
            back.Depth = 9000;
            front.Depth = -10500;
            backGlass = new Image(GFX.Game["objects/PuzzleIslandHelper/labElevator/glassBack"]);
            frontGlass = new Image(GFX.Game["objects/PuzzleIslandHelper/labElevator/glassFront"]);
            backGlass.Origin = new Vector2(0, 17);
            frontGlass.Origin = new Vector2(0, 40);
            back.Add(backGlass);
            front.Add(frontGlass);
            doorSprite.Play("idle");

            if (SceneAs<Level>().Session.GetFlag(flag))
            {
                ResetPlatforms(startPosition.Y);
            }
            else
            {
                ResetPlatforms(endPosition.Y);
            }

        }
        private void ResetPlatforms(float value)
        {
            MoveToY(value);
            barrierOne.MoveToY(value - 30);
            barrierTwo.MoveToY(value - 30);
            barrierThree.MoveToY(value - 40);
        }
        private void MovePlatforms(float value)
        {
            MoveTowardsY(value, moveSpeed * Engine.DeltaTime);
            barrierOne.MoveTowardsY(value - 30, moveSpeed * Engine.DeltaTime);
            barrierTwo.MoveTowardsY(value - 30, moveSpeed * Engine.DeltaTime);
            barrierThree.MoveTowardsY(value - 40, moveSpeed * Engine.DeltaTime);
        }
        public IEnumerator MoveElevator()
        {
            bool jitterState = false;
            if (!moving)
            {
                //talk.Enabled = false;
                moving = true;
                if (SceneAs<Level>().Session.GetFlag(flag))
                {
                    while (Position.Y < endPosition.Y)
                    {
                        MovePlatforms(endPosition.Y);
                        JitterPlatforms(jitterState);
                        jitterState = !jitterState;
                        yield return null;
                    }
                    MoveToX(endPosition.X);
                    SceneAs<Level>().Session.SetFlag(flag, false);

                }
                else
                {
                    while (Position.Y > startPosition.Y)
                    {
                        MovePlatforms(startPosition.Y);
                        JitterPlatforms(jitterState);
                        jitterState = !jitterState;
                        yield return null;
                    }
                    MoveToX(startPosition.X);
                    SceneAs<Level>().Session.SetFlag(flag, true);
                }
                moving = false;
                //talk.Enabled = true;
            }
        }
    }
}