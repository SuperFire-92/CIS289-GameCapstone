using UnityEngine;

public class Gateway : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public GameObject leftTrigger;
    [SerializeField] public GameObject rightTrigger;

    [Header("Zone Numbers")]
    [Tooltip("Zone to move to if the player is proceeding through the game")]
    [SerializeField] public int zoneForward;
    [Tooltip("Zone to move to if the player is going backwards")]
    [SerializeField] public int zoneBackward;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
