using System;
using UnityEngine;

public class EnemyTriggers : MonoBehaviour
{
    /*[NonSerialized]*/
    public bool playerInRange;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    public bool getPlayerInRange()
    {
        return playerInRange;
    }
}
