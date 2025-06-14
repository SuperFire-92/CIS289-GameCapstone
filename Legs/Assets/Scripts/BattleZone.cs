using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BattleZone : MonoBehaviour
{
    //This variable 
    [NonSerialized] public bool playerInside;
    [NonSerialized] public bool playerEntered;
    [SerializeField] private GameObject[] enemiesToSpawn;
    [Tooltip("A list of locations where enemies can spawn within the BattleZone.\nThe maximum enemies at once is equal to the number of spawn locations.")]
    [SerializeField] private Vector2[] spawnLocations;
    //Store lists of enemies and dead enemies to be removed when the player leaves the area or dies.
    private List<GameObject> currentEnemies = new();
    private List<GameObject> deadEnemies = new();
    //A value to keep track of which spawn location goes next
    int nextSpawnSpot = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInside = false;
        playerEntered = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Handle the player entering the battle zone and either spawning or reactivating enemies
    public void handlePlayerEntry()
    {
        //Spawn the first wave of enemies
        if (playerEntered == false)
        {
            for (int i = 0; i < enemiesToSpawn.Length; i++)
            {
                if (enemiesToSpawn[i] == null)
                {
                    Debug.LogWarning("Battlezone Enemy " + i + " Missing Gameobject");
                    continue;
                }
                GameObject enemy = Instantiate(enemiesToSpawn[i]);
                //enemy.transform.position = new Vector2(transform.position.x + enemiesToSpawn[i].position.x, transform.position.y + enemiesToSpawn[i].position.y);
            }
        }
    }

    //Trigger enters and exits prevent enemies inside from attacking you if you are not inside the battle zone. Man I love the battle zone. May your fight be honorable and meaningful, young soldier.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            handlePlayerEntry();
            playerInside = true;
            playerEntered = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            playerInside = false;
        }
    }

    //Code to display where the enemies will spawn
    void OnDrawGizmosSelected()
    {
        //Display red circles at every instance of an enemy spawn location
        Gizmos.color = Color.red;

        for (int i = 0; i < spawnLocations.Length; i++)
        {
            Gizmos.DrawSphere(spawnLocations[i] + new Vector2(transform.position.x, transform.position.y), .2f);
        }
    } 
}
