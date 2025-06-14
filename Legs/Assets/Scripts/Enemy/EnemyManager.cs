using System.Transactions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Attributes")]
    [SerializeField] public int baseHealth;
    private int health;
    [SerializeField] public float speed;

    [Header("Timers")]
    [Tooltip("Time range in between checks for the player being in vision range (In Milliseconds)")]
    [SerializeField] public Vector2 blinkTimerRange;
    private float blinkTimer;
    private bool playerFound = false;
    [Tooltip("Time range for the enemy to idle before charging an attack (In Milliseconds)")]
    [SerializeField] public Vector2 attackDelayRange;
    private bool inAttackDelayMode = false;
    private float attackDelayTimer;
    [Tooltip("Time for an attack chargeup (In Milliseconds)")]
    [SerializeField] public float attackChargeup;
    private bool inAttackChargeupMode = false;
    private bool attackingUp = false;
    private float attackChargeupTimer;

    [Header("Attacks")]
    [SerializeField] public bool sideAttackBasic;
    [SerializeField] public bool verticalAttackBasic;

    [Header("References")]
    [SerializeField] public GameObject leftTrigger;
    [SerializeField] public GameObject rightTrigger;
    [SerializeField] public GameObject topTrigger;
    [SerializeField] public GameObject visionTrigger;
    [SerializeField] public GameObject enemy;
    [SerializeField] public GameObject deathPrefab;
    [SerializeField] public GameObject damageCounter;
    private int damageCounterDirection;
    private Animator animator;

    private GameObject player;
    //To keep track of what direction the player should be thrown when dealing damage
    private int facingDirection;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = baseHealth;
        blinkTimer = Random.Range(blinkTimerRange.x, blinkTimerRange.y);
        player = GameStats.getPlayer();
        damageCounterDirection = (int)Mathf.Floor(Random.Range(0, 2.9f));

        //Set up animator
        animator = GetComponent<Animator>();
        animator.SetBool("IsWalking", false);
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
    private void enemyAI()
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
        if (leftTrigger.GetComponent<EnemyTriggers>().getPlayerInRange() == true || rightTrigger.GetComponent<EnemyTriggers>().getPlayerInRange() == true || topTrigger.GetComponent<EnemyTriggers>().getPlayerInRange() == true)
        {
            playerInRange = true;
            //If the player just entered OR an attack just ended, switch into attack delay mode. If not, continue past this if statement
            if (inAttackDelayMode == false && inAttackChargeupMode == false)
            {
                animator.SetBool("IsWalking", false);
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
            chargeAttack(playerInRange);
            if (playerInRange)
                return;
        }
        //When the attack chargeup is over, attack
        if (inAttackChargeupMode == true && attackChargeupTimer < 0f)
        {
            attack();
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
            enemy.transform.localScale = new Vector3(Mathf.Abs(enemy.transform.localScale.x) * -playerDirection, enemy.transform.localScale.y, enemy.transform.localScale.z);
            facingDirection = -playerDirection;
            animator.SetBool("IsWalking", true);
            return;
        }
        //IDLING
    }

    private void chargeAttack(bool playerInRange)
    {
        if (playerInRange)
        {
            //Switch into chargeup animation
            if (topTrigger.GetComponent<EnemyTriggers>().getPlayerInRange() == true && verticalAttackBasic)
            {
                animator.SetTrigger("UpwardCharge");
                attackingUp = true;
            }
            else if (sideAttackBasic)
            {
                animator.SetTrigger("ForwardCharge");
                attackingUp = false;
            }
            //Start counting down until the attack
            inAttackChargeupMode = true;
            inAttackDelayMode = false;
            attackChargeupTimer = attackChargeup;
            return;
        }
        //If the player has left the range, switch out of delay mode without attacking
        else
            inAttackDelayMode = false;
    }

    private void attack()
    {
        //Deal damage to player and animate the attack
        if (attackingUp)
        {
            animator.SetTrigger("UpwardAttack");
            if (topTrigger.GetComponent<EnemyTriggers>().getPlayerInRange() == true)
            {
                GameStats.getPlayer().GetComponent<PlayerManager>().takeDamage(1, -facingDirection);
            }
        }
        else
        {
            animator.SetTrigger("ForwardAttack");
            if (leftTrigger.GetComponent<EnemyTriggers>().getPlayerInRange() == true && facingDirection == 1)
            {
                GameStats.getPlayer().GetComponent<PlayerManager>().takeDamage(1, -facingDirection);
            }
            if (rightTrigger.GetComponent<EnemyTriggers>().getPlayerInRange() == true && facingDirection == -1)
            {
                GameStats.getPlayer().GetComponent<PlayerManager>().takeDamage(1, -facingDirection);
            }
        }
        inAttackChargeupMode = false;
        inAttackDelayMode = false; /* Redundancy */
        return;
    }

    public void takeDamage(int d)
    {
        health -= d;
        GameObject dam = Instantiate(damageCounter, new Vector2(transform.position.x, transform.position.y + (GetComponent<BoxCollider2D>().size.y + 0.5f)), new Quaternion());
        dam.GetComponent<DamageCounter>().setupDamage(damageCounterDirection);
        damageCounterDirection = damageCounterDirection >= 2 ? 0 : damageCounterDirection + 1;
        if (health <= 0)
        {
            Debug.Log("I died lol");
            Instantiate(deathPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion());
            Destroy(gameObject);
        }
    }

    
}
