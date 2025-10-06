using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class InspectScene : Entity
    {
        public float Alpha = 0;
        public float FadeTime = 1;
        public bool Open;
        public InspectScene() : base()
        {

        }
        public void Close()
        {
            Open = false;
        }
        public IEnumerator sequence()
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime / FadeTime)
            {
                Alpha = i;
                yield return null;
            }
            Open = true;
            while (Open)
            {
                yield return null;
            }
        }
    }
}