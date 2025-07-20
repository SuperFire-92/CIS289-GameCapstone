using UnityEngine;

public class PlayerTriggers : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerManager;
    [SerializeField] private bool enemyTrigger;
    [SerializeField] private bool gatewayTrigger;
    [SerializeField] private bool playerCollider;

    //Here we need to collect each enemy that is within range of the attacks, and store them in an array, which will be in the PlayerManager. Then remove each enemy when they leave the range.
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && enemyTrigger)
        {
            playerManager.GetComponent<PlayerManager>().enemyEntered(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("Gateway") && gatewayTrigger)
        {
            playerManager.GetComponent<PlayerManager>().setInteractObject(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("EndCutscene") && playerCollider)
        {
            playerManager.GetComponent<PlayerManager>().enteredEndCutscene();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && enemyTrigger)
        {
            playerManager.GetComponent<PlayerManager>().enemyExited(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("Gateway") && gatewayTrigger)
        {
            playerManager.GetComponent<PlayerManager>().removeInteractObject(collision.gameObject);
        }
    }
}
