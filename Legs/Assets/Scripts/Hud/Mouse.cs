using UnityEngine;

public class Mouse : MonoBehaviour
{
    public GameObject menuButton = null;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("HUDButton"))
        {
            menuButton = collision.gameObject;
            collision.gameObject.GetComponent<HudButtons>().hover();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("HUDButton"))
        {
            menuButton = null;
            collision.gameObject.GetComponent<HudButtons>().unhover();
        }
    }
}
