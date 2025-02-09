using System;
using System.Linq;
using System.Numerics;
using ChessChallenge.API;
using Board = ChessChallenge.API.Board;
using Move = ChessChallenge.API.Move;

public class MyBot : IChessBot
{
    private const int IntMinValue = -1000000;
    private const int IntMaxValue = 1000000;
    
    public Move Think(Board board, Timer timer) => MiniMax(board, 6);

    private Move MiniMax(Board board, int depth)
    {
        bool isMyBotWhite = board.IsWhiteToMove;
        
        Move[] moves = board.GetLegalMoves().OrderByDescending(move => move.IsCapture).ToArray();

        int alpha = IntMinValue;
        int beta = IntMaxValue;

        if (isMyBotWhite)
        {
            Move? maxValueMove = null;
            int maxValue = IntMinValue;

            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int minValue = MinValue(board, depth - 1, alpha, beta);
                board.UndoMove(move);

                if (minValue > maxValue || maxValueMove is null)
                {
                    maxValue = minValue;
                    maxValueMove = move;
                }

                alpha = Math.Max(alpha, minValue);
            }

            return maxValueMove ?? moves[0];
        }
        else
        {
            Move? minValueMove = null;
            int minValue = IntMaxValue;

            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int maxValue = MaxValue(board, depth - 1, alpha, beta);
                board.UndoMove(move);

                if (maxValue < minValue || minValueMove is null)
                {
                    minValue = maxValue;
                    minValueMove = move;
                }

                beta = Math.Min(beta, maxValue);
            }

