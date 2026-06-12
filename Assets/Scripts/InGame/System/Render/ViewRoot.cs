using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 렌더 시스템이 오브젝트를 생성할 트랜스폼의 집합입니다.
    /// </summary>
    public class ViewRoot : MonoBehaviour
    {
        [SerializeField] private Transform gridMesh;
        [SerializeField] private Transform gridColliders;
        [SerializeField] private Transform entities;

        public Transform GridMesh => gridMesh;
        public Transform GridColliders => gridColliders;
        public Transform Entities => entities;
    }
}