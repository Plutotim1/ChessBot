using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private float lastVal;
    private int depth;
    private long operations;
    /*
        TODO:
        make code more efficient
        prioritize captures and checks -> calculate those further into the future
    */
    public Move Think(Board board, Timer timer)
    {
        operations = 0;
        depth = CalculateDepth(timer);
        Move bestMove = GetBestMove(board, depth, float.MinValue);
        Console.WriteLine("evaluations/second: " + (operations / timer.MillisecondsElapsedThisTurn * 1000));
        Console.WriteLine("evaluations:" + operations);
        return bestMove;
    }


    // returns the best legal move in the current position
    // recursionLevel dictates how many moves into the future the bot will see
    private Move GetBestMove(Board board, int recursionDepth, float currentMaxVal) {
        Move[] moves = board.GetLegalMoves();
        float val;
        float maxVal = -1000;
        Move bestMove = Move.NullMove;

        foreach (Move move in moves) {
            val = 0;
            board.MakeMove(move);
            val = EvaluateResultingChange(board, move, board.IsWhiteToMove);
            if (recursionDepth != 0) {
                GetBestMove(board, recursionDepth -1, -maxVal);
                val -= lastVal;
            }
            board.UndoMove(move);
            if (val > maxVal) {
                //if (val > currentMaxVal) {
                //    lastVal = val;
                //    return move;
                //}
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
        float val = 0f;
        //pieces that should be in the middle of the board
        if ((type == PieceType.Knight) || (type == PieceType.Bishop) || (type == PieceType.Queen)) {
            if (sq.Rank >= 3 &&  sq.File >= 3 && sq.Rank <= 6 && sq.File <= 6) {
                val += 0.2f * ((float)type);
            }
        }
        if (type == PieceType.Pawn) {
            val += isWhite ? sq.File * 0.03f : sq.File * -0-03f + 8;
        }
        return val;
    }


    //add the additional value the new board has over the old one to the initial value and return it
    //needs to be given the new board
    private float EvaluateResultingChange(Board board, Move move, bool isWhite) {
        float val = 0;

        if (board.IsInCheckmate()) {
            return float.MaxValue;
        }
        if (board.IsDraw()) {
            return 0f;
        }
        if (board.IsInCheck()) {
            val += 0.7f;
        }
        PieceType type = PieceType.None;
        if (move.IsCapture) {
            type = move.CapturePieceType;
            val += EvaluatePosition(move.TargetSquare, move.CapturePieceType, isWhite);
        }
        if (move.IsPromotion) {
            type = move.PromotionPieceType;
            //remove one from the final value because you "lose" a pawn
            val--;
        }
        if (type != PieceType.None) {
            switch(type) {
                case PieceType.Pawn:
                    val += 1f;
                    break;
                case PieceType.Knight:
                    val += 3f;
                    break;
                case PieceType.Bishop:
                    val += 3f;
                    break;
                case PieceType.Rook:
                    val += 5f;
                    break;
                case PieceType.Queen:
                    val += 9f;
                    break; 
            }
        }
        //evaluate piece positions
        val += EvaluatePosition(move.TargetSquare, move.MovePieceType, isWhite) - EvaluatePosition(move.StartSquare, move.MovePieceType, isWhite);
        return val;
    }


    /*
    < 5 seconds --> 2
    if > 20 and more than opponent --> 4
    else --> 3
    */
    private int CalculateDepth(Timer timer) {
        int t = timer.MillisecondsRemaining;
        if (t < 5000) {
            return 2;
        }
        if (t > 20000 && t > timer.OpponentMillisecondsRemaining) {
            return 4;
        }
        return 3;
    }
}