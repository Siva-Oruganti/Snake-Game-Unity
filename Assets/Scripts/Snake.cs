using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Snake : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gridSize = 1f;
    public GameObject tailPrefab;
    public Vector2 direction = Vector2.right;

    private List<Transform> tail = new List<Transform>();
    public List<Transform> snakeSegments => tail;

    private bool ate = false;
    private float moveRate = 0.2f;

    public int score = 0;

    public static Snake instance;
    public bool IsGameRunning { get; private set; } = true;

    private ScoreManager scoreManager;
    private UIManager uiManager;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        IsGameRunning = true;
        InvokeRepeating(nameof(Move), moveRate, moveRate);

        scoreManager = FindObjectOfType<ScoreManager>();
        uiManager = FindObjectOfType<UIManager>();

        Vector2 startPos = transform.position;
        for (int i = 1; i <= 3; i++)
        {
            Vector2 tailPos = startPos - new Vector2(i * gridSize, 0);
            GameObject tailPart = Instantiate(tailPrefab, tailPos, Quaternion.identity);
            tail.Add(tailPart.transform);
        }
    }

    void Update()
    {
    if (!IsGameRunning) return;

    if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && direction != Vector2.down)
        direction = Vector2.up;
    else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && direction != Vector2.up)
        direction = Vector2.down;
    else if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && direction != Vector2.right)
        direction = Vector2.left;
    else if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && direction != Vector2.left)
        direction = Vector2.right;
    }

    void Move()
    {
        if (!IsGameRunning) return;

        Vector2 previousPosition = transform.position;
        transform.Translate(direction * gridSize);

        if (ate)
        {
            GameObject tailPart = Instantiate(tailPrefab, previousPosition, Quaternion.identity);
            tail.Insert(0, tailPart.transform);
            ate = false;
        }
        else if (tail.Count > 0)
        {
            tail[tail.Count - 1].position = previousPosition;
            tail.Insert(0, tail[tail.Count - 1]);
            tail.RemoveAt(tail.Count - 1);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Food"))
        {
            ate = true;
            AddScore(10);

            collision.GetComponent<Food>()?.RandomizeNewPosition();
        }
        else if (collision.CompareTag("Obstacle") || collision.CompareTag("Tail"))
        {
            GameOver();
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log("Score: " + score);

        scoreManager?.IncreaseScore(amount);

        if (score % 50 == 0)
        {
            moveRate = Mathf.Max(0.05f, moveRate - 0.02f);
            CancelInvoke(nameof(Move));
            InvokeRepeating(nameof(Move), moveRate, moveRate);
        }
    }

    void GameOver()
    {
        IsGameRunning = false;
        CancelInvoke(nameof(Move));

        if (uiManager != null)
        {
            uiManager.ShowGameOver(score);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public bool IsPositionOnSnake(Vector3 position)
    {
        if (transform.position == position)
            return true;

        foreach (Transform segment in snakeSegments)
        {
            if (segment.position == position)
                return true;
        }
        return false;
    }
}
