// PuzzleIslandHelper.PuzzleIslandHelperCommands
using Celeste;
using Celeste.Mod.PuzzleIslandHelper;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Celeste.Mod.PuzzleIslandHelper.Helpers;
using Celeste.Mod.PuzzleIslandHelper.Loaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities.PlayerCalidus;

public class PianoCommands
{
    [Command("tile_image", "")]
    public static void GenerateTileImage(string tile, int tilesX, int tilesY, string name)
    {
        TileGrid grid = GFX.FGAutotiler.GenerateBox(tile[0], tilesX, tilesY).TileGrid;
        VirtualRenderTarget target = VirtualContent.CreateRenderTarget("temptarget", tilesX * 8, tilesY * 8);
        Engine.Scene.Add(new renderEntity(grid, target, name));
    }
    private class renderEntity : Entity
    {
        public TileGrid grid;
        public VirtualRenderTarget target;
        private bool rendered;
        private bool createdFile;
        private string name;
        public renderEntity(TileGrid grid, VirtualRenderTarget target, string name)
        {
            this.name = name;
            this.grid = grid;
            Add(grid);
            this.target = target;
            Add(new BeforeRenderHook(() =>
            {
                target.SetAsTarget();
                Draw.SpriteBatch.Begin();
                Color color = Color.White;
                for (int i = 0; i < grid.TilesX; i++)
                {
                    for (int j = 0; j < grid.TilesY; j++)
                    {
                        grid.Tiles[i, j]?.Draw(new Vector2(i * grid.TileWidth, j * grid.TileHeight), Vector2.Zero, color);
                    }
                }
                Draw.SpriteBatch.End();
                PianoUtils.SaveTargetAsPng((RenderTarget2D)target, "Screenshots/TileGrids/" + name, 0, 0, grid.TilesX * 8, grid.TilesY * 8);
                target.Dispose();
                RemoveSelf();
            }));
        }
    }

