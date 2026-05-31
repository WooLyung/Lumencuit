using TMPro;
using UnityEngine;

namespace Lumencuit
{
    public class GateViewObject : ViewObject
    {
        [SerializeField] private TextMeshProUGUI text;

        [SerializeField] private SpriteRenderer leftPort;
        [SerializeField] private SpriteRenderer rightPort;
        [SerializeField] private SpriteRenderer upPort;
        [SerializeField] private SpriteRenderer downPort;

        public override void SetColor(Color color)
        {
            text.color = color;
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