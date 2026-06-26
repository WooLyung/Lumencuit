using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 와이어 뷰 오브젝트입니다.
    /// </summary>
    public class WireViewObject : ViewObject
    {
        [SerializeField] private Transform type1Transform;
        [SerializeField] private Transform type2Transform;
        [SerializeField] private Transform leftTransform;
        [SerializeField] private Transform rightTransform;
        [SerializeField] private Transform upTransform;
        [SerializeField] private Transform downTransform;

        private ViewPart type1, type2, left, right, up, down;

        private void Awake()
        {
            type1 = new ViewPart(type1Transform);
            type2 = new ViewPart(type2Transform);
            left = new ViewPart(leftTransform);
            right = new ViewPart(rightTransform);
            up = new ViewPart(upTransform);
            down = new ViewPart(downTransform);
        }

        private void Start()
        {
            Appear(type1);
            Appear(type2);
        }

        public override void SetSignal(QuantumSignal signal)
        {
            ApplySignal(signal, type1);
            ApplySignal(signal, type2);
        }

        public override void SetPortSignal(Vector2Int dir, QuantumSignal signal)
        {
            if (dir == Vector2Int.left)
                ApplySignal(signal, left);
            else if (dir == Vector2Int.right)
                ApplySignal(signal, right);
            else if (dir == Vector2Int.up)
                ApplySignal(signal, up);
            else if (dir == Vector2Int.down)
                ApplySignal(signal, down);
        }

        public override void PortUpdate(Entity.Ports ports)
        {
            PortUpdate(left, ports.Left, Quaternion.Euler(0, 0, 90), Quaternion.Euler(0, 0, 270));
            PortUpdate(right, ports.Right, Quaternion.Euler(0, 0, 270), Quaternion.Euler(0, 0, 90));
            PortUpdate(up, ports.Up, Quaternion.Euler(0, 0, 0), Quaternion.Euler(0, 0, 180));
            PortUpdate(down, ports.Down, Quaternion.Euler(0, 0, 180), Quaternion.Euler(0, 0, 0));

            if (ports.Up != Entity.PortType.None && ports.Down != Entity.PortType.None)
            {
                type1.Transform.gameObject.SetActive(true);
                type2.Transform.gameObject.SetActive(false);
                type1.Transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            if (ports.Left != Entity.PortType.None && ports.Right != Entity.PortType.None)
            {
                type1.Transform.gameObject.SetActive(true);
                type2.Transform.gameObject.SetActive(false);
                type1.Transform.rotation = Quaternion.Euler(0, 0, 270);
            }

            if (ports.Left != Entity.PortType.None && ports.Up != Entity.PortType.None)
            {
                type1.Transform.gameObject.SetActive(false);
                type2.Transform.gameObject.SetActive(true);
                type2.Transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            if (ports.Left != Entity.PortType.None && ports.Down != Entity.PortType.None)
            {
                type1.Transform.gameObject.SetActive(false);
                type2.Transform.gameObject.SetActive(true);
                type2.Transform.rotation = Quaternion.Euler(0, 0, 90);
            }

            if (ports.Right != Entity.PortType.None && ports.Up != Entity.PortType.None)
            {
                type1.Transform.gameObject.SetActive(false);
                type2.Transform.gameObject.SetActive(true);
                type2.Transform.rotation = Quaternion.Euler(0, 0, 270);
            }

            if (ports.Right != Entity.PortType.None && ports.Down != Entity.PortType.None)
            {
                type1.Transform.gameObject.SetActive(false);
                type2.Transform.gameObject.SetActive(true);
                type2.Transform.rotation = Quaternion.Euler(0, 0, 180);
            }
        }

        public override void Destroy()
        {
            StopAllCoroutines();
            Disappear(left);
            Disappear(right);
            Disappear(up);
            Disappear(down);
            Disappear(type1, true);
            Disappear(type2, true);
        }
    }
}