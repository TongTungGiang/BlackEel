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
        private const int CELL_SIZE = 10;

        public bool Equals(Cell other)
        {
            return other.x == x && other.y == y;
        }

        public static Cell FromPos(float3 position)
        {
            return new Cell { x = (int)position.x / CELL_SIZE, y = (int)position.z / CELL_SIZE };
        }
    }

    public struct CellCoordinate : ISharedComponentData
    {
        public Cell Cell;
    }
}
