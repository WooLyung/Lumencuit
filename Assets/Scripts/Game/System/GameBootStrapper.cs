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

        // 시스템 변수
        private WorldSystem worldSystem;
        private SimulationSystem simulationSystem;
        private InputSystem inputSystem;
        private RenderSystem renderSystem;

        private void Awake()
        {
            worldSystem = new();
            simulationSystem = new(worldSystem);
            renderSystem = new(worldSystem, root);
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
            worldSystem.CreateEntity(new Entity(Source.Instance, Signal.Red), 0, 0);
            worldSystem.CreateEntity(new Entity(Wire.Instance), 1, 0);
            worldSystem.CreateEntity(new Entity(Lamp.Instance), 2, 0);
        }

        private void Update()
        {
            inputSystem.Update();
        }
    }
}