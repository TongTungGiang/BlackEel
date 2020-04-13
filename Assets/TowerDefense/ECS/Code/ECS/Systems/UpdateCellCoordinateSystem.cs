//using System.Collections.Generic;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Burst;
//using Unity.Mathematics;
//using Unity.Transforms;

//namespace BE.ECS
//{
//    class UpdateCellCoordinateSystem : ComponentSystem
//    {
//        private EntityQuery m_AgentQuery;


//        protected override void OnUpdate()
//        {
//            Entities.With(m_AgentQuery).ForEach((Entity e) =>
//            {
//                Cell cell = Cell.FromPos(EntityManager.GetComponentData<Translation>(e).Value);
//                EntityManager.SetSharedComponentData(e, new CellCoordinate() { Cell = cell });
//            });
//        }


//        protected override void OnCreate()
//        {
//            m_AgentQuery = GetEntityQuery(new EntityQueryDesc()
//            {
//                All = new ComponentType[]
//                {
//                    ComponentType.ReadOnly<AgentTag>(),
//                    typeof(Translation),
//                    typeof(CellCoordinate),
//                }
//            });
//        }
//    }
//}
