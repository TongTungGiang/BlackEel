using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

namespace BE.ECS
{
    public struct Cell : IEquatable<Cell>
    {
        public int x, y;

        public bool Equals(Cell other)
        {
            return other.x == x && other.y == y;
        }
    }

    public struct CellCoordinate : ISharedComponentData
    {
        public Cell Cell;
    }
}
