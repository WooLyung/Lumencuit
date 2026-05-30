using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 게임의 시스템들을 생성 및 초기화하는 진입점입니다.
    /// 유니티의 생명주기와 게임의 시스템을 연결합니다.
    /// </summary>
    public sealed class GameBootStrapper : MonoBehaviour
    {
        // 인스펙터 노출 변수
        [SerializeField] private Transform root;
        [SerializeField] private RenderPrefabRegistry prefabs;

        // 시스템 변수
        private WorldSystem worldSystem;
        private SimulationSystem simulationSystem;
        private InputSystem inputSystem;
        private RenderSystem renderSystem;

        private void Awake()
        {
            StageContext context = GameObject.Find("StageContext")?.GetComponent<StageContext>();
            if (context == null)
                return;
            Destroy(context.gameObject);
            StageData stageData = context.SelectedStage;

            worldSystem = new(stageData);
            simulationSystem = new(worldSystem, stageData);
            renderSystem = new(worldSystem, prefabs, root);
#if UNITY_ANDROID || UNITY_IOS
            inputSystem = new NullInputSystem();
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX || UNITY_EDITOR
            inputSystem = new PCInputSystem();
#else
            inputSystem = new NullInputSystem();
#endif
        }

        private void Start()
        {
        }

        private void Update()
        {
            inputSystem.Update();
        }
    }
}