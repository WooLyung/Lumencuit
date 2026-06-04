using System.Collections;
using System.Linq;
using UnityEngine;

namespace Lumencuit
{
    public class WireViewObject : ViewObject
    {
        [SerializeField] private SpriteRenderer center;

        [SerializeField] private SpriteRenderer leftPort;
        [SerializeField] private SpriteRenderer rightPort;
        [SerializeField] private SpriteRenderer upPort;
        [SerializeField] private SpriteRenderer downPort;

        private QuantumSignal main = QuantumSignal.Null;
        private QuantumSignal left = QuantumSignal.Null;
        private QuantumSignal right = QuantumSignal.Null;
        private QuantumSignal up = QuantumSignal.Null;
        private QuantumSignal down = QuantumSignal.Null;

        private void Update()
        {
            var centerSignals = main.GetSignals().ToList();
            center.color = centerSignals.Count == 0 ? Color.grey : centerSignals[RenderRegistry.N % centerSignals.Count].Color;
            var leftSignals = left.GetSignals().ToList();
            leftPort.color = leftSignals.Count == 0 ? Color.grey : leftSignals[RenderRegistry.N % leftSignals.Count].Color;
            var rightSignals = right.GetSignals().ToList();
            rightPort.color = rightSignals.Count == 0 ? Color.grey : rightSignals[RenderRegistry.N % rightSignals.Count].Color;
            var upSignals = up.GetSignals().ToList();
            upPort.color = upSignals.Count == 0 ? Color.grey : upSignals[RenderRegistry.N % upSignals.Count].Color;
            var downSignals = down.GetSignals().ToList();
            downPort.color = downSignals.Count == 0 ? Color.grey : downSignals[RenderRegistry.N % downSignals.Count].Color;
        }

        public override void SetSignal(QuantumSignal signal)
        {
            main = signal;
        }

        public override void SetPortSignal(Vector2Int dir, QuantumSignal signal)
        {
            if (dir == Vector2Int.left)
                left = signal;
            else if (dir == Vector2Int.right)
                right = signal;
            else if (dir == Vector2Int.up)
                up = signal;
            else if (dir == Vector2Int.down)
                down = signal;
        }

        public override void PortUpdate(Entity entity)
        {
            leftPort.gameObject.SetActive(entity.LeftPort != Entity.PortType.None);
            rightPort.gameObject.SetActive(entity.RightPort != Entity.PortType.None);
            upPort.gameObject.SetActive(entity.UpPort != Entity.PortType.None);
            downPort.gameObject.SetActive(entity.DownPort != Entity.PortType.None);
        }
    }
}