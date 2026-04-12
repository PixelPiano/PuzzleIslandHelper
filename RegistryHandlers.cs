using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities;
using Microsoft.Xna.Framework.Input;
using Celeste.Mod.Registry;
using Celeste.Mod.Registry.DecalRegistryHandlers;
using System.Xml;
using System.Collections;
using Monocle;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Microsoft.Xna.Framework;
using System;
using Celeste.Mod.PuzzleIslandHelper.Components;
using System.Reflection;
namespace Celeste.Mod.PuzzleIslandHelper
{
    internal sealed class RegistryHandlers
    {
        [OnLoad]
        public static void Load()
        {
            DecalRegistry.AddPropertyHandler<MonumentComponentDecalRegistryHandler>();
        }
        internal sealed class MonumentComponentDecalRegistryHandler : DecalRegistryHandler
        {
            public string Room;
            public string[] IDs;
            public int PadY;
            public int PadX;
            public float MOEPercent;
            public float Width;
            public float Height;
            private bool useEntityWidth;
            private bool useEntityHeight;
            public Vector2 Offset;
            public int CellSize = 8;
            public bool RetainCollision;
            public override string Name => "PuzzleIslandHelper_monumentComponentDecal";

            public override void ApplyTo(Decal decal)
            {
                string levelName = decal.SceneAs<Level>().Session.Level;
                if (string.IsNullOrEmpty(Room) || levelName == Room)
                {
                    if (useEntityWidth)
                    {
                        Width = decal.textures[(int)decal.frame].Width;
                    }
                    if (useEntityHeight)
                    {
                        Height = decal.textures[(int)decal.frame].Height;
                    }
                    decal.Add(new MonumentComponent()
                    {
                        IDs = IDs,
                        PadY = PadY,
                        PadX = PadX,
                        MOEPercent = MOEPercent,
                        Width = Width,
                        Height = Height,
                        useEntityWidth = useEntityWidth,
                        useEntityHeight = useEntityHeight,
                        Offset = Offset,
                        CellSize = CellSize,
                        RetainCollision = RetainCollision,
                    });
                }
            }

            public override void Parse(XmlAttributeCollection xml)
            {
                string id = GetString(xml, "ids", "");
                Room = GetString(xml, "room", "");
                IDs = id.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                PadY = Get<int>(xml, "padY", 0);
                PadX = Get<int>(xml, "padX", 0);
                MOEPercent = Get<float>(xml, "moe", 0);
                string w = GetString(xml, "width", "");
                string h = GetString(xml, "height", "");
                useEntityWidth = string.IsNullOrEmpty(w);
                useEntityHeight = string.IsNullOrEmpty(h);
                if (!useEntityWidth)
                {
                    if (float.TryParse(w, out float result))
                    {
                        Width = result;
                    }
                }
                if (!useEntityHeight)
                {
                    if (float.TryParse(h, out float result))
                    {
                        Height = result;
                    }
                }
                Offset = GetVector2(xml, "offsetX", "offsetY", default);
                CellSize = Get(xml, "cellSize", 8);
                RetainCollision = GetBool(xml, "retainCollision", false);
            }
        }
        internal sealed class BlowAwayDecalRegistryHandler : DecalRegistryHandler
        {
            public Vector2? Speed;
            public Vector2? Acceleration;
            public float Rotation;
            public float Spin;
            public float? Friction;
            public float? Life;
            public float? SpeedMultiplier = 1;
            public float? WaveSpeed;
            public float? WaveAmplitude;
            public int? SliceSize;
            public float? SliceSinIncrement;
            public bool? EaseDown;
            public float? Offset;
            public BlowAwayComponent.FadeModes FadeMode = BlowAwayComponent.FadeModes.None;
            public BlowAwayComponent.RotationModes RotationMode = BlowAwayComponent.RotationModes.None;
            public override string Name => "PuzzleIslandHelper_blowAwayDecal";

