using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ThiefManager : MonoBehaviour
{
    public static ThiefManager Instance { get; private set; }

    public TMP_Text counterText;
    public string nextSceneName;

    int totalThieves;
    int caughtThieves;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        totalThieves = GameObject.FindGameObjectsWithTag("Thief").Length;
        UpdateUI();
    }

    public void ThiefCaught()
    {
        caughtThieves++;
        UpdateUI();

        if (caughtThieves >= totalThieves)
            AllThievesCaught();
    }

    void UpdateUI()
    {
        if (counterText != null)
            counterText.text = $"Ladrones atrapados: {caughtThieves}/{totalThieves}";
    }

    void AllThievesCaught()
    {
        Debug.Log("Todos los ladrones fueron atrapados!");
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}
