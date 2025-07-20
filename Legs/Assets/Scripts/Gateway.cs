using UnityEngine;

public class Gateway : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject controlSymbolL;
    [SerializeField] private GameObject controlSymbolR;
    [SerializeField] private Sprite keySprite;
    [SerializeField] private Sprite gamepadSprite;

    [Header("Zone Numbers")]
    [Tooltip("Zone to move to if the player is proceeding through the game")]
    [SerializeField] public int zoneForward;
    [Tooltip("Zone to move to if the player is going backwards")]
    [SerializeField] public int zoneBackward;
    [SerializeField] public bool finalZone;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controlSymbolL.SetActive(false);
        controlSymbolR.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void playerEnter()
    {
        if ((GameStats.getPlayerLevel() >= zoneForward || GameStats.isGodMode()) && !GameStats.isInFinalZone())
        {
            controlSymbolL.SetActive(true);
            controlSymbolL.GetComponent<UnityEngine.UI.Image>().sprite = GameStats.getControlScheme() == "keyboard&mouse" ? keySprite : gamepadSprite;
            controlSymbolR.SetActive(true);
            controlSymbolR.GetComponent<UnityEngine.UI.Image>().sprite = GameStats.getControlScheme() == "keyboard&mouse" ? keySprite : gamepadSprite;
        }
    }
    public void playerExit()
    {
        controlSymbolL.SetActive(false);
        controlSymbolR.SetActive(false);
    }
}
