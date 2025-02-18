using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/WipTeleport")]
    [Tracked]
    public class WipTeleport : Entity
    {
        public string Room;
        public string MarkerID;
        public Vector2 TeleportPosition;
        public float Alpha;
        public string Flag;
        public string FlagOnUse;
        public WipTeleport(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Room = data.Attr("room");
            MarkerID = data.Attr("markerID");
            Flag = data.Attr("flag");
            FlagOnUse = data.Attr("flagOnUse");
            Collider = new Hitbox(data.Width, data.Height);
            DotX3 talk;
            Add(talk = new DotX3(Collider, Interact));
            talk.PlayerMustBeFacing = false;
        }
        public override void Render()
        {
            base.Render();
            if (Alpha > 0)
            {
                Draw.Rect(Collider, Color.White * Alpha);
            }
            Draw.HollowRect(Collider, Color.White);
        }
        public void Interact(Player player)
        {
            player.DisableMovement();
            Scene.Add(new Cutscene(this));
        }
        public class Cutscene(WipTeleport teleport) : CutsceneEntity()
        {
            public WipTeleport Teleport = teleport;
            public override void OnBegin(Level level)
            {
                Depth = -100000;
                if (level.GetPlayer() is not Player player) return;
                player.DisableMovement();
                Add(new Coroutine(cutscene(player)));
            }
            private IEnumerator cutscene(Player player)
            {
                if (Teleport.Flag.GetFlag())
                {
                    Teleport.FlagOnUse.SetFlag(true);
                    AddTag(Tags.Persistent);
                    AddTag(Tags.TransitionUpdate);
                    yield return player.DummyWalkTo(Teleport.CenterX);
                    yield return 0.5f;
                    yield return PianoUtils.Lerp(Ease.CubeIn, 0.9f, f => Teleport.Alpha = f);
                    yield return null;
                    Level.Flash(Color.White);
                    InstantTeleport(Level);
                    yield return null;
                }
                else
                {
                    yield return Textbox.Say("Is doesn't seem to work.");
                }
                EndCutscene(Engine.Scene as Level);
            }
            public void InstantTeleport(Scene scene)
            {
                PianoUtils.InstantTeleportToMarker(scene, Teleport.Room, Teleport.MarkerID);
            }
            public override void OnEnd(Level level)
            {
                if (level.GetPlayer() is Player player)
                {
                    player.EnableMovement();
                }
                if (WasSkipped && Teleport.Flag.GetFlag())
                {
                    Teleport.FlagOnUse.SetFlag(true);
                    InstantTeleport(level);
                }
            }
        }
    }
}