using Lumencuit;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Back : MonoBehaviour
{
    public void BackScene()
    {
        SaveManagement.ClearCurrentStage();
        SceneManager.LoadScene("StageSelectScene");
    }
}
