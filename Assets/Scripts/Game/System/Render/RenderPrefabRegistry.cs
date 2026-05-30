using System;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 렌더 시스템이 생성할 프리팹 집합입니다.
    /// </summary>
    [Serializable]
    public sealed class RenderPrefabRegistry
    {
        [SerializeField] private GameObject tile;

        public GameObject Tile => tile;
    }
}
