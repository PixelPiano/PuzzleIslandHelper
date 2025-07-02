using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities;
using Microsoft.Xna.Framework.Input;
using Celeste.Mod.Registry;
using Celeste.Mod.Registry.DecalRegistryHandlers;
using System.Xml;
using System.Collections;
using Monocle;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
namespace Celeste.Mod.PuzzleIslandHelper
{
    internal sealed class RegistryHandlers
    {
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
                foreach(Decal decal in FadingDecals)
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
