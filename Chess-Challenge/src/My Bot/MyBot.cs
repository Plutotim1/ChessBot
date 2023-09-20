using System;
using System.Collections.Generic;
using ChessChallenge.API;


public class MyBot : IChessBot {

    public int color;
    private readonly Dictionary<PieceType, int> PieceValue;


    public MyBot() {
        PieceValue = new Dictionary<PieceType, int> {
            {PieceType.Pawn, 100},
            {PieceType.Bishop, 320},
            {PieceType.Knight, 300},
            {PieceType.Rook, 480},
            {PieceType.Queen, 900},
            {PieceType.King, 0},
            {PieceType.None, 0}
        };
    }


    public Move Think(Board board, ChessChallenge.API.Timer timer) {
        int time = timer.MillisecondsRemaining / 50;
        color = board.IsWhiteToMove ? 1 : -1;
        return InitializeMiniMax(board, 3);
    }

    
    //returns the value of all the material on the board for white;
    public int EvaluateBoardMaterial(Board board) {
        int val = 0;
        PieceList[] pieces = board.GetAllPieceLists();
        foreach (PieceList list in pieces) {
            val += PieceValue[list[0].PieceType] * list.Count * (list.IsWhitePieceList ? 1 : -1);
        }
        return val * color;
    }


    public Move InitializeMiniMax(Board board, int depth) {

        Move[] moves = board.GetLegalMoves();

        int maxVal = int.MinValue;
        Move bestMove = Move.NullMove;
        int val;

        foreach (Move move in moves) {
            board.MakeMove(move);
            val = Min(board, depth);
            if (val >= maxVal) {
                maxVal = val;
                bestMove = move;
            }
            board.UndoMove(move);
        }

        return bestMove;

    }

    public int Max(Board board, int depth) {
        if (depth == 0) {
            return EvaluateBoardMaterial(board);
        }

        int val = 0;
        int bestVal = int.MinValue;

        Move[] moves = board.GetLegalMoves();

        foreach (Move move in moves) {
            board.MakeMove(move);
            
            val = Min(board, depth - 1);
            bestVal = Math.Max(bestVal, val);
            board.UndoMove(move);
        }

        return bestVal;
    }


    public int Min(Board board, int depth) {
        if (depth == 0) {
            return EvaluateBoardMaterial(board);
        }

        int val = 0;
        int bestVal = int.MaxValue;

        Move[] moves = board.GetLegalMoves();

        foreach (Move move in moves) {
            board.MakeMove(move);
            
            val = Max(board, depth - 1);
            bestVal = Math.Min(bestVal, val);
            board.UndoMove(move);
        }

        return bestVal;
    }



}