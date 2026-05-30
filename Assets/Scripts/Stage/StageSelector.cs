using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 스테이지의 시작 진입점입니다.
    /// </summary>
    public class StageSelector : MonoBehaviour
    {
        [SerializeField] private StageData stageData;
        [SerializeField] private StageContext stageContext;

        public void StartStage()
        {
            stageContext.StartStage(stageData);
        }
    }
}