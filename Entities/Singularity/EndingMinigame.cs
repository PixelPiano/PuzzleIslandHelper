using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Singularity
{
    public class EndingMinigame : Entity
    {
        public class Chord
        {
            public int PulseIndex;
            public int[] Frequencies;
            public Vector2 Direction;
            public int PlayerFrequency;

            public Chord(Vector2 dir, int[] orbFrequencies, int playerFrequency, int pulseIndex)
            {
                Direction = dir;
                Frequencies = orbFrequencies;
                PlayerFrequency = playerFrequency;
                PulseIndex = pulseIndex;
            }
        }
        public Chord[] Chords;
        public Player Player;
        public SingularityActor Singularity;
        private bool orbsInPlace;
        public int ChordIndex;
        private float[] whiteLerpPulseAmounts = new float[4];
        private float whiteLerpProximityAmount;
        private float[] pulseRadiusOffsets = new float[4];
        private float proximityRadiusOffset;
        private Coroutine chordCoroutine;
        private Coroutine[] pulseCoroutines;
        private float orbVariableMult = 1;
        private Coroutine resetCoroutine;
        private bool updatePercent = true;
        public EndingMinigame(Player player, SingularityActor singularity)
        {
            Add(chordCoroutine = new Coroutine(false));
            Add(resetCoroutine = new Coroutine(false));
            pulseCoroutines = new Coroutine[4];
            for (int i = 0; i < pulseCoroutines.Length; i++)
            {
                Add(pulseCoroutines[i] = new Coroutine(false));
            }
            Player = player;
            Singularity = singularity;
            //float[] freqA = [12, 36, 60, 84];
            Chords = new Chord[]
            {

                    new Chord(new Vector2(-1, -1),[12, 36, 84],60, 2),
                    new Chord(new Vector2(1, -1), [12, 36, 84],60,2),
                    new Chord(new Vector2(1, 1),  [12, 36, 84],60,2),
                    new Chord(new Vector2(-1, 1), [12, 36, 84],60, 2)
            };
        }
        private Color whiteLerp(Color c, int i)
        {
            return Color.Lerp(c, Color.White, whiteLerpPulseAmounts[i] + whiteLerpProximityAmount * orbVariableMult);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Singularity.State = SingularityActor.StDummy;
            Singularity.Sprite.AutoHandleOrbit = false;
            Singularity.Sprite.AutoHandlePositions = false;
            for (int i = 0; i < Singularity.Sprite.ActiveOrbs.Count; i++)
            {
                SingularityOrb orb = Singularity.Sprite.ActiveOrbs[i];
                orb.CustomGetCenterColor.Add((c) => whiteLerp(c, i));
                orb.CustomGetEdgeColor.Add((c, i2) => whiteLerp(c, i));
                orb.CustomGetFillColor.Add((c, i2) => whiteLerp(c, i2));
            }
            Singularity.Orbit = 0;
            Singularity.OrbitRate = 0;
            Singularity.OrbitRateMult = 0;
            chordCoroutine.Replace(ChordRoutine(0, 0));
        }
        private IEnumerator resetRoutine()
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                orbVariableMult = 1 - i;
                yield return null;
            }
            orbVariableMult = 0;
            whiteLerpProximityAmount = 0;
            proximityRadiusOffset = 0;
            for (int i = 0; i < pulseRadiusOffsets.Length; i++)
            {
                pulseRadiusOffsets[i] = 0;
                whiteLerpPulseAmounts[i] = 0;
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                orbVariableMult = i; ;
                yield return null;
            }
            orbVariableMult = 1;

        }
        public void SetChord(int index)
        {
            ChordIndex = index;
            updatePercent = false;
            percent = 0;
            resetCoroutine.Replace(resetRoutine());
            chordCoroutine.Replace(ChordRoutine(index, 0.8f));
            foreach (Coroutine c in pulseCoroutines)
            {
                c.Cancel();
            }
        }
        public IEnumerator ChordRoutine(int index, float delay)
        {
            if (delay > 0) yield return delay;
            Chord chord = Chords[index];
            List<SingularityOrb> orbs = Singularity.Sprite.ActiveOrbs;
            float offset = 30;
            bool allReady = false;
            while (!allReady)
            {
                allReady = true;
                for (int i = 0; i < orbs.Count; i++)
                {
                    SingularityOrb orb = orbs[i];
                    Vector2 targetPosition = Position + chord.Direction * (offset + chord.Frequencies[i]);
                    allReady &= PianoUtils.TryRubberbandApproach(orb.RenderPosition, targetPosition, out Vector2 output, 1);
                    orb.RenderPosition = output;
                }
                yield return null;
            }
            updatePercent = true;
            float pulseDelay = 0.7f;
            while (true)
            {
                yield return 1f;
                for (int i = 0; i < orbs.Count + 1; i++)
                {
                    pulseCoroutines[i].Replace(PulseOrb(i, 6, 0.07f, 1.2f));
                    yield return pulseDelay;
                }
                yield return 1.5f;
            }
        }
        public void PulseNote(int i)
        {
            Chord chord = Chords[ChordIndex];
            int pitch;
            float volume = 1;
            if (i == chord.PulseIndex)
            {
                pitch = chord.PlayerFrequency;
                volume = percent;
            }
            else
            {
                if (i > chord.PulseIndex)
                {
                    i--;
                }
                pitch = chord.Frequencies[i];
            }
            //todo: play note
        }
        private IEnumerator PulseOrb(int orbIndex, float radiusOffset, float timeIn, float timeOut)
        {
            pulseRadiusOffsets[orbIndex] = 0;
            for (float i = 0; i < 1; i += Engine.DeltaTime / timeIn)
            {
                float ease = Ease.SineOut(i);
                pulseRadiusOffsets[orbIndex] = ease * radiusOffset;
                whiteLerpPulseAmounts[orbIndex] = ease * 0.5f;
                yield return null;
            }
            whiteLerpPulseAmounts[orbIndex] = 1;
            pulseRadiusOffsets[orbIndex] = radiusOffset;
            PulseNote(orbIndex);
            for (float i = 0; i < 1; i += Engine.DeltaTime / timeOut)
            {
                float ease = (1 - Ease.SineInOut(i));
                pulseRadiusOffsets[orbIndex] = ease * radiusOffset;
                if (ease > 0.3f)
                {
                    whiteLerpPulseAmounts[orbIndex] = (1 - (ease - 0.3f) / 0.7f);
                }
                yield return null;
            }
            whiteLerpPulseAmounts[orbIndex] = 0;
            pulseRadiusOffsets[orbIndex] = 0;
        }
        private float percent;
        public override void Update()
        {
            base.Update();
            Chord chord = Chords[ChordIndex];
            List<SingularityOrb> orbs = Singularity.Sprite.ActiveOrbs;
            float playerFrequency = chord.PlayerFrequency;
            float currentFrequency = FrequencyEntities.FrequencyData.GetRate(Scene, ChordIndex);

            if (updatePercent)
            {
                percent = 1 - (Math.Min(MathHelper.Distance(playerFrequency, currentFrequency), 2) / 2f);
            }

            for (int i = 0; i < orbs.Count; i++)
            {
                if (i == chord.PulseIndex) continue;
                else if (i > chord.PulseIndex)
                {
                    orbs[i - 1].FillRadiusOffset = (pulseRadiusOffsets[i - 1] + proximityRadiusOffset) * orbVariableMult;
                    orbs[i - 1].EdgeRadiusOffset = orbs[i - 1].FillRadiusOffset * 1.1f * orbVariableMult;
                }
                else
                {
                    orbs[i].FillRadiusOffset = (pulseRadiusOffsets[i] + proximityRadiusOffset) * orbVariableMult;
                    orbs[i].EdgeRadiusOffset = orbs[i].FillRadiusOffset * 1.1f * orbVariableMult;
                }

            }


        }
    }
}