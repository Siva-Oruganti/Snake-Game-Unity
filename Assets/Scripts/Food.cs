using UnityEngine;

public class Food : MonoBehaviour
{
    public BoxCollider2D gridArea; // Assign in inspector if possible
    private Snake snake;

    private void Start()
    {
        snake = FindObjectOfType<Snake>();

        if (gridArea == null)
        {
            gridArea = FindObjectOfType<BoxCollider2D>();
            if (gridArea == null)
            {
                Debug.LogError("No Grid Area found! Please assign one in the inspector.");
                return;
            }
        }

        RandomizeNewPosition();
    }

    // Called from Snake when food is eaten
    public void RandomizeNewPosition()
    {
        if (gridArea == null) return;

        Bounds bounds = gridArea.bounds;
        Vector3 newPos;
        int maxAttempts = 100;
        int attempts = 0;

        do
        {
            float x = Mathf.Round(Random.Range(bounds.min.x, bounds.max.x));
            float y = Mathf.Round(Random.Range(bounds.min.y, bounds.max.y));
            newPos = new Vector3(x, y, 0f);
            attempts++;

            if (attempts > maxAttempts)
            {
                Debug.LogWarning("Could not find a valid food position after many attempts.");
                break;
            }
        }
        while (snake.IsPositionOnSnake(newPos));

        transform.position = newPos;
    }
}
