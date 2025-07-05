using System;
using TMPro;
using UnityEngine;

public class DialogueObject : MonoBehaviour
{
    //Dialogue is going to be pretty simple. When a player approaches a spot,
    //dialogue will start appearing over whatever character is talking. The player
    //will have no options to respond, but instead will just be listening. Dialogue
    //will appear one letter at a time, and once a dialogue box fully appears, it
    //will stick around for a moment before fading as the next dialogue box starts.
    //Unfortunately, we'll probably need an asset for each individual character.
    //How I'm thinking of doing this is having a hash map type of thing, where each
    //character in a string will be the key for it's affiliated sprite. Alternatively,
    //we might be able to use a built in Unity text display, but I'm not sure if I
    //can achieve the effect I want with that.

    //After some quick research, I discovered that canvases can be placed in worldspace.
    //So I can use a built-in Unity font (bleh but oh well).

    [Header("References")]
    [SerializeField] private GameObject textBox;
    [SerializeField] private GameObject text;
    [SerializeField] private GameObject trigger;
    [Header("Dialogue")]
    [Tooltip("Keep dialogues short to prevent overflow.")]
    [SerializeField] private string[] dialogues;
    [Tooltip("This will be read off after the player finishes the original dialogue\nGood for restating important aspects, like controls")]
    [SerializeField] private string[] postDialogue;


    private float dialogueTimer;
    private int currentDialogue;
    private float timeBetweenCharacters;
    private bool typingParagraph;
    private float waitingTime;
    private bool inPost = false;

    private void resetDialogue()
    {
        dialogueTimer = 0f;
        currentDialogue = 0;
        timeBetweenCharacters = 0.1f;
        typingParagraph = false;
        waitingTime = 12f;
        text.GetComponent<TextMeshProUGUI>().text = "";
    }

    void Start()
    {
        resetDialogue();
    }

    void Update()
    {
        //The dialogue system is going to be as basic as possible. There will be a timer object, and as that timer progresses,
        //more dialogue within the current dialogue paragraph will become visible. Each character will appear
        //after a certain amount of time has passed. Each dialogue will be stored in a string array called dialogues.
        //So for example, dialogues[0][5] would appear after the timer has reached 0.5f or beyond if the time between
        //each character is 0.1f.
        if (trigger.GetComponent<EnemyTriggers>().getPlayerInRange() || typingParagraph)
        {
            textBox.SetActive(true);
            string[] contextDialogues;
            if (inPost)
                contextDialogues = postDialogue;
            else
                contextDialogues = dialogues;
            if (currentDialogue < contextDialogues.Length)
            {
                typingParagraph = true;
                dialogueTimer += Time.deltaTime;
                int numOfCharacters = (int)Mathf.Floor(dialogueTimer / timeBetweenCharacters);
                numOfCharacters = Mathf.Min(numOfCharacters, contextDialogues[currentDialogue].Length);
                string textToDisplay = contextDialogues[currentDialogue].Substring(0, numOfCharacters);
                if (textToDisplay.Contains('<'))
                {
                    int i = contextDialogues[currentDialogue].IndexOf('<', 0);
                    int j = contextDialogues[currentDialogue].IndexOf('>', 0);
                    numOfCharacters += j - i;
                    numOfCharacters = Mathf.Min(numOfCharacters, contextDialogues[currentDialogue].Length);
                    textToDisplay = contextDialogues[currentDialogue].Substring(0, numOfCharacters);
                }
                if (dialogueTimer / timeBetweenCharacters > contextDialogues[currentDialogue].Length + waitingTime)
                {
                    typingParagraph = false;
                    textToDisplay = "";
                    currentDialogue++;
                    dialogueTimer = 0;
                }
                text.GetComponent<TextMeshProUGUI>().text = textToDisplay;
            }
            else
            {
                inPost = true;
                textBox.gameObject.SetActive(false);
            }
        }
        else
        {
            resetDialogue();
            textBox.SetActive(false);
        }
    }

}
