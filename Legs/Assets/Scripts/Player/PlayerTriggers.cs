using UnityEngine;

public class PlayerTriggers : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerManager;

    //Here we need to collect each enemy that is within range of the attacks, and store them in an array, which will be in the PlayerManager. Then remove each enemy when they leave the range.
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            playerManager.GetComponent<PlayerManager>().enemyEntered(collision.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            playerManager.GetComponent<PlayerManager>().enemyExited(collision.gameObject);
        }
    }
}
