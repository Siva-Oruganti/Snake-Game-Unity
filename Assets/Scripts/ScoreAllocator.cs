using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreAllocator : MonoBehaviour
{
    [SerializeField]
    private int _killScore;

    private ScoreController _scoreController;

    private void Awake()
    {
        _scoreController = Object.FindFirstObjectByType<ScoreController>();
    }

    public void AllocateScore()
    {
        _scoreController.AddScore(_killScore);
    }
}