            public override void Parse(XmlAttributeCollection xml)
            {
                Speed = GetVector2(xml, "speedX", "speedY", Vector2.Zero);
                Acceleration = GetVector2(xml, "accelX", "accelY", Vector2.Zero);
                Friction = GetNullable<float>(xml, "friction");
                Life = GetNullable<float>(xml, "life");
                SpeedMultiplier = GetNullable<float>(xml, "speedMult");
                string r = GetString(xml, "rotationMode", "None");
                string f = GetString(xml, "fadeMode", "None");
                if (Enum.TryParse<BlowAwayComponent.RotationModes>(r, out var result))
                {
                    RotationMode = result;
                }
                if (Enum.TryParse<BlowAwayComponent.FadeModes>(f, out var result2))
                {
                    FadeMode = result2;
                }
            }
            public override void ApplyTo(Decal decal)
            {
                if (decal.textures != null && decal.textures.Count > 0)
                {
                    float width = decal.textures[0].Width;
                    float height = decal.textures[0].Height;
                    BlowAwayComponent component = new()
                    {
                        Speed = Speed ?? default,
                        Friction = Friction ?? default,
                        Offset = Offset ?? default,
                        Life = Life ?? 1,
                        StartLife = Life ?? 1,
                        WaveSpeed = WaveSpeed ?? 1,
                        WaveAmplitude = WaveAmplitude ?? 1,
                        SliceSize = SliceSize ?? 2,
                        SliceSinIncrement = SliceSinIncrement ?? 1,
                        EaseDown = EaseDown ?? false,
                        OnlyIfWindy = default,
                        Segments = []
                    };
                    PlayerCollider collider = new PlayerCollider(p =>
                    {

                    }, new Hitbox(width, height));
                }
                else
                {
                    throw new System.Exception("BlowAwayDecal does not have any textures assigned to it.");
                }
            }
            public class BlowAwayComponent : Decal.Banner
            {
                private Vector2 Position;
                public Vector2 Speed;
                public Vector2 Acceleration;
                public float Rotation;
                public float Spin;
                public float Friction;
                public float Life;
                public float StartLife;
                public float SpeedMultiplier = 1;
                public float Alpha = 1;
                public FadeModes FadeMode;
                public RotationModes RotationMode;
                public enum FadeModes
                {
                    None,
                    Linear,
                    Late,
                    InAndOut
                }
                public enum RotationModes
                {
                    None,
                    Random,
                    SameAsDirection
                }
                public BlowAwayComponent(Decal decal, Vector2 speed, Vector2 acceleration, float friction, float life, float waveSpeed, float waveAmplitude, int sliceSize, float sliceSinIncrement, bool easeDown, float offset = 0f, bool onlyIfWindy = false)
                {
                }
                public BlowAwayComponent() { }
                public override void Added(Entity entity)
                {
                    base.Added(entity);
                    if (entity is not Decal decal)
                    {
                        RemoveSelf();
                        return;
                    }
                    foreach (MTexture texture in decal.textures)
                    {
                        List<MTexture> list = new List<MTexture>();
                        for (int i = 0; i < texture.Height; i += SliceSize)
                        {
                            list.Add(texture.GetSubtexture(0, i, texture.Width, SliceSize));
                        }
                        Segments.Add(list);
                    }
                }
                public override void Update()
                {
                    float num2 = Life / StartLife;
                    Life -= Engine.DeltaTime;
                    if (Life <= 0f)
                    {
                        Active = false;
                        return;
                    }

                    if (RotationMode == RotationModes.SameAsDirection)
                    {
                        if (Speed != Vector2.Zero)
                        {
                            Rotation = Speed.Angle();
                        }
                    }
                    else
                    {
                        Rotation += Spin * Engine.DeltaTime;
                    }

                    Alpha = ((FadeMode == FadeModes.Linear) ? num2 : ((FadeMode == FadeModes.Late) ? Math.Min(1f, num2 / 0.25f) : ((FadeMode != FadeModes.InAndOut) ? 1f : ((num2 > 0.75f) ? (1f - (num2 - 0.75f) / 0.25f) : ((!(num2 < 0.25f)) ? 1f : (num2 / 0.25f))))));

                    Position += Speed * Engine.DeltaTime;
                    Speed += Acceleration * Engine.DeltaTime;
                    Speed = Calc.Approach(Speed, Vector2.Zero, Friction * Engine.DeltaTime);
                    if (SpeedMultiplier != 1f)
                    {
                        Speed *= (float)Math.Pow(SpeedMultiplier, Engine.DeltaTime);
                    }
                }
            }
        }

