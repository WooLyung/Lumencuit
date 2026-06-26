using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 렌더 프리팹를 가지는 컴포넌트입니다.
    /// </summary>
    public sealed class RenderRegistry : MonoBehaviour
    {
        [SerializeField] private RenderPrefab prefabs;
        [SerializeField] private Mesh tileMesh;

        public RenderPrefab Prefabs => prefabs;
        public Mesh TileMesh => tileMesh;
    }
}
