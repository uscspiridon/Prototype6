using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Score : MonoBehaviour {
    public static Score Instance;

    [HideInInspector] public int currentScore;
    public int distancePerPoint;
    public int reshufflePenalty;
    public TextMeshProUGUI pointsDisplay;

    private float lastPlayerPos;

    private void Awake() {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        lastPlayerPos = Player.Instance.transform.position.x;
        UpdatePointsDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.Instance.transform.position.x > lastPlayerPos + distancePerPoint) {
            lastPlayerPos = Player.Instance.transform.position.x;
            currentScore++;
            UpdatePointsDisplay();
        }
    }

    public void ApplyReshufflePenalty() {
        currentScore -= reshufflePenalty;
        UpdatePointsDisplay();
    }

    private void UpdatePointsDisplay() {
        pointsDisplay.text = currentScore.ToString();
    }
}
