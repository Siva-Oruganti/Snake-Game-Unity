using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int score = 0;
    public TextMeshProUGUI scoreText;

    public void IncreaseScore(int value)
    {
        score += value;
        scoreText.text = "Score: " + score.ToString();
    }
}
