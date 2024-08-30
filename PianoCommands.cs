// PuzzleIslandHelper.PuzzleIslandHelperCommands
using Celeste;
using Celeste.Mod.PuzzleIslandHelper;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.PuzzleIslandHelper.Entities.PlayerCalidus;

public class PianoCommands
{
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
    [Command("ftwrite", "sends text to the fake terminal entity")]
    private static void AddFakeText(string text)
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        string[] words = text.Split('_');
        string t = "";
        foreach (string s in words)
        {
            t += s.Replace('_', ' ');
        }
        foreach (FakeTerminal terminal in level.Tracker.GetEntities<FakeTerminal>())
        {
            terminal.AddText(t);
        }
    }
    [Command("ftclear", "Clears text from the fake terminal entity")]
    private static void ClearFakeText()
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        foreach (TerminalProgram p in level.Tracker.GetEntities<TerminalProgram>())
        {
            p.RemoveSelf();
        }
        foreach (FakeTerminal terminal in level.Tracker.GetEntities<FakeTerminal>())
        {
            terminal.Clear();
        }

    }
    [Command("di", "sets a debug int value")]
    private static void SetDebugInt(int num)
    {
        PianoModule.Session.DEBUGINT = num;
    }
    [Command("db", "sets a debug bool value")]
    private static void SetDebugBool(bool value)
    {
        PianoModule.Session.DEBUGBOOL = value;
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
        PianoModule.Session.HasBrokenPipes = realNum > 2;
    }
    [Command("generator", "turns the generator on/off")]
    private static void Generator(bool state = true)
    {
        LabGeneratorPuzzle.Completed = state;
        LabGenerator.Laser = state;
        if (!state)
        {
            LabGeneratorPuzzle.PuzzlesCompleted = 0;
            PianoModule.StageData.Reset();
        }
    }
    [Command("labpower", "Sets the power state of the lab in Puzzle Island")]
    private static void LabPower(bool state = true)
    {
        PianoModule.Session.RestoredPower = state;
    }
    [Command("escapestate", "Sets the escaped condition to true or false")]
    private static void EscapeState(bool state = true)
    {
        PianoModule.Session.Escaped = state;
    }
    [Command("printcollect", "Displays how many Puzzle Island collectables the Player has")]
    private static void PrintCollectables()
    {
        int hearts = 0;
        int blocks = 0;
        foreach (DashCodeCollectable entity in PianoModule.Session.CollectedIDs)
        {
            if (entity.isHeart)
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
    [Command("print_floors", "")]
    private static void PrintFloors()
    {
        string output = "";
        foreach (int i in PianoModule.Session.FixedFloors)
        {
            output += i + ",";
        }
        Engine.Commands.Log(output);
    }
    [Command("print_gears", "")]
    private static void PrintGears()
    {
        string output = "";
        foreach (string s in PianoModule.Session.ContinuousGearIDs)
        {
            output += s + ",";
        }
        Engine.Commands.Log(output);
    }
    [Command("pi_collect", "Gives the Player x amount of Puzzle Island collectables you CHEATER :3")]
    private static void GiveCollectable(int amount, bool heart = true)
    {
        for (int i = 0; i < amount; i++)
        {
            PianoModule.Session.CollectedIDs.Add(new DashCodeCollectable(Vector2.Zero, heart));
        }
    }

    [Command("pi_resetcollect", "Clears all obtained collectables in Puzzle Island")]
    private static void ResetCollectables()
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        foreach (DashCodeCollectable entity in PianoModule.Session.CollectedIDs)
        {
            if (level.Session.DoNotLoad.Contains(entity.ID))
            {
                level.Session.DoNotLoad.Remove(entity.ID);
            }
            if (!string.IsNullOrEmpty(entity.flag))
            {
                level.Session.SetFlag(entity.flag, false);
            }
            if (!string.IsNullOrEmpty(entity.collectedFlag))
            {
                level.Session.SetFlag(entity.collectedFlag, false);
            }
        }
        PianoModule.Session.CollectedIDs.Clear();
    }
    [Command("pi_arti", "Gives or takes away Puzzle Island artifact")]
    private static void SetArtifact(bool state = true)
    {
        PianoModule.Session.HasArtifact = state;
    }
    [Command("pi_pillar", "Resets the pillar puzzle in Puzzle Island")]
    private static void ResetPillars()
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        PianoModule.Session.BrokenPillars.Clear();
        PianoModule.Session.PillarBlockState = 0;
        level.Session.SetFlag("pillarBlockSpinnerFlag", false);
        level.Session.SetFlag("pillarBlockSpinner", false);
    }
    [Command("pi_clearswitch", "Deactivates all pressed TSwitches")]
    private static void ResetTSwitches()
    {
        PianoModule.Session.PressedTSwitches.Clear();
    }
    [Command("pi_getswitch", "Displays interacted T Switches")]
    private static void TSwitch()
    {
        string output = "Activated switches: ";
        foreach (KeyValuePair<EntityID, Vector2> pair in PianoModule.Session.PressedTSwitches)
        {
            output += "{" + pair.Key + ", " + pair.Value + "} ";
        }
        Engine.Commands.Log(output);
    }
    [Command("pi_subwarp", "Enables or Disables PI sub world portals")]
    private static void SubWarpState(bool state = true)
    {
        SubWarp.Enabled = state;
        Engine.Commands.Log("Sub Warp portals have been " + (state ? "enabled" : "disabled"));
    }
    [Command("pi_digital", "Set's the state of the digital world in Puzzle Island")]
    private static void DigitalState(int state = 0)
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        string[] flags = { "labLight1", "labLight2", "labLight3", "labComputerAccess", "allDigitalAreasCleared" };
        foreach (string flag in flags)
        {
            level.Session.SetFlag(flag, false);
        }

        if (state > 0)
        {
            level.Session.SetFlag(flags[0]);
            if (state > 1)
            {
                level.Session.SetFlag(flags[1]);
                if (state > 2)
                {
                    level.Session.SetFlag(flags[2]);
                    level.Session.SetFlag(flags[3]);
                    if (state > 3)
                    {
                        level.Session.SetFlag(flags[4]);
                    }
                }
            }
        }
    }
    [Command("pi_resetflags", "resets all the flags for puzzle island to their initial states")]
    private static void ResetLevelFlags()
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        string[] startFlags = {"bingus","bingus2","newWater","waterTest","blueSwitchBlock","coreModeAlt2",
                                "voidPlatformToggle","coreModeAlt","4dCheck","wToggle","decal_flag",
                                "error_flag","rightFactory","voidOff"};
        bool secondTimeState = level.Session.GetFlag("secondTime");
        foreach (string flag in level.Session.LevelFlags)
        {
            level.Session.SetFlag(flag, false);
        }
        foreach (string flag in startFlags)
        {
            level.Session.SetFlag(flag);
        }
        level.Session.SetFlag("secondTime", secondTimeState);
        SetInvert(false);
        Faces(false);
        CmdGShow(false);
        ClearKeys();
        ResetLights();

    }
    [Command("pi_getinvert", "Returns the state of 'HasInvert' from PuzzleIslandHelper save data")]
    private static void WriteInvertState()
    {
        Engine.Commands.Log($"{PianoModule.Settings.InvertAbility}");
    }
    [Command("pi_setinvertdelay", "Changes how long the Player needs to hold down dash before the invert ability activates")]
    private static void SetInvert(float time)
    {
        InvertOverlay.WaitTime = time;
        PianoModule.Session.InvertWaitTime = time;

    }
    [Command("pi_monitor", "Turns monitors on or off")]
    private static void SetMonitor(bool state)
    {
        SetInvert(state);
        if (state)
        {
            PianoModule.Session.ChainedMonitorsActivated.Clear();
        }
    }
    [Command("pi_setinvert", "Gives or takes away the invert ability")]
    private static void SetInvert(bool state)
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        level.Session.SetFlag("invertOverlay", state);
        PianoModule.Settings.InvertAbility = state;
    }
    [Command("pi_facestate", "Sets the state of the 'Faces' decals in Puzzle Island")]
    private static void Faces(bool state = true)
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        string[] setTrue = { "teleportReady", "2dHint_flag", "2dCondition1", "circleHint_flag" };
        string[] setFalse = { "decal_flag", "hasBeenFixed", "error_flag" };
        if (state)
        {
            for (int i = 0; i < setTrue.Length; i++)
            {
                level.Session.SetFlag(setTrue[i]);
            }
            for (int i = 0; i < setFalse.Length; i++)
            {
                level.Session.SetFlag(setFalse[i], false);
            }
        }
        else
        {
            for (int i = 0; i < setTrue.Length; i++)
            {
                level.Session.SetFlag(setTrue[i], false);
            }
            for (int i = 0; i < setFalse.Length; i++)
            {
                level.Session.SetFlag(setFalse[i]);
            }
        }
        level.Session.SetFlag("gameshowWin");
        level.Session.SetFlag("noResponse");
    }

    [Command("pi_gameshowstate", "Sets the completion state of the Puzzle Island gameshow segment")]
    private static void CmdGShow(bool state = true)
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        if (state)
        {
            level.Session.SetFlag("teleportReady");
            level.Session.SetFlag("torch");
            level.Session.SetFlag("gameshowWin");
            level.Session.SetFlag("noResponse", false);

        }
        else
        {
            string[] flagFalse = {
                "rewindTeleport", "laughing", "light1", "light2", "light3", "light4", "light5", "light6",
                "bigLose",
                "eraseLife1", "eraseLife2", "eraseLife3", "eraseLife4", "eraseLife5", "notIntro", "lifeFlash"
                ,"teleportReady","gameshowWin","noResponse","gameStart"
                ,"2bHint_flag"};
            for (int i = 0; i < flagFalse.Length; i++)
            {
                level.Session.SetFlag(flagFalse[i], false);
            }
            string[] flagTrue = { "decal_flag", "hasBeenFixed", "error_flag" };
            for (int i = 0; i < flagTrue.Length; i++)
            {
                level.Session.SetFlag(flagTrue[i]);
            }
        }
    }

    [Command("pi_portalstate", "Activates any Puzzle Island portals in the current level")]
    private static void SetPortals(bool state = true)
    {
        bool found = false;
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        foreach (TrianglePortal portal in level.Tracker.GetEntities<TrianglePortal>())
        {
            found = true;
            portal.PortalState = state;

        }
        level.Session.SetFlag("chainedComp1", state);
        level.Session.SetFlag("chainedComp2", state);
        level.Session.SetFlag("chainedComp3", state);
        if (found)
        {
            Engine.Commands.Log($"Portal states set to {state}.");
        }
        else
        {
            Engine.Commands.Log($"No valid portals found in level.");
        }
    }

    [Command("pi_getkeys", "Displays the id's of the collected keys from Puzzle Island")]
    private static void GetKeys()
    {
        bool found = false;
        int num = 0;
        string output = "IDs tied to the keys:";
        foreach (FadeWarpKey.KeyData data in PianoModule.Session.Keys)
        {
            if (data.id > -1)
            {
                if (found)
                {
                    output += $", {data.id}";
                }
                else
                {
                    output += $" {data.id}";
                }
                found = true;
                num++;
            }
        }
        if (num == 0)
        {
            output = "No keys in possesion.";
        }
        else
        {
            output += ".";
        }
        Engine.Commands.Log($"Total keys held: {num}. " + output);
    }

    [Command("pi_clearkeys", "Clears data for keys from Puzzle Island")]
    private static void ClearKeys()
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        PianoModule.Session.Keys.Clear();
        foreach (FadeWarp warp in level.Tracker.GetEntities<FadeWarp>())
        {
            if (warp != null)
            {
                if (warp.isDoor && warp.keyId != -1 && warp.keyId != -2)
                {
                    warp.ResetDoor();
                }
            }
        }
    }

    [Command("pi_light", "Resets the light puzzle in Puzzle Island")]
    private static void ResetLights()
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        foreach (IconHolder holder in level.Tracker.GetEntities<IconHolder>())
        {

            holder.Reset();
        }
        foreach (LightsPuzzle puzzle in level.Tracker.GetEntities<LightsPuzzle>())
        {
            puzzle.Reset();
        }
    }
}
