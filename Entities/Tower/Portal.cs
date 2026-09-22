using Celeste.Mod.CommunalHelper.Utils;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [Tracked]
    public class Portal : Entity
    {
        public enum States
        {
            Inactive,
            OneOrb,
            TwoOrbs,
            Dead
        }
        public bool AutoAdvanceState = true;
        public const int StInactive = 0;
        public const int StOneOrb = 1;
        public const int StTwoOrb = 2;
        public const int StDead = 3;
        public const float MaxOrbRadius = 10;
        public const int MaxOrbCorners = 32;
        public const float RedOrbDetectRadius = 64;
        public StateMachine StateMachine;
        public readonly float Size;
        public readonly VertexPositionColor[] Vertices;
        public readonly VertexPositionColor[] OutlineVertices;
        public VertexOrb[] Capsules = new VertexOrb[3];
        public VertexOrb CenterHole, GlowOrb;
        public VertexLine[] CenterLines = new VertexLine[3];
        public SpikeVertexLine[] SpikeLines;
        public Vector2[] CornerOffsets = new Vector2[3];
        public VertexOffsetComponent[] TriOffsets = new VertexOffsetComponent[6];
        public VertexOffsetComponent[] OutlineOrbAlphaOffsets = new VertexOffsetComponent[MaxOrbCorners * 3];
        public VertexOffsetComponent[] OutlineAlphaOffsets = new VertexOffsetComponent[9];
        public VertexOffsetComponent FullAlphaOffset;
        public float[] TriangleLengthPercents = new float[3];
        public float OutlineLengthPercent;
        private bool[] capsulesFlickering = new bool[3];
        public Flicker GoldenFlicker;
        public Color Color = Color.White;
        public Vector2 Scale = Vector2.One;
        public Vector2 CenterOffset = Vector2.Zero;

        public float Rotation = -MathF.PI / 2;
        public float RotationRate;
        public float RotationRateDegrees
        {
            get => RotationRate.ToDeg();
            set => RotationRate = value.ToRad();
        }
        public float TriOffsetMult;
        private bool afterImagesActive;
        private SpikeVertexLine testLine;
        public int State
        {
            get => StateMachine.State;
            set => StateMachine.State = value;
        }
        private float flickerDelayTimer;
        private bool doFlicker;
        private List<VertexOrb.AfterImage> afterImages = [];
        private bool renderAfterImages = true;
        private float flickerAlpha = 1;
        private float flickerOffTimer;
        public bool AttractsRedOrb;
        private float ratePauseTimer;
        public float RedOrbFillRadiusOffset;
        public bool StateTransitioning = false;
        public float FlickerMult;
        public bool JitterEnabled;
        public bool AlreadyAwake;
        private States startState;
        public Portal(States startState, Vector2 center, float size) : base(center)
        {
            Depth = 5;
            Collider = new Hitbox(size, size, -size / 2, -size / 2);
            Collidable = false;
            Vertices = new VertexPositionColor[9];
            OutlineVertices = new VertexPositionColor[12];
            Size = size;
            Add(StateMachine = new StateMachine());
            StateMachine.SetCallbacks(StInactive, InactiveUpdate, InactiveTransition, InactiveBegin, InactiveEnd);
            StateMachine.SetCallbacks(StOneOrb, OneOrbUpdate, OneOrbTransition, OneOrbBegin, OneOrbEnd);
            StateMachine.SetCallbacks(StTwoOrb, TwoOrbUpdate, TwoOrbTransition, TwoOrbBegin, TwoOrbEnd);
            StateMachine.SetCallbacks(StDead, DeadUpdate, null, DeadBegin, DeadEnd);
            this.startState = startState;
            KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.T, () =>
            {
                State = StTwoOrb;
                if (Scene.Tracker.GetEntity<PortalAttackCutscene>() is PortalAttackCutscene entity)
                {
                    entity.RemoveSelf();
                }
                else
                {
                    Scene.Add(new PortalAttackCutscene(this, true));
                }
            });
        }
        public void InactiveBegin()
        {
            StateTransitioning = false;
            PrepareGeneralComponents(true, true, true, true, false);
        }
        public int InactiveUpdate()
        {
            return StInactive;
        }
        public void InactiveEnd()
        {
            StateTransitioning = false;
        }
        public IEnumerator InactiveTransition()
        {
            while (!StateTransitioning) yield return null;
            ParticleSystem particlesBG = SceneAs<Level>().ParticlesBG;
            yield return new SwapImmediately(CenterHoleForms(0.8f, 6.5f, Ease.CubeIn, 4, Color.White, Color.Black));
            yield return new SwapImmediately(GlowOrbForms(0.55f, 8 + 0.5f, 0.6f));
            yield return new SwapImmediately(GlowOrbShrinks(0.6f, Ease.CubeOut));
            for (int j = 0; j < 3; j++)
            {
                capsulesFlickering[j] = true;
                Capsules[j].Visible = true;
                Capsules[j].ColorB = Capsules[j].EdgeColorB = Color.Transparent;
                Capsules[j].Color = Color.White;
                Capsules[j].FillColorLerp = Capsules[j].EdgeColorLerp = 1;
                Add(new Coroutine(Capsules[j].GrowReveal(0.6f, 0, MaxOrbRadius, MaxOrbCorners)));
            }
            yield return new SwapImmediately(LinesToOrbs(0.6f, Ease.CubeInOut));
            yield return 1f;
            StartFlicker();
            Add(new Coroutine(OutlineLineRoutine(2f, Ease.ExpoOut)));
            for (int i = 0; i < 3; i++)
            {
                OrbTurnsOn(i, particlesBG);
                yield return 0.3f;
            }
            yield return 0.7f;
            StopTBlocksFiring();
            yield return 1.2f;
            yield return new SwapImmediately(LerpOutlineOrbColors(2, Ease.CubeOut));

            if (Scene.Tracker.GetEntity<BlueMemoryOrb>() is BlueMemoryOrb orb && orb.State != MemoryOrb.StPortal)
            {
                orb.Orb.FadeAllShivers(1);
                orb.MoveToPortal(this);
                while (orb.State != MemoryOrb.StPortal) yield return null;
            }
            else if (Scene.Tracker.GetEntity<GreenMemoryOrb>() is GreenMemoryOrb orb2 && orb2.State != MemoryOrb.StPortal)
            {
                orb2.Orb.FadeAllShivers(1);
                orb2.MoveToPortal(this);
                while (orb2.State != MemoryOrb.StPortal) yield return null;
            }
            StateTransitioning = false;
            State = StOneOrb;
        }
        public void OneOrbBegin()
        {
            AttractsRedOrb = true;
            StateTransitioning = false;
            PrepareGeneralComponents(false, true, false, false, false);
            //also figure out what's making the baba is you squiggle effect cause it's cool but it doesn't activate from this function for some reason (maybe the orb's rotation rate? debug check that too.)
            foreach (var o in Capsules)
            {
                o.Visible = true;
                o.Radius = MaxOrbRadius;
                o.Color = Color.Transparent;
                o.EdgeColor = Color.White;
                o.FillColorLerp = o.EdgeColorLerp = o.CenterColorLerp = 0;
                o.EdgeAlpha = 1;
                o.Corners = MaxOrbCorners;
            }
            for (int i = 0; i < OutlineVertices.Length; i++)
            {
                OutlineVertices[i].Color = Color.White;
            }
            OutlineLengthPercent = 1;
            if (!doFlicker) StartFlicker();

            if (OrbFlags.BlueCollected) EnsureOrbIsAttaching(OrbColors.BFill, OrbColors.BCenter, OrbColors.BEdge, () => new BlueMemoryOrb(Vector2.Zero));
            else EnsureOrbIsAttaching(OrbColors.GFill, OrbColors.GCenter, OrbColors.GEdge, () => new GreenMemoryOrb(Vector2.Zero));


            UpdateVertices();
        }
        public void EnsureOrbIsAttaching<T>(Color fill, Color center, Color edge, Func<T> factory) where T : MemoryOrb
        {
            T orb = Scene.Tracker.GetEntity<T>();
            if (orb == null)
            {
                Scene.Add(orb = factory.Invoke());
            }
            orb.AttachToPortal(this);
            orb.CenterColor = center;
            orb.Color = fill;
            orb.EdgeColor = edge;
            orb.Orb.FillColorLerp = orb.Orb.EdgeColorLerp = orb.Orb.CenterColorLerp = 0;
            orb.Orb.Visible = true;

        }
        public int OneOrbUpdate()
        {
            return StOneOrb;
        }
        public void OneOrbEnd()
        {
            StateTransitioning = false;
        }
        public IEnumerator OneOrbTransition()
        {
            while (!StateTransitioning) yield return null;
            MemoryOrb orb;
            if (OrbFlags.BlueCollected)
            {
                orb = Scene.Tracker.GetEntity<GreenMemoryOrb>();
            }
            else
            {
                orb = Scene.Tracker.GetEntity<BlueMemoryOrb>();
            }
            orb.Orb.FadeAllShivers(1);
            orb.MoveToPortal(this);
            while (orb.State != MemoryOrb.StPortal) yield return null;
            ParticleSystem particlesBG = SceneAs<Level>().ParticlesBG;
            yield return 1f;
            for (int i = 0; i < 3; i++)
            {
                Add(new Coroutine(TwoOrbTriangleColorRoutine(i, 0.5f, 2, 1, 0.5f, Ease.ExpoOut, Ease.SineInOut)));
                SpikeVertexLineTurnsOn();
                yield return 0.3f;
            }
            yield return 0.7f;
            BeginTriVertexOffsetTween(1, Ease.SineInOut);
            StopTBlocksFiring();

            for (int i = 0; i < 3; i++)
            {
                Add(new Coroutine(SpikeVertexLineRelaxTo(i, 0.5f, 1.2f, Ease.BackIn)));
                //todo: add logic to snap function for spike vertex lines
            }
            yield return 1.2f;
            StateTransitioning = false;
            yield return null;
            if (AutoAdvanceState) State = StTwoOrb;
        }
        public void TwoOrbBegin()
        {
            AttractsRedOrb = true;
            StateTransitioning = false;
            PrepareGeneralComponents(true, false, false, false, true);
            Collidable = true;
            foreach (var o in Capsules)
            {
                o.Visible = true;
                o.Radius = MaxOrbRadius;
                o.Corners = MaxOrbCorners;
                o.FillColorLerp = o.EdgeColorLerp = o.CenterColorLerp = 0;
            }
            Color[] baseColors = [Color.Red, Color.Lime, Color.Blue];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Vertices[i * 3 + j].Color = baseColors[i];
                }
                TriangleLengthPercents[i] = 1;
            }
            TriOffsetMult = 1;
            RotationRateDegrees = 2;
            /*            if (!AlreadyAwake)
                        {*/
            EnsureOrbIsAttaching(OrbColors.BFill, OrbColors.BCenter, OrbColors.BEdge, () => new BlueMemoryOrb(Vector2.Zero));
            EnsureOrbIsAttaching(OrbColors.GFill, OrbColors.GCenter, OrbColors.GEdge, () => new GreenMemoryOrb(Vector2.Zero));
            //}
            UpdateVertices();
        }
        public int TwoOrbUpdate()
        {
            /*            if(CollideFirst<Player>() is Player player && !SceneAs<Level>().InCutscene && player.StateMachine.State == Player.StNormal)
                        {
                            Scene.Add(new PortalAttackCutscene(this, true));
                        }*/
            return StTwoOrb;
        }
        public void TwoOrbEnd()
        {
            StateTransitioning = false;
        }

        private IEnumerator TwoOrbTransition()
        {
            while (!StateTransitioning) yield return null;
            float target = MathHelper.PiOver2 * 3;
            float start = MathHelper.PiOver2;
            while (Rotation > start) yield return null;
            while (Rotation < start) yield return null;
            Rotation = start;
            float orig = RotationRate;
            while (RotationRate > 0.0001f)
            {
                RotationRate = orig * ((1 - (Rotation - start) / MathHelper.Pi));
                yield return null;
            }
            RotationRate = 0;
            Rotation = target;
            yield return null;
            StateTransitioning = false;
            yield return null;
            if (AutoAdvanceState) State = StDead;
        }
        public void DeadBegin()
        {
            StateTransitioning = false;
        }
        public int DeadUpdate()
        {
            return StDead;
        }
        public void DeadEnd()
        {
            StateTransitioning = false;
        }
        public override void Added(Scene scene)
        {
            void randomizeInterval(VertexOffsetComponent v)
            {
                v.Interval = Calc.Random.Range(0.3f, 0.8f);
            }
            for (int i = 0; i < 3; i++)
            {
                Capsules[i] = new(Vector2.Zero, 0, 0, Color.White) { Visible = false, EdgeAlpha = 0, ResetOnRemoved = true };
                int index = i;
                Capsules[i].CustomGetEdgeColor.Add((c, i2) => { return GetOutlineOrbEdgeColor(c, i2, index); });
                CenterLines[i] = new VertexLine(CenterOffset, Vector2.Zero);
            }
            CenterHole = new VertexOrb(CenterOffset, 0, 0, Color.Black, Color.Red, 0) { Visible = false, WobbleMult = 3 };
            GlowOrb = new VertexOrb(CenterOffset, 0, 0, Color.Yellow, Color.Orange, 0) { Visible = false };
            GoldenFlicker = new Flicker(0.07f, 0, false, [Color.Gold, Color.DarkGoldenrod, Color.Gold, Color.Goldenrod]);
            for (int i = 0; i < 6; i++)
            {
                TriOffsets[i] = new VertexOffsetComponent(4, -4, 0.4f, VertexOffsetComponent.ApproachModes.RubberbandApproach, true, randomizeInterval);
            }
            for (int i = 0; i < OutlineOrbAlphaOffsets.Length; i++)
            {
                OutlineOrbAlphaOffsets[i] = new VertexOffsetComponent(-0.2f, -0.3f, 0.4f, VertexOffsetComponent.ApproachModes.RubberbandApproach, true, randomizeInterval) { WaitForTimer = false, RubberbandFactorMult = 10 };
            }
            for (int i = 0; i < OutlineAlphaOffsets.Length; i++)
            {
                OutlineAlphaOffsets[i] = new VertexOffsetComponent(-0.3f, -0.4f, 0.4f, VertexOffsetComponent.ApproachModes.RubberbandApproach, true, randomizeInterval) { WaitForTimer = false, RubberbandFactorMult = 10 };
            }
            FullAlphaOffset = new(-0.4f, -0.1f, 0.4f, VertexOffsetComponent.ApproachModes.LerpApproach, false,
                (v) =>
                {
                    v.Interval = Calc.Random.Range(0.1f, 0.4f);
                    v.LerpEase = Ease.CubeInOut;
                    v.ConstantApproachSpeed = Calc.Random.Range(20, 40);
                });

            Add(CenterHole);
            Add(CenterLines);
            Add(Capsules);
            Add(GlowOrb);
            Add(TriOffsets);
            Add(OutlineAlphaOffsets);
            Add(OutlineOrbAlphaOffsets);
            Add(FullAlphaOffset);
            Add(GoldenFlicker);
            base.Added(scene);
        }
        private float[] capsuleColorLerps = new float[3];
        private readonly Color[] capsuleAltColors = [Color.Red, Color.Lime, Color.Blue];

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            State = (int)startState;
            AlreadyAwake = true;//Used to determine if any MemoryOrbs need to be added to the scene based on the Portal's current state.
        }
        public override void Update()
        {
            RedMemoryOrb red = Scene.Tracker.GetEntity<RedMemoryOrb>();
            GreenMemoryOrb green = Scene.Tracker.GetEntity<GreenMemoryOrb>();
            BlueMemoryOrb blue = Scene.Tracker.GetEntity<BlueMemoryOrb>();
            capsuleColorLerps[0] = Calc.Approach(capsuleColorLerps[0], red == null || red.State != MemoryOrb.StPortal ? 0 : 1, Engine.DeltaTime);
            capsuleColorLerps[1] = Calc.Approach(capsuleColorLerps[1], green == null || green.State != MemoryOrb.StPortal ? 0 : 1, Engine.DeltaTime);
            capsuleColorLerps[2] = Calc.Approach(capsuleColorLerps[2], blue == null || blue.State != MemoryOrb.StPortal ? 0 : 1, Engine.DeltaTime);
            if (doFlicker)
            {
                if (FlickerMult != 1)
                {
                    FlickerMult = Calc.Approach(FlickerMult, 1, Engine.DeltaTime);
                }
                if (flickerOffTimer >= 0)
                {
                    flickerAlpha = 1;
                    flickerOffTimer -= Engine.DeltaTime;
                    if (flickerOffTimer <= 0)
                    {
                        flickerDelayTimer = Calc.Random.Range(0.1f, 1f);
                    }
                }
                else if (flickerDelayTimer >= 0)
                {
                    if (Scene.OnInterval(0.05f) && JitterEnabled)
                    {
                        flickerAlpha = Calc.Random.Range(0.5f, 0.9f);
                        Rotation += ((float)Calc.Random.Range(3, 7)).ToRad() * Calc.Random.Sign();
                        ratePauseTimer = 0.05f;
                    }
                    flickerDelayTimer -= Engine.DeltaTime;
                    if (flickerDelayTimer <= 0)
                    {
                        flickerOffTimer = Calc.Random.Range(0, 8f);
                    }
                }
            }
            else
            {
                flickerOffTimer = 0;
                flickerDelayTimer = 0;
                flickerAlpha = 1;
                if (FlickerMult != 0)
                {
                    FlickerMult = Calc.Approach(FlickerMult, 0, Engine.DeltaTime * 2);
                }
            }
            if (ratePauseTimer > 0)
            {
                ratePauseTimer -= Engine.DeltaTime;
            }
            if (ratePauseTimer <= 0)
            {
                Rotation = (Rotation + RotationRate) % MathHelper.TwoPi;
                for (int i = 0; i < 3; i++)
                {
                    //RGBOrbs[i].Rotation = (RGBOrbs[i].Rotation + RotationRate) % MathHelper.TwoPi;
                    Capsules[i].Rotation = (Capsules[i].Rotation + RotationRate) % MathHelper.TwoPi;
                    //RGBOrbs[i].RotationRate = 0;
                }
            }
            if (CenterHole.Visible && CenterHole.Active)
            {
                CenterHole.EdgeRotationOffset = (CenterHole.EdgeRotationOffset + 0.5f * (MathF.PI / 180f)) % MathHelper.TwoPi;
            }
            if (afterImagesActive && Scene.OnInterval(0.1f))
            {
                for (int i = 0; i < 3; i++)
                {
                    var oB = Capsules[i];
                    if (oB.Visible && oB.Active)
                    {
                        CreateOrbAfterImage(oB);
                    }
                }
            }
            UpdateVertices();
            base.Update();
        }
        [Tracked]
        public class PortalHookComponent : Component
        {
            public Action<Portal> OnUpdateVertices;
            public Action<Portal> OnRender;
            public Action<Portal> OnRenderAfterImages;
            public PortalHookComponent(Action<Portal> onUpdateVertices, Action<Portal> onRender, Action<Portal> onRenderAfterImages) : base(true, false)
            {
                OnUpdateVertices = onUpdateVertices;
                OnRender = onRender;
                OnRenderAfterImages = onRenderAfterImages;
            }
        }
        public override void Render()
        {
            base.Render();
            if (Size > 0 && !(State == StInactive && !StateTransitioning))
            {
                Draw.SpriteBatch.End();
                PianoUtils.DrawUserPrimitives<VertexPositionColor>(SceneAs<Level>().Camera.Matrix, DirectlyRenderVertices);
                GameplayRenderer.Begin();
            }
        }
        public override void DebugRender(Camera camera)
        {
            foreach (var o in Capsules)
            {
                if (o != null)
                {
                    Draw.Line(o.RenderPosition, Center, Color.Yellow);
                }
            }

            if (StateTransitioning) Draw.HollowRect(Collider, Color.Cyan);
            else Draw.HollowRect(Collider, Collidable ? Color.Red : Color.DarkRed);
        }
        internal void DirectlyRenderVertices(EffectPass pass)
        {
            if (renderAfterImages)
            {
                foreach (VertexOrb.AfterImage afterImage in afterImages)
                {
                    afterImage.DirectRenderVertices();
                }
                foreach (PortalHookComponent c in Scene.Tracker.GetComponents<PortalHookComponent>())
                {
                    c.OnRenderAfterImages?.Invoke(this);
                }
            }
            if (OutlineLengthPercent > 0)
            {
                Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, OutlineVertices, 0, 6);
            }
            var b = Engine.Graphics.GraphicsDevice.BlendState;
            Engine.Graphics.GraphicsDevice.BlendState = BlendState.Additive;
            Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, Vertices, 0, 3);
            Engine.Graphics.GraphicsDevice.BlendState = b;
            if (CenterHole.Visible && CenterHole.Active) CenterHole.DirectRenderVertices();
            foreach (var a in CenterLines)
            {
                if (a.Visible && a.Active) a.DirectRenderVertices();
            }
            if (GlowOrb.Visible && GlowOrb.Active) GlowOrb.DirectRenderVertices();
            foreach (VertexOrb orb in Capsules)
            {
                if (orb.Visible && orb.Active) orb.DirectRenderVertices();
            }
            foreach (PortalHookComponent c in Scene.Tracker.GetComponents<PortalHookComponent>())
            {
                c.OnRender?.Invoke(this);
            }
        }
        public void PrepareGeneralComponents(bool stopFlicker, bool hideTriangles, bool hideOutlines, bool hideCapsules, bool spawnAfterImages)
        {
            afterImagesActive = spawnAfterImages;
            Components.RemoveAll<Tween>();
            ResetCenterLines(false);
            // CenterHole.Reset(false);
            GlowOrb.Reset(false);
            for (int i = 0; i < 3; i++)
            {
                capsulesFlickering[i] = false;
            }
            if (hideTriangles)
            {
                for (int i = 0; i < 3; i++)
                {
                    TriangleLengthPercents[i] = 0;
                }
                for (int i = 0; i < Vertices.Length; i++)
                    Vertices[i].Color = Color.Transparent;
            }
            if (hideOutlines)
            {
                OutlineLengthPercent = 0;
                for (int i = 0; i < OutlineVertices.Length; i++)
                    OutlineVertices[i].Color = Color.Transparent;
            }
            if (stopFlicker)
            {
                StopFlicker();
            }
            if (hideCapsules)
            {
                ResetCapsules(false);
            }
        }

        public void UpdateVertices()
        {
            float angle = Rotation;
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = CornerOffsets[i] = Calc.AngleToVector(MathHelper.TwoPi / 3 * i + angle, Size / 2);
                CenterLines[i].To = offset * Scale;
                Capsules[i].Position = offset * Scale;
            }
            int count = 0;
            float mult = doFlicker ? FlickerMult : 1;
            for (int i = 0; i < 12; i++)
            {
                float alpha = 1;
                if (doFlicker)
                {
                    if (int.IsEvenInteger(i))
                    {
                        alpha += OutlineAlphaOffsets[count + 3].FloatOffset;
                        count++;
                    }
                    else
                    {
                        alpha += OutlineAlphaOffsets[i / 4].FloatOffset;
                    }
                    alpha *= (1 + FullAlphaOffset.FloatOffset);
                }
                alpha *= flickerAlpha;
                OutlineVertices[i].Color = Color.White * alpha * mult;
            }
            Vector2 targetB = CornerOffsets[2];
            for (int i = 0; i < 3; i++)
            {
                float radiusA = Capsules[i].Radius;
                Vector2 from = CornerOffsets[i];
                Vector2 targetA = CornerOffsets[(i + 1) % 3];
                float percent = OutlineLengthPercent;
                int baseIndex = i * 4;
                OutlineVertices[baseIndex].Position = (Position + Calc.Approach(from, targetA, radiusA) * Scale).ToVec3();
                OutlineVertices[baseIndex + 1].Position = (Position + Vector2.Lerp(from, targetA, percent / 2) * Scale).ToVec3();
                OutlineVertices[baseIndex + 2].Position = (Position + Calc.Approach(from, targetB, radiusA) * Scale).ToVec3();
                OutlineVertices[baseIndex + 3].Position = (Position + Vector2.Lerp(from, targetB, percent / 2) * Scale).ToVec3();
                targetB = from;
            }
            for (int i = 0; i < 3; i++)
            {
                Vertices[i * 3].Position = (Position + CornerOffsets[i] * Scale).ToVec3();
            }
            for (int i = 0; i < 3; i++)
            {
                float triSize = Size * (0.2f + 0.8f * TriangleLengthPercents[i]);
                int baseIndex = i * 3;
                for (int j = 1; j < 3; j++)
                {
                    Vertices[baseIndex + j].Position = Calc.Approach(Vertices[baseIndex].Position, Vertices[(baseIndex + j * 3) % 9].Position, triSize);

                    if (TriOffsetMult > 0)
                    {
                        Vertices[baseIndex + j].Position += new Vector3(TriOffsets[i * 2 + (j - 1)].Vec2Offset, 0) * TriOffsetMult;
                    }
                }
            }
            if (GoldenFlicker.Active)
            {
                foreach (VertexLine line in CenterLines)
                {
                    line.Color = GoldenFlicker.Color;
                }
                GlowOrb.Color = GoldenFlicker.Color;
                for (int i = 0; i < 3; i++)
                {
                    if (capsulesFlickering[i])
                    {
                        Capsules[i].ColorB = GoldenFlicker.Color;
                        Capsules[i].EdgeColorB = GoldenFlicker.GetColor(GoldenFlicker.Index + 1);
                        Capsules[i].EdgeAlpha = Calc.Approach(Capsules[i].EdgeAlpha, 1, Engine.DeltaTime * 2);
                    }
                }
            }
            foreach (PortalHookComponent c in Scene.Tracker.GetComponents<PortalHookComponent>())
            {
                c.OnUpdateVertices?.Invoke(this);
            }
        }
        public void ResetCenterLines(bool visible)
        {
            foreach (var line in CenterLines)
            {
                line.Reset(visible);
            }
        }
        public void ResetCapsules(bool visible)
        {
            foreach (var capsule in Capsules)
            {
                capsule.Reset(visible);
            }
        }
        public IEnumerator InactiveToOneOrbRotationBegin()
        {
            yield return new SwapImmediately(StartRotation(MathHelper.PiOver4 / 2, 5f, 0, 2, 2, Ease.QuintIn, 1, 0.05f));
        }
        public void CreateOrbAfterImage(VertexOrb orb)
        {
            VertexOrb.AfterImage afterImage = new(orb, 0.5f, 0.5f, 1, afterImages);
            afterImage.FillAlpha *= 0.2f;
            afterImage.EdgeAlpha *= 0.6f;
            Add(afterImage);
        }
        public Color GetOutlineOrbEdgeColor(Color baseColor, int cornerIndex, int orbIndex)
        {
            float alpha = Capsules[orbIndex].EdgeAlpha;
            alpha *= (1 + OutlineOrbAlphaOffsets[orbIndex * MaxOrbCorners + cornerIndex % MaxOrbCorners].FloatOffset * flickerAlpha * FlickerMult);
            alpha *= (1 + FullAlphaOffset.FloatOffset * flickerAlpha * FlickerMult);
            baseColor = Color.Lerp(baseColor, capsuleAltColors[orbIndex], capsuleColorLerps[orbIndex]);
            return baseColor * alpha;
            /*            try
                        {
                            VertexOrb capsule = Capsules[orbIndex];
                            float alpha = capsule.EdgeAlpha;
                            alpha *= (1 + OutlineOrbAlphaOffsets[orbIndex * MaxCorners + cornerIndex % capsule.Corners].FloatOffset * flickerAlpha * FlickerMult);
                            alpha *= (1 + FullAlphaOffset.FloatOffset * flickerAlpha * FlickerMult);
                            baseColor = Color.Lerp(baseColor, capsuleAltColors[orbIndex], capsuleColorLerps[orbIndex]);
                            return baseColor * alpha;
                        }
                        catch (IndexOutOfRangeException exception)
                        {
                            string output = $"Orb Index: {orbIndex}\n OutlineOrbAlphaOffsets Index: {orbIndex * MaxCorners + cornerIndex % MaxCorners}"
                            Logger.Log("PuzzleIslandHelper.Portal",)
                            throw exception;
                        }*/
        }
        public void StartFlicker()
        {
            doFlicker = true;
            FullAlphaOffset.Start();
            foreach (var c in OutlineOrbAlphaOffsets) c.Start();
            foreach (var c in OutlineAlphaOffsets) c.Start();
        }
        public void StopFlicker()
        {
            doFlicker = false;
            foreach (VertexOffsetComponent c in OutlineOrbAlphaOffsets)
                c?.Reset();
            foreach (VertexOffsetComponent c in OutlineAlphaOffsets)
                c?.Reset();
            FullAlphaOffset?.Reset();

        }
        public IEnumerator SpikeVertexLineRelaxTo(int index, float mult, float time, Ease.Easer ease)
        {
            yield return null;
            //todo: this
        }
        public void SpikeVertexLineTurnsOn()
        {
            //todo: this
        }
        public IEnumerator CenterHoleForms(float time, float radius, Ease.Easer ease, float edgeRadiusOffset, Color color, Color edgeColor)
        {
            CenterHole.Visible = true;
            CenterHole.EdgeRadiusOffset = edgeRadiusOffset;
            CenterHole.Corners = MaxOrbCorners;
            CenterHole.Color = color;
            CenterHole.EdgeColor = edgeColor;
            yield return PianoUtils.Lerp(ease, time, f => CenterHole.Radius = f * radius, true);
        }
        public IEnumerator GlowOrbForms(float duration, float radius, float endDelay)
        {
            GlowOrb.Visible = true;
            GlowOrb.Corners = MaxOrbCorners;
            GoldenFlicker.Start();
            yield return PianoUtils.Lerp(Ease.CubeOut, duration, f => GlowOrb.Radius = f * radius, true);
            yield return endDelay;
            for (int i = 0; i < 3; i++)
            {
                CenterLines[i].Visible = true;
                CenterLines[i].ToFromLerp = 1;
                CenterLines[i].FromToLerp = 0;
            }
        }
        public IEnumerator GlowOrbShrinks(float duration, Ease.Easer ease)
        {
            float r = GlowOrb.Radius;
            for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
            {
                float eased = ease(i);
                GlowOrb.Radius = Calc.LerpClamp(r, 0, eased);
                for (int j = 0; j < 3; j++)
                {
                    CenterLines[j].ToFromLerp = 1 - eased;
                }
                yield return null;
            }
        }
        public IEnumerator LinesToOrbs(float duration, Ease.Easer ease)
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
            {
                float eased = ease(i);
                for (int j = 0; j < 3; j++)
                {
                    CenterLines[j].FromToLerp = eased;
                }
                yield return null;
            }
        }
        private IEnumerator OutlineLineRoutine(float time, Ease.Easer ease)
        {
            yield return PianoUtils.Lerp(ease, time, f =>
            {
                for (int i = 0; i < OutlineVertices.Length; i++)
                {
                    OutlineVertices[i].Color = Color.White * f;
                }
                OutlineLengthPercent = f;

            }, true);
        }
        private IEnumerator TwoOrbTriangleColorRoutine(int i, float timeIn, float timeOut, float endAlpha, float wait, Ease.Easer easeIn, Ease.Easer easeOut)
        {

            //Ease.ExpoOut, Ease.SineInOut
            //0.3, 2, 0.34, 0.5
            Color baseColor = i switch
            {
                0 => Color.Red,
                1 => Color.Lime,
                _ => Color.Blue
            };
            yield return PianoUtils.Lerp(easeIn, timeIn, f =>
            {
                for (int j = 0; j < 3; j++)
                {
                    Vertices[i * 3 + j].Color = Color.White * f;
                }
            }, true);
            yield return wait;
            yield return PianoUtils.Lerp(easeOut, timeOut, f =>
            {
                for (int j = 0; j < 3; j++)
                {
                    Vertices[i * 3 + j].Color = Color.Lerp(Color.White, baseColor * endAlpha, f);
                }
                TriangleLengthPercents[i] = f;
            }, true);
        }
        public IEnumerator LerpOutlineOrbColors(float time, Ease.Easer ease)
        {
            for (int i = 0; i < 3; i++)
            {
                Capsules[i].Color = Color.Transparent;
                Capsules[i].EdgeColor = Color.White;
            }
            yield return PianoUtils.Lerp(ease, time, f =>
            {
                for (int j = 0; j < 3; j++)
                {
                    Capsules[j].FillColorLerp = Capsules[j].EdgeColorLerp = 1 - f;
                    Capsules[j].EdgeColor = Color.White;
                }
            }, true);
        }
        public void StopTBlocksFiring()
        {
            foreach (TBlock3D block in Scene.Tracker.GetEntities<TBlock3D>())
            {
                block.BeamActive = false;
            }
        }
        public IEnumerator JitterRotation(float offset, float interval)
        {
            float og = Rotation;
            Rotation += offset;
            yield return interval;
            offset *= -1;
            for (int i = 0; i < 3; i++)
            {
                Rotation += offset;
                yield return interval;
                offset *= -1;
            }
            Rotation = og;
        }
        public IEnumerator StartRotation(float joltOffset, float rotationSpeed, float wait, int endDegrees, float endDuration, Ease.Easer endEase, float jitterDegrees, float jitterInterval)
        {
            float jitterRad = jitterDegrees * MathF.PI / 180f;
            float prevRotation = Rotation;
            float speed = 0;
            float targetJoltRotation = prevRotation - joltOffset;
            if (jitterRad != 0 && jitterInterval != 0) yield return new SwapImmediately(JitterRotation(jitterRad, jitterInterval));
            while (Rotation != targetJoltRotation)
            {
                Rotation = Calc.Approach(Rotation, targetJoltRotation, rotationSpeed * Engine.DeltaTime);
                yield return null;
            }

            if (jitterRad != 0 && jitterInterval != 0) yield return new SwapImmediately(JitterRotation(jitterRad * 1.2f, jitterInterval * 0.5f));

            /*           while (Rotation != prevRotation)
                       {
                           Rotation = Calc.Approach(Rotation, prevRotation, rotationSpeed * Engine.DeltaTime);
                           yield return null;
                       }
                       if (jitterRad != 0 && jitterInterval != 0) yield return new SwapImmediately(JitterRotation(jitterRad, jitterInterval));
           */
            Rotation = prevRotation;
            afterImagesActive = true;
            if (wait > 0) yield return wait;
            for (float i = 0; i < 1; i += Engine.DeltaTime / endDuration)
            {
                float eased = endEase(i);
                RotationRateDegrees = Calc.LerpClamp(0, endDegrees, eased);
                yield return null;
            }
            RotationRateDegrees = endDegrees;
        }
        public void OrbTurnsOn(int i, ParticleSystem ps)
        {
            EventInstance instance = Audio.Play("event:/PianoBoy/invertGlitch2");
            instance.setVolume(Calc.Random.Range(0.2f, 0.4f));
            instance.getPitch(out float pitch, out float finalpitch);
            instance.setPitch(pitch + 12);
            capsulesFlickering[i] = false;
            //do particles and also sceen effects here maybe
            //temp particles
            //todo: make these thematically nice
            VertexOrb[] orbs = Capsules;
            orbs[i].ColorB = orbs[i].EdgeColorB = Color.White;
            orbs[i].EdgeColorLerp = orbs[i].FillColorLerp = 1;
            for (int a = 0; a < 360; a += 30)
            {
                ps.Emit(Booster.P_RedAppear, 1, orbs[i].RenderPosition, Vector2.One * 2f, (float)a * (MathF.PI / 180f));
            }
        }
        public void BeginTriVertexOffsetTween(float inDuration, Ease.Easer ease)
        {
            Tween.Set(this, Tween.TweenMode.Oneshot, inDuration, ease, t => TriOffsetMult = t.Eased, t => TriOffsetMult = 1);
        }
        public class SpikeVertexLine : GraphicsComponent
        {
            public VertexPositionColor[] Vertices;
            public class Data
            {
                public float SinTimeOffset;
                public Vector2 Offset;
                public float Target;
            }
            public float Angle;
            public float Length;
            public int Spikes;
            public float MaxHeight;
            private SineWave wave;
            public List<Data> WobbleData = [];
            public double FactorMult = 1;
            public float CenterOffsetAmount;
            public bool MethodA;
            public SpikeVertexLine(Vector2 position, float angle, float length, float maxHeight, int spikes) : base(true)
            {
                wave = new SineWave(2);
                wave.StartUp();
                Position = position;
                Angle = angle;
                Length = length;
                Spikes = spikes;
                MaxHeight = maxHeight;
                Vertices = new VertexPositionColor[Spikes + 2];
                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vertices[i].Color = Color.White;
                }
            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                for (int i = 0; i < Spikes; i++)
                {
                    var d = new Data();
                    d.SinTimeOffset = Calc.Random.Range(0, 1f);
                    WobbleData.Add(d);
                }

                UpdateVertices();
            }
            public override void Update()
            {
                base.Update();
                wave.Update();
                UpdateVertices();
            }
            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                Vector2 prev = Vertices[0].Position.XY();
                for (int i = 1; i < Vertices.Length; i++)
                {
                    Vector2 current = Vertices[i].Position.XY();
                    Draw.Line(prev, current, Color.White * 0.5f);
                    prev = current;
                }
            }
            public void DrawLines()
            {
                Matrix matrix = SceneAs<Level>().Camera.Matrix;
                PianoUtils.DrawUserPrimitives(PrimitiveType.LineStrip, matrix, Vertices, Spikes + 2);
            }
            public void DirectRenderVertices()
            {
                if (CenterOffsetAmount < Spikes / 2)
                {
                    Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, Vertices, 0, Spikes);
                }
            }
            public void UpdateVertices()
            {
                Vector2 lineAngleOffset = Calc.AngleToVector(Angle, Length / 2);
                Vector2 from = RenderPosition + lineAngleOffset;
                Vector2 to = RenderPosition - lineAngleOffset;

                Vertices[0].Position = from.ToVec3();
                Vertices[^1].Position = to.ToVec3();


                int dir = 1;
                double factor = 0.0099999997764825821 * FactorMult;
                for (int i = 0; i < Spikes; i++)
                {
                    float sin = (wave.ValueOffset(WobbleData[i].SinTimeOffset) + 1) / 2f;
                    float percent = Ease.SineInOut((float)i / Spikes);
                    float amp = (percent > 0.5f ? 0.5f - (percent - 0.5f) : percent) / 0.5f;
                    if (MethodA)
                    {
                        float length = (amp / 2) * MaxHeight + (amp / 2) * sin * MaxHeight;
                        Vector2 pointAngleOffset = Calc.AngleToVector(Angle, length * dir);
                        WobbleData[i].Offset = PianoUtils.RubberbandApproach(WobbleData[i].Offset, pointAngleOffset, 0.05f, factor);
                        Vertices[i + 1].Position = (Vector2.Lerp(from, to, percent) + WobbleData[i].Offset).ToVec3();
                        dir *= -1;
                    }
                    else
                    {
                        Vector2 pointAngleOffset = Calc.AngleToVector(Angle, WobbleData[i].Target);
                        WobbleData[i].Offset = PianoUtils.RubberbandApproach(WobbleData[i].Offset, pointAngleOffset, 0.05f, factor);
                        if (WobbleData[i].Offset == pointAngleOffset)
                        {
                            WobbleData[i].Target = amp * Calc.Random.Range(MaxHeight / 2, MaxHeight) * -Math.Sign(WobbleData[i].Target);
                        }
                        Vertices[i + 1].Position = (Vector2.Lerp(from, to, percent) + WobbleData[i].Offset).ToVec3();
                    }
                }


            }
            #region spike lines spring to life. each spike line begins at both ends approaching the center of the line. once the two ends meet, the spike line gains a temporary burst of energy that's applied as a multiplier to the spike amplitude.
            #endregion
        }
        public class VertexLine : GraphicsComponent
        {
            public Vector2 From, To;
            public float FromToLerp, ToFromLerp;
            public VertexPositionColor[] Vertices;
            public Color ColorB;
            public float ColorLerp;
            private Color finalColor;
            private Color origColor, origColorB;
            private float origFromTo, origToFrom, origColorLerp;
            private Vector2 origFrom, origTo;
            public VertexLine(Vector2 from, Vector2 to) : base(true)
            {
                From = from;
                To = to;
                Vertices = new VertexPositionColor[2];
                Visible = false;
            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                origColorB = ColorB;
                origColorLerp = ColorLerp;
                origFromTo = FromToLerp;
                origToFrom = ToFromLerp;
                origFrom = From;
                origTo = To;
                UpdateVertices();
            }

            public void Reset(bool visible)
            {
                Color = origColor;
                ColorB = origColorB;
                From = origFrom;
                To = origTo;
                FromToLerp = origFromTo;
                ToFromLerp = origToFrom;
                ColorLerp = origColorLerp;
                UpdateVertices();
                Visible = visible;
            }
            public void UpdateVertices()
            {
                finalColor = Color.Lerp(Color, ColorB, ColorLerp);
                Vector2 p = RenderPosition;
                Vertices[0].Position = (p + Vector2.Lerp(From, To, FromToLerp)).ToVec3();
                Vertices[1].Position = (p + Vector2.Lerp(To, From, ToFromLerp)).ToVec3();
                Vertices[0].Color = Vertices[1].Color = finalColor;
            }
            public override void Update()
            {
                base.Update();
                finalColor = Color.Lerp(Color, ColorB, ColorLerp);
                Vector2 p = RenderPosition;
                Vertices[0].Position = (p + Vector2.Lerp(From, To, FromToLerp)).ToVec3();
                Vertices[1].Position = (p + Vector2.Lerp(To, From, ToFromLerp)).ToVec3();
                Vertices[0].Color = Vertices[1].Color = finalColor;
            }
            public void RenderVertices()
            {
                PianoUtils.DrawUserPrimitives(PrimitiveType.LineStrip, SceneAs<Level>().Camera.Matrix, Vertices, 1);
            }
            public void DirectRenderVertices()
            {
                Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, Vertices, 0, 1);
            }
        }
        public class VertexOffsetComponent : Component
        {
            public enum ApproachModes
            {
                Instant,
                ConstantApproach,
                SpeedApproach,
                LerpApproach,
                RubberbandApproach
            }
            public enum ValuePriority
            {
                Float,
                Vec2,
                Both
            }
            public ValuePriority Priority;
            public bool WaitForTimer = true;
            public ApproachModes ApproachMode;
            public float ConstantApproachSpeed;
            public float Interval;
            public Vector2 Vec2Offset;
            public Vector2 TargetVec2Offset;
            public float FloatOffset;
            public float TargetFloatOffset;
            public float MaxOffset, MinOffset;
            public float Speed;
            public float XSpeed, YSpeed;
            public float MaxSpeed;
            public float SpeedIncrement;
            private float timer;
            private bool start;
            private Vector2 prevVec2Offset;
            private float prevFloatOffset;
            public float RubberbandFactorMult = 1;
            public Ease.Easer LerpEase = Ease.SineInOut;
            public Action<VertexOffsetComponent> OnRoll;
            public VertexOffsetComponent(float maxOffset, float minOffset, float interval, ApproachModes approachMode, bool start = true, Action<VertexOffsetComponent> onRoll = null) : base(false, false)
            {
                Interval = interval;
                MaxOffset = maxOffset;
                MinOffset = minOffset;
                this.start = start;
                OnRoll = onRoll;
                ApproachMode = approachMode;

            }
            public void Start()
            {
                Active = true;
                Roll();
            }
            public void Pause()
            {
                Active = false;
            }
            public void Resume()
            {
                Active = true;
            }
            public void Reset()
            {
                Active = false;
                Vec2Offset = Vector2.Zero;
                FloatOffset = 0;
                Speed = 0;
                XSpeed = 0;
                YSpeed = 0;
                TargetFloatOffset = 0;
                TargetVec2Offset = Vector2.Zero;
            }
            public void Roll()
            {
                OnRoll?.Invoke(this);
                prevVec2Offset = Vec2Offset;
                prevFloatOffset = FloatOffset;
                if (MinOffset == MaxOffset)
                {
                    TargetFloatOffset = MaxOffset;
                    TargetVec2Offset = new Vector2(MaxOffset);
                }
                else
                {
                    TargetFloatOffset = Calc.Random.Range(MinOffset, MaxOffset);
                    TargetVec2Offset = new Vector2(Calc.Random.Range(MinOffset, MaxOffset), Calc.Random.Range(MinOffset, MaxOffset));
                }
                timer = Interval;
                if (ApproachMode == ApproachModes.Instant)
                {
                    Vec2Offset = TargetVec2Offset;
                    FloatOffset = TargetFloatOffset;
                }
            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                if (start) Start();
            }
            public override void Update()
            {
                base.Update();
                if (timer > 0)
                {
                    timer -= Engine.DeltaTime;
                    switch (ApproachMode)
                    {
                        case ApproachModes.ConstantApproach:
                            Vec2Offset = Calc.Approach(Vec2Offset, TargetVec2Offset, ConstantApproachSpeed * Engine.DeltaTime);
                            FloatOffset = Calc.Approach(FloatOffset, TargetFloatOffset, ConstantApproachSpeed * Engine.DeltaTime);
                            break;
                        case ApproachModes.LerpApproach:
                            Vec2Offset = Vector2.Lerp(prevVec2Offset, TargetVec2Offset, LerpEase(1 - timer / Interval));
                            FloatOffset = Calc.LerpClamp(prevFloatOffset, TargetFloatOffset, LerpEase(1 - timer / Interval));
                            break;
                        case ApproachModes.RubberbandApproach:
                            double factor = 0.0099999997764825821 * RubberbandFactorMult;
                            Vec2Offset = PianoUtils.RubberbandApproach(Vec2Offset, TargetVec2Offset, 0.01f, factor);
                            FloatOffset = PianoUtils.RubberbandApproach(FloatOffset, TargetFloatOffset, 0.01f, factor);
                            break;
                        case ApproachModes.SpeedApproach:
                            Speed = Calc.Approach(Speed, Math.Sign(TargetFloatOffset - FloatOffset) * MaxSpeed, SpeedIncrement * Engine.DeltaTime);
                            XSpeed = Calc.Approach(XSpeed, Math.Sign(TargetVec2Offset.X - Vec2Offset.X) * MaxSpeed, SpeedIncrement * Engine.DeltaTime);
                            YSpeed = Calc.Approach(YSpeed, Math.Sign(TargetVec2Offset.Y - Vec2Offset.Y) * MaxSpeed, SpeedIncrement * Engine.DeltaTime);
                            FloatOffset += Speed * Engine.DeltaTime;
                            Vec2Offset.X += XSpeed * Engine.DeltaTime;
                            Vec2Offset.Y += YSpeed * Engine.DeltaTime;
                            break;
                    }
                    if (!WaitForTimer)
                    {
                        switch (Priority)
                        {
                            case ValuePriority.Float:
                                if (FloatOffset == TargetFloatOffset)
                                {
                                    Roll();
                                    return;
                                }
                                break;
                            case ValuePriority.Vec2:
                                if (Vec2Offset == TargetVec2Offset)
                                {
                                    Roll();
                                    return;
                                }
                                break;
                            default:
                                if (FloatOffset == TargetFloatOffset && Vec2Offset == TargetVec2Offset)
                                {
                                    Roll();
                                    return;
                                }
                                break;
                        }
                    }
                    if (timer <= 0)
                    {
                        timer = 0;
                        Roll();
                    }
                }
            }
        }
    }
}