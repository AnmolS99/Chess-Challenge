using System;
using System.Numerics;
using ChessChallenge.API;
using Board = ChessChallenge.API.Board;
using Move = ChessChallenge.API.Move;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer) => MiniMax(board, 5);

    private Move MiniMax(Board board, int depth)
    {
        bool isMyBotWhite = board.IsWhiteToMove;
        
        Move[] moves = board.GetLegalMoves();

        int alpha = int.MinValue;
        int beta = int.MaxValue;

        if (isMyBotWhite)
        {
            Move? maxValueMove = null;
            int maxValue = int.MinValue;

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
            int minValue = int.MaxValue;

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
            return board.IsWhiteToMove ? int.MinValue : int.MaxValue;
        if (depth == 0)
            return EvaluateBoardState(board);

        int minValue = int.MaxValue;
        foreach (Move move in board.GetLegalMoves())
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
     * Searches "depth" moves ahead.
     */
    private int MaxValue(Board board, int depth, int alpha, int beta)
    {
        if (board.IsDraw())
            return 0;
        if (board.IsInCheckmate())
            return board.IsWhiteToMove ? int.MinValue : int.MaxValue;
        if (depth == 0)
            return EvaluateBoardState(board);

        int maxValue = int.MinValue;
        foreach (Move move in board.GetLegalMoves())
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
        int[] pieceValues = {0, 100, 300, 350, 500, 900};
        
        // The value of all white pieces minus the value of all black pieces
        int totalBoardValue = 0;
        
        for (int pieceInt = 1; pieceInt < 6; pieceInt++)
        {
            // Number of white pieces of this type minus number of black pieces of this type
            int relativeNumWhitePieces = 
                BitOperations.PopCount(board.GetPieceBitboard((PieceType) pieceInt, true)) - 
                BitOperations.PopCount(board.GetPieceBitboard((PieceType) pieceInt, false));
            totalBoardValue += relativeNumWhitePieces * pieceValues[pieceInt];
        }
        return totalBoardValue;
    }
}