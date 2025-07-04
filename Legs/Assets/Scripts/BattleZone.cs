using System;
using System.Collections.Generic;
using System.Linq;
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
    [System.Serializable]
    public struct CEnemies
    {
        public GameObject enemy;
        public int spawnLocationNumber;
    }
    private List<CEnemies> currentEnemies = new();
    private List<GameObject> deadEnemies = new();
    [SerializeField] public float spawnTimer;
    float currentSpawnTimer;
    private int nextEnemyIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInside = false;
        playerEntered = false;
        currentSpawnTimer = spawnTimer;
    }

    // Update is called once per frame
    void Update()
    {
        //The battle zone will do the following tasks:
        //#1: When the player enters, begin spawning waves of enemies. The number of enemies will be
        //equal to the number of spawn locations. The enemies will spawn on a constantly running timer.
        //For example, every 10 seconds, the game may check to see if the number of living enemies
        //is equal to the number of spawn locations. If the number of living enemies is 1 and the number
        //of spawn locations is 3, spawn two enemies at any unused spawn location. Each spawn location will
        //keep track of it's enemy, and will not spawn more than one at a time.
        //#2: When the player exits the battle zone, all remaining enemies will travel back to their
        //spawn locations.
        //#3: When the player dies, the battle zone will destroy all living and dead enemies, and reset
        //back to it's original state.
        //#4: When the player kills the last enemy of the zone, it will unlock entry to the next zone.
        if (playerInside)
        {
            currentSpawnTimer -= Time.deltaTime;
            if (currentSpawnTimer <= 0f && nextEnemyIndex < enemiesToSpawn.Length)
            {
                Debug.Log("Spawning Enemies, with " + currentEnemies.Count() + " enemies alive, spawning " + (spawnLocations.Length - currentEnemies.Count()) + " enemies");
                //Store the number of enemies to spawn, as currentEnemies.Count() will increase as the loop progresses, which causes issues if we compare directly
                int numOfEnemiesToSpawn = spawnLocations.Length - currentEnemies.Count();
                for (int i = 0; i < numOfEnemiesToSpawn; i++)
                {
                    if (nextEnemyIndex >= enemiesToSpawn.Length)
                    {
                        Debug.Log("WE'RE OUT OF ENEMIES TO SPAWN! IF THAT'S NOT TRUE, PANIC");
                        break;
                    }
                    Debug.Log("Spawning Enemy " + i);
                    spawnEnemy();
                }
                currentSpawnTimer = spawnTimer;
            }
        }
    }

    //Handle the player entering the battle zone and either spawning or reactivating enemies
    public void handlePlayerEntry()
    {
        //Spawn the first wave of enemies
        if (playerEntered == false)
        {
            playerEntered = true;
            for (int i = 0; i < spawnLocations.Length; i++)
            {
                spawnEnemy();
            }
        }
    }

    private void spawnEnemy()
    {
        //Lets do some weird number stuff here! We're gonna look through the list of enemies, find out every used up spawn point, and spawn the enemy at whatever one is unused
        int lowestSpawnLocation = 0;
        //First let's check if there are any spawn locations in use. If there aren't, we can simply spawn the next enemy at 0
        //Before that, quick error check if the battle zone isn't ready
        if (spawnLocations.Length == 0)
        {
            Debug.LogWarning("No Spawn Locations Available in " + name);
            return;
        }
        //Another error check to ensure there is an enemy available to spawn
        if (enemiesToSpawn[nextEnemyIndex] == null)
        {
            Debug.LogWarning("No Enemy Available At Index " + nextEnemyIndex + " in " + name);
            return;
        }
        if (currentEnemies.Count() == 0)
        {
            lowestSpawnLocation = 0;
        }
        //If there are enemies, find the lowest unused spawn location
        else
        {
            for (int i = 0; i < currentEnemies.Count(); i++)
            {
                for (int j = i; j < currentEnemies.Count(); j++)
                {
                    if (lowestSpawnLocation == currentEnemies[j].spawnLocationNumber)
                    {
                        lowestSpawnLocation++;
                        break;
                    }
                }
            }
        }
        if (lowestSpawnLocation >= spawnLocations.Length)
        {
            Debug.LogWarning("Lowest Spawn Location was greater than the number of spawn locations. Did you mean to call this?");
            return;
        }
        Debug.Log(lowestSpawnLocation);
        GameObject enemy = Instantiate( enemiesToSpawn[nextEnemyIndex],
                                        new Vector3(transform.position.x + spawnLocations[lowestSpawnLocation].x, transform.position.y + spawnLocations[lowestSpawnLocation].y, transform.position.z), new Quaternion());
        enemy.GetComponent<EnemyManager>().setBattleZone(gameObject);
        CEnemies newEnemy = new()
        {
            enemy = enemy,
            spawnLocationNumber = lowestSpawnLocation
        };
        //enemy.transform.position = new Vector2(transform.position.x + enemiesToSpawn[i].position.x, transform.position.y + enemiesToSpawn[i].position.y);
        currentEnemies.Add(newEnemy);
        nextEnemyIndex++;
    }

    public void removeEnemy(GameObject enemyToRemove, GameObject deadEnemy)
    {
        for (int i = 0; i < currentEnemies.Count(); i++)
        {
            if (currentEnemies[i].enemy.gameObject == enemyToRemove)
            {
                currentEnemies.Remove(currentEnemies[i]);
                deadEnemies.Add(deadEnemy);
            }
        }
    }

    public void removeEnemy(GameObject enemyToRemove)
    {
        for (int i = 0; i < currentEnemies.Count(); i++)
        {
            if (currentEnemies[i].enemy.gameObject == enemyToRemove)
            {
                currentEnemies.Remove(currentEnemies[i]);
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
