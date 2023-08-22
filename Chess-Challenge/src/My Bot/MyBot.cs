using System;
using System.Dynamic;
using System.Numerics;
using System.Runtime.InteropServices;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private bool isWhite;
    private long operations;
    /*
        TODO:
        Reward moves based on what position pieces are moved to     X
        make code more efficient
        prioritize captures and checks -> calculate those further into the future
        generally evaluate for white to simplify;
        evaluate only boards instead of including moves
    */
    public Move Think(Board board, Timer timer)
    {
        operations = 0;
        isWhite = board.IsWhiteToMove;
        Move bestMove = GetBestMove(board, 3);
        Console.WriteLine("evaluations/second: " + (operations / timer.MillisecondsElapsedThisTurn * 1000));
        return bestMove;
    }

    //evaluates how many points a move is worth by the pieces promoted or taken
    private float Evaluate(Move move) {
        float val = 0f;
        PieceType type = PieceType.None;
        if (move.IsCapture) {
            type = move.CapturePieceType;
            val += EvaluatePosition(move.TargetSquare, move.CapturePieceType);
        }
        if (move.IsPromotion) {
            type = move.PromotionPieceType;
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
        val += EvaluatePosition(move.TargetSquare, move.MovePieceType) - EvaluatePosition(move.StartSquare, move.MovePieceType);
        return val;
    }

    // returns the best legal move in the current position
    // recursionLevel dictates how many moves into the future the bot will see
    private Move GetBestMove(Board board, int recursionLevel) {
        Move[] moves = board.GetLegalMoves();
        float val;
        Move move1;
        float bestVal = float.MinValue;
        Move bestMove = Move.NullMove;

        foreach (Move move in moves) {
            val = 0;
            board.MakeMove(move);
            val = EvaluateResultingChange(board, move);
            if (recursionLevel != 0) {
                move1 = GetBestMove(board, recursionLevel -1);
                board.MakeMove(move1);
                val -= EvaluateResultingChange(board, move1);
                board.UndoMove(move1);
            }
            board.UndoMove(move);
            if (val > bestVal) {
                bestMove = move;
                bestVal = val;
            }
            
        }
        if(recursionLevel == 3) {
            Console.WriteLine(bestVal);
        }
        return bestMove;
    }

    private float EvaluatePosition(Square sq, PieceType type) {
        operations++;
        float val = 0f;
        //pieces that should be in the middle of the board
        if ((type == PieceType.Knight) || (type == PieceType.Bishop) || (type == PieceType.Queen)) {
            if (sq.Rank >= 3 &&  sq.File >= 3 && sq.Rank <= 6 && sq.File <= 6) {
                val += 0.2f * ((float)type);
            }
        }
        if (type == PieceType.Pawn) {
            val += isWhite ? sq.File * 0.03f : sq.File * -0-03f;
        }
        return val;
    }

    //add the additional value the new board has over the old one to the initial value and return it
    //needs to be given the new board
    private float EvaluateResultingChange(Board board, Move move) {
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
            val += EvaluatePosition(move.TargetSquare, move.CapturePieceType);
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
        val += EvaluatePosition(move.TargetSquare, move.MovePieceType) - EvaluatePosition(move.StartSquare, move.MovePieceType);
        return val;
    }
}