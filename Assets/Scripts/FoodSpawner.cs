using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;
    public Vector2 spawnAreaMin = new Vector2(-8, -4);
    public Vector2 spawnAreaMax = new Vector2(8, 4);

    public void SpawnFood()
    {
        Vector2 spawnPos = new Vector2(
            Mathf.Round(Random.Range(spawnAreaMin.x, spawnAreaMax.x)),
            Mathf.Round(Random.Range(spawnAreaMin.y, spawnAreaMax.y))
        );

        Instantiate(foodPrefab, spawnPos, Quaternion.identity);
    }
}
