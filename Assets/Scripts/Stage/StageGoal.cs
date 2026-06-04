using System;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 스테이지의 램프에 도달해야 하는 최종 목록입니다.
    /// </summary>
    [Serializable]
    public class StageGoal
    {
        [SerializeField] private QuantumSignal signal = QuantumSignal.Null;
        [SerializeField] private int count = 1;

        public QuantumSignal Signal => signal;
        public int Count => count;
    }
}
