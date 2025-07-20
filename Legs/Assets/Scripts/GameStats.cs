using System.Collections.Generic;
using UnityEngine;

public static class GameStats
{
    private static GameObject player;
    private static int playerLevel;
    private static int currentZone;
    private static Vector2 respawnPosition;
    /// <summary>
    /// A number to be multiplied by any timers and animations to allow the game to be paused while in menus
    /// </summary>
    private static float gamePaused;
    private static Vector2[] listOfRespawnPositions = { new Vector2(11f, 0.015f), new Vector2(91f, 0.015f), new Vector2(171f, 0.015f), new Vector2(251f, 0.015f), new Vector2(331f, 0.015f) };
    /// <summary>
    /// The location where the player spawns when the game begins
    /// </summary>
    private static Vector2 spawnLocation = new Vector2(-4f, 0.015f);
    private const int STARTING_HEALTH = 5;
    private static Vector4 cameraLimits;
    private static Vector4[] listOfCameraLimits = { new Vector4(-8f, -6.5f, 85f, 20f), new Vector4(85f, -6.5f, 165f, 20f), new Vector4(165f, -6.5f, 245f, 20f), new Vector4(245f, -6.5f, 325f, 20f), new Vector4(325f, -6.5f, 375f, 9f) };
    /// <summary>
    /// Keeps track of what menu the player is in.
    /// 0 for Main Menu, 1 for inGame, 2 for Pause Menu, 3 for cutscene, 4 for credits
    /// </summary>
    private static int gameMode;

    private static string controlScheme;

    private static List<GameObject> battleZones;
    private static List<GameObject> dialogues;
    private static GameObject[] animatedObjects;
    private static GameObject[] cutsceneRemoveableObjects;
    private static GameObject[] cutsceneAddedObjects;
    private static bool inFinalZone;

    private static bool godMode = false;
    private const int GOD_MODE_SWORDS = 16;
    private const int GOD_MODE_HEALTH = 50;

    //Set up or reset all initial variables for the game
    public static void setupVariables()
    {
        player = null;
        playerLevel = 0;
        currentZone = 0;
        respawnPosition = listOfRespawnPositions[0];
        gamePaused = 0;
        cameraLimits = listOfCameraLimits[0];
        gameMode = 0;
        controlScheme = "keyboard&mouse";
        battleZones = new List<GameObject>();
        dialogues = new List<GameObject>();
    }

    /// <summary>
    /// Resets all values to their default, and resets all necessary gameobjects
    /// </summary>
    public static void startGame()
    {
        foreach (GameObject gameObject in battleZones)
            gameObject.GetComponent<BattleZone>().resetBattleZone();
        foreach (GameObject gameObject in dialogues)
            gameObject.GetComponent<DialogueObject>().resetDialogue();
        playerLevel = 0;
        currentZone = 0;
        respawnPosition = listOfRespawnPositions[0];
        cameraLimits = listOfCameraLimits[0];
        godMode = false;
        inFinalZone = false;
        player.GetComponent<PlayerManager>().resetPlayer();
        foreach (GameObject gameObject in cutsceneRemoveableObjects)
        {
            gameObject.SetActive(true);
        }
        foreach (GameObject gameObject in cutsceneAddedObjects)
        {
            gameObject.SetActive(false);
        }

    }


    public static void setPlayer(GameObject p)
    {
        player = p;
    }

    public static GameObject getPlayer()
    {
        return player;
    }

    /// <summary>
    /// Upgrades the player level and zone based off the provided number
    /// </summary>
    /// <param name="zone">The area that the player is moving to</param>
    public static void increasePlayerLevel()
    {
        playerLevel++;
    }

    public static int getPlayerLevel()
    {
        return playerLevel;
    }

    public static void setCurrentZone(int cz)
    {
        currentZone = cz;
        respawnPosition = listOfRespawnPositions[currentZone];
        cameraLimits = listOfCameraLimits[currentZone];
        foreach (GameObject gameObject in battleZones)
        {
            if (gameObject.GetComponent<BattleZone>().zone == currentZone)
            {
                if (playerLevel <= currentZone)
                    gameObject.GetComponent<BattleZone>().resetBattleZone();
            }
        }
    }

    public static int getCurrentZone()
    {
        return currentZone;
    }

    public static Vector2 getRespawnPosition()
    {
        return respawnPosition;
    }

    public static Vector4 getCameraLimits()
    {
        return cameraLimits;
    }

    public static int getStartingPlayerHealth()
    {
        return STARTING_HEALTH;
    }

    public static void setGamePaused(bool paused)
    {
        if (!paused)
        {
            gamePaused = 1f;
        }
        else
        {
            gamePaused = 0f;
        }
        toggleUnscriptedAnimations();
    }

    public static float isGamePaused()
    {
        return gamePaused;
    }

    public static Vector2 getSpawnLocation()
    {
        return spawnLocation;
    }

    public static void setGameMode(int gm)
    {
        if (gm < 0 || gm > 4)
        {
            Debug.LogWarning("Variable GameStats.gameMode cannot be set to " + gm);
            return;
        }
        gameMode = gm;
        if (gm == 0 || gm == 2)
        {
            setGamePaused(true);
        }
        else
        {
            setGamePaused(false);
        }
    }

    public static int getGameMode()
    {
        return gameMode;
    }

    public static void setControlScheme(string cs)
    {
        //Ensure the control scheme is valid
        if (cs.ToLower() == "keyboard&mouse" || cs.ToLower() == "gamepad")
        {
            controlScheme = cs.ToLower();
        }
        else
        {
            Debug.LogWarning("Control Scheme [" + cs + "] is invalid.");
        }
    }

    public static string getControlScheme()
    {
        return controlScheme;
    }

    public static void addBattleZone(GameObject bz)
    {
        battleZones.Add(bz);
    }

    public static void addDialogue(GameObject dl)
    {
        dialogues.Add(dl);
    }

    public static void playerDied()
    {
        //Reset the battlezone the player died in
        //also I recently discovered foreach and i kinda like it
        foreach (GameObject gameObject in battleZones)
        {
            if (gameObject.GetComponent<BattleZone>().zone == currentZone)
            {
                if (playerLevel <= currentZone)
                    gameObject.GetComponent<BattleZone>().resetBattleZone();
            }
        }

    }

    public static void setGodMode(bool gm)
    {
        godMode = gm;
        player.GetComponent<PlayerManager>().takeDamage(5000, 0);
    }

    public static bool isGodMode()
    {
        return godMode;
    }

    public static int getGodModeSwords()
    {
        return GOD_MODE_SWORDS;
    }

    public static int getGodModeHealth()
    {
        return GOD_MODE_HEALTH;
    }

    public static void enteredFinalZone()
    {
        foreach (GameObject gameObject in cutsceneRemoveableObjects)
        {
            gameObject.SetActive(false);
        }
        foreach (GameObject gameObject in cutsceneAddedObjects)
        {
            gameObject.SetActive(true);
        }
        inFinalZone = true;
    }

    public static bool isInFinalZone()
    {
        return inFinalZone;
    }

    public static void receiveCutsceneAndAnimationObjects(GameObject[] an, GameObject[] re, GameObject[] ad)
    {
        animatedObjects = an;
        cutsceneRemoveableObjects = re;
        cutsceneAddedObjects = ad;
    }

    public static void toggleUnscriptedAnimations()
    {
        foreach (GameObject gameObject in animatedObjects)
        {
            gameObject.GetComponent<Animator>().speed = isGamePaused();
        }
    }
}
