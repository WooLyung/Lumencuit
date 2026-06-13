using Lumencuit;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Back : MonoBehaviour
{
    public void BackScene()
    {
        foreach (var x in SaveManagement.GlobalData.ClearedStageIds)
            Debug.Log(x);
        SceneManager.LoadScene("StageSelectScene");
    }
}
