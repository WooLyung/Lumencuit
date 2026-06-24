using System;
using UnityEngine;

namespace Lumencuit
{
    public abstract class ViewObject : MonoBehaviour
    {
        public abstract void SetSignal(QuantumSignal signal);
        public abstract void SetPortSignal(Vector2Int dir, QuantumSignal signal);
        public abstract void PortUpdate(Entity entity);

        public void SetPortSignal(QuantumSignal signal)
        {
            SetPortSignal(Vector2Int.left, signal);
            SetPortSignal(Vector2Int.right, signal);
            SetPortSignal(Vector2Int.up, signal);
            SetPortSignal(Vector2Int.down, signal);
        }

        public abstract void Destroy();
    }
}