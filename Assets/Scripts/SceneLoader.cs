using UnityEngine;
using UnityEngine.SceneManagement;

// Attached to UI buttons so their OnClick() can call LoadScene(sceneName)
// with the target scene's name as the fixed string argument.
public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        
    }
}
