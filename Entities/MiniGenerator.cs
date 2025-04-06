using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Components;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/MiniGenerator")]
    [Tracked]
    public class MiniGenerator : Entity
    {
        public EntityID ID;
        public bool Activated;
        public float Amount;
        private float duration = 1.2f;
        private float timer;
        private float flashLerp;
        private float colorLerp;
        private Color StreakDefaultColor = Calc.HexToColor("203747");
        private Color SymbolDefaultColor = Calc.HexToColor("020812");
        private readonly Color StreakActiveColor = Calc.HexToColor("3CFFF6");
        private readonly Color SymbolActiveColor = Calc.HexToColor("AA3CFF");
        private Color StreakColor;
        private Color SymbolColor;
        public bool Registered => PianoModule.Session.MiniGenStates.ContainsKey(ID);
        public bool RegistryState => Registered && PianoModule.Session.MiniGenStates[ID];
        private Color FlashColor = Color.White;
        public MTexture Machine = GFX.Game["objects/PuzzleIslandHelper/miniGenerator/machine"];
        public MiniGenerator(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            Depth = 1;
            ID = id;
            Collider = new Hitbox(Machine.Width, Machine.Height);
            StreakColor = StreakDefaultColor;
            SymbolColor = SymbolDefaultColor;
        }
        public void Register()
        {
            if (!Registered)
            {
                PianoModule.Session.MiniGenStates.Add(ID, Activated);
            }
        }
        public void UpdateRegistry(bool state)
        {
            if (Registered)
            {
                PianoModule.Session.MiniGenStates[ID] = state;
            }
        }
        public override void Render()
        {
            base.Render();
            
            Draw.Rect(Position + new Vector2(19, 8), 2, 26, StreakColor);
            Draw.Rect(Position + new Vector2(35, 8), 2, 25, StreakColor);
            Draw.Rect(Position + new Vector2(25, 12), 7, 16, SymbolColor);
            float progress = Amount * 16;
            if (!Activated)
            {
                Draw.Rect(Position + new Vector2(25, 28 - progress), 7, progress, Color.White);
            }
            Draw.SpriteBatch.Draw(Machine.Texture.Texture_Safe, Position, Color.White);
        }
        private IEnumerator ColorLerp()
        {
            float duration = 3;
            while (true)
            {
                for (float i = 0; i < 1; i += Engine.RawDeltaTime / duration)
                {
                    colorLerp = Ease.SineInOut(i);
                    yield return null;
                }
                colorLerp = 1;
                for (float i = 0; i < 1; i += Engine.RawDeltaTime / 0.5f)
                {
                    colorLerp = Calc.LerpClamp(1, 0.7f, Ease.SineInOut(i));
                    yield return null;
                }
                colorLerp = 0.7f;
                yield return null;
                for (float i = 0; i < 1; i += Engine.RawDeltaTime / 0.5f)
                {
                    colorLerp = Calc.LerpClamp(0.7f, 1, Ease.SineInOut(i));
                    yield return null;
                }
                colorLerp = 1;
                yield return null;
                for (float i = 0; i < 1; i += Engine.RawDeltaTime / duration)
                {
                    colorLerp = 1 - Ease.SineInOut(i);
                    yield return null;
                }
                colorLerp = 0;
                yield return null;
            }
        }
        private IEnumerator FlashRoutine()
        {
            Activated = true;
            for (float i = 0; i < 1; i += Engine.RawDeltaTime)
            {
                flashLerp = Calc.LerpClamp(0, 1, i);
                yield return null;
            }
            yield return 0.4f;
            for (float i = 0; i < 1; i += Engine.RawDeltaTime)
            {
                flashLerp = Calc.LerpClamp(1, 0, i);
                yield return null;
            }
            yield return null;
        }
        public void SetToActive(bool inLevel)
        {
            SymbolDefaultColor = StreakDefaultColor = Color.White;
            Activated = true;
            UpdateRegistry(true); //set dictionary value to true
            Add(new Coroutine(ColorLerp()) { UseRawDeltaTime = true });
            if (inLevel)
            {
                Add(new Coroutine(FlashRoutine()) { UseRawDeltaTime = true });
                Amount = 1;
            }
        }
        public DotX3 Talk;
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!Registered)
            {
                Register();
            }
            else if (RegistryState)
            {
                SetToActive(false);
            }
            Add(Talk = new DotX3(Collider, Interact));
            Talk.PlayerMustBeFacing = false;
        }
        public void Interact(Player player)
        {
            //todo: swap this out with an actual mechanic to activate these
            IEnumerator cutscene()
            {
                player.DisableMovement();
                yield return PianoUtils.Lerp(Ease.SineIn, duration, f => Amount = f, true);
                SetToActive(true);
                player.EnableMovement();
            }
            Add(new Coroutine(cutscene()));
        }
        public override void Update()
        {
            base.Update();
            StreakColor = Color.Lerp(Color.Lerp(StreakDefaultColor, StreakActiveColor, colorLerp), FlashColor, flashLerp);
            SymbolColor = Color.Lerp(Color.Lerp(SymbolDefaultColor, SymbolActiveColor, colorLerp), FlashColor, flashLerp);
            Talk.Enabled = !Activated;
/*            if (!Activated)
            {
                if (OnStandby && CollideCheck<Player>())
                {
                    timer += Engine.RawDeltaTime;
                }
                else
                {
                    timer = Calc.Max(0, timer - Engine.RawDeltaTime / 2);
                }
                Amount = Calc.Clamp(timer / duration, 0, 1);
                if (Amount >= 1)
                {
                    SetToActive(true);
                }
            }*/
        }
    }
}