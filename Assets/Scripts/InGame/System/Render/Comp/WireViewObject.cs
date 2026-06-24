using UnityEngine;

namespace Lumencuit
{
    public class WireViewObject : ViewObject
    {
        [SerializeField] private Transform type1;
        [SerializeField] private Transform type2;
        [SerializeField] private Transform leftPort;
        [SerializeField] private Transform rightPort;
        [SerializeField] private Transform upPort;
        [SerializeField] private Transform downPort;

        private Renderer renderer1;
        private Renderer renderer2;
        private Renderer leftRenderer;
        private Renderer rightRenderer;
        private Renderer upRenderer;
        private Renderer downRenderer;

        private MaterialPropertyBlock block;

        private void Awake()
        {
            block = new MaterialPropertyBlock();

            renderer1 = type1.GetComponent<Renderer>();
            renderer2 = type2.GetComponent<Renderer>();
            leftRenderer = leftPort.GetComponent<Renderer>();
            rightRenderer = rightPort.GetComponent<Renderer>();
            upRenderer = upPort.GetComponent<Renderer>();
            downRenderer = downPort.GetComponent<Renderer>();
        }

        public override void SetSignal(QuantumSignal signal)
        {
            renderer1.GetPropertyBlock(block);
            block.SetColor("_BaseColor", signal.ToSignal()?.Color ?? Color.gray);
            renderer1.SetPropertyBlock(block);

            renderer2.GetPropertyBlock(block);
            block.SetColor("_BaseColor", signal.ToSignal()?.Color ?? Color.gray);
            renderer2.SetPropertyBlock(block);
        }

        public override void SetPortSignal(Vector2Int dir, QuantumSignal signal)
        {
            Renderer targetRenderer = null;

            if (dir == Vector2Int.left)
                targetRenderer = leftRenderer;
            else if (dir == Vector2Int.right)
                targetRenderer = rightRenderer;
            else if (dir == Vector2Int.up)
                targetRenderer = upRenderer;
            else if (dir == Vector2Int.down)
                targetRenderer = downRenderer;

            targetRenderer?.GetPropertyBlock(block);
            block.SetColor("_BaseColor", signal.ToSignal()?.Color ?? Color.gray);
            targetRenderer?.SetPropertyBlock(block);
        }

        public override void PortUpdate(Entity entity)
        {
            leftPort.gameObject.SetActive(entity.LeftPort != Entity.PortType.None);
            rightPort.gameObject.SetActive(entity.RightPort != Entity.PortType.None);
            upPort.gameObject.SetActive(entity.UpPort != Entity.PortType.None);
            downPort.gameObject.SetActive(entity.DownPort != Entity.PortType.None);

            if (entity.UpPort != Entity.PortType.None && entity.DownPort != Entity.PortType.None)
            {
                type1.gameObject.SetActive(true);
                type2.gameObject.SetActive(false);
                type1.rotation = Quaternion.Euler(0, 0, 0);
            }

            if (entity.LeftPort != Entity.PortType.None && entity.RightPort != Entity.PortType.None)
            {
                type1.gameObject.SetActive(true);
                type2.gameObject.SetActive(false);
                type1.rotation = Quaternion.Euler(0, 0, 90);
            }

            if (entity.LeftPort != Entity.PortType.None && entity.UpPort != Entity.PortType.None)
            {
                type1.gameObject.SetActive(false);
                type2.gameObject.SetActive(true);
                type2.rotation = Quaternion.Euler(0, 0, 0);
            }

            if (entity.LeftPort != Entity.PortType.None && entity.DownPort != Entity.PortType.None)
            {
                type1.gameObject.SetActive(false);
                type2.gameObject.SetActive(true);
                type2.rotation = Quaternion.Euler(0, 0, 90);
            }

            if (entity.RightPort != Entity.PortType.None && entity.UpPort != Entity.PortType.None)
            {
                type1.gameObject.SetActive(false);
                type2.gameObject.SetActive(true);
                type2.rotation = Quaternion.Euler(0, 0, 270);
            }

            if (entity.RightPort != Entity.PortType.None && entity.DownPort != Entity.PortType.None)
            {
                type1.gameObject.SetActive(false);
                type2.gameObject.SetActive(true);
                type2.rotation = Quaternion.Euler(0, 0, 180);
            }
        }

        public override void Destroy()
        {
            Destroy(gameObject);
        }
    }
}