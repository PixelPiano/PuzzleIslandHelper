using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [CustomEntity("PuzzleIslandHelper/TowerStairLock")]
    public class StairLock : Entity
    {
        public Vector2 OrigPosition;
        public enum States
        {
            Decayed,
            Young
        }
        public States State;
        public bool Unlocked;
        public TalkComponent Talk;
        private BetterShaker Shaker;
        private Image empty, icon;
        public string FlagOnSet;
        public bool FlagState;
        public string LockID;
        public bool Managed;

        public StairLock(EntityData data, Vector2 offset) : this(data.Position + offset, data.Attr("lockID"), data.Attr("flagOnUnlocked"), data.Bool("flagState"), data.Enum<States>("state"))
        {
        }
        public StairLock(Vector2 position, string id, string flagOnUnlocked, bool flagState, States state) : base(position)
        {
            State = state;
            LockID = id;
            FlagOnSet = flagOnUnlocked;
            FlagState = flagState;
            Add(Shaker = new BetterShaker(OnShake));
            Depth = 1;
        }
        public void OnShake(Vector2 shake)
        {
            empty.Position += shake;
            icon.Position += shake;
        }
        public void OnTalk(Player player)
        {
            var a = PianoModule.Session.FindUnusedTowerKey(State == States.Decayed);
            bool valid = a != null;

            if (!valid)
            {
                //dialogue cutscene
                Scene.Add(new UnlockStairsCutscene(player));
            }
            else
            {
                Column manager = Scene.Tracker.GetEntity<Column>();
                if (manager != null)
                {
                    for (int i = 0; i < manager.Locks.Count; i++)
                    {
                        if (manager.Locks[i] == this)
                        {
                            //column rise cutscene
                            Scene.Add(new UnlockStairsCutscene(manager, i, player));
                            break;
                        }
                    }
                }
                else
                {
                    Stairs s = null;
                    foreach (Stairs s2 in Scene.Tracker.GetEntities<Stairs>())
                    {
                        if (s2.LockID == LockID)
                        {
                            s = s2;
                        }
                    }
                    if (s != null)
                    {
                        //isolated stairs extend cutscene
                        Scene.Add(new UnlockStairsCutscene(this, s, player));
                    }
                    else
                    {
                        //set flag
                        Unlock(true);
                    }
                }
            }
        }
        public void Unlock(bool shake, bool setFlag = true)
        {
            if (setFlag) SetFlag();
            Unlocked = true;
            icon.Visible = true;
            if (!string.IsNullOrEmpty(LockID))
            {
                SceneAs<Level>().Session.SetFlag("TowerLock:" + LockID);
            }
            if (shake) Shaker.ShakeFor(0.3f);
        }
        public void SetFlag()
        {
            if (!string.IsNullOrEmpty(FlagOnSet))
            {
                SceneAs<Level>().Session.SetFlag(FlagOnSet, FlagState);
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            OrigPosition = Position;
            string type = State == States.Decayed ? "heart" : "sigil";
            empty = new Image(GFX.Game["objects/PuzzleIslandHelper/towerLock/" + type + "00"]);
            icon = new Image(GFX.Game["objects/PuzzleIslandHelper/towerLock/" + type + "01"]);
            empty.CenterOrigin();
            icon.CenterOrigin();
            Add(empty, icon);
            Collider = empty.ColliderCentered();
            if (SceneAs<Level>().Session.GetFlag("TowerLock:" + LockID))
            {
                Unlocked = true;
            }
            else
            {
                icon.Visible = false;
                Unlocked = false;
            }
        }
        public void UpdateTalkBounds()
        {
            float bottom = SceneAs<Level>().Bounds.Bottom;
            Vector2 check = Position - empty.HalfSize();
            while (check.Y < bottom && check.Y - Y < Height * 2 && !CollideCheck<Platform, Stairs.StairsPlatform>(check))
            {
                check.Y++;
            }
            float height = check.Y - Y + Height;
            Rectangle bounds = new Rectangle(-4 - (int)(Width / 2), -(int)(empty.Height / 2), (int)empty.Width + 8, (int)height);
            Talk.Bounds = bounds;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Talk = new TalkComponent(default, -Vector2.UnitY * empty.Height / 2, OnTalk);
            UpdateTalkBounds();
            Add(Talk);

        }
        public override void Update()
        {
            Talk.Enabled = true;
            if (string.IsNullOrEmpty(LockID))
            {
                if (!string.IsNullOrEmpty(FlagOnSet) && SceneAs<Level>().Session.GetFlag(FlagOnSet) == FlagState)
                {
                    Talk.Enabled = false;
                }
            }
            else if (SceneAs<Level>().Session.GetFlag("TowerLock:" + LockID))
            {
                Talk.Enabled = false;
            }
            base.Update();
        }
        public override void Render()
        {
            empty.DrawSimpleOutline();
            base.Render();
        }

    }
}