            return minValueMove ?? moves[0];
        }
    }
    
    /*
     * Function for finding the lowest value possible (given optimal play) from the current board state.
     * Searches "depth" moves ahead.
     */
    private int MinValue(Board board, int depth, int alpha, int beta)
    {
        if (board.IsDraw())
            return 0;
        if (board.IsInCheckmate())
            return board.IsWhiteToMove ? IntMinValue - depth : IntMaxValue + depth;
        if (depth == 0)
            return EvaluateBoardState(board);

        int minValue = IntMaxValue;
        foreach (Move move in board.GetLegalMoves().OrderByDescending(move => move.IsCapture).ToArray())
        {
            board.MakeMove(move);
            // Take the lowest of the current minValue and the maximum value the child node (opponent) can achieve
            minValue = Math.Min(minValue, MaxValue(board, depth - 1, alpha, beta));
            board.UndoMove(move);
            
            // If minValue is lower or equal to alpha (the current best move white has)
            // There is no point in searching through any more moves, as this means white
            // has an equal or better move already
            if (minValue <= alpha)
                return minValue;
            
            beta = Math.Min(minValue, beta);
        }
        return minValue;
    }

    /*
     * Function for finding the highest value possible (given optimal play) from the current board state.
     * Searches "depth" moves (ply) ahead.
     */
    private int MaxValue(Board board, int depth, int alpha, int beta)
    {
        if (board.IsDraw())
            return 0;
        if (board.IsInCheckmate())
            return board.IsWhiteToMove ? IntMinValue - depth : IntMaxValue + depth;
        if (depth == 0)
            return EvaluateBoardState(board);

        int maxValue = IntMinValue;
        foreach (Move move in board.GetLegalMoves().OrderByDescending(move => move.IsCapture).ToArray())
        {
            board.MakeMove(move);
            // Take the highest of the current maxValue and the minimum value the child node (opponent) can achieve
            maxValue = Math.Max(maxValue, MinValue(board, depth - 1, alpha, beta));
            board.UndoMove(move);
            
            // If maxValue is higher or equal to beta (the current best move black has)
            // There is no point in searching through any more moves, as this means black
            // has an equal or better move already.
            if (maxValue >= beta)
                return maxValue;
            alpha = Math.Max(maxValue, alpha);
        }
        return maxValue;
    }

    private int EvaluateBoardState(Board board)
    {
        
        int[] pieceValues = [1000, 3000, 3500, 5000, 9000];
        
        int totalBoardValue = 0;
        
        for (int pieceInt = 1; pieceInt < 7; pieceInt++)
        {
            // No point calculating material difference for king
            if (pieceInt != 6)
            {
                // Number of white pieces of this type minus number of black pieces of this type
                int relativeNumWhitePieces = 
                    BitOperations.PopCount(board.GetPieceBitboard((PieceType) pieceInt, true)) - 
                    BitOperations.PopCount(board.GetPieceBitboard((PieceType) pieceInt, false));
                
                totalBoardValue += relativeNumWhitePieces * pieceValues[pieceInt - 1];
            }
            
            // Compute sum of positional values for white
            ulong mask = board.GetPieceBitboard((PieceType)pieceInt, true);

            while (mask != 0)
            {
                int bitIndex = BitOperations.TrailingZeroCount(mask); // Get index of the least significant set bit

                mask &= mask - 1; // Clear the least significant bit

                int row = 7 - bitIndex / 8;
                int col = bitIndex % 8;

                totalBoardValue += _pieceEvalTable[pieceInt - 1, row, col];
            }
            
            // Compute sum of positional values for black
            mask = board.GetPieceBitboard((PieceType)pieceInt, false);

            while (mask != 0)
            {
                int bitIndex = BitOperations.TrailingZeroCount(mask); // Get index of the least significant set bit

                mask &= mask - 1; // Clear the least significant bit
                
                int row = bitIndex / 8;
                int col = 7 - bitIndex % 8;

                totalBoardValue -= _pieceEvalTable[pieceInt - 1, row, col];
            }
        }
        return totalBoardValue;
    }

    private readonly int[,,] _pieceEvalTable =
    {
        // Pawns
        {
            { 0,  0,  0,  0,  0,  0,  0,  0 },
            { 50, 50, 50, 50, 50, 50, 50, 50 },
            { 10, 10, 20, 30, 30, 20, 10, 10 },
            { 5,  5, 10, 25, 25, 10,  5,  5 },
            { 0,  0,  0, 20, 20,  0,  0,  0 },
            { 5, -5, -10, 0,  0, -10, -5, 5 },
            { 5, 10, 10, -20, -20, 10, 10, 5 },
            { 0,  0,  0,  0,  0,  0,  0,  0 }
        },
        // Knights
        {
            { -50,-40,-30,-30,-30,-30,-40,-50 },
            { -40,-20,  0,  0,  0,  0,-20,-40 },
            { -30,  0, 10, 15, 15, 10,  0,-30 },
            { -30,  5, 15, 20, 20, 15,  5,-30 },
            { -30,  0, 15, 20, 20, 15,  0,-30 },
            { -30,  5, 10, 15, 15, 10,  5,-30 },
            { -40,-20,  0,  5,  5,  0,-20,-40 },
            { -50,-40,-30,-30,-30,-30,-40,-50 }
        },
        // Bishop
        {
            { -20,-10,-10,-10,-10,-10,-10,-20 },
            { -10,  0,  0,  0,  0,  0,  0,-10 },
            { -10,  0,  5, 10, 10,  5,  0,-10 },
            { -10,  5,  5, 10, 10,  5,  5,-10 },
            { -10,  0, 10, 10, 10, 10,  0,-10 },
            { -10, 10, 10, 10, 10, 10, 10,-10 },
            { -10,  5,  0,  0,  0,  0,  5,-10 },
            { -20,-10,-10,-10,-10,-10,-10,-20 }
        },
        // Rook
        {
            { 0,  0,  0,  0,  0,  0,  0,  0 },
            { 5, 10, 10, 10, 10, 10, 10,  5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5, },
            { -5,  0,  0,  0,  0,  0,  0, -5, },
            { 0,  0,  0,  5,  5,  0,  0,  0 }
        },
        // Queen
        {
            { -20,-10,-10, -5, -5,-10,-10,-20 },
            { -10,  0,  0,  0,  0,  0,  0,-10 },
            { -10,  0,  5,  5,  5,  5,  0,-10 },
            { -5,  0,  5,  5,  5,  5,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5, },
            { -5,  0,  0,  0,  0,  0,  0, -5, },
            { 0,  0,  0,  5,  5,  0,  0,  0 }
        },
        // King
        {
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -20,-30,-30,-40,-40,-30,-30,-20 },
            { -10,-20,-20,-20,-20,-20,-20,-10 },
            { 20, 20,  0,  0,  0,  0, 20, 20 },
            { 20, 30, 10,  0,  0, 10, 30, 20 }
        }
        
    };
}