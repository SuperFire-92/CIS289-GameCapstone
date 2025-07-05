using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    #region Variables

    [Header("Input System")]
    [SerializeField] public InputActionAsset inputSystem;

    [Header("Player Stats")]
    [Tooltip("The maximum speed at which the player can move")]
    [SerializeField] public float movementSpeed;
    [Tooltip("The speed at which the player increases speed (Each fixed update, this number is added to the current movement speed)")]
    [SerializeField] public float acceleration;
    [SerializeField] public float jumpHeight;
    [SerializeField] public bool canJump;

    [Header("Swords")]
    [SerializeField] public float swordRadius;
    [SerializeField] private int numOfSwords;

    [Header("Camera")]
    [SerializeField] public GameObject playerCamera;
    [SerializeField] public GameObject cityBackground;
    [SerializeField] public GameObject cloudBackground;
    [SerializeField] public float cameraSize;

    [Header("References")]

    [SerializeField] public GameObject sword;
    [SerializeField] public GameObject wheel;
    [SerializeField] public GameObject player;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction interactAction;

    private float move;
    private float attack;
    /// <summary>
    /// A number between -movementSpeed to movementSpeed
    /// </summary>
    private float currentMovementSpeed = 0;
    /// <summary>
    /// A number that matches movement speed but will increase (or decrease) if the player is not moving but instead attack
    /// </summary>
    private float currentAttackSpeed = 0;
    /// <summary>
    /// Keeps track of whether or not the player is at the maximum movement speed
    /// </summary>
    private bool maxSpeed = false;
    [SerializeField] private float dragScale = 1;
    [SerializeField] private float damageTimer = 0;
    private float damageCooldown = 0;
    private int health;
    private GameObject interactObject;

    /// <summary>
    /// Gameobject array to store any enemy thats currently in attacking range
    /// </summary>
    [SerializeField] private GameObject[] enemiesInRange;

    #endregion

    #region Start/Update

    void OnEnable()
    {
        //Enable the action map for controlling the player
        inputSystem.FindActionMap("PlayerMovement").Enable();
    }

    void Awake()
    {
        //Declare GameStats variables before anything else accesses them
        GameStats.setupVariables();
        GameStats.setPlayer(this.gameObject);
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Set up actions
        declareActions();
        //Create wheel of swords
        generateSwords(numOfSwords);
        //Set up variables
        enemiesInRange = new GameObject[0];
        health = GameStats.getStartingPlayerHealth();
    }

    // Update is called once per frame
    void Update()
    {
        jumpPlayer();
    }

    void FixedUpdate()
    {
        movePlayer();
        moveCamera();

    }

    #endregion

    #region Controls

    private void declareActions()
    {
        //Horizontal Movement
        moveAction = InputSystem.actions.FindAction("Move");
        //Jumping
        jumpAction = InputSystem.actions.FindAction("Jump");
        //Attacking
        attackAction = InputSystem.actions.FindAction("Attack");
        //Interacting
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    #endregion

    #region Player Movement

    private void jumpPlayer()
    {
        //Jump!
        if (jumpAction.WasPressedThisFrame() && canJump)
        {
            //Find out if the player should be able to jump
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            //Create a list of colliders so that the GetContacts function will work
            List<Collider2D> newlist = new List<Collider2D>();
            if (rb.linearVelocityY < 0.05f && rb.linearVelocityY > -0.05f && rb.GetContacts(newlist) > 0)
            {
                rb.linearVelocityY = jumpHeight;
            }
        }
    }

    private void movePlayer()
    {
        move = moveAction.ReadValue<float>();
        attack = attackAction.ReadValue<float>();

        //Switch which direction the player is facing
        if (move > 0.1f)
        {
            player.transform.localScale = new Vector3(Math.Abs(player.transform.localScale.x), player.transform.localScale.y, player.transform.localScale.z);
        }
        if (move < -0.1f)
        {
            player.transform.localScale = new Vector3(-Math.Abs(player.transform.localScale.x), player.transform.localScale.y, player.transform.localScale.z);
        }

        //Find out if the player is in the air. If they are, we will reduce their movement speed and drag significantly
        //For this, we'll do the same check we do to test if the player can jump
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        //Create a list of colliders so that the GetContacts function will work
        List<Collider2D> newlist = new();
        if (rb.linearVelocityY > 0.05f || rb.linearVelocityY < -0.05f || rb.GetContacts(newlist) <= 0)
        {
            dragScale = 0.1f;
        }
        else
        {
            dragScale = 1f;
        }


        //Increase or decrease currentMovementSpeed for a natural movement feel.
        if (move < 0.1f && move > -0.1f)
        {

            //When the user isnt trying to move but is trying to attack, increase the attack speed
            if (attack != 0f)
            {
                if (currentAttackSpeed >= 0f)
                {
                    currentAttackSpeed += acceleration;
                    if (currentAttackSpeed > movementSpeed)
                    {
                        currentAttackSpeed = movementSpeed;
                        maxSpeed = true;
                    }
                }
                if (currentAttackSpeed < 0f)
                {
                    currentAttackSpeed -= acceleration;
                    if (currentAttackSpeed < -movementSpeed)
                    {
                        currentAttackSpeed = -movementSpeed;
                        maxSpeed = true;
                    }
                }
            }
            //When the user is not trying to move, reduce or increase the currentMovementSpeed to 0.
            if (currentMovementSpeed > 0f)
            {
                currentMovementSpeed += -acceleration * dragScale;
                if (currentMovementSpeed < 0f)
                    currentMovementSpeed = 0f;
            }
            if (currentMovementSpeed < 0f)
            {
                currentMovementSpeed += acceleration * dragScale;
                if (currentMovementSpeed > 0f)
                    currentMovementSpeed = 0f;
            }
            //When the user is not attacking, reduce the attack speed to 0
            if (attack == 0f)
            {
                if (currentAttackSpeed < 0f)
                {
                    currentAttackSpeed += acceleration;
                    if (currentAttackSpeed > 0f)
                        currentAttackSpeed = 0f;
                }
                if (currentAttackSpeed > 0f)
                {
                    currentAttackSpeed += -acceleration;
                    if (currentAttackSpeed < 0f)
                        currentAttackSpeed = 0f;
                }
            }
        }
        else
        {
            //Calculate the number of frames until maximum speed, and move the attack speed to meet that speed at the same time
            //For this, we will use the equation y=((m-k)/(l))(x)+k, where
            //k is the currentAttackSpeed, l is the frames until maximum speed, m is the maximum speed, and x is 1 (the next frame)
            //First we need to calculate when the frame where maximum speed is achieved will be
            int maxSpeedFrame = 0;
            float calculatedSpeed = currentMovementSpeed;
            while (calculatedSpeed < movementSpeed && calculatedSpeed > -movementSpeed && move != 0f)
            {
                calculatedSpeed += move * acceleration;
                if (calculatedSpeed <= movementSpeed && move > 0.1f)
                {
                    maxSpeedFrame++;
                }
                if (calculatedSpeed >= -movementSpeed && move < -0.1f)
                {
                    maxSpeedFrame++;
                }
            }
            //Now we run the equation to find out how much speed to add this frame
            float addedRotationSpeed;
            //If we are already at max speed, just set the rotation speed to the current movement speed, or else we will divide by 0
            if (maxSpeedFrame == 0)
                addedRotationSpeed = currentMovementSpeed;
            else
                addedRotationSpeed = ((movementSpeed * move - currentAttackSpeed) / maxSpeedFrame) + currentAttackSpeed;
            currentAttackSpeed = addedRotationSpeed;
            currentMovementSpeed += move * acceleration * dragScale;
        }
        //Set movement speed to maximum if they exceed the maximum
        if (currentMovementSpeed > movementSpeed)
        {
            currentMovementSpeed = movementSpeed;
            maxSpeed = true;
        }
        else if (currentMovementSpeed < -movementSpeed)
        {
            currentMovementSpeed = -movementSpeed;
            maxSpeed = true;
        }
        else if (currentAttackSpeed != movementSpeed && currentAttackSpeed != -movementSpeed)
        {
            maxSpeed = false;
        }



        GetComponent<Rigidbody2D>().linearVelocityX = currentMovementSpeed;
        //Get the distance rotated, and rotate the wheel
        float startRotation = wheel.transform.eulerAngles.z;
        wheel.transform.Rotate(new Vector3(0, 0, -currentAttackSpeed));
        float distanceRotated = wheel.transform.eulerAngles.z - startRotation;
        damageTimer += distanceRotated;
        //Without the damageCooldown, enemies take damage more frequently than they should. Not really sure why.
        //Higher sword counts may cause issues down the line with this solution.
        damageCooldown -= 1;
        if (damageTimer < 0f)
        {
            damageTimer += 360f / numOfSwords;
            if (damageCooldown < 0f)
            {
                damageCooldown = 10f;
                dealDamage();
            }

        }
        else if (damageTimer > 360f / numOfSwords)
        {
            damageTimer -= 360f / numOfSwords;
            if (damageCooldown < 0f)
            {
                damageCooldown = 10f;
                dealDamage();
            }
        }
        bobPlayer();
    }

    //Bobs the player up and down based on how their swords are facing
    private void bobPlayer()
    {
        //This solution uses the x position of the player to calculate the bob, which may become inaccurate if we teleport the player.
        // float scaleOfBob = 16f;
        // float lateralAdjustment = (Mathf.Cos(2f * numOfSwords * Mathf.PI * transform.position.x / 7f) / scaleOfBob) - (1f / scaleOfBob);
        // Debug.Log("LATERAL: " + lateralAdjustment);
        // player.transform.localPosition = new Vector2(player.transform.localPosition.x, 0 + lateralAdjustment);
        // wheel.transform.localPosition = new Vector2(wheel.transform.localPosition.x, -0.55f + lateralAdjustment);

        //Instead, let's take the rotation of the wheel and translate it into a number from 0-7. The number ranges from 0 to 360.
        //Lets take the rotation, add 90 (upon spawning, the wheel is at 270 degrees, so this evens out the equation), and use the equation 7*rot/360 to find the location on the graph.
        float scaleOfBob = 16f;
        float lateralAdjustment = (Mathf.Cos(2f * numOfSwords * Mathf.PI * (7 * (wheel.transform.eulerAngles.z + 90) / 360) / 7f) / scaleOfBob) - (1f / scaleOfBob);
        player.transform.localPosition = new Vector2(player.transform.localPosition.x, 0 + lateralAdjustment);
        wheel.transform.localPosition = new Vector2(wheel.transform.localPosition.x, 0.7f + lateralAdjustment);
    }

    #endregion

    #region Camera

    private void setupCamera()
    {
        //Get the starting camera and background details
    }

    private void moveCamera()
    {
        //Set the camera to match the position of the player
        //playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + 1.1f, playerCamera.transform.position.z);
        //First, lets get the coordinates of the current camera. Then we want to set the new coordinates of the camera to be where the player is.
        //If the camera edges would exceed the limits of where the camera should be allowed to go, set the camera's X position to be the closest point it can be without exceeding the limits.
        //As I do not know how the camera calculates its edges, and it can be different depending on the ratio of the camera, i will instead set the X or Y back to how it was in the previous frame.
        //This should give a decent illusion of an limit on the camera, though where it stops might be off by a few pixels every time the player approaches the edge. 
        //Using this method, we also have to make sure the camera never spawns in an illegal spot, or else it will remain stuck in that uncomfortable spot until it can snap into a legal position.
        Camera cam = playerCamera.GetComponent<Camera>();
        //Vector3 lastFramePosition = playerCamera.transform.position;
        playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + 1.1f, playerCamera.transform.position.z);
        //Get the outer limits of the camera
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        float lowestX = bottomLeft.x;
        float lowestY = bottomLeft.y;
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        float highestX = topRight.x;
        float highestY = topRight.y;
        //Get the cameralimits that are stored in GameStats
        Vector4 cameraLimits = GameStats.getCameraLimits();

        //Now we do 4 comparisons to see if the camera needs to be snapped back
        // if (lowestX < cameraLimits.x || highestX > cameraLimits.z)
        // {
        //     //Snap back on the X
        //     playerCamera.transform.position = new Vector3(lastFramePosition.x, playerCamera.transform.position.y, playerCamera.transform.position.z);
        // }
        // if (lowestY < cameraLimits.y || highestY > cameraLimits.w)
        // {
        //     //Snap back on the Y
        //     playerCamera.transform.position = new Vector3(playerCamera.transform.position.x, lastFramePosition.y, playerCamera.transform.position.z);
        // }
        //Instead of this solution, lets use the size and aspect provided by the camera to calculate where it should go
        if (lowestX < cameraLimits.x)
        {
            //Put the left of the camera to the limit
            playerCamera.transform.position = new Vector3(cameraLimits.x + cam.orthographicSize * cam.aspect, playerCamera.transform.position.y, playerCamera.transform.position.z);
        }
        else if (highestX > cameraLimits.z)
        {
            //Put the right of the camera to the limit
            playerCamera.transform.position = new Vector3(cameraLimits.z - cam.orthographicSize * cam.aspect, playerCamera.transform.position.y, playerCamera.transform.position.z);
        }
        if (lowestY < cameraLimits.y)
        {
            //Put the bottom of the camera to the limit
            playerCamera.transform.position = new Vector3(playerCamera.transform.position.x, cameraLimits.y + cam.orthographicSize, playerCamera.transform.position.z);
        }
        else if (highestY > cameraLimits.w)
        {
            //Put the top of the camera to the limit
            playerCamera.transform.position = new Vector3(playerCamera.transform.position.x, cameraLimits.w - cam.orthographicSize, playerCamera.transform.position.z);
        }
        float cityDistance = 2f;
        float cloudDistance = 1.05f;
        cityBackground.transform.position = new Vector3(playerCamera.transform.position.x / cityDistance, (playerCamera.transform.position.y + 0.75f * cityDistance * 3f) / (cityDistance * 3f), 0);
        cloudBackground.transform.position = new Vector3(playerCamera.transform.position.x / cloudDistance, (playerCamera.transform.position.y + 7f * cloudDistance) / cloudDistance, 0);
    }

    #endregion

    #region Swords

    private void generateSwords(int swords)
    {
        numOfSwords = swords;
        for (int i = 0; i < numOfSwords; i++)
        {
            //x(n) = x(0) + r*cos(2*pi*n/N) where x(0) is the center of the circle, r is radius, n is the iteration and N is the number of points to find
            float x = 0f + swordRadius * Mathf.Cos(2 * Mathf.PI * i / numOfSwords);
            //y(n) = y(0) + r*sin(2*pi*n/N) where y(0) is the center of the circle, r is radius, n is the iteration and N is the number of points to find
            float y = 0f + swordRadius * Mathf.Sin(2 * Mathf.PI * i / numOfSwords);
            //Get the rotation of the sword
            float rot = Mathf.Atan2(y, x) * 180 / Mathf.PI;

            GameObject thisSword = Instantiate(sword, wheel.transform, worldPositionStays: false);
            thisSword.transform.localPosition = new Vector2(thisSword.transform.localPosition.x + x, thisSword.transform.localPosition.y + y);
            thisSword.transform.eulerAngles = new Vector3(0, 0, rot - 180);
        }
    }

    #endregion

    #region Gizmos

    void OnDrawGizmosSelected()
    {
        //Draw the location of the swords
        Gizmos.color = Color.gray4;

        for (int i = 0; i < numOfSwords; i++)
        {
            float x = 0f + swordRadius * Mathf.Cos(2 * Mathf.PI * i / numOfSwords);
            float y = 0f + swordRadius * Mathf.Sin(2 * Mathf.PI * i / numOfSwords);

            Gizmos.DrawSphere(new Vector2(wheel.transform.position.x + x, wheel.transform.position.y + y), 0.1f);
        }

        //Draw the location of the camera boundries
        Gizmos.color = Color.blueViolet;
        Vector4 cameraLimits = GameStats.getCameraLimits();

        Gizmos.DrawLine(new Vector3(cameraLimits.x, cameraLimits.y, 0), new Vector3(cameraLimits.x, cameraLimits.w));
        Gizmos.DrawLine(new Vector3(cameraLimits.x, cameraLimits.w, 0), new Vector3(cameraLimits.z, cameraLimits.w));
        Gizmos.DrawLine(new Vector3(cameraLimits.z, cameraLimits.w, 0), new Vector3(cameraLimits.z, cameraLimits.y));
        Gizmos.DrawLine(new Vector3(cameraLimits.z, cameraLimits.y, 0), new Vector3(cameraLimits.x, cameraLimits.y));
    }

    #endregion

    #region Attacking/Enemies

    public void enemyEntered(GameObject newEnemy)
    {
        GameObject[] tempArray = new GameObject[enemiesInRange.Length + 1];
        for (int i = 0; i < enemiesInRange.Length; i++)
        {
            // if (enemiesInRange[i] == null)
            //     continue;
            tempArray[i] = enemiesInRange[i];
        }
        tempArray[tempArray.Length - 1] = newEnemy;
        enemiesInRange = tempArray;
    }

    public void enemyExited(GameObject exitingEnemy)
    {
        GameObject[] tempArray = new GameObject[enemiesInRange.Length - 1];
        for (int i = 0, j = 0; i < enemiesInRange.Length; i++)
        {
            if (exitingEnemy == enemiesInRange[i] || enemiesInRange[i] == null)
            {
                continue;
            }
            else
            {
                tempArray[j] = enemiesInRange[i];
                j++;
            }
        }
        enemiesInRange = tempArray;
    }

    private void dealDamage()
    {
        if (maxSpeed)
        {
            for (int i = 0; i < enemiesInRange.Length; i++)
            {
                // if (enemiesInRange[i] == null)
                // {
                //     continue;
                // }
                if (enemiesInRange[i].GetComponent<EnemyManager>().takeDamage(1))
                {
                    i--;
                }
            }
        }
    }

    public void takeDamage(int damage, int direction)
    {
        currentMovementSpeed = movementSpeed * direction;
        GetComponent<Rigidbody2D>().linearVelocityY = 12f;
    }

    #endregion

    #region Interactions

    public void setInteractObject(GameObject interaction)
    {
        interactObject = interaction;
    }

    public void removeInteractObject(GameObject interaction)
    {
        if (interactObject == interaction)
        {
            interactObject = null;
        }
    }

    /// <summary>
    /// Handle all interaction inputs, and trigger gateways and dialogues
    /// </summary>
    public void interactPlayer()
    {
        if (interactAction.WasPressedThisFrame())
        {
            if (interactObject == null)
            {
                return;
            }
            if (interactObject.CompareTag("Gateway"))
            {
                //PERFORM TRANSITION TO NEXT ZONE
            }
            if (interactObject.CompareTag("Dialogue"))
            {
                //BEGIN DIALOGUE INTERACTION
            }
        }
    }

    #endregion
}