        internal sealed class FlagFadeHandler : DecalRegistryHandler
        {
            public static List<Decal> FadingDecals = [];
            private enum routineModes
            {
                OnlyOnce,
                Instant,
                Looping
            }
            private enum fadeModes
            {
                In,
                Out,
                InOut,
                None
            }
            private enum exitModes
            {
                Pause,
                ToFrom,
                ToTo,
                Hide,
                SnapTarget,
                SnapOrigin
            }
            private string flag;
            private bool? inverted = false;
            private float fadeDuration;
            private float fadeTo;
            private float fadeFrom;
            private float alpha = 1;
            private bool onlyOnce => routineMode == routineModes.OnlyOnce;
            private routineModes routineMode;
            private fadeModes fadeMode;
            private fadeModes last = fadeModes.None;
            private exitModes exitMode;
            private bool inout;
            private Easings easing = Easings.Linear;
            private Ease.Easer ease;
            private bool flagEmpty;
            private bool resetOnTransition = true;
            public override string Name => "PuzzleIslandHelper_FlagFade";

            [OnLoad]
            public static void Load()
            {
                //DecalRegistry.AddPropertyHandler<FlagFadeHandler>();
                //Everest.Events.Level.OnTransitionTo += Level_OnTransitionTo;
            }

            private static void Level_OnTransitionTo(Level level, LevelData next, Microsoft.Xna.Framework.Vector2 direction)
            {
                foreach (Decal decal in FadingDecals)
                {

                }
            }

            public override void Parse(XmlAttributeCollection xml)
            {
                flagEmpty = string.IsNullOrEmpty(flag);
                ease = easing.ToEase();
                inout = fadeMode == fadeModes.InOut;
                if (inout) last = fadeModes.Out;
            }
            private bool getFlag(Decal decal) => string.IsNullOrEmpty(flag) || decal.SceneAs<Level>().Session.GetFlag(flag) != inverted;

            public override void ApplyTo(Decal decal)
            {
                FadingDecals.Add(decal);
                decal.Add(new Coroutine(Routine(decal)));
            }
            private void snap(float from, float to)
            {
                switch (exitMode)
                {
                    case exitModes.ToFrom:
                        alpha = fadeFrom;
                        break;
                    case exitModes.ToTo:
                        alpha = fadeTo;
                        break;
                    case exitModes.Hide:
                        alpha = 0;
                        break;
                    case exitModes.SnapOrigin:
                        alpha = from;
                        break;
                    case exitModes.SnapTarget:
                        alpha = to;
                        break;
                }
            }
            private IEnumerator fade(Decal decal, float from, float to)
            {
                if (fadeDuration <= 0 || (!flagEmpty && !getFlag(decal)))
                {
                    snap(from, to);
                    yield break;
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime / fadeDuration)
                {
                    alpha = Calc.LerpClamp(from, to, ease(i));
                    yield return null;
                    if (!flagEmpty && !getFlag(decal))
                    {
                        snap(from, to);
                        yield break;
                    }
                }
                alpha = to;
            }
            private IEnumerator Routine(Decal decal)
            {
                while (true)
                {
                    while (!getFlag(decal))
                    {
                        yield return null;
                    }
                    switch (fadeMode)
                    {
                        case fadeModes.In:
                            yield return new SwapImmediately(fade(decal, fadeFrom, fadeTo));
                            break;
                        case fadeModes.Out:
                            yield return new SwapImmediately(fade(decal, fadeTo, fadeFrom));
                            break;
                        case fadeModes.InOut:
                            switch (last)
                            {
                                case fadeModes.In:
                                    yield return new SwapImmediately(fade(decal, fadeFrom, fadeTo));
                                    last = fadeModes.In;
                                    break;
                                case fadeModes.Out:
                                    yield return new SwapImmediately(fade(decal, fadeTo, fadeFrom));
                                    last = fadeModes.Out;
                                    break;
                            }
                            break;
                    }
                }
                if (onlyOnce)
                {
                    if (fadeMode == fadeModes.InOut)
                    {
                        if (last == fadeModes.Out)
                        {
                            yield break;
                        }
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
        }
    }
}
