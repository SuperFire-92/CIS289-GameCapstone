using UnityEngine;

public class DamageCounter : MonoBehaviour
{
    [Tooltip("Number of frames the damage counter will last for")]
    public int lifeTimer;
    [Tooltip("Speed at which the damage counter rises")]
    public float speed;

    private float timer = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        if (timer >= lifeTimer)
        {
            Destroy(gameObject);
        }
        transform.Translate(new Vector3(0, speed / 100, 0));
        //Using a square root equation x = sqrt(k)*sqrt(-y+k) where k is the startingTimer and y is the lifeTimer
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, Mathf.Sqrt(lifeTimer) * Mathf.Sqrt(-timer + lifeTimer) / lifeTimer);
        timer++;
    }

    public void setupDamage(int direction)
    {
        //Point in a direction based on what is setn by the enemyManager
        //0 is left, 1 is straight up, 2 is to the right
        //!~_ Alternatively we could send these in a random direction _~!

        transform.eulerAngles = new Vector3(0, 0, 20 - (direction * 20));
        return;

    }
}
