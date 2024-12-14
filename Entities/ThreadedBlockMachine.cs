using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ThreadedBlockMachine")]
    [Tracked]
    public class ThreadedBlockMachine : Entity
    {
        public const string StartOfSessionFlag = "ThreadedBlockMachineSetDefaultValueFlag";
        public const int Max = 9;
        public string[] CounterNames = new string[3];
        private Image[] cubes = new Image[3];
        private Image screen;
        private float cubeHeight;
        public static Dictionary<string, int> Counters = new();
        private int countA => GetCounter(0);
        private int countB => GetCounter(1);
        private int countC => GetCounter(2);
        [OnUnload]
        public static void Unload()
        {
            Counters.Clear();
        }
        public ThreadedBlockMachine(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            CounterNames[0] = data.Attr("counterNameA");
            CounterNames[1] = data.Attr("counterNameB");
            CounterNames[2] = data.Attr("counterNameC");
            foreach (string s in CounterNames)
            {
                if (!Counters.ContainsKey(s))
                {
                    Counters.Add(s, 0);
                }
            }
            Tag |= Tags.TransitionUpdate;
            Vector2[] nodes = data.NodesOffset(offset);
            Add(screen = new Image(GFX.Game["objects/PuzzleIslandHelper/threadedBlock/screen"]));
            screen.RenderPosition = nodes[0];
            Vector2 screenPosition = screen.RenderPosition;
            Image machine = new Image(GFX.Game["objects/PuzzleIslandHelper/threadedBlock/machine"]);
            Add(machine);
            Collider = new Hitbox(machine.Width, machine.Height);
            MTexture cube = GFX.Game["objects/PuzzleIslandHelper/threadedBlock/block"];
            cubeHeight = cube.Height;
            for (int i = 0; i < 3; i++)
            {
                cubes[i] = new Image(cube);
                cubes[i].Position = screen.Position + new Vector2(i * (2 + cube.Width) + 2f, screen.Height - 3 - cube.Height);

                Collider c = new Hitbox(11, 11, 6 + i * 11, 9);
                Action<Player> ontalk = i switch
                {
                    0 => interact1,
                    1 => interact2,
                    _ => interact3
                };
                Add(new DotX3(c, ontalk) { PlayerMustBeFacing = false });
            }
            Add(cubes);
            Image glare;
            Add(glare = new Image(GFX.Game["objects/PuzzleIslandHelper/threadedBlock/glare"]));
            glare.RenderPosition = nodes[0];

        }
        private void interact1(Player player)
        {
            interact(player, 0);
        }
        private void interact2(Player player)
        {
            interact(player, 1);
        }
        private void interact3(Player player)
        {
            interact(player, 2);
        }
        private void interact(Player player, int index)
        {
            Increment(index);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!StartOfSessionFlag.GetFlag())
            {
                SetAll(0);
                StartOfSessionFlag.SetFlag();
            }
        }
        public int GetCounter(int index)
        {
            if (CounterNames.Length <= index || string.IsNullOrEmpty(CounterNames[index])) return 0;
            return Counters[CounterNames[index]];
        }
        public override void Update()
        {
            float y = screen.Y + screen.Height - cubeHeight - 3;
            for (int i = 0; i < 3; i++)
            {
                cubes[i].Y = y - (GetCounter(i) * 2);
            }
            base.Update();
        }
        public void SetAll(int count)
        {
            Set(0, count);
            Set(1, count);
            Set(2, count);
        }
        private bool nameValid(int index)
        {
            return !(CounterNames.Length <= index || string.IsNullOrEmpty(CounterNames[index]));
        }
        public void Set(int index, int count)
        {
            if (!nameValid(index)) return;
            string s = CounterNames[index];
            if (!Counters.ContainsKey(s))
            {
                Counters[s] = count % Max;
            }
        }
        public void Increment(int index, bool playSound = true)
        {
            if (!nameValid(index)) return;
            int num = Counters[CounterNames[index]];
            Counters[CounterNames[index]] = (num + 1) % Max;
            if (playSound)
            {
                //todo play sound
            }
        }
    }
}