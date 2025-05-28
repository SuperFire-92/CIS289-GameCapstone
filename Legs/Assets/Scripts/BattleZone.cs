using System;
using Unity.VisualScripting;
using UnityEngine;

public class BattleZone : MonoBehaviour
{
    //This variable 
    [NonSerialized] public bool playerInside;
    [NonSerialized] public bool playerEntered;
    //This will store every enemy that is affiliated with the battlezone, and activate/deactivate them when needed.
    [System.Serializable]
    public struct Enemy
    {
        public int health;
        public Vector2 position;
        public GameObject enemyGameObject;
    }
    [SerializeField] private Enemy[] enemies;
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
        //Here we check to see if its the first time that the player has entered the region. If it is, spawn the enemies anew. If it is not, turn the enemy's AI back on.
        if (playerEntered == false)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].enemyGameObject == null)
                {
                    Debug.LogWarning("Battlezone Enemy " + i + " Missing Gameobject");
                    continue;
                }
                GameObject enemy = Instantiate(enemies[i].enemyGameObject);
                enemy.transform.position = new Vector2(transform.position.x + enemies[i].position.x, transform.position.y + enemies[i].position.y);
                // -- ENEMY HEALTH SET HERE --
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

        for (int i = 0; i < enemies.Length; i++)
        {
            Gizmos.DrawSphere(enemies[i].position + new Vector2(transform.position.x, transform.position.y), .2f);
        }
    } 
}
