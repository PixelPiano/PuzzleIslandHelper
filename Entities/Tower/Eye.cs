using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [CustomEntity("PuzzleIslandHelper/BackendTowerEye")]
    [Tracked]
    public class Eye : Entity
    {
        public static bool SpawnPortalOnAwake = true;
        [OnLoad]
        public static void Load()
        {
            SpawnPortalOnAwake = true;
        }
        public MemoryOrb Orb;
        private HeavenlyText.Data[] TextData;
        public Portal gate;
        private Coroutine testCoroutine;
        public Vector3 ModelScale = Vector3.One;
        public float Size;
        public FlagData PortalFlag;
        public enum Types
        {
            Backend,
            Transit
        }
        public Types Type;
        public Eye(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Type = data.Enum("type", Types.Transit);
            PortalFlag = data.Flag("portalFlag");
            string textData = data.Attr("textData");
            string[] array = textData.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            TextData = new HeavenlyText.Data[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                TextData[i] = new HeavenlyText.Data(array[i]);
            }
            Collider = new Hitbox(data.Width, data.Height);
            Size = Math.Min(data.Width, data.Height);
            testCoroutine = new Coroutine(false);
            Add(testCoroutine);
            KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.Q, () =>
            {
                if (!gate.StateTransitioning)
                {
                    gate.StateTransitioning = true;
                }
                else
                {
                    gate.State = (gate.State + 1) % 4;
                }
            });

            KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.S, () =>
            {
                Player player = Scene.GetPlayer();
                if (player != null)
                {
                    player.MoveToY(player.Y + debug);
                }
            });
            KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.A, () =>
            {
                debug--;
            });
            KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.D, () =>
            {
                debug++;
            });
        }
        private int debug;

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if ((Type == Types.Backend && !OrbFlags.BlueCollected) || (Type == Types.Transit && !OrbFlags.GreenCollected))
            {
                scene.Add(Orb = (Type is Types.Backend ? new BlueMemoryOrb(Center) : new GreenMemoryOrb(Center)));
            }
            int collected = 0;
            if (OrbFlags.BlueCollected) collected++;
            if (OrbFlags.GreenCollected) collected++;
            Portal.States state = OrbFlags.PortalDead ? Portal.States.Dead : collected == 2 ? Portal.States.TwoOrbs : collected == 1 ? Portal.States.OneOrb : Portal.States.Inactive;
            gate = new Portal(state, Center, Size);
            scene.Add(gate);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Orb != null)
            {
                Orb.Orb.ColorB = OrbColors.OffFill;
                Orb.Orb.EdgeColorB = OrbColors.OffEdge;
                Orb.Orb.CenterColorB = OrbColors.OffCenter;
                Orb.Orb.EdgeColorLerp = Orb.Orb.FillColorLerp = Orb.Orb.CenterColorLerp = 1;
            }
        }
        public IEnumerator CollapseIntoPortal()
        {
            yield return PianoUtils.Lerp(Ease.SineIn, 1.3f, f => { ModelScale = Vector3.One * f; }, true);
        }
        public IEnumerator AdvancePortalState()
        {
            gate.AutoAdvanceState = false;
            gate.StateTransitioning = true;
            while (gate.StateTransitioning) yield return null;
        }
        public IEnumerator RevealRoutine()
        {
            //t blocks begin to spin
            //lightning shoots out of tips, colliding in the center
            //portal appears
            //portal design:
            //triangle with colored circles at it's points
            //triangle has a hold punched through the middle of it with a black ring outlining the edge of the hole. A thin black ring with a larger radius is also visible.
            //parallel to each side of the triangle is a snaking stroke.
            /*
                        gate = new Portal(Center, Width);
                        Scene.Add(gate);
                        yield return gate.Intro();*/
            yield return null;
        }
        public IEnumerator HeavenlyVoicesRoutine(HeavenlyText.Data data)
        {
            HeavenlyText text = new HeavenlyText(data.Main, data.Red, data.Green, data.Blue);
            Scene.Add(text);
            while (!text.Finished)
            {
                yield return null;
            }
            yield return null;
        }
    }
}