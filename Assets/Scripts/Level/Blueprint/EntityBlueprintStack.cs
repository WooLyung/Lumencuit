using System;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 개수가 포함된 엔티티 청사진입니다.
    /// </summary>
    [Serializable]
    public sealed class EntityBlueprintStack
    {
        [SerializeReference] private EntityBlueprint blueprint;
        [SerializeField] private int count = 1;

        public EntityBlueprint Blueprint => blueprint;
        public int Count { get => count; set => count = value; }

        public EntityBlueprintStack()
        {
        }

        public EntityBlueprintStack(EntityBlueprint blueprint, int count)
        {
            this.blueprint = blueprint;
            this.count = count;
        }

        public EntityBlueprintStack Clone()
        {
            return new EntityBlueprintStack(blueprint.Clone(), count);
        }
    }
}