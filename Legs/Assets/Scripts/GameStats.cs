using UnityEngine;
using UnityEngine.InputSystem.Composites;

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
    private static Vector2[] listOfRespawnPositions = { new Vector2(11f, 0.015f), new Vector2(91f, 0.015f), new Vector2(171f, 0.015f), new Vector2(251f, 0.015f) };
    /// <summary>
    /// The location where the player spawns when the game begins
    /// </summary>
    private static Vector2 spawnLocation = new Vector2(-4f, 0.015f);
    private const int STARTING_HEALTH = 5;
    private static Vector4 cameraLimits;
    private static Vector4[] listOfCameraLimits = { new Vector4(-8f, -6.5f, 85f, 20f), new Vector4(85f, -6.5f, 165f, 20f), new Vector4(165f, -6.5f, 245f, 20f), new Vector4(245f, -6.5f, 325f, 20f), new Vector4(325f, -6.5f, 5000f, 20f) };

    //Set up or reset all initial variables for the game
    public static void setupVariables()
    {
        player = null;
        playerLevel = 0;
        currentZone = 0;
        respawnPosition = listOfRespawnPositions[0];
        gamePaused = 0;
        cameraLimits = listOfCameraLimits[0];
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
    public static void setNewZone(int zone)
    {
        currentZone = zone;
        cameraLimits = listOfCameraLimits[zone];
        if (playerLevel < zone)
        {
            playerLevel = zone;
            respawnPosition = listOfRespawnPositions[zone];
        }
    }

    public static int getCurrentZone()
    {
        return currentZone;
    }

    public static int getPlayerLevel()
    {
        return playerLevel;
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
        if (paused)
            gamePaused = 1f;
        else
            gamePaused = 0f;
    }

    public static float isGamePaused()
    {
        return gamePaused;
    }

    public static Vector2 getSpawnLocation()
    {
        return spawnLocation;
    }
}
