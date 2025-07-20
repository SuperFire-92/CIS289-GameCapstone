using UnityEngine;
using UnityEngine.UI;

public class HudButtons : MonoBehaviour
{
    enum MenuButtons
    {
        Start,
        Quit,
        Resume,
        Menu,
        GodMode
    }

    [SerializeField] private MenuButtons options;
    [SerializeField] private Sprite button;
    [SerializeField] private Sprite hoverButton;
    /// <summary>
    /// Only used if the button is set as a GodMode button
    /// </summary>
    private bool inGodMode;


    public string getButton()
    {
        if (options == MenuButtons.Start)
            return "Start";
        if (options == MenuButtons.Quit)
            return "Quit";
        if (options == MenuButtons.Resume)
            return "Resume";
        if (options == MenuButtons.Menu)
            return "Menu";
        if (options == MenuButtons.GodMode)
            return "GodMode";
        return null;
    }

    public void hover()
    {
        if (hoverButton != null)
            GetComponent<Image>().sprite = hoverButton;
    }

    public void unhover()
    {
        if (button != null)
            GetComponent<Image>().sprite = button;
    }

    public void clickGodMode()
    {
        inGodMode = GameStats.isGodMode();
        if (!inGodMode)
            GetComponent<Image>().color = new Color(0.4716f, 0.4716f, 0.4716f);
        else
            GetComponent<Image>().color = new Color(1f, 0f, 0f);
    }
}
