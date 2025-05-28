using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Attributes")]
    [SerializeField] public int baseHealth;
    [SerializeField] public float speed;

    [Header("Timers")]
    [Tooltip("Time range in between checks for the player being in vision range (In Milliseconds)")]
    [SerializeField] public Vector2 blinkTimerRange;
    [Tooltip("Time range for the enemy to idle before charging an attack (In Milliseconds)")]
    [SerializeField] public Vector2 attackDelayRange;
    [Tooltip("Time for an attack chargeup (In Milliseconds)")]
    [SerializeField] public float attackChargeup;

    [Header("Attacks")]
    [SerializeField] public bool sideAttackBasic;
    [SerializeField] public bool verticalAttackBasic;

    [Header("References")]
    [SerializeField] public GameObject leftTrigger;
    [SerializeField] public GameObject rightTrigger;
    [SerializeField] public GameObject topTrigger;
    [SerializeField] public GameObject visionTrigger;

    private int health;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = baseHealth;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        if (leftTrigger.GetComponent<EnemyTriggers>().playerInRange == true || rightTrigger.GetComponent<EnemyTriggers>().playerInRange == true)
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
        }
    }
}
