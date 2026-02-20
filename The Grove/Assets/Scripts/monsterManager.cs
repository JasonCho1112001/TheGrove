using UnityEngine;
using UnityEngine.InputSystem;

public class monsterManager : MonoBehaviour
{
    //Variables

    //Distance Detection
    [Header("--Distance Detection--")]
    public float distanceFromFriends = 0f;
    public float maxDistanceFromFriends = 100f;

    [Header("--Proximity Meter--")]
    public float proximityMeter = 0f;
    public float proximityMax = 100f;
    public float proximityIncreaseRate = 10f;
    public float proximityDecreaseRate = 5f;

    //References
    //Automatically found
    [Header("--Automatically Found--")]
    public gameManager gameManager;
    public uiManager ui;

    //Manually assigned
    [Header("--Manually Assigned--")]
    public GameObject player;
    public GameObject friendGroup;

    void Awake()
    {
        gameManager = FindFirstObjectByType<gameManager>();
        ui = FindFirstObjectByType<uiManager>();
    }

    void Start()
    {
        //ui.EnableGroup(GameObject.Find("Movement Stats"), false);
    }

    void Update()
    {
        //Distance from friends
        if (player != null && friendGroup != null)
        {
            distanceFromFriends = Vector3.Distance(player.transform.position, friendGroup.transform.position);
            //UI
            ui.SetSlider(ui.distanceSlider, distanceFromFriends / maxDistanceFromFriends);
            ui.SetText(ui.distanceText, $"{distanceFromFriends:F1} / {maxDistanceFromFriends}");
        }

        //Proximity meter
        if (distanceFromFriends < maxDistanceFromFriends)
        {
            proximityMeter += proximityIncreaseRate * Time.deltaTime;
            //Initiate proximity attack if meter is full
            if (proximityMeter >= proximityMax)
            {
                proximityMeter = 0f;
                //Do Proximity Attack
                Debug.Log("Proximity Attack!");
            }
        }
        else
        {
            proximityMeter -= proximityDecreaseRate * Time.deltaTime;
            if (proximityMeter < 0f)
            {
                proximityMeter = 0f;
            }
        }
        //UI
        ui.SetSlider(ui.proximitySlider, proximityMeter / proximityMax);
        ui.SetText(ui.proximityText, $"{proximityMeter:F1} / {proximityMax}");
    }
}
