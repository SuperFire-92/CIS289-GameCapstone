using UnityEngine;

public static class GameStats
{
    public static GameObject player;

    public static void setPlayer(GameObject p)
    {
        player = p;
    }

    public static GameObject getPlayer()
    {
        return player;
    }
}