    [Command("flaglist", "")]
    public static void flaglist()
    {
        string flags = "hello,goodbye,,corrent, ,incorrent";
        FlagList list = new FlagList(flags);
        Engine.Commands.Log(list.ToString());
    }
    [Command("get_flag", "returns the value of a flag")]
    public static void GetFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag))
        {
            Engine.Commands.Log("No flag provided", Color.Red);
        }
        else if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is not a level", Color.Red);
        }
        else
        {
            Engine.Commands.Log(level.Session.GetFlag(flag), Color.Lime);
        }
    }
    [Command("dummy", "dummies the player")]
    public static void Dummy()
    {
        if (Engine.Scene.GetPlayer() is Player player)
        {
            if (player.StateMachine.State == Player.StDummy)
            {
                Engine.Commands.Log("The player is already dummy, dummy", Color.Magenta);
            }
            else
            {
                player.DisableMovement();
            }
        }
    }
    [Command("undummy", "undummies the player")]
    public static void Undummy()
    {
        if (Engine.Scene.GetPlayer() is Player player)
        {
            if (player.StateMachine.State != Player.StDummy)
            {
                Engine.Commands.Log("The player is already not dummy, dummy", Color.Magenta);
            }
            else
            {
                player.EnableMovement();
            }
        }
    }
    [Command("get_counter", "returns the counter value of the specified string")]
    public static void GetCounter(string value)
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        if (string.IsNullOrEmpty(value))
        {
            Engine.Commands.Log("Provided string is either null or empty.");
            return;
        }
        Engine.Commands.Log("Counter {" + value + ": " + level.Session.GetCounter(value));
    }
    [Command("set_counter", "sets the counter value of the specified string")]
    public static void SetCounter(string value, int num)
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        if (string.IsNullOrEmpty(value))
        {
            Engine.Commands.Log("Provided string is either null or empty.");
            return;
        }
        level.Session.SetCounter(value, num);
    }
    [Command("inc_counter", "adds one to the counter value of the specified string")]
    public static void IncCounter(string value)
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        if (string.IsNullOrEmpty(value))
        {
            Engine.Commands.Log("Provided string is either null or empty.");
            return;
        }
        int num = level.Session.GetCounter(value);
        level.Session.SetCounter(value, num + 1);
    }
    [Command("save_vert", "stores drawn vertices in a struct")]
    public static void SaveVertices(string name)
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        PolygonDrawing.RotatorDisplay drawing = level.Tracker.GetEntity<PolygonDrawing.RotatorDisplay>();
        if (drawing != null)
        {
            VertexStorage.Store(name, drawing.Vertices.ToArray());
        }
    }

    [Command("shakeIntensity", "sets the level shake intensity")]
    public static void SetIntensity(float value)
    {
        LevelShaker.Intensity = value;
    }
    [Command("memTest", "create and play a recorded memory")]
    public static void MemoryTest(int? frames = null, bool? persist = null)
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        if (frames == null)
        {
            Engine.Commands.Log("Please provide the number of frames to capture.");
            return;
        }
        persist ??= false;
        RecordedMemory memory = new RecordedMemory(frames.Value, persist.Value);
        level.Add(memory);
    }
    [Command("play_as_calidus", "turns the Player entity into PlayerCalidus")]
    public static void BecomeCalidus()
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        if (level.GetPlayer() is not Player player)
        {
            Engine.Commands.Log("Current Scene does not contain a player.");
            return;
        }
        if (player is PlayerCalidus)
        {
            Engine.Commands.Log("Current Scene already contains PlayerCalidus.");
            return;
        }
        level.Remove(player);
        player = new PlayerCalidus(player.Position - Vector2.UnitY * 3, new());
        level.Add(player);
    }
    [Command("play_as_player", "turns the PlayerCalidus entity into Player")]
    public static void BecomePlayer()
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        if (level.GetPlayer() is not Player player)
        {
            Engine.Commands.Log("Current Scene does not contain a player.");
            return;
        }
        if (player is PlayerCalidus)
        {
            level.Remove(player);
            player = new Player(player.Position + Vector2.UnitY * 3, player.DefaultSpriteMode);
            level.Add(player);
        }
        else
        {
            Engine.Commands.Log("Current Scene does not contain a PlayerCalidus player.");
            return;
        }
    }
    [Command("cInv", "sets any CalidusPlayer entity's RoboInventory")]
    public static void SetCalidusInventory(int inventory)
    {
        Upgrades upgrade = (Upgrades)Calc.Clamp(inventory, 0, Enum.GetValues(typeof(Upgrades)).Length);
        SetCalidusInventory(upgrade);
    }
    public static void SetCalidusInventory(Upgrades upgrade)
    {
        SetInventory(upgrade);
    }
    [Command("ftprogram", "starts a program on the fake terminal entity")]
    public static void AddFakeProgram(string programName)
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        FakeTerminal terminal = level.Tracker.GetEntity<FakeTerminal>();
        if (terminal == null)
        {
            Engine.Commands.Log("Current Scene does not contain a FakeTerminal entity.");
            return;
        }
        if (!TerminalProgramLoader.LoadCustomProgram(programName, terminal, level))
        {
            Engine.Commands.Log($"\"{programName}\" is not a valid program name.");
            return;
        }
    }
    [Command("di", "sets a debug int value")]
    private static void SetDebugInt(int num)
    {
        PianoModule.Session.DEBUGINT = num;
    }
    [Command("db1", "sets a debug bool value")]
    private static void SetDebugBool1(bool value)
    {
        PianoModule.Session.DEBUGBOOL1 = value;
    }
    [Command("db2", "sets a debug bool value")]
    private static void SetDebugBool2(bool value)
    {
        PianoModule.Session.DEBUGBOOL2 = value;
    }
    [Command("db3", "sets a debug bool value")]
    private static void SetDebugBool3(bool value)
    {
        PianoModule.Session.DEBUGBOOL3 = value;
    }
    [Command("db4", "sets a debug bool value")]
    private static void SetDebugBool4(bool value)
    {
        PianoModule.Session.DEBUGBOOL4 = value;
    }
    [Command("ds", "sets a debug string value")]
    private static void SetDebugString(string value)
    {
        PianoModule.Session.DEBUGSTRING = value;
    }
    [Command("df", "sets a debug float value")]
    private static void SetDebugFloat(float value)
    {
        PianoModule.Session.DEBUGFLOAT1 = value;
    }
    [Command("dv", "sets a debug vector value")]
    private static void SetDebugVector(int x, int y)
    {
        PianoModule.Session.DEBUGVECTOR = new Vector2(x, y);
    }
    [Command("sspinnerflag", "sets a flag for sound spinners")]
    private static void SetSoundSpinnerFlag(string id, bool state = true)
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        if (string.IsNullOrEmpty(id))
        {
            Engine.Commands.Log("Flag cannot be empty.");
            return;
        }
        level.Session.SetFlag("sound_spinner_shatter_flag_" + id, state);
    }

    [Command("resetgameshow", "resets gameshow progress")]
    private static void ResetGameshow()
    {
        Gameshow.RoomOrder.Clear();
    }
    [Command("add_batteryid", "debug shiz")]
    private static void AddBatteryID(string id)
    {
        PianoModule.Session.DrillBatteryIds.Add(id);
    }
    [Command("drill_state", "these are tiring to explain")]
    private static void SetDrill(bool state = true)
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        level.Session.SetFlag("drillExploded", state);
    }
    [Command("remove_batteries", "shuts off all interacted drill batteries")]
    private static void RemoveBatteries()
    {
        PianoModule.Session.DrillBatteryIds.Clear();
    }
    [Command("open_fountain", "opens the ruins fountain")]
    private static void OpenFountain(bool permanent = false)
    {
        PianoModule.Session.ForceFountainOpen = true;
        if (permanent)
        {
            PianoModule.Session.FountainCanOpen = true;
        }
    }
    [Command("resetmgens", "clears the list of registered mini generators in puzzle island")]
    private static void ResetMiniGens()
    {
        PianoModule.Session.MiniGenStates.Clear();
    }
    [Command("reset_gears", "resets all gears to their original positions and frees locked gears from any holders")]
    private static void ResetGears()
    {
        PianoModule.Session.GearData.Reset();
        PianoModule.Session.ContinuousGearIDs.Clear();
        PianoModule.Session.FixedFloors.Clear();
    }
    [Command("ejectDSPs", "removes any injected dsps")]
    private static void EjectDsps(string id = "")
    {
        AudioEffectGlobal.RemoveID(id);
    }
    [Command("clear_disks", "clears collected floppy disks")]
    private static void ClearFloppys()
    {
        PianoModule.Session.CollectedDisks.Clear();
        PianoModule.Session.HasFirstFloppy = false;
    }
    [Command("pipestate", "sets the state of pipes")]
    private static void PipeState(int state = 1)
    {
        PianoModule.Session.SetPipeState(Calc.Clamp(state, 1, 4));
        switch (state)
        {
            case >= 3: PipeValves(5); break;
            default: PipeValves(0); break;
        }
    }
    [Command("pi_pipevalves", "sets the number of awake pipe valves")]
    private static void PipeValves(int num = 0)
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        bool extremeCondition = num < 1 || num > 4;
        bool allState = num > 4;
        if (extremeCondition)
        {
            for (int i = 0; i < 5; i++)
            {
                level.Session.SetFlag("valve" + (i + 1), allState);
            }
        }
        else
        {
            for (int i = 1; i < num + 1; i++)
            {
                level.Session.SetFlag("valve" + i, true);
            }
            for (int i = num + 1; i < 6; i++)
            {
                level.Session.SetFlag("valve" + i, false);
            }
        }
    }
    [Command("pipeattempts", "sets the attempts of switching the pipe route")]
    private static void PipeAttempts(int num = 0)
    {
        int realNum = (int)Calc.Max(0, num);
        PianoModule.Session.PipeSwitchAttempts = realNum;
        if (realNum == 0)
        {
            PianoModule.Session.ResetPipeScrew();
        }
        PianoModule.Session.PipesBroken = realNum > 2;
    }
    [Command("generator", "turns the generator on/off")]
    private static void Generator(bool state = true)
    {
        if (Engine.Scene is Level level)
        {
            //LabGeneratorPuzzle.Completed.State = state;
            //LabGenerator.Laser.State = state;
        }
        if (!state)
        {
            //LabGeneratorPuzzle.PuzzlesCompleted = 0;
            //PianoModule.StageData.Reset();
        }
    }
    [Command("labpower", "Sets the power state of the lab in Puzzle Island")]
    private static void LabPower(int state = 0)
    {
        PianoModule.Session.PowerState = (LabPowerState)state;
    }
    [Command("printcollect", "Displays how many Puzzle Island collectables the Player has")]
    private static void PrintCollectables()
    {
        int hearts = 0;
        int blocks = 0;
        foreach (DashCodeCollectable entity in PianoModule.Session.CollectedIDs)
        {
            if (entity.IsHeart)
            {
                hearts++;
            }
            else
            {
                blocks++;
            }
        }
        Engine.Commands.Log($"{PianoModule.Session.CollectedIDs.Count} Collected. Mini Hearts: {hearts}, 'T' Blocks: {blocks}.");

    }
    [Command("pi_collect", "Gives the Player x amount of Puzzle Island collectables you CHEATER")]
    private static void GiveCollectable(int amount, bool heart = true)
    {
        for (int i = 0; i < amount; i++)
        {
            PianoModule.Session.CollectedIDs.Add(new DashCodeCollectable(Vector2.Zero, heart));
        }
    }
    [Command("pi_subwarp", "Enables or Disables PI sub world portals")]
    private static void SubWarpState(bool state = true)
    {
        SubWarp.Enabled = state;
        Engine.Commands.Log("Sub Warp portals have been " + (state ? "enabled" : "disabled"));
    }
    [Command("pi_setinvertdelay", "Changes how long the Player needs to hold down dash before the invert ability activates")]
    private static void SetInvert(float time)
    {
        InvertOverlay.WaitTime = time;
        PianoModule.Session.InvertWaitTime = time;
    }
}
