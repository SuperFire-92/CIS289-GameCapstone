using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Hud : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public InputActionAsset inputSystem;
    [SerializeField] private GameObject healthBack;
    [SerializeField] private GameObject health;
    [SerializeField] private GameObject healthText;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject menuStart;
    [SerializeField] private GameObject menuQuit;
    [SerializeField] private GameObject menuLogo;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject pauseResume;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject pauseGodMode;
    [SerializeField] private GameObject credits;
    [SerializeField] private GameObject mouse;

    private GameObject[] menuHud;
    private GameObject[] inGameHud;
    private GameObject[] pauseHud;
    private GameObject[] creditsHud;

    private int curGameMode;

    private InputAction hudMove;
    private InputAction hudInteract;

    void OnEnable()
    {
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inGameHud = new GameObject[] { healthBack, health, healthText };
        menuHud = new GameObject[] { menuPanel, menuStart, menuQuit, menuLogo };
        pauseHud = new GameObject[] { pausePanel, pauseResume, pauseMenu, pauseGodMode};
        creditsHud = new GameObject[] { credits };
        curGameMode = -1;
        hudMove = InputSystem.actions.FindAction("HUDMove");
        hudInteract = InputSystem.actions.FindAction("HUDInteract");
    }

    // Update is called once per frame
    void Update()
    {
        if (curGameMode != GameStats.getGameMode())
        {
            curGameMode = GameStats.getGameMode();
            mouse.SetActive(curGameMode == 0 || curGameMode == 2);
            //The gamemode has just changed. Set the activeness of each related
            //canvas object that needs to be swapped
            foreach (GameObject gameObject in menuHud)
            {
                gameObject.SetActive(curGameMode == 0);
            }
            foreach (GameObject gameObject in inGameHud)
            {
                gameObject.SetActive(curGameMode == 1);
            }
            foreach (GameObject gameObject in pauseHud)
            {
                gameObject.SetActive(curGameMode == 2);
            }
            foreach (GameObject gameObject in creditsHud)
            {
                gameObject.SetActive(curGameMode == 4);
            }
        }
        //Update every HUD menu based on whats currently being displayed
        if (curGameMode == 0)
        {
            updateMenuDisplay();
        }
        else if (curGameMode == 1)
        {
            updateHealthDisplay();
        }
        else if (curGameMode == 2)
        {
            updatePauseDisplay();
        }

    }

    private void updateMenuDisplay()
    {
        moveMouse();
    }

    private void updateHealthDisplay()
    {
        int playerHealth = GameStats.getPlayer().GetComponent<PlayerManager>().getHealth();
        health.transform.localScale = new Vector2((float)playerHealth / (GameStats.isGodMode() ? (float)GameStats.getGodModeHealth() : (float)GameStats.getStartingPlayerHealth()), health.transform.localScale.y);
        healthText.GetComponent<TextMeshProUGUI>().text = ((float)playerHealth / (float)GameStats.getStartingPlayerHealth() * 100) + "%";
    }

    private void updatePauseDisplay()
    {
        moveMouse();
    }

    private void moveMouse()
    {
        //Move the mouse either using the user's mouse or the user's gamepad left stick
        if (GameStats.getControlScheme() == "keyboard&mouse")
            mouse.transform.position = Input.mousePosition;
        else
        {
            Vector2 move = hudMove.ReadValue<Vector2>();
            if (move.x < -0.1f || move.x > 0.1f || move.y < -0.1f || move.y > 0.1f)
                mouse.transform.position = new Vector2(mouse.transform.position.x + move.x * Time.deltaTime * 300, mouse.transform.position.y + move.y * Time.deltaTime * 300);
        }

        //Check to see if a button on the menu was pressed, and if it was, perform that button's action
        if (hudInteract.WasPressedThisFrame())
        {
            GameObject menuButton = mouse.GetComponent<Mouse>().menuButton;
            if (menuButton != null)
            {
                //Perform an action based on what button was clicked
                string menuAction = menuButton.GetComponent<HudButtons>().getButton();
                if (menuAction == "Start")
                {
                    //Start the game, switch all related GameStats value to their defaults, and clean up any object that are left in the world (such as enemies)
                    GameStats.startGame();
                    GameStats.setGameMode(1);
                }
                else if (menuAction == "Quit")
                {
                    Application.Quit();
                }
                else if (menuAction == "Resume")
                {
                    GameStats.setGameMode(1);
                    GameStats.getPlayer().GetComponent<PlayerManager>().unpauseGame();
                }
                else if (menuAction == "Menu")
                {
                    GameStats.setGameMode(0);
                }
                else if (menuAction == "GodMode")
                {
                    GameStats.setGodMode(!GameStats.isGodMode());
                    menuButton.GetComponent<HudButtons>().clickGodMode();
                }
            }
        }
    }
}
