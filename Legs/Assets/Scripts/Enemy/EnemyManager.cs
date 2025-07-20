using System.Collections.Generic;
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
    [Tooltip("If the hammer attack is enabled, all other attacks will be disabled")]
    [SerializeField] public bool hammerAttack;
    [Header("Fish")]
    [Tooltip("For small fish. The fish will bounce whenever it hits the floor, and try to move towards the player.")]
    [SerializeField] public bool isFish;
    [Tooltip("For the Office Fish. Will jump and smash on top of the player.")]
    [SerializeField] public bool isOfficeFish;

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
    private GameObject battleZone;
    //To keep track of what direction the player should be thrown when dealing damage
    private int facingDirection = 1;
    private Vector2 spawnLocation;
    private bool touchingPlayer;

    private bool officeFishJump = false;
    private bool officeFishFall = false;
    private Vector2 officeFishJumpLocalCoordinates = new();
    private Vector2 officeFishStartCoordinates = new();
    private float officeFishTimer;
    private float officeFishFallTimer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = baseHealth;
        blinkTimer = Random.Range(blinkTimerRange.x, blinkTimerRange.y);
        player = GameStats.getPlayer();
        if (player == null)
        {
            Debug.LogWarning("Player not found");
        }
        damageCounterDirection = (int)Mathf.Floor(Random.Range(0, 2.9f));

        //Set up animator
        animator = GetComponent<Animator>();
        animator.SetBool("IsWalking", false);

        if (battleZone == null)
            spawnLocation = transform.position;
        touchingPlayer = false;

        //For debugging
        if (battleZone == null)
        {
            Debug.LogWarning(this.name + " object missing BattleZone reference");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        animator.speed = GameStats.isGamePaused();
        if (GameStats.isGamePaused() != 0f)
            enemyAI();
        else
            GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0f, 0f);
    }

    #region AI

    //Code related to the movement and behavior of the enemy
    private void enemyAI()
    {
        if (!isFish && !isOfficeFish)
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
                if (sideAttackBasic || verticalAttackBasic || hammerAttack)
                {
                    chargeAttack(playerInRange);
                    if (playerInRange)
                        return;
                }
                //If the enemy does not have any attacks enabled, give up on attacking. For the CEO.
                else
                    inAttackDelayMode = false;
            }
            //When the attack chargeup is over, attack
            if (inAttackChargeupMode == true && attackChargeupTimer < 0f)
            {
                attack();
                return;
            }
            //If the player is outside the battle zone, head back to the spawn location
            if (battleZone != null)
            {
                if (!battleZone.GetComponent<BattleZone>().playerInside)
                {
                    //If the enemy is not in the spawn location, walk towards it
                    playerFound = false;
                    if (spawnLocation.x - 0.2f > transform.position.x)
                    {
                        //Walk in that direction
                        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
                        GetComponent<Rigidbody2D>().linearVelocity = new Vector2(speed, 0);
                        enemy.transform.localScale = new Vector3(Mathf.Abs(enemy.transform.localScale.x) * -1, enemy.transform.localScale.y, enemy.transform.localScale.z);
                        facingDirection = -1;
                        animator.SetBool("IsWalking", true);
                        return;
                    }
                    if (spawnLocation.x + 0.2f < transform.position.x)
                    {
                        //Walk in that direction
                        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
                        GetComponent<Rigidbody2D>().linearVelocity = new Vector2(speed * -1, 0);
                        enemy.transform.localScale = new Vector3(Mathf.Abs(enemy.transform.localScale.x) * 1, enemy.transform.localScale.y, enemy.transform.localScale.z);
                        facingDirection = 1;
                        animator.SetBool("IsWalking", true);
                        return;
                    }
                    animator.SetBool("IsWalking", false);
                    GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
                    return;
                }
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
        //If the enemy is a small fish, behavior is completely different. It will hop in the direction
        //of the player and be a general nuisance
        else if (isFish)
        {
            //First, check to see if the player is in range
            if (visionTrigger.GetComponent<EnemyTriggers>().getPlayerInRange() == true)
            {
                GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
                //Now that we know the player is in range, bounce towards them
                //Check to see if the fish is on the ground. If it is, jump towards the player
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                //Create a list of colliders so that the GetContacts function will work
                List<Collider2D> newlist = new List<Collider2D>();
                Debug.Log("Fish " + name + " sees player.");
                if (rb.linearVelocityY < 0.05f && rb.linearVelocityY > -0.05f && rb.GetContacts(newlist) > 0)
                {
                    Debug.Log("Fish " + name + " launching at " + speed * facingDirection + ".");
                    rb.linearVelocityY = speed;
                    facingDirection = player.transform.position.x > transform.position.x ? 1 : -1;
                    rb.linearVelocityX = speed * facingDirection;
                    Debug.Log("Fish " + name + " launching at " + speed * facingDirection + ".");
                    enemy.transform.localScale = new Vector3(Mathf.Abs(enemy.transform.localScale.x) * -facingDirection, enemy.transform.localScale.y, enemy.transform.localScale.z);
                }
            }
            else
            {
                //The player is out of range. Go back to starting position.
                if (transform.position.x < spawnLocation.x - 0.3f)
                {
                    GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
                    Rigidbody2D rb = GetComponent<Rigidbody2D>();
                    List<Collider2D> newlist = new List<Collider2D>();
                    if (rb.linearVelocityY < 0.05f && rb.linearVelocityY > -0.05f && rb.GetContacts(newlist) > 0)
                        GetComponent<Rigidbody2D>().linearVelocity = new Vector2(speed * 1, speed);
                    enemy.transform.localScale = new Vector3(Mathf.Abs(enemy.transform.localScale.x) * -1, enemy.transform.localScale.y, enemy.transform.localScale.z);
                    facingDirection = -1;
                    return;
                }
                if (transform.position.x > spawnLocation.x + 0.3f)
                {
                    //Walk in that direction
                    GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
                    Rigidbody2D rb = GetComponent<Rigidbody2D>();
                    List<Collider2D> newlist = new List<Collider2D>();
                    if (rb.linearVelocityY < 0.05f && rb.linearVelocityY > -0.05f && rb.GetContacts(newlist) > 0)
                        GetComponent<Rigidbody2D>().linearVelocity = new Vector2(speed * -1, speed);
                    enemy.transform.localScale = new Vector3(Mathf.Abs(enemy.transform.localScale.x) * 1, enemy.transform.localScale.y, enemy.transform.localScale.z);
                    facingDirection = 1;
                    return;
                }
                GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX;
            }
        }
        //If the enemy is an office fish, it will leap in the air and smash down on the player.
        //Note for animations, Charge means switch to crouched position, Jump means switch to standing.
        #region Office Fish AI
        else if (isOfficeFish)
        {
            //Keep track if the enemy "blinked" this frame, and if it did, begin walking towards the player
            bool hasBlinked = false;
            const float OFFICE_FISH_TIMER = 1.6f;
            const float OFFICE_FISH_FALL_TIMER = 5f;
            const float OFFICE_FISH_JUMP_HEIGHT = 5f;
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
            if (officeFishJump)
                officeFishTimer -= Time.fixedDeltaTime;
            if (officeFishFall)
                officeFishFallTimer -= 1f;
            if (!touchingPlayer && !officeFishFall)
                GetComponent<BoxCollider2D>().enabled = true;

            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;

            if (officeFishFall && officeFishFallTimer >= 0f)
            {
                transform.position = new Vector2(transform.position.x, officeFishFallTimer / OFFICE_FISH_FALL_TIMER * OFFICE_FISH_JUMP_HEIGHT);
                if (officeFishFallTimer <= 0f)
                {
                    officeFishFall = false;
                    inAttackDelayMode = true;
                    attackDelayTimer = Random.Range(attackDelayRange.x, attackDelayRange.y);
                }
                GetComponent<BoxCollider2D>().enabled = false;
            }
            if (officeFishTimer <= 0f && officeFishJump)
            {
                officeFishTimer = 0f;
                officeFishJump = false;
                officeFishFall = true;
                officeFishFallTimer = OFFICE_FISH_FALL_TIMER;
                //HIT THE PLAYER!
                if (leftTrigger.GetComponent<EnemyTriggers>().playerInRange)
                {
                    player.GetComponent<PlayerManager>().takeDamage(1, -1);
                }
                else if (rightTrigger.GetComponent<EnemyTriggers>().playerInRange)
                {
                    player.GetComponent<PlayerManager>().takeDamage(1, 1);
                }
            }
            if (officeFishJump && officeFishTimer >= 0f)
            {
                //Here we need to calculate the fish's position based on the following formula:
                //positionRelativeToStart = (targetPosition / (totalTime^(1/4))) * ((facingDirection * currentTimer)^(1/4))
                Debug.Log("officeFishStartCoordinates: " + officeFishStartCoordinates + "  officeFishJumpLocalCoordinates: " + officeFishJumpLocalCoordinates + "  officeFishTimer: " + officeFishTimer + "  facingDirection: " + facingDirection);
                transform.position = new Vector2(
                    officeFishStartCoordinates.x - (officeFishJumpLocalCoordinates.x / Mathf.Pow(OFFICE_FISH_TIMER, 1f / 2f) * -1 * Mathf.Pow(OFFICE_FISH_TIMER - officeFishTimer, 1f / 2f)),
                    officeFishStartCoordinates.y - (officeFishJumpLocalCoordinates.y / Mathf.Pow(OFFICE_FISH_TIMER, 1f / 4f) * -1 * Mathf.Pow(OFFICE_FISH_TIMER - officeFishTimer, 1f / 4f)));

                return;

            }
            if (inAttackChargeupMode && attackChargeupTimer <= 0f)
            {
                inAttackChargeupMode = false;
                //Figure out where we're jumping to, and calculate how to get there
                //The fish has a range of how far it can jump. Find out if the player is within that range,
                //and if they are, jump to that coordinate. If not, jump to the farthest possible point.
                //At this point, the direction is already decided, so the fish will jump in that direction
                //no matter what. If it is the wrong direction, it will jump a shorter distance.
                float jumpRange = 8;
                float playerOffset = player.transform.position.x - transform.position.x;
                officeFishJump = true;
                officeFishStartCoordinates = transform.position;
                officeFishTimer = OFFICE_FISH_TIMER;
                animator.SetTrigger("Jump");
                if (facingDirection < 0)
                {
                    //We are facing left. Jump left.
                    if (Mathf.Abs(playerOffset) > jumpRange)
                    {
                        officeFishJumpLocalCoordinates = new Vector2(-jumpRange, OFFICE_FISH_JUMP_HEIGHT);
                    }
                    else if (playerOffset > 0)
                    {
                        //Jump shorter, as this is the wrong direction
                        officeFishJumpLocalCoordinates = new Vector2(-(jumpRange / 2), OFFICE_FISH_JUMP_HEIGHT);
                    }
                    else
                    {
                        //Player is in range, jump right above the player
                        officeFishJumpLocalCoordinates = new Vector2(playerOffset, OFFICE_FISH_JUMP_HEIGHT);
                    }
                }
                if (facingDirection > 0)
                {
                    //We are facing right. Jump right.
                    if (Mathf.Abs(playerOffset) > jumpRange)
                    {
                        officeFishJumpLocalCoordinates = new Vector2(jumpRange, OFFICE_FISH_JUMP_HEIGHT);
                    }
                    else if (playerOffset < 0)
                    {
                        //Jump shorter, as this is the wrong direction
                        officeFishJumpLocalCoordinates = new Vector2(jumpRange / 2, OFFICE_FISH_JUMP_HEIGHT);
                    }
                    else
                    {
                        //Player is in range, jump right above the player
                        officeFishJumpLocalCoordinates = new Vector2(playerOffset, OFFICE_FISH_JUMP_HEIGHT);
                    }
                }
                return;
            }
            if (inAttackDelayMode && attackDelayTimer <= 0f && !inAttackChargeupMode && !officeFishJump)
            {
                if (!visionTrigger.GetComponent<EnemyTriggers>().getPlayerInRange())
                {
                    inAttackDelayMode = false;
                    return;
                }
                inAttackDelayMode = false;
                inAttackChargeupMode = true;
                animator.SetTrigger("Charge");
                facingDirection = player.transform.position.x > transform.position.x ? 1 : -1;
                enemy.transform.localScale = new Vector3(Mathf.Abs(enemy.transform.localScale.x) * -facingDirection, enemy.transform.localScale.y, enemy.transform.localScale.z);
                attackChargeupTimer = attackChargeup;
                return;
            }
            if (visionTrigger.GetComponent<EnemyTriggers>().getPlayerInRange() == true && hasBlinked == true && !inAttackDelayMode && !inAttackChargeupMode && !officeFishJump)
            {
                inAttackDelayMode = true;
                facingDirection = player.transform.position.x > transform.position.x ? 1 : -1;
                enemy.transform.localScale = new Vector3(Mathf.Abs(enemy.transform.localScale.x) * -facingDirection, enemy.transform.localScale.y, enemy.transform.localScale.z);
                attackDelayTimer = Random.Range(attackDelayRange.x, attackDelayRange.y);
                return;
            }
        }

    }

    #endregion
    #endregion
    #region Attack

    private void chargeAttack(bool playerInRange)
    {
        if (playerInRange)
        {
            //Switch into chargeup animation
            if (hammerAttack)
            {
                animator.SetTrigger("HammerCharge");

            }
            else if (topTrigger.GetComponent<EnemyTriggers>().getPlayerInRange() == true && verticalAttackBasic)
            {
                animator.SetTrigger("UpwardCharge");
                attackingUp = true;
            }
            else if (sideAttackBasic)
            {
                animator.SetTrigger("ForwardCharge");
                attackingUp = false;
                //Find the direction the player is in, and face them
                int playerDirection;
                if (player.transform.position.x > transform.position.x)
                    playerDirection = 1;
                else
                    playerDirection = -1;
                enemy.transform.localScale = new Vector3(Mathf.Abs(enemy.transform.localScale.x) * -playerDirection, enemy.transform.localScale.y, enemy.transform.localScale.z);
                facingDirection = -playerDirection;
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
        //
        if (attackingUp && !hammerAttack)
        {
            animator.SetTrigger("UpwardAttack");
            if (topTrigger.GetComponent<EnemyTriggers>().getPlayerInRange() == true)
            {
                GameStats.getPlayer().GetComponent<PlayerManager>().takeDamage(1, -facingDirection);
            }
        }
        else
        {
            if (hammerAttack)
            {
                animator.SetTrigger("HammerAttack");
                if (topTrigger.GetComponent<EnemyTriggers>().getPlayerInRange() == true || leftTrigger.GetComponent<EnemyTriggers>().getPlayerInRange() == true || rightTrigger.GetComponent<EnemyTriggers>().getPlayerInRange() == true)
                {
                    GameStats.getPlayer().GetComponent<PlayerManager>().takeDamage(2, -facingDirection);
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
        }
        inAttackChargeupMode = false;
        inAttackDelayMode = false; /* Redundancy */
        return;
    }

    public bool takeDamage(int d)
    {
        health -= d;
        GameObject dam = Instantiate(damageCounter, new Vector2(transform.position.x, transform.position.y + (GetComponent<BoxCollider2D>().size.y + 0.5f)), new Quaternion());
        dam.GetComponent<DamageCounter>().setupDamage(damageCounterDirection);
        damageCounterDirection = damageCounterDirection >= 2 ? 0 : damageCounterDirection + 1;
        if (health <= 0)
        {
            killEnemy();
            return true;
        }
        return false;
    }

    public void killEnemy()
    {
        GameObject dead;
        if (deathPrefab != null)
        {
            dead = Instantiate(deathPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion());
            if (battleZone != null)
                battleZone.GetComponent<BattleZone>().removeEnemy(gameObject, dead);
        }
        else
        {
            if (battleZone != null)
                battleZone.GetComponent<BattleZone>().removeEnemy(gameObject);
        }
        Destroy(gameObject);
    }

    #endregion

    #region Collision

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            touchingPlayer = true;
            //When a fish hits the player, kill the fish and knock the player back
            if (isFish)
            {
                collision.gameObject.GetComponent<PlayerManager>().knockback(facingDirection);
                killEnemy();
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            touchingPlayer = false;
        }
    }

    #endregion

    public void setBattleZone(GameObject bz)
    {
        battleZone = bz;
    }

    public void setSpawnPosition(Vector2 spawn)
    {
        spawnLocation = spawn;
    }
}
