using UnityEngine;

public class AnimationCutsceneObjects : MonoBehaviour
{
    [SerializeField] public GameObject[] listOfAnimations;
    [SerializeField] public GameObject[] listOfCutsceneObjectsToRemove;
    [SerializeField] public GameObject[] listOfCutsceneObjectsToAdd;

    void Start()
    {
        GameStats.receiveCutsceneAndAnimationObjects(listOfAnimations, listOfCutsceneObjectsToRemove, listOfCutsceneObjectsToAdd);
    }
}
