using UnityEngine;

public class Blacksmith : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Animator>().SetBool("Standing", false);
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Animator>().speed = GameStats.isGamePaused(); 
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GetComponent<Animator>().SetBool("Standing", true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GetComponent<Animator>().SetBool("Standing", false);
        }
    }
}
