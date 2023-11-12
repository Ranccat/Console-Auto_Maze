using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_Maze
{
    class Pos
    {
        public Pos(int y, int x) { Y = y; X = x; }
        public int X;
        public int Y;
    }

    class Player
    {
        public int PosY { get; private set; }
        public int PosX { get; private set; }
        
        Random _random = new Random();
        Board _board;

        enum Dir
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3
        }

        int _dir = (int)Dir.Up;
        List<Pos> _points = new List<Pos>();

        public void Initialize(int posY, int posX, Board board)
        {
            this.PosX = posX;
            this.PosY = posY;
            _board = board;

            SearchWithAStar();
        }

        struct PQNode : IComparable<PQNode>
        {
            public int F;
            public int G;
            public int X;
            public int Y;

            public int CompareTo(PQNode other)
            {
                if (F == other.F)
                    return 0;
                return F < other.F ? 1 : -1;
            }
        }

        void SearchWithAStar()
        {
            int[] deltaY = new int[] { -1, 0, 1, 0 };
            int[] deltaX = new int[] { 0, 1, 0, -1 };
            int[] cost = new int[] { 1, 1, 1, 1 };

            bool[,] closed = new bool[_board.Size, _board.Size];
            int[,] open = new int[_board.Size, _board.Size];

            for (int y =0; y < _board.Size; y++)
            {
                for (int x = 0; x < _board.Size; x++)
                {
                    open[y, x] = Int32.MaxValue;
                }
            }

            Pos[,] parent = new Pos[_board.Size, _board.Size];

            PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>();
            pq.Push(new PQNode() { F = Math.Abs(_board.DestY - PosY) + Math.Abs(_board.DestX - PosX), G = 0, Y = PosY, X = PosX });
            parent[PosY, PosX] = new Pos(PosY, PosX);

            while (true)
            {
                PQNode node = pq.Pop();

                if (closed[node.Y, node.X]) continue;
                closed[node.Y, node.X] = true;

                if (node.Y == _board.DestY && node.X == _board.DestX) break;

                for (int i = 0; i < 4; i++)
                {
                    int nextY = node.Y + deltaY[i];
                    int nextX = node.X + deltaX[i];

                    if (nextX < 0 || nextY < 0 || nextX >= _board.Size || nextY >= _board.Size) continue;
                    if (_board.Tile[nextY, nextX] == Board.TileType.Wall) continue;
                    if (closed[nextY, nextX]) continue;

                    // Calculate Cost
                    int g = node.G + cost[i];
                    int h = Math.Abs(_board.DestY - nextY) + Math.Abs(_board.DestX - nextX);

                    if (open[nextY, nextX] < g + h) continue;

                    open[nextY, nextX] = g + h;
                    pq.Push(new PQNode { F = h + g, G = g, Y = nextY, X = nextX });
                    parent[nextY, nextX] = new Pos(node.Y, node.X);
                }
            }

            CalcPathFromParent(parent);
        }

        void SearchWithBFS()
        {
            int[] deltaY = new int[] {-1, 0, 1, 0};
            int[] deltaX = new int[] { 0, 1, 0, -1 };

            bool[,] found = new bool[_board.Size, _board.Size];
            Pos[,] parent = new Pos[_board.Size, _board.Size];
            Queue<Pos> q = new Queue<Pos>();

            q.Enqueue(new Pos(PosY, PosX));
            parent[PosY, PosX] = new Pos(PosY, PosX);
            found[PosY, PosX] = true;

            while (q.Count > 0)
            {
                Pos pos = q.Dequeue();
                int nowY = pos.Y;
                int nowX = pos.X;

                for (int i = 0; i < 4; i++)
                {
                    int nextY = nowY + deltaY[i];
                    int nextX = nowX + deltaX[i];

                    if (nextX < 0 || nextY < 0 || nextX >= _board.Size || nextY >= _board.Size) continue;
                    if (_board.Tile[nextY, nextX] == Board.TileType.Wall) continue;
                    if (found[nextY, nextX]) continue;

                    q.Enqueue(new Pos(nextY, nextX));
                    parent[nextY, nextX] = new Pos(nowY, nowX);
                    found[nextY, nextX] = true;
                }
            }

            CalcPathFromParent(parent);
        }

        void SearchWithRightHand()
        {
            int[] frontY = new int[] { -1, 0, 1, 0 };
            int[] frontX = new int[] { 0, 1, 0, -1 };

            int[] rightY = new int[] { 0, 1, 0, -1 };
            int[] rightX = new int[] { 1, 0, -1, 0 };

            _points.Add(new Pos(PosY, PosX));
            while (PosY != _board.DestY || PosX != _board.DestX)
            {
                if (_board.Tile[PosY + rightY[_dir], PosX + rightX[_dir]] == Board.TileType.Empty)
                {
                    _dir = (_dir + 1) % 4;
                    PosX = PosX + frontX[_dir];
                    PosY = PosY + frontY[_dir];
                    _points.Add(new Pos(PosY, PosX));
                }
                else if (_board.Tile[PosY + frontY[_dir], PosX + frontX[_dir]] == Board.TileType.Empty)
                {
                    PosX = PosX + frontX[_dir];
                    PosY = PosY + frontY[_dir];
                    _points.Add(new Pos(PosY, PosX));
                }
                else
                {
                    _dir = (_dir + 3) % 4;
                }
            }
        }

        void CalcPathFromParent(Pos[,] parent)
        {
            int y = _board.DestY;
            int x = _board.DestX;
            while (parent[y, x].Y != y || parent[y, x].X != x)
            {
                _points.Add(new Pos(y, x));
                Pos pos = parent[y, x];
                y = pos.Y;
                x = pos.X;
            }
            _points.Add(new Pos(y, x));
            _points.Reverse();
        }

        const int MOVE_TICK = 10;
        int _sumTick = 0;
        int _lastIndex = 0;
        public void Update(int deltaTick)
        {
            if (_lastIndex >= _points.Count)
            {
                _lastIndex = 0;
                _points.Clear();
                _board.Initialize(_board.Size, this);
                Initialize(1, 1, _board);
            }

            _sumTick += deltaTick;
            if (_sumTick >= MOVE_TICK)
            {
                _sumTick = 0;

                PosY = _points[_lastIndex].Y;
                PosX = _points[_lastIndex].X;
                _lastIndex++;
            }
        }
    }
}
