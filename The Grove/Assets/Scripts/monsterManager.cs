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

    [Header("--Monster Left Right Management--")]
    public MonsterSide monsterSide = MonsterSide.Left;
    public enum MonsterSide { Left, Right }
    public float monsterIntervalMin = 2.5f;
    public float monsterIntervalMax = 7.5f;
    private float monsterTimer;

    [Header("--Same Track Attack--")]
    public float sameTrackMeter = 0f;
    public float sameTrackMax = 100f;
    public float sameTrackIncreaseRate = 10f;
    public float sameTrackDecreaseRate = 5f;
    
    [Header("--Monster Prefab Management--")]
    public GameObject[] monsterPrefabs;
    public float monsterPrefabDistance = 25f;
    public Vector3 monsterPrefabLeftTargetPosition;
    public Vector3 monsterPrefabRightTargetPosition;
    public float monsterPrefabVerticalOffset = 0f;
    public float monsterPrefabLateralOffset = 0f;
    public Vector3 playerForward;



    //References
    //Automatically found
    [Header("--Automatically Found--")]
    public gameManager gameManager;
    public uiManager ui;
    public rowBoatInput playerInput;
    
    //Manually assigned
    [Header("--Manually Assigned--")]
    public GameObject player;
    public GameObject friendGroup;
    //public GameObject monsterPrefab;
    

    void Awake()
    {
        gameManager = FindFirstObjectByType<gameManager>();
        ui = FindFirstObjectByType<uiManager>();
        monsterTimer = monsterIntervalMin;

        if (player != null) playerInput = player.GetComponent<rowBoatInput>(); else Debug.LogError("Player GameObject not assigned in monsterManager");
        
    }

    void Start()
    {
        //Store playerForward
        playerForward = player.transform.forward;
    }

    void Update()
    {
        DistanceFromFriends();

        ProximityMeter();

        //HandleMonsterPrefabMovement();

        HandleMonsterLeftRight();

        CheckSameTrackAttack();

        //Constant UI
        ui.SetText(ui.monsterTimerValue, $"{monsterTimer:F2}");
    }

    void DistanceFromFriends()
    {
        if (player != null && friendGroup != null)
        {
            distanceFromFriends = Vector3.Distance(player.transform.position, friendGroup.transform.position);
            //UI
            ui.SetSlider(ui.distanceSlider, distanceFromFriends / maxDistanceFromFriends);
            ui.SetText(ui.distanceText, $"{distanceFromFriends:F1}");
        }
    }

    void ProximityMeter()
    {
        if (distanceFromFriends > maxDistanceFromFriends)
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
        ui.SetText(ui.proximityText, $"{proximityMeter:F1}");
    }

    public void RotatePrefabs(Quaternion rotation)
    {
        //We do this when cameraRotate code is called
        //Simply move monster prefabs to the new angle
        Debug.Log("Rotating Prefabs, Target Rotation: " + rotation.eulerAngles);

        //Position
        if (monsterPrefabs.Length >= 2)
        {
            Debug.Log("PlayerForward before rotation: " + playerForward);
            Vector3 leftTargetPosition = new Vector3(Mathf.Round(monsterPrefabVerticalOffset * playerForward.x), monsterPrefabLateralOffset * playerForward.y, Mathf.Round(monsterPrefabVerticalOffset * playerForward.z));
            Vector3 rightTargetPosition = new Vector3(Mathf.Round(-monsterPrefabVerticalOffset * playerForward.x), monsterPrefabLateralOffset * playerForward.y, Mathf.Round(-monsterPrefabVerticalOffset * playerForward.z));

            //We have to rotate the inital positions based on the new playerForward
            monsterPrefabs[0].transform.position = player.transform.position + leftTargetPosition;
            monsterPrefabs[1].transform.position = player.transform.position + rightTargetPosition;

            Debug.Log("Left Prefab leftTargetPosition: " + leftTargetPosition + ", World Position: " + monsterPrefabs[0].transform.position);
            Debug.Log("Right Prefab rightTargetPosition: " + rightTargetPosition + ", World Position: " + monsterPrefabs[1].transform.position);
        }
        //Rotation
        foreach (GameObject prefab in monsterPrefabs)
        {
            if (prefab != null)
            {
                prefab.transform.rotation = rotation;
            }
        }
    }

    void HandleMonsterPrefabMovement()
    {
        //Only move the prefabs in the direction of playerForward
        foreach (GameObject prefab in monsterPrefabs)
        {
            if (prefab != null)
            {
                Vector3 targetPosition;
                targetPosition.x = prefab.transform.position.x;
                targetPosition.y = prefab.transform.position.y;
                targetPosition.z = player.transform.position.z;

                //Determine movement direction based on playerForward
                if(Mathf.Abs(playerForward.x) > 0.5f)
                {
                    //Moving in x direction
                    targetPosition.x = player.transform.position.x + (playerForward.x > 0 ? -monsterPrefabDistance : monsterPrefabDistance);
                }
                else if (Mathf.Abs(playerForward.z) > 0.5f)
                {
                    //Moving in z direction
                    targetPosition.z = player.transform.position.z + (playerForward.z > 0 ? -monsterPrefabDistance : monsterPrefabDistance);
                }
                prefab.transform.position = targetPosition;
            }
        }

        //Disable and enable meshrenderer based on monsterSide
        if (monsterSide == MonsterSide.Left)
        {
            monsterPrefabs[0].GetComponent<MeshRenderer>().enabled = true;
            monsterPrefabs[0].transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
            monsterPrefabs[1].GetComponent<MeshRenderer>().enabled = false;
            monsterPrefabs[1].transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        }
        else
        {
            monsterPrefabs[0].GetComponent<MeshRenderer>().enabled = false;
            monsterPrefabs[0].transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            monsterPrefabs[1].GetComponent<MeshRenderer>().enabled = true;
            monsterPrefabs[1].transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
        }
    }

    void HandleMonsterLeftRight()
    {
        //Randomly switch monster side every monsterInterval seconds
        monsterTimer -= Time.deltaTime;
        if (monsterTimer <= 0f)
        {
            monsterTimer = Random.Range(monsterIntervalMin, monsterIntervalMax);
            monsterSide = (monsterSide == MonsterSide.Left) ? MonsterSide.Right : MonsterSide.Left;
            //Audio
            if (audioManager.instance != null)
            {
                //TODO: Make the emitter the prefab that's enabled
                audioManager.instance.Play("TripSound", gameObject);
            }
            //UI
            ui.SetText(ui.monsterSideValue, $"{monsterSide}");
        }
    }

    void CheckSameTrackAttack()
    {
        //If player is on the same side as the monster, increase meter
        if ((monsterSide == MonsterSide.Left && playerInput.GetPlayerSide() == -1) ||
            (monsterSide == MonsterSide.Right && playerInput.GetPlayerSide() == 1))
        {
            sameTrackMeter += sameTrackIncreaseRate * Time.deltaTime;
            if (sameTrackMeter >= sameTrackMax)
            {
                sameTrackMeter = 0f;
                //Do Same Track Attack
                Debug.Log("Same Track Attack!");
            }
        }
        else
        {
            sameTrackMeter -= sameTrackDecreaseRate * Time.deltaTime;
            if (sameTrackMeter < 0f)
            {
                sameTrackMeter = 0f;
            }
        }

        //UI
        ui.SetSlider(ui.sameTrackSlider, sameTrackMeter / sameTrackMax);
        ui.SetText(ui.sameTrackText, $"{sameTrackMeter:F1}");
        
    }

    public void StorePlayerForward()
    {
        Debug.Log("Storing Player Forward: " + player.transform.forward);
        playerForward = player.transform.forward;
    }
}
