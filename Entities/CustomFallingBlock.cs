using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
// PuzzleIslandHelper.TSwitch
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CustomFallingBlock")]
    [Tracked]
    public class CustomFallingBlock : Solid
    {
        public Coroutine sequence;
        public bool Triggered;
        public float FallDelay;
        public char TileType;
        private TileGrid tiles;
        private TileGrid highlight;
        private bool finalBoss;
        private bool climbFall;
        private string ShakeSFX = "event:/game/general/fallblock_shake";
        private string ImpactSFX = "event:/game/general/fallblock_impact";

        private string FlagOnImpact;
        private string FlagOnTriggered;
        private string TangibleFlag;
        private string SnapToFallenFlag;

        private bool tangibleFlagState;
        private bool snapToFallenState;
        private bool prevRemoveFlagState;
        private Vector2 ImpactCenter => TopLeft + new Vector2(Width / 2f, Height);
        public bool HasStartedFalling;
        public CustomFallingBlock(Vector2 position, char tile, int width, int height, bool finalBoss, bool behind, bool climbFall, string flagOnImpact, string flagOnTriggered, string onOffFlag, string snapToFallenFlag)
            : base(position, width, height, safe: false)
        {
            this.finalBoss = finalBoss;
            this.climbFall = climbFall;
            int newSeed = Calc.Random.Next();
            Calc.PushRandom(newSeed);
            Add(tiles = GFX.FGAutotiler.GenerateBox(tile, width / 8, height / 8).TileGrid);
            Calc.PopRandom();
            if (finalBoss)
            {
                Calc.PushRandom(newSeed);
                Add(highlight = GFX.FGAutotiler.GenerateBox('G', width / 8, height / 8).TileGrid);
                Calc.PopRandom();
                highlight.Alpha = 0f;
            }
            Add(sequence = new Coroutine(Sequence()));
            Add(new LightOcclude());
            Add(new TileInterceptor(tiles, highPriority: false));
            TileType = tile;
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tile];
            if (behind)
            {
                base.Depth = 5000;
            }
            FlagOnImpact = flagOnImpact;
            FlagOnTriggered = flagOnTriggered;
            TangibleFlag = onOffFlag;
            SnapToFallenFlag = snapToFallenFlag;
        }
        public CustomFallingBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height, finalBoss: false, data.Bool("behind"), data.Bool("climbFall", defaultValue: true), data.Attr("flagOnImpact"), data.Attr("flagOnTriggered"), data.Attr("tangibleFlag"), data.Attr("snapToFallenFlag"))
        {
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            UpdateFlags();

        }
        private bool snapped;
        public void UpdateFlags()
        {
            if (Scene is not Level level) return;
            tangibleFlagState = !string.IsNullOrEmpty(TangibleFlag) && level.Session.GetFlag(TangibleFlag);
            snapToFallenState = !string.IsNullOrEmpty(SnapToFallenFlag) && level.Session.GetFlag(SnapToFallenFlag);
            if (tangibleFlagState != prevRemoveFlagState)
            {
                if (tangibleFlagState) HideBlock();
                else ShowBlock();
            }
            if (snapToFallenState && !snapped)
            {
                SnapDown();
                snapped = true;
            }
            prevRemoveFlagState = tangibleFlagState;
        }
        public void SnapDown()
        {
            Remove(sequence);
            Triggered = true;
            float speed = 0f;
            float maxSpeed = (finalBoss ? 130f : 160f);
            while (true)
            {
                Level level = SceneAs<Level>();
                speed = Calc.Approach(speed, maxSpeed, 500f * Engine.DeltaTime);
                if (MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
                {
                    break;
                }

                if (Top > (float)(level.Bounds.Bottom + 16) || (Top > (float)(level.Bounds.Bottom - 1) && CollideCheck<Solid>(Position + new Vector2(0f, 1f))))
                {
                    Collidable = (Visible = false);
                    RemoveSelf();
                    DestroyStaticMovers();
                    break;
                }
            }
        }
        public override void Update()
        {
            base.Update();
            UpdateFlags();

        }
        public void HideBlock()
        {
            sequence.Active = false;
            Visible = false;
            Collidable = false;
        }
        public void ShowBlock()
        {
            sequence.Active = true;
            Visible = true;
            Collidable = true;
        }
        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            tiles.Position += amount;
            if (highlight != null)
            {
                highlight.Position += amount;
            }
        }

        public override void OnStaticMoverTrigger(StaticMover sm)
        {
            if (!finalBoss)
            {
                Triggered = true;
            }
        }

        public bool PlayerFallCheck()
        {
            if (climbFall)
            {
                return HasPlayerRider();
            }

            return HasPlayerOnTop();
        }
        public bool PlayerWaitCheck()
        {
            if (Triggered)
            {
                return true;
            }

            if (PlayerFallCheck())
            {
                return true;
            }

            if (climbFall)
            {
                if (!CollideCheck<Player>(Position - Vector2.UnitX))
                {
                    return CollideCheck<Player>(Position + Vector2.UnitX);
                }

                return true;
            }

            return false;
        }
        public IEnumerator Sequence()
        {
            while (!Triggered && (finalBoss || !PlayerFallCheck()))
            {
                yield return null;
            }
            if (!string.IsNullOrEmpty(FlagOnTriggered))
            {
                SceneAs<Level>().Session.SetFlag(FlagOnTriggered);
            }
            while (FallDelay > 0f)
            {
                FallDelay -= Engine.DeltaTime;
                yield return null;
            }

            HasStartedFalling = true;
            while (true)
            {
                ShakeSfx();
                StartShaking();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                if (finalBoss)
                {
                    Add(new Coroutine(HighlightFade(1f)));
                }

                yield return 0.2f;
                float timer = 0.4f;
                if (finalBoss)
                {
                    timer = 0.2f;
                }

                while (timer > 0f && PlayerWaitCheck())
                {
                    yield return null;
                    timer -= Engine.DeltaTime;
                }
                StopShaking();
                for (int i = 2; (float)i < Width; i += 4)
                {
                    if (Scene.CollideCheck<Solid>(TopLeft + new Vector2(i, -2f)))
                    {
                        SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(X + (float)i, Y), Vector2.One * 4f, (float)Math.PI / 2f);
                    }

                    SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(X + (float)i, Y), Vector2.One * 4f);
                }

                float speed = 0f;
                float maxSpeed = (finalBoss ? 130f : 160f);
                while (true)
                {
                    Level level = SceneAs<Level>();
                    speed = Calc.Approach(speed, maxSpeed, 500f * Engine.DeltaTime);
                    if (MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
                    {
                        if (!string.IsNullOrEmpty(FlagOnImpact))
                        {
                            SceneAs<Level>().Session.SetFlag(FlagOnImpact);
                        }
                        break;
                    }

                    if (Top > (float)(level.Bounds.Bottom + 16) || (Top > (float)(level.Bounds.Bottom - 1) && CollideCheck<Solid>(Position + new Vector2(0f, 1f))))
                    {
                        Collidable = (Visible = false);
                        yield return 0.2f;
                        if (level.Session.MapData.CanTransitionTo(level, new Vector2(Center.X, Bottom + 12f)))
                        {
                            yield return 0.2f;
                            SceneAs<Level>().Shake();
                            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                        }

                        RemoveSelf();
                        DestroyStaticMovers();
                        yield break;
                    }

                    yield return null;
                }

                ImpactSfx();
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                SceneAs<Level>().DirectionalShake(Vector2.UnitY, finalBoss ? 0.2f : 0.3f);
                if (finalBoss)
                {
                    Add(new Coroutine(HighlightFade(0f)));
                }

                StartShaking();
                LandParticles();
                yield return 0.2f;
                StopShaking();
                if (CollideCheck<SolidTiles>(Position + new Vector2(0f, 1f)))
                {
                    break;
                }

                while (CollideCheck<Platform>(Position + new Vector2(0f, 1f)))
                {
                    yield return 0.1f;
                }
            }

            Safe = true;
        }


        private IEnumerator HighlightFade(float to)
        {
            float from = highlight.Alpha;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.5f)
            {
                highlight.Alpha = MathHelper.Lerp(from, to, Ease.CubeInOut(p));
                tiles.Alpha = 1f - highlight.Alpha;
                yield return null;
            }

            highlight.Alpha = to;
            tiles.Alpha = 1f - to;
        }

        private void LandParticles()
        {
            for (int i = 2; (float)i <= base.Width; i += 4)
            {
                if (base.Scene.CollideCheck<Solid>(base.BottomLeft + new Vector2(i, 3f)))
                {
                    SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(base.X + (float)i, base.Bottom), Vector2.One * 4f, -(float)Math.PI / 2f);
                    float direction = ((!((float)i < base.Width / 2f)) ? 0f : ((float)Math.PI));
                    SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(base.X + (float)i, base.Bottom), Vector2.One * 4f, direction);
                }
            }
        }

        private void ShakeSfx()
        {
            Audio.Play(ShakeSFX, base.Center);
        }

        private void ImpactSfx()
        {
            Audio.Play(ImpactSFX, ImpactCenter);
        }
    }
}