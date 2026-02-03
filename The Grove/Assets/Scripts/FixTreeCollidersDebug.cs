using UnityEngine;


//https://www.reddit.com/r/Unity3D/comments/ny0zke/solved_tree_colliders_not_working_on_unity_terrain/
//Fixing bugs where tree colliders are not working on Unity Terrain
//Attach this script to the terrain game object


public class FixTreeCollidersDebug : MonoBehaviour
{
    private void Awake()
    {
        TerrainCollider terrainCollider = GetComponent<TerrainCollider>();

        if (terrainCollider != null)
        {
            terrainCollider.enabled = false;
            terrainCollider.enabled = true;
        }
    }
}
