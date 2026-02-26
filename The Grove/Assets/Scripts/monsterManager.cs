using UnityEngine;
using UnityEngine.InputSystem;

public class monsterManager : MonoBehaviour
{
    //Variables

    [Header("--Switches--")]
    public bool disableJumpscare = false;

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
    public float jumpScareDelay = 0.4f;
    
    
    [Header("--Monster Prefab Management--")]
    public GameObject[] monsterPrefabs;
    public float monsterPrefabDistance = 25f;
    public Vector3 monsterPrefabLeftTargetPosition;
    public Vector3 monsterPrefabRightTargetPosition;
    public float monsterPrefabVerticalOffset = 0f;
    public float monsterPrefabLateralOffset = 0f;
    public Vector3 playerForward;

    public GameObject[] monsterEyes;



    //References
    //Automatically found
    [Header("--Automatically Found--")]
    public gameManager gameManager;
    public uiManager ui;
    public rowBoatInput playerInput;
    public RockQTE rockQTE;
    
    //Manually assigned
    [Header("--Manually Assigned--")]
    public GameObject player;
    public GameObject friendGroup;
    public GameObject jumpscareScreen;
    //public GameObject monsterPrefab;
    

    void Awake()
    {
        gameManager = FindFirstObjectByType<gameManager>();
        ui = FindFirstObjectByType<uiManager>();
        monsterTimer = monsterIntervalMin;

        if (player != null) playerInput = player.GetComponent<rowBoatInput>(); else Debug.LogError("Player GameObject not assigned in monsterManager");
        if (rockQTE != null) rockQTE = player.GetComponent<RockQTE>(); else Debug.LogError("RockQTE script not assigned in monsterManager");
    }

    void Start()
    {
        //Store playerForward
        playerForward = player.transform.forward;
    }

    void Update()
    {
        //Temp: Pause all logic when rockQTE is active
        if (rockQTE != null && rockQTE.isRockQTEActive)
        {
            return;
        }

        DistanceFromFriends();

        ProximityMeter();

        //HandleMonsterPrefabMovement();

        HandleMonsterLeftRight();

        CheckSameTrackAttack();

        MonsterEyes();

        //Constant UI
        ui.SetText(ui.monsterTimerValue, $"{monsterTimer:F2}");

        //Temporary input to test proximity attack
        if (Keyboard.current.leftShiftKey.isPressed && Keyboard.current.pKey.wasPressedThisFrame)
        {
            ProximityAttack();
        }
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
                ProximityAttack();
                
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
    
    void HandleMonsterLeftRight()
    {
        //Randomly switch monster side every monsterInterval seconds
        monsterTimer -= Time.deltaTime;
        if (monsterTimer <= 0f)
        {
            monsterTimer = Random.Range(monsterIntervalMin, monsterIntervalMax);
            monsterSide = (monsterSide == MonsterSide.Left) ? MonsterSide.Right : MonsterSide.Left;
            //Audio: Play a sound specifically from the new active side of the monster prefab's location in the woods
            if (audioManager.instance != null && monsterPrefabs.Length >= 2)
            {
                // Determine which side the monster is
                GameObject activeEmitter = (monsterSide == MonsterSide.Left) ? monsterPrefabs[0] : monsterPrefabs[1];
                
                // Play the sound specifically from the active emitter's position
                audioManager.instance.Play("MonsterMove", activeEmitter);
            }
            
            //UI
            ui.SetText(ui.monsterSideValue, $"{monsterSide}");
        }

        //Disable and enable meshrenderer based on monsterSide
        if (monsterPrefabs.Length >= 2)
        {
            if (monsterSide == MonsterSide.Left)
            {
                EnableMonsterMesh(MonsterSide.Left);
            }
            else
            {
                EnableMonsterMesh(MonsterSide.Right);
            }
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
                SameTrackAttack();
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
    
    //TODO: refactor
    void MonsterEyes()
    {
        //Monster eyes are red if player is on the same track as the monster, otherwise they are white
        //Angry eyes: 1, 3, 5, 7
        bool sameSide = (monsterSide == MonsterSide.Left && playerInput.GetPlayerSide() == -1) || (monsterSide == MonsterSide.Right && playerInput.GetPlayerSide() == 1);
        monsterEyes[1].SetActive(sameSide);
        monsterEyes[3].SetActive(sameSide);
        monsterEyes[5].SetActive(sameSide);
        monsterEyes[7].SetActive(sameSide);

        //Neutral eyes: 0, 2, 4, 6
        monsterEyes[0].SetActive(!sameSide);
        monsterEyes[2].SetActive(!sameSide);
        monsterEyes[4].SetActive(!sameSide);
        monsterEyes[6].SetActive(!sameSide);
    }

    void ProximityAttack()
    {
        if (disableJumpscare)
        {
            Debug.Log("Proximity Attack Triggered, but jumpscare is disabled.");
            return;
        }

        Debug.Log("Proximity Attack!");
        jumpscareScreen.SetActive(true);

        //play the audio with a slight delay
        Invoke("PlayJumpscareSound", jumpScareDelay);

        audioManager.instance.Play("JumpscareSound", gameObject);
        
    }

    void SameTrackAttack()
    {
        if (disableJumpscare)
        {
            Debug.Log("Same Track Attack Triggered, but jumpscare is disabled.");
            return;
        }
        
        Debug.Log("Same Track Attack!");
        audioManager.instance.Play("JumpscareSound", gameObject);
        jumpscareScreen.SetActive(true);
    }

    void EnableMonsterMesh(MonsterSide sideToEnable)
    {
        //Currently hardcoding what meshes to disable
        if (monsterPrefabs.Length >= 2)
        {
            if (sideToEnable == MonsterSide.Left)
            {
                monsterPrefabs[0].SetActive(true);
                monsterPrefabs[1].SetActive(false);
            }
            else
            {
                monsterPrefabs[0].SetActive(false);
                monsterPrefabs[1].SetActive(true);
            }
        }
    }
}
