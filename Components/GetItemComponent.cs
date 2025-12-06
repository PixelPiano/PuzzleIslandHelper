using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;
using System;
using System.Collections;
using static Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs.Osc;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Pulse;
using static System.Net.Mime.MediaTypeNames;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [TrackedAs(typeof(PlayerCollider))]
    public class GetItemComponent : PlayerCollider
    {
        public Glimmer Glimmer;
        public Vector2 GlimmerOffset;
        public Vector2 EntityOffset;
        public bool WaitForInput = true;
        public string Text = "";
        public string Subtext = "";
        public FancyText.Text fancyText;
        public FancyText.Text fancySubText;
        public Action<Player> OnCollect;
        private string flag;
        private bool removeEntity;
        private Vector2? prevPosition;
        public bool RevertPlayerState;
        private int prevState;
        public bool Running;
        private Vector2? prevSpeed;
        private Coroutine routine;
        private int? prevDepth;
        private class renderer : Entity
        {
            public FancyText.Text Text, Sub;
            public int TextEnd;
            public int SubEnd;
            public renderer(FancyText.Text text, FancyText.Text subtext) : base()
            {
                Tag |= TagsExt.SubHUD;
                Text = text;
                Sub = subtext;
            }

            public override void Render()
            {
                base.Render();
                if (Text != null)
                {
                    float w = Text.WidestLine();
                    Text.DrawOutlineJustifyPerLine(new Vector2(160 * 6, 180), new Vector2(0.5f, 0), Vector2.One, 1, 0, TextEnd);
                }
                if (Sub != null)
                {
                    float w = Sub.WidestLine();
                    float y = 0;
                    if (Text != null)
                    {
                        y = Text.Lines * Text.BaseSize * 6;
                    }
                    Sub.DrawOutlineJustifyPerLine(new Vector2(160 * 6, 180 + y), new Vector2(0.5f, 0), Vector2.One * 0.7f, 1, 0, SubEnd);
                }
            }
        }
        private renderer text;
        private Entity fakeEntity;
        public GetItemComponent(Action<Player> onCollect, string flag, bool removeEntity, string text = "", string subText = "") : base(null)
        {
            this.removeEntity = removeEntity;
            this.flag = flag;
            OnCollide = Activate;
            Glimmer = new Glimmer(Vector2.Zero, Color.White, 20, 8, 2, 3)
            {
                LineWidthTarget = 8,
                LineWidth = 4
            };
            Glimmer.AlphaMult = 0;
            OnCollect = onCollect;
            Text = text;
            Subtext = subText;
        }
        public override void Update()
        {
            base.Update();
            if (Running)
            {
                Glimmer.AlphaMult = Calc.Approach(Glimmer.AlphaMult, 1, Engine.DeltaTime * 5);
            }
        }
        public void End(Player player)
        {
            Scene.Remove(text);
            if (prevDepth.HasValue)
            {
                Entity.Depth = prevDepth.Value;
            }
            if (player != null)
            {
                if (RevertPlayerState)
                {
                    player.StateMachine.State = prevState;
                }
                if (prevSpeed.HasValue)
                {
                    player.Speed = prevSpeed.Value;
                }
            }
            if (prevPosition.HasValue)
            {
                Entity.Position = prevPosition.Value;
            }
            fakeEntity.RemoveSelf();
            if (removeEntity)
            {
                Entity.RemoveSelf();
            }
            Running = false;
        }
        public void Activate(Player player)
        {
            prevDepth = Entity.Depth;
            Entity.Depth = int.MinValue;
            fakeEntity = new Entity(Entity.Position);
            fakeEntity.Depth = Entity.Depth + 1;
            if (!string.IsNullOrEmpty(Text))
            {
                fancyText = FancyText.Parse(Text, 320 * 6, 10, 1, Color.White);
            }
            if (!string.IsNullOrEmpty(Subtext))
            {
                fancySubText = FancyText.Parse(Subtext, 240 * 6, 10, 1, Color.Gray);
            }
            Scene.Add(text = new renderer(fancyText, fancySubText));
            Running = true;
            prevState = player.StateMachine.State;
            if (!string.IsNullOrEmpty(flag))
            {
                SceneAs<Level>().Session.SetFlag(flag);
            }
            prevPosition = Entity.Position;

            fakeEntity.Add(Glimmer);
            Scene.Add(fakeEntity);
            if(Entity.Collider != null)
            {
                Glimmer.Position = Entity.Collider.HalfSize;
            }

            player.DisableMovement();
            player.DummyAutoAnimate = false;
            player.DummyGravity = false;
            prevSpeed = player.Speed;
            player.Speed.Y = 0;
            player.Speed.X = 0;
            player.Sprite.Play("pickup");
            Entity.Center = player.TopCenter - Vector2.UnitY * Entity.Height + EntityOffset;
            Audio.Play("event:/game/general/secret_revealed", player.Center);

            OnCollect?.Invoke(player);
            fakeEntity.Add(routine = new Coroutine(cutscene(player)));
        }
        public override void Removed(Entity entity)
        {
            fakeEntity?.RemoveSelf();
            if (Running)
            {
                End(Scene.GetPlayer());
            }
            base.Removed(entity);
        }
        private IEnumerator cutscene(Player player)
        {
            yield return 1;
            if (fancyText != null)
            {
                while (text.TextEnd < fancyText.Count)
                {
                    var node = fancyText[text.TextEnd];
                    if (node is FancyText.Char)
                    {
                        yield return (node as FancyText.Char).Delay;
                    }
                    else if (node is FancyText.Wait)
                    {
                        yield return (node as FancyText.Wait).Duration;
                    }
                    text.TextEnd++;
                    if (Input.MenuConfirm.Pressed)
                    {
                        text.TextEnd = fancyText.Count - 1;
                        break;
                    }
                }
                yield return 0.1f;
            }
            if (fancySubText != null)
            {

                while (text.SubEnd < fancySubText.Count)
                {
                    var node = fancySubText[text.SubEnd];
                    if (node is FancyText.Char)
                    {
                        yield return (node as FancyText.Char).Delay;
                    }
                    else if (node is FancyText.Wait)
                    {
                        yield return (node as FancyText.Wait).Duration;
                    }
                    text.SubEnd++;
                    if (Input.MenuConfirm.Pressed)
                    {
                        text.SubEnd = fancySubText.Count - 1;
                        break;
                    }
                }
            }
            yield return 0.6f;
            while (!Input.MenuConfirm.Pressed)
            {
                yield return null;
            }
            End(player);
        }
    }
}
