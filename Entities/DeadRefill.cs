using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DeadRefill")]
    [Tracked]
    public class DeadRefill : Entity
    {
        private static bool PlayerColorMod;
        public Sprite sprite;
        public SineWave sine;
        public Sprite flash;
        public Image outline;
        public Level level;
        public ParticleType p_shatter;
        private EntityID id;
        private Vector2 _position;
        public bool Explosive;
        public DeadRefill(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset)
        {
            this.id = id;
            Explosive = data.Bool("explosive");
            _position = Position;
            Add(sine = new SineWave(0.2f));
            sine.Randomize();
            Collider = new Hitbox(16f, 16f, -8f, -8f);

            string text;

            text = "objects/PuzzleIslandHelper/deadRefill/";
            p_shatter = Refill.P_Shatter;
            Add(sprite = new Sprite(GFX.Game, text + "idle" + data.Attr("type")));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();

            Add(new MirrorReflection());
            Depth = data.Int("depth", -100);
            if (data.Bool("collidable", true))
            {
                Add(new PlayerCollider(OnPlayer));
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(ColorFlash()));
            level = SceneAs<Level>();
        }

        public override void Update()
        {
            base.Update();
            Position.Y = _position.Y + sine.Value * 2;
        }
        public override void Render()
        {
            if (sprite.Visible)
            {
                sprite.DrawOutline();
            }

            base.Render();
        }
        public static void Load()
        {
            PlayerColorMod = false;
            On.Celeste.PlayerSprite.Render += RenderHook;
        }
        public static void Unload()
        {
            PlayerColorMod = false;
            On.Celeste.PlayerSprite.Render -= RenderHook;
        }
        private IEnumerator ColorFlash()
        {
            while (Explosive)
            {
                yield return 2;
                sprite.Color = Color.Lerp(Color.White, Color.OrangeRed, 0.3f);
                yield return 0.1f;
                sprite.Color = Color.White;
                yield return null;
            }
        }
        private void DrainPlayer(Player player)
        {
            if (player.Dashes > 0)
            {
                player.Dashes--;
            }
            player.Stamina = 0;
        }
        public void OnPlayer(Player player)
        {
            if (Explosive)
            {
                Add(new Coroutine(ExplodeRoutine(player)));
                Audio.Play("event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            }
            else
            {
                DrainPlayer(player);
                Audio.Play("event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                Collidable = false;
                Add(new Coroutine(RefillRoutine(player)));
            }

        }
        public IEnumerator ExplodeRoutine(Player player)
        {
            Celeste.Freeze(0.05f);
            yield return null;
            DrainPlayer(player);
            Collidable = false;

            level.Shake();
            sprite.Visible = false;
            Depth = 8999;
            yield return 0.05f;
            float num = player.Speed.Angle();
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, Color.White, num - (float)Math.PI / 2f);
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, Color.Gray, num + (float)Math.PI / 2f);
            SlashFx.Burst(Position, num);

            yield return 0.5f;
            Input.Rumble(RumbleStrength.Climb, RumbleLength.TwoSeconds);
            yield return 2;

            for (int i = 0; i < 4; i++)
            {
                PlayerColorMod = false;
                if (i > 2)
                {
                    PlayerColorMod = true;
                    player.Sprite.Color = i % 0.2f == 0 ? Color.White : Color.Gray;
                }
                yield return Engine.DeltaTime;
            }
            PlayerColorMod = false;
            //yield return 2;

            for (int i = 0; i < 5; i++)
            {
                Audio.Play("event:/game/general/diamond_touch", player.Position);
                level.ParticlesFG.Emit(p_shatter, 5, player.Position, Vector2.One * 4f, Color.Black, num - (float)Math.PI / 2f);
                level.ParticlesFG.Emit(p_shatter, 5, player.Position, Vector2.One * 4f, Color.Gray, num + (float)Math.PI / 2f);
                SlashFx.Burst(player.Position, num);

                yield return null;
                yield return null;
            }
            player.Die(Vector2.Zero, false, false);
            SceneAs<Level>().Session.DoNotLoad.Add(id);
            RemoveSelf();
            yield return null;
        }

        public IEnumerator RefillRoutine(Player player)
        {
            Celeste.Freeze(0.05f);
            yield return null;
            level.Shake();
            sprite.Visible = false;
            Depth = 8999;
            yield return 0.05f;
            float num = player.Speed.Angle();
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, Color.White, num - (float)Math.PI / 2f);
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, Color.Gray, num + (float)Math.PI / 2f);
            SlashFx.Burst(Position, num);
            SceneAs<Level>().Session.DoNotLoad.Add(id);
            RemoveSelf();

        }
        private static void RenderHook(On.Celeste.PlayerSprite.orig_Render orig, PlayerSprite self)
        {
            Color prev = self.Color;
            if (PlayerColorMod)
            {
                self.Color = Color.Gray;
            }

            orig(self);
            self.Color = prev;
        }
    }
}