using System;
using UnityEngine;

namespace Lumencuit
{
    public abstract class ViewObject : MonoBehaviour
    {
        public abstract void SetColor(Color color);
        public abstract void SetPortColor(Vector2Int dir, Color color);
        public abstract void PortUpdate(Entity entity);

        public void SetPortColor(Color color)
        {
            SetPortColor(Vector2Int.left, color);
            SetPortColor(Vector2Int.right, color);
            SetPortColor(Vector2Int.up, color);
            SetPortColor(Vector2Int.down, color);
        }
    }
}