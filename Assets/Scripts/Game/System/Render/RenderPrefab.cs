using System;
using UnityEngine;
using static Lumencuit.CircuitElement;

namespace Lumencuit
{
    /// <summary>
    /// 렌더 시스템이 생성할 프리팹 집합입니다.
    /// </summary>
    [Serializable]
    public sealed class RenderPrefab
    {
        [SerializeField] private GameObject tile;
        [SerializeField] private GameObject gridCollider;

        [SerializeField] private GameObject lamp;
        [SerializeField] private GameObject source;
        [SerializeField] private GameObject wire;

        [SerializeField] private GameObject andGate;
        [SerializeField] private GameObject notGate;
        [SerializeField] private GameObject orGate;
        [SerializeField] private GameObject splitGate;
        [SerializeField] private GameObject xorGate;

        public GameObject Tile => tile;
        public GameObject GridCollider => gridCollider;

        public GameObject GetCircuitElement(CircuitElementType type)
        {
            return type switch
            {
                CircuitElementType.Lamp => lamp,
                CircuitElementType.Source => source,
                CircuitElementType.Wire => wire,
                CircuitElementType.AndGate => andGate,
                CircuitElementType.NotGate => notGate,
                CircuitElementType.OrGate => orGate,
                CircuitElementType.SplitGate => splitGate,
                CircuitElementType.XorGate => xorGate,
                _ => null
            };
        }
    }
}
