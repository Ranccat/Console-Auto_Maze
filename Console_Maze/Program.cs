namespace Console_Maze
{
    class Program
    {
        static void Main(string[] args)
        {
            const int WAIT_TICK = 1000 / 30; // 1/30sec

            Console.CursorVisible = false;

            while (true)
            {
                Board board = new Board();
                Player player = new Player();
                board.Initialize(25, player);
                player.Initialize(1, 1, board);

                int lastTick = 0;
                while (true)
                {
                    #region Frame Management
                    int currentTick = System.Environment.TickCount; // +1ms
                    if (currentTick - lastTick < WAIT_TICK) continue;
                    int deltaTick = currentTick - lastTick;
                    lastTick = currentTick;
                    #endregion

                    // Input


                    // Logic
                    player.Update(deltaTick);

                    // Rendering
                    Console.SetCursorPosition(0, 0);
                    board.Render();
                }
            }
            
        }
    }
}