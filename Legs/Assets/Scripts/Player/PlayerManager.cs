using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    #region Variables

    [Header("Input System")]
    [SerializeField] public InputActionAsset inputSystem;

    [Header("Player Stats")]
    [SerializeField] public int health;
    [SerializeField] public float movementSpeed;
    [SerializeField] public float acceleration;


    [Header("References")]
    [SerializeField] public GameObject playerCamera;
    [SerializeField] public GameObject sword;
    [SerializeField] public GameObject wheel;
    [SerializeField] public GameObject player;

    [Header("Swords")]
    [SerializeField] public float swordRadius;
    [SerializeField] private int numOfSwords;

    private InputAction moveAction;

    private float move;
    //A number between -movementSpeed to movementSpeed
    private float currentMovementSpeed = 0;

    //Gameobject array to store any enemy thats currently in attacking range
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
    }

    // Update is called once per frame
    void Update()
    {

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
    }

    private void movePlayer()
    {
        move = moveAction.ReadValue<float>();

        //Switch which direction the player is facing
        if (move > 0.1f)
        {
            player.transform.localScale = new Vector3(Math.Abs(player.transform.localScale.x), player.transform.localScale.y, player.transform.localScale.z);
        }
        if (move < -0.1f)
        {
            player.transform.localScale = new Vector3(-Math.Abs(player.transform.localScale.x), player.transform.localScale.y, player.transform.localScale.z);
        }

        //Increase or decrease currentMovementSpeed for a natural movement feel.
        if (move < 0.1f && move > -0.1f && currentMovementSpeed != 0)
        {
            //When the user is not trying to move, reduce or increase the currentMovementSpeed to 0.
            if (currentMovementSpeed > 0)
            {
                currentMovementSpeed += -acceleration;
                if (currentMovementSpeed < 0)
                    currentMovementSpeed = 0;
            }
            if (currentMovementSpeed < 0)
            {
                currentMovementSpeed += acceleration;
                if (currentMovementSpeed > 0)
                    currentMovementSpeed = 0;
            }
        }
        else
            currentMovementSpeed += move * acceleration;
        if (currentMovementSpeed > movementSpeed)
        {
            currentMovementSpeed = movementSpeed;
        }
        if (currentMovementSpeed < -movementSpeed)
        {
            currentMovementSpeed = -movementSpeed;
        }

        this.GetComponent<Rigidbody2D>().linearVelocityX = currentMovementSpeed;
        wheel.transform.Rotate(new Vector3(0, 0, -currentMovementSpeed));
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
        wheel.transform.localPosition = new Vector2(wheel.transform.localPosition.x, -0.55f + lateralAdjustment);
    }

    #endregion

    #region Camera

    private void moveCamera()
    {
        //Set the camera to match the position of the player
        playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y, playerCamera.transform.position.z);
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

            Gizmos.DrawSphere(new Vector2(wheel.transform.position.x + x, wheel.transform.position.y + y), .1f);
        }
    }

    #endregion

    #region Attacking/Enemies

    public void enemyEntered(GameObject newEnemy)
    {
        GameObject[] tempArray = new GameObject[enemiesInRange.Length + 1];
        for (int i = 0; i < enemiesInRange.Length; i++)
        {
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
            if (exitingEnemy == enemiesInRange[i])
            {
                continue;
            }
            else
            {
                tempArray[j] = enemiesInRange[i];
            }
        }
        enemiesInRange = tempArray;
    }

    #endregion
}
