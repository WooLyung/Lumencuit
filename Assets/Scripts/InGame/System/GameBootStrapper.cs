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
        [SerializeField] private ViewRoot viewRoot;
        [SerializeField] private RenderRegistry renderRegistry;
        [SerializeField] private InGameUIAdaptor gameUIAdaptor;

        // 시스템
        private WorldSystem worldSystem;
        private SimulationSystem simulationSystem;
        private InputSystem inputSystem;
        private RenderSystem renderSystem;
        private StageController stageController;

        private void Awake()
        {
            // 스테이지 컨텍스트로부터 스테이지 데이터 불러오기
            StageContext context = GameObject.Find("StageContext")?.GetComponent<StageContext>();
            if (context == null)
            {
                Logger.Error("StageContext is missing.", "GameBootStrapper");
                enabled = false;
                return;
            }

            Destroy(context.gameObject);
            StageData stageData = context.SelectedStage;

            if (stageData == null)
            {
                Logger.Error("SelectedStage is missing.", "GameBootStrapper");
                enabled = false;
                return;
            }

            // 객체 생성
            worldSystem = new(stageData);
            simulationSystem = new(worldSystem, stageData);
            stageController = new(simulationSystem);
            renderSystem = new(worldSystem, simulationSystem, renderRegistry.Prefabs, renderRegistry.TileMesh, viewRoot);
#if UNITY_ANDROID || UNITY_IOS
            inputSystem = new NullInputSystem(worldSystem, stageController);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX || UNITY_EDITOR
            inputSystem = new PCInputSystem(worldSystem, stageController, Camera.main, stageData);
#else
            inputSystem = new NullInputSystem(worldSystem, stageController);
#endif

            // 초기화
            worldSystem.Init();
            gameUIAdaptor.Init(worldSystem, inputSystem, simulationSystem, stageData);
        }

        private void Start()
        {
        }

        private void Update()
        {
            inputSystem?.Update();
        }

        private void OnDestroy()
        {
            GameEventBus.Clear();
        }
    }
}