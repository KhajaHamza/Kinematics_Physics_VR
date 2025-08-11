using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public void LoadSingleCarScene()
    {
        SceneManager.LoadScene("SingleCarScene");
    }

    public void LoadMultipleCarScene()
    {
        SceneManager.LoadScene("MultipleCarScene");
    }
}
