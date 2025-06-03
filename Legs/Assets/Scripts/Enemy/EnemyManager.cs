using System.Transactions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Attributes")]
    [SerializeField] public int baseHealth;
    [SerializeField] public float speed;

    [Header("Timers")]
    [Tooltip("Time range in between checks for the player being in vision range (In Milliseconds)")]
    [SerializeField] public Vector2 blinkTimerRange;
    /**/ [SerializeField] private float blinkTimer;
    /**/ [SerializeField] private bool playerFound = false;
    [Tooltip("Time range for the enemy to idle before charging an attack (In Milliseconds)")]
    [SerializeField] public Vector2 attackDelayRange;
    /**/ [SerializeField] private bool inAttackDelayMode = false;
    /**/ [SerializeField] private float attackDelayTimer;
    [Tooltip("Time for an attack chargeup (In Milliseconds)")]
    [SerializeField] public float attackChargeup;
    /**/ [SerializeField] private bool inAttackChargeupMode = false;
    /**/ [SerializeField] private float attackChargeupTimer;

    [Header("Attacks")]
    [SerializeField] public bool sideAttackBasic;
    [SerializeField] public bool verticalAttackBasic;

    [Header("References")]
    [SerializeField] public GameObject leftTrigger;
    [SerializeField] public GameObject rightTrigger;
    [SerializeField] public GameObject topTrigger;
    [SerializeField] public GameObject visionTrigger;

    private int health;
    private GameObject player;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = baseHealth;
        blinkTimer = Random.Range(blinkTimerRange.x, blinkTimerRange.y);
        player = GameStats.getPlayer();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        enemyAI();
    }

    //Code related to the movement and behavior of the enemy
    void enemyAI()
    {
        //Keep track of whether or not the player is next to the enemy
        bool playerInRange = false;
        //Keep track if the enemy "blinked" this frame, and if it did, begin walking towards the player
        bool hasBlinked = false;
        //Count down each timer if necessary
        blinkTimer -= Time.fixedDeltaTime;
        if (blinkTimer < 0f)
        {
            blinkTimer = Random.Range(blinkTimerRange.x, blinkTimerRange.y);
            hasBlinked = true;
        }
        if (inAttackDelayMode)
            attackDelayTimer -= Time.fixedDeltaTime;
        if (inAttackChargeupMode)
            attackChargeupTimer -= Time.fixedDeltaTime;

        //When the player is directly next to the enemy, stop moving and start counting down until an attack
        if (leftTrigger.GetComponent<EnemyTriggers>().getPlayerInRange() == true || rightTrigger.GetComponent<EnemyTriggers>().getPlayerInRange() == true)
        {
            playerInRange = true;
            //If the player just entered OR an attack just ended, switch into attack delay mode. If not, continue past this if statement
            if (inAttackDelayMode == false && inAttackChargeupMode == false)
            {
                GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
                inAttackDelayMode = true;
                attackDelayTimer = Random.Range(attackDelayRange.x, attackDelayRange.y);
                return;
            }
        }
        else
        {
            //GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
            // _~! POTENTIALLY ADD "FAST ENEMY" HERE, CHANGE MODE IMMEDIATELY UPON PLAYER MOVING !~_
            //inAttackDelayMode = false;
        }
        //If attackDelay is just ending, move into attack mode or cancel the attack
        if (inAttackDelayMode == true && attackDelayTimer < 0f && inAttackChargeupMode == false)
        {

            if (playerInRange == true)
            {
                //_~! SWITCH INTO CHARGEUP ANIMATION !~_
                inAttackChargeupMode = true;
                inAttackDelayMode = false;
                attackChargeupTimer = attackChargeup;
                return;
            }
            //If the player has left the range, switch out of delay mode without attacking
            if (playerInRange == false)
                inAttackDelayMode = false;
        }
        //When the attack chargeup is over, attack
        if (inAttackChargeupMode == true && attackChargeupTimer < 0f)
        {
            //_~! DEAL DAMAGE TO THE PLAYER !~_
            inAttackChargeupMode = false;
            inAttackDelayMode = false; /* Redundancy */
            return;
        }
        //When the player is in the vision range, walk in that direction
        //Lower priority, if anything above this has triggered, we do not need to worry about walking as the player is too close
        if (hasBlinked && !inAttackChargeupMode && !inAttackDelayMode && !playerInRange)
        {
            if (visionTrigger.GetComponent<EnemyTriggers>().getPlayerInRange() == true)
            {
                playerFound = true;
                //Delay a frame before we start walking
                return;
            }
            else
                playerFound = false;
        }
        if (playerFound && !inAttackChargeupMode && !inAttackDelayMode && !playerInRange)
        {
            //Find the direction the player is in
            int playerDirection;
            if (player.transform.position.x > transform.position.x)
                playerDirection = 1;
            else
                playerDirection = -1;
            //Now walk in that direction
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
            GetComponent<Rigidbody2D>().linearVelocity = new Vector2(speed * playerDirection, 0);
            return;
        }
        //IDLING
    }
}
