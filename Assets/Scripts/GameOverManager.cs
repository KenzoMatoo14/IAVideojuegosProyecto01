using UnityEngine;
using UnityEngine.Serialization;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [FormerlySerializedAs("gameOverText")]
    public TMP_Text statusText;
    public float survivalTime = 30f;
    public float restartDelay = 2f;

    float timeRemaining;
    bool gameEnded;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        timeRemaining = survivalTime;
        UpdateCountdownText();
    }

    void Update()
    {
        if (gameEnded) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            Win();
        }
        else
        {
            UpdateCountdownText();
        }
    }

    void UpdateCountdownText()
    {
        if (statusText != null)
            statusText.text = $"Sobrevive: {Mathf.CeilToInt(timeRemaining)}";
    }

    public void GameOver()
    {
        if (gameEnded) return;
        gameEnded = true;

        if (statusText != null)
            statusText.text = "Game Over";

        Invoke(nameof(RestartScene), restartDelay);
    }

    void Win()
    {
        gameEnded = true;

        if (statusText != null)
            statusText.text = "Ganaste";

        CopCatcher copCatcher = FindObjectOfType<CopCatcher>();
        if (copCatcher != null)
            copCatcher.enabled = false;
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
