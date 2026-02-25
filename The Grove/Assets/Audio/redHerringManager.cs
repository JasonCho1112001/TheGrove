using UnityEngine;

public class redHerringManager : MonoBehaviour
{
    [Header("Timing")]
    public float minWaitTime = 10f;
    public float maxWaitTime = 15f;
    private float herringTimer; 

    [Header("Spatial Audio")]
    public float spawnRadius = 20f; 

    [Header("Audio Library")]
    public string[] redHerringSounds = { "wolfHowl", "leavesRustling", "leavesRustling2", "leavesRustling3", "rockFalling" };

    private int lastSoundIndex = -1;

    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        herringTimer = Random.Range(minWaitTime, maxWaitTime);
    }

    void Update()
    {
        HandleRedHerring();
    }

    void HandleRedHerring()
    {
        herringTimer -= Time.deltaTime;

        if (herringTimer <= 0f)
        {
            herringTimer = Random.Range(minWaitTime, maxWaitTime);

            if (playerTransform != null && audioManager.instance != null && redHerringSounds.Length > 0)
            {
                // Pick an initial random number
                int randomIndex = Random.Range(0, redHerringSounds.Length);

                // If we get a repeatkeep rerolling until we get a new one
                if (redHerringSounds.Length > 1)
                {
                    while (randomIndex == lastSoundIndex)
                    {
                        randomIndex = Random.Range(0, redHerringSounds.Length);
                    }
                }

                // Save this new choice so we don't repeat it
                lastSoundIndex = randomIndex;
                string randomSound = redHerringSounds[randomIndex];


                // Execute the spatial audio spawn logic
                Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
                Vector3 raycastStartPos = playerTransform.position + new Vector3(randomCircle.x, 50f, randomCircle.y);
                Vector3 finalSpawnPos;
                if (Physics.Raycast(raycastStartPos, Vector3.down, out RaycastHit hit, 100f))
                {
                    finalSpawnPos = hit.point; 
                }
                else
                {
                    finalSpawnPos = playerTransform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
                }

                GameObject tempEmitter = new GameObject("FakeAudioOrigin");
                tempEmitter.transform.position = finalSpawnPos;

                audioManager.instance.Play(randomSound, tempEmitter);
                Destroy(tempEmitter, 0.5f);
            }
        }
    }
}