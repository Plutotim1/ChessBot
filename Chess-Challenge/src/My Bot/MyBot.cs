using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private float lastVal;
    private int depth;
    private long operations;
    public Move Think(Board board, ChessChallenge.API.Timer timer)
    {
        operations = 0;
        depth = CalculateDepth(timer);
        Move bestMove = GetBestMove(board, depth, float.PositiveInfinity);
        if (!(operations == 0 || timer.MillisecondsElapsedThisTurn == 0)) {
            Console.WriteLine("evaluations/second: " + (operations / timer.MillisecondsElapsedThisTurn * 1000));
            Console.WriteLine("evaluations:" + operations);
        }
        return bestMove;
    }


    private Move GetBestMove(Board board, int recursionDepth, float currentMaxVal) {
        Move[] moves = board.GetLegalMoves();
        //assign a estimated value to each move
        float[] vals = new float[moves.Length];
        for (int i = 0; i < vals.Length; i++) {
            vals[i] = EstimateMove(moves[i]);
        }
        //sort moves by estimated value with Quicksort
        QuickSortNow(moves, vals, 0, vals.Length - 1);
        float val;
        float maxVal = float.MinValue;
        Move bestMove = moves[0];
        foreach (Move move in moves) {
            board.MakeMove(move);
            if (board.IsInCheckmate()) {
                board.UndoMove(move);
                lastVal = 10000 - depth * 1000;
                return move;
            }
            val = EvaluateResultingChange(board, move, board.IsWhiteToMove);
            if (recursionDepth != 0 && !IsGameOver(board)) {
                GetBestMove(board, recursionDepth -1, -maxVal);
                val -= lastVal;
            }
            board.UndoMove(move);
            if (val > maxVal) {
                if (val > currentMaxVal) {
                    lastVal = val;
                    return move;
                }
                bestMove = move; 
                maxVal = val;
            }
        }
        if (recursionDepth == depth) {
            Console.WriteLine(maxVal);
        }
        lastVal =  maxVal;
        return bestMove;
    }


    private float EvaluatePosition(Square sq, PieceType type, bool isWhite) {
        operations++;
        //pieces that should be in the middle of the board
        if ((int) type > 1 && (int) type < 6) {
            if (sq.Rank >= 3 &&  sq.File >= 3 && sq.Rank <= 6 && sq.File <= 6) {
                return 0.25f;
            }
        }
        if (type == PieceType.Pawn) {
            return isWhite ? sq.File * 0.03f : sq.File * -0.3f + 8;
        }
        return 0f;
    }


    private float EvaluateResultingChange(Board board, Move move, bool isWhite) {
        float val = 0;

        if (board.IsDraw()) {
            return -0.5f;
        }
        if (board.IsInCheck()) {
            if (board.FiftyMoveCounter < 30) {
                val += 0.4f;
            }
        }

        PieceType type = PieceType.None;
        if (move.IsCapture) {
            type = move.CapturePieceType;
            val += EvaluatePosition(move.TargetSquare, move.CapturePieceType, isWhite);
        }
        if (move.IsPromotion) {
            type = move.PromotionPieceType;
            //pawn "loss"
            val--;
        }
        val += GetPieceValue(type);
        //evaluate piece positions
        val += EvaluatePosition(move.TargetSquare, move.MovePieceType, isWhite) - EvaluatePosition(move.StartSquare, move.MovePieceType, isWhite);
        return val;
    }

    private float EstimateMove(Move move) {
        if (move.IsCapture) {
            return GetPieceValue(move.CapturePieceType);
        }
        return 0f;
    }

    private float GetPieceValue(PieceType type) {
        switch(type) {
                case PieceType.Pawn:
                    return 1f;
                case PieceType.Knight:
                    return 3f;
                case PieceType.Bishop:
                    return 3f;
                    
                case PieceType.Rook:
                    return 5f;
                    
                case PieceType.Queen:
                    return 9f;
                default:
                    return 0f;
            }
    }


    /*
    < 5 seconds --> 2
    if > 20 and more than opponent --> 4
    else --> 3
    */
    private int CalculateDepth(ChessChallenge.API.Timer timer) {
        int t = timer.MillisecondsRemaining;
        if (t < 5000) {
            return 3;
        }
        if ((t > 20000 && t > timer.OpponentMillisecondsRemaining) || t > 60000) {
            return 5;
        }
        return 4;
    }


    private bool IsGameOver(Board board) {
        if (board.IsInCheckmate() || board.IsDraw()) {
            return true;
        }
        else {
            return false;
        }
    }


    public static void QuickSortNow(Move[] moves, float[] iInput, int start, int end)
    {
        if (start < end)
        {
            int pivot = Partition(moves, iInput, start, end);
            QuickSortNow(moves, iInput, start, pivot - 1);
            QuickSortNow(moves, iInput, pivot + 1, end);
        }
    }

    public static int Partition(Move[] moves, float[] iInput, int start, int end)
    {
        float pivot = iInput[end];
        int pIndex = start;

        for (int i = start; i < end; i++)
        {
            if (iInput[i] >= pivot)
            {
                float temp = iInput[i];
                Move tempM = moves[i];

                iInput[i] = iInput[pIndex];
                moves[i] = moves[pIndex];
                iInput[pIndex] = temp;
                moves[pIndex] = tempM;
                pIndex++;
            }
        }
        float anotherTemp = iInput[pIndex];
        Move anotherTempM = moves[pIndex];
        iInput[pIndex] = iInput[end];
        moves[pIndex] = moves[end];
        iInput[end] = anotherTemp;
        moves[end] = anotherTempM;
        return pIndex;
    }
}