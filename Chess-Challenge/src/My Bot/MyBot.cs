using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using ChessChallenge.API;
using Board = ChessChallenge.API.Board;
using Move = ChessChallenge.API.Move;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer) => MiniMax(board, 2);

    private Move MiniMax(Board board, int depth)
    {
        bool isMyBotWhite = board.IsWhiteToMove;
        Move[] moves = board.GetLegalMoves().Where(m => m.IsPromotion && m.PromotionPieceType == PieceType.Queen || !m.IsPromotion).ToArray();
        
        
        Random r = new Random();
        Move bestMove = moves[r.Next(0, moves.Length)];
        
        board.MakeMove(bestMove);
        int bestMoveValue = EvaluateBoardState(board);
        board.UndoMove(bestMove);
        
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            // Evaluate the board position after making the move
            int moveValue = EvaluateBoardState(board);
            if ((isMyBotWhite && moveValue > bestMoveValue) || (!isMyBotWhite && moveValue < bestMoveValue))
            {
                bestMoveValue = moveValue;
                bestMove = move;
            }
            board.UndoMove(move);
        }
        return bestMove;
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