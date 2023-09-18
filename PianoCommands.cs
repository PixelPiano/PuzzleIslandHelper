// PuzzleIslandHelper.PuzzleIslandHelperCommands
using Celeste;
using Celeste.Mod.PuzzleIslandHelper;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

public class PianoCommands
{
    [Command("pi_escapestate", "Gives or takes away Level 5 clearance from the player")]
    private static void EscapeState(bool state = true)
    {
        PianoModule.SaveData.Escaped = state;
    }
    [Command("pi_setclearance", "Gives or takes away Level 5 clearance from the player")]
    private static void Clearance(bool state = true)
    {
        PianoModule.SaveData.HasClearance = state;
    }
    [Command("pi_printcollect","Displays how many Puzzle Island collectables the player has")]
    private static void PrintCollectables()
    {
        int hearts = 0;
        int blocks = 0;
        foreach(DashCodeCollectable entity in PianoModule.SaveData.CollectedIDs)
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
        Engine.Commands.Log($"{PianoModule.SaveData.CollectedIDs.Count} Collected. Mini Hearts: {hearts}, 'T' Blocks: {blocks}.");

    }
    [Command("pi_collect", "Gives the player x amount of Puzzle Island collectables you CHEATER :3")]
    private static void GiveCollectable(int amount, bool heart = true)
    {
        for(int i = 0; i<amount; i++)
        {
            PianoModule.SaveData.CollectedIDs.Add(new DashCodeCollectable(Vector2.Zero, heart));
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
        foreach (DashCodeCollectable entity in PianoModule.SaveData.CollectedIDs)
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
        PianoModule.SaveData.CollectedIDs.Clear();
    }
    [Command("pi_art", "Gives or takes away Puzzle Island artifact")]
    private static void SetArtifact(bool state = true)
    {
        PianoModule.SaveData.HasArtifact = state;
    }
    [Command("pi_pillar", "Resets the pillar puzzle in Puzzle Island")]
    private static void ResetPillars()
    {
        if (Engine.Scene is not Level level)
        {
            Engine.Commands.Log("Current Scene is currently not a level.");
            return;
        }
        PianoModule.SaveData.BrokenPillars.Clear();
        PianoModule.SaveData.PillarBlockState = 0;
        level.Session.SetFlag("pillarBlockSpinnerFlag", false);
        level.Session.SetFlag("pillarBlockSpinner", false);
    }
    [Command("pi_clearswitch", "Deactivates all pressed TSwitches")]
    private static void ResetTSwitches()
    {
        PianoModule.SaveData.PressedTSwitches.Clear();
    }
    [Command("pi_getswitch", "Displays activated T Switches")]
    private static void TSwitch()
    {
        string output = "Activated switches: ";
        foreach (KeyValuePair<EntityID, Vector2> pair in PianoModule.SaveData.PressedTSwitches)
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
        Engine.Commands.Log($"{PianoModule.SaveData.HasInvert}");
    }
    [Command("pi_setinvertdelay", "Changes how long the player needs to hold down dash before the invert ability activates")]
    private static void SetInvert(float time)
    {
        InvertOverlay.WaitTime = time;
        PianoModule.Session.InvertWaitTime = time;

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
        PianoModule.SaveData.HasInvert = state;
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
            portal.portalState = state;

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
