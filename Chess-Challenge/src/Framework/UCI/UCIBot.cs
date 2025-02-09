using ChessChallenge.API;
using ChessChallenge.Application;
using ChessChallenge.Application.APIHelpers;
using System;

namespace ChessChallenge.UCI
{
    class UCIBot
    {
        IChessBot bot;
        ChallengeController.PlayerType type;
        Chess.Board board;

		public UCIBot(IChessBot bot, ChallengeController.PlayerType type)
        {
            this.bot = bot;
            this.type = type;
            board = new Chess.Board();
        }

        void PositionCommand(string[] args)
        {
            int idx = Array.FindIndex(args, x => x == "moves");
            if (idx == -1)
            {
                if (args[1] == "startpos")
                {
                    board.LoadStartPosition();
                }
                else
                {
                    board.LoadPosition(String.Join(" ", args.AsSpan(1, args.Length - 1).ToArray()));
                }
            }
            else
            {
                if (args[1] == "startpos")
				{
					board.LoadStartPosition();
				}
                else
				{
					board.LoadPosition(String.Join(" ", args.AsSpan(1, idx - 1).ToArray()));
				}

                for (int i = idx + 1; i < args.Length; i++)
                {
                    // this is such a hack
                    Move move = new Move(args[i], new Board(board));
                    board.MakeMove(new Chess.Move(move.RawValue), false);
                }
            }

        }

        void GoCommand(string[] args)
        {
            int wtime = 0, btime = 0;
            Board apiBoard = new Board(board);
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "wtime")
                {
                    wtime = Int32.Parse(args[i + 1]);
                }
                else if (args[i] == "btime")
                {
                    btime = Int32.Parse(args[i + 1]);
                }
            }
            if (!apiBoard.IsWhiteToMove)
            {
                int tmp = wtime;
                wtime = btime;
                btime = tmp;
            }
            Timer timer = new Timer(wtime, btime, 0);
            Move move = bot.Think(apiBoard, timer);
            Console.WriteLine($"bestmove {move.ToString().Substring(7, move.ToString().Length - 8)}");
        }

        void ExecCommand(string line)
        {
            // default split by whitespace
            var tokens = line.Split();

            if (tokens.Length == 0)
                return;

            switch (tokens[0])
            {
                case "uci":
                    Console.WriteLine("id name Chess Challenge");
                    Console.WriteLine("id author AnmolS99");
                    Console.WriteLine("uciok");
                    break;
                case "ucinewgame":
                    bot = ChallengeController.CreateBot(type) ?? new MyBot();
                    break;
                case "position":
                    PositionCommand(tokens);
                    break;
                case "isready":
                    Console.WriteLine("readyok");
                    break;
                case "go":
                    GoCommand(tokens);
                    break;
            }
        }

        public void Run()
        {
            while (true)
            {
                string? line = Console.ReadLine();
                
                if (String.IsNullOrEmpty(line))
                    continue;

                if (line == "quit" || line == "exit")
                    return;
                
                
                ExecCommand(line);
            }
        }
    }
}