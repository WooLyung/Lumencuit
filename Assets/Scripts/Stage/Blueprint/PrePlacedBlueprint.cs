using System;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 스테이지 시작시 미리 배치되어 고정된 블루프린트입니다.
    /// </summary>
    [Serializable]
    public sealed class PrePlacedBlueprint
    {
        [SerializeReference] private EntityBlueprint blueprint = new EntityBlueprint();
        [SerializeField] private Vector2Int position;
        [SerializeField] private Entity.Ports ports = Entity.Ports.None;

        public EntityBlueprint Blueprint => blueprint;
        public Vector2Int Position => position;
        public Entity.Ports Ports => ports;

        public PrePlacedBlueprint()
        {
        }

        public PrePlacedBlueprint(EntityBlueprint blueprint, Vector2Int position, Entity.Ports ports)
        {
            this.blueprint = blueprint;
            this.position = position;
            this.ports = ports;
        }
    }
}
