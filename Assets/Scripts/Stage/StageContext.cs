using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lumencuit
{
    /// <summary>
    /// 선택된 스테이지 정보를 다음 씬으로 전달하는 오브젝트입니다.
    /// </summary>
    public class StageContext : MonoBehaviour
    {
        private StageData selectedStage;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void StartStage(StageData stageData)
        {
            selectedStage = stageData;
            SceneManager.LoadScene("GameScene");
        }

        public StageData SelectedStage => selectedStage;
    }
}