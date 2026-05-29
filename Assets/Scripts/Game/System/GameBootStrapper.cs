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
        [SerializeField] private readonly GameObject root;

        // 시스템 변수
        private WorldSystem gameWorld;
        private SimulationSystem simulationSystem;
        private InputSystem inputSystem;
        private RenderSystem renderSystem;

        // 유니티 생명주기 메소드
        public void Awake()
        {
            gameWorld = new WorldSystem();
            simulationSystem = new SimulationSystem();
            inputSystem = new InputSystem();
            renderSystem = new RenderSystem();
        }

        public void Start()
        {

        }
    }
}