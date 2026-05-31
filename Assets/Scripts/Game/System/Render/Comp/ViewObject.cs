using System;
using UnityEngine;

namespace Lumencuit
{
    public abstract class ViewObject : MonoBehaviour
    {
        public abstract void SetColor(Color color);
        public abstract void PortUpdate(Entity entity);
    }
}