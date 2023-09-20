using System;
using System.Collections.Generic;
using ChessChallenge.API;

public class MyBot1 : IChessBot
{
    private int lastVal;
    private int depth;
    private long operations;

    private bool isWhite;

    private readonly Dictionary<PieceType, int> PieceValue;


    public MyBot1() {
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


    public Move Think(Board board, ChessChallenge.API.Timer timer)
    {
        operations = 0;
        isWhite = board.IsWhiteToMove;
        depth = CalculateDepth(timer);
        Move bestMove = GetBestMove(board, depth, -1000000, 1000000, 1);
        if (!(operations == 0 || timer.MillisecondsElapsedThisTurn == 0)) {
            Console.WriteLine("evaluations/second: " + (operations / timer.MillisecondsElapsedThisTurn * 1000));
            Console.WriteLine("evaluations:" + operations);
        }
        return bestMove;
    }


    private Move GetBestMove(Board board, int recursionDepth, int alpha, int beta/*, int extraScore*/, int color) {
        Move[] moves = board.GetLegalMoves();

        //assign a estimated value to each move
        int[] vals = new int[moves.Length];
        for (int i = 0; i < vals.Length; i++) {
            vals[i] = EstimateMove(moves[i]);
        }

        //sort moves by estimated value with Quicksort
        QuickSortNow(moves, vals, 0, vals.Length - 1);

        //initialize variables
        int val;
        int maxVal = int.MinValue;
        Move bestMove = moves[0];

        //loop over moves
        foreach (Move move in moves) {
            board.MakeMove(move);

            /*
            //instantly return value if a checkmate is found
            if (board.IsInCheckmate()) {
                board.UndoMove(move);
                lastVal = 10000 - depth * 1000;
                return move;
            }

            //evaluate board and subtract the opponents best move afterwards
            if (recursionDepth != 0 && !IsGameOver(board)) {
                GetBestMove(board, recursionDepth - 1, -alpha, -beta, EvaluateResultingChange(board, move, board.IsWhiteToMove) * color, -color);
                val = -lastVal;
            } else {
                val = (EvaluateResultingChange(board, move, board.IsWhiteToMove) + extraScore) * color;
            }
            */
            if (recursionDepth == 0) {
                val = EvaluateBoard(board);
            }
            else {
                GetBestMove(board, recursionDepth - 1, -alpha, -beta/*, EvaluateResultingChange(board, move, board.IsWhiteToMove) * color*/, -color);
                val = -lastVal;
            }
            board.UndoMove(move);
            
            //check if the move is the best move until now
            if (val > maxVal) {
                bestMove = move; 
                maxVal = val;
            }
        }

        //debugging
        if (recursionDepth == depth) {
            Console.WriteLine(maxVal);
        }

        //return value and move
        lastVal =  maxVal;
        return bestMove;
    }


    private int EvaluatePosition(Square sq, PieceType type, bool isWhite) {
        //measuring
        operations++;

        //pieces that should be in the middle of the board
        if ((int) type > 1 && (int) type < 6) {
            if (sq.Rank >= 3 &&  sq.File >= 3 && sq.Rank <= 6 && sq.File <= 6) {
                return 25;
            }
        }

        //pawns
        if (type == PieceType.Pawn) {
            return isWhite ? sq.File * 5 : (-sq.File + 8) * 5;
        }
        return 0;
    }


    private int EvaluateResultingChange(Board board, Move move, bool isWhite) {
        int val = 0;

        if (board.IsDraw()) {
            return 0;
        }
        if (board.IsInCheck()) {
            if (board.FiftyMoveCounter < 30) {
                val += 40;
            }
        }
        if (move.IsCapture) {
            val += PieceValue[move.CapturePieceType];
            val += EvaluatePosition(move.TargetSquare, move.CapturePieceType, isWhite);
        }
        if (move.IsPromotion) {
            val += PieceValue[move.PromotionPieceType];
            //pawn "loss"
            val--;
        }
        //evaluate piece positions
        val += EvaluatePosition(move.TargetSquare, move.MovePieceType, isWhite) - EvaluatePosition(move.StartSquare, move.MovePieceType, isWhite);
        return val;
    }

    private int EstimateMove(Move move) {
        if (move.IsCapture) {
            return PieceValue[move.CapturePieceType];
        }
        return 0;
    }


    private int EvaluateBoard(Board board) {
        operations++;
        int val = 0;
        PieceList[] pieces = board.GetAllPieceLists();
        foreach (PieceList list in pieces) {
            val += PieceValue[list[0].PieceType] * list.Count * (list.IsWhitePieceList ? 1 : -1);
        }
        return isWhite ? val : -val;
    }


    /*
    < 5 seconds --> 2
    if > 20 and more than opponent --> 4
    else --> 3
    */
    private int CalculateDepth(ChessChallenge.API.Timer timer) {
        return 3;
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


    public static void QuickSortNow(Move[] moves, int[] iInput, int start, int end)
    {
        if (start < end)
        {
            int pivot = Partition(moves, iInput, start, end);
            QuickSortNow(moves, iInput, start, pivot - 1);
            QuickSortNow(moves, iInput, pivot + 1, end);
        }
    }

    public static int Partition(Move[] moves, int[] iInput, int start, int end)
    {
        int pivot = iInput[end];
        int pIndex = start;

        for (int i = start; i < end; i++)
        {
            if (iInput[i] >= pivot)
            {
                int temp = iInput[i];
                Move tempM = moves[i];

                iInput[i] = iInput[pIndex];
                moves[i] = moves[pIndex];
                iInput[pIndex] = temp;
                moves[pIndex] = tempM;
                pIndex++;
            }
        }
        int anotherTemp = iInput[pIndex];
        Move anotherTempM = moves[pIndex];
        iInput[pIndex] = iInput[end];
        moves[pIndex] = moves[end];
        iInput[end] = anotherTemp;
        moves[end] = anotherTempM;
        return pIndex;
    }
}