using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuController : MonoBehaviour
{
    public GameObject creditContainer;
    public void OnClickStartButton(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void OnClickCreditButton()
    {
        creditContainer.SetActive(!creditContainer.activeSelf);
    }

    public void OnClickQuitButton()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
    }
}
