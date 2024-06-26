using System.Collections.Generic;
using System.Net.Mail;
using Unity.VisualScripting;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board {get; private set;}
    public TetrominoData data {get; private set;}
    public List<TetrominoData> holdTetromino;
    public Vector3Int[] cells {get; private set;}
    public Vector3Int position {get; private set;}
    public int rotationIndex {get; private set;}
    
    public float stepDelay = 1f;
    public float lockDelay = 0.5f;

    private float stepTime;
    private float lockTime;

    private bool canHold;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.data = data;
        this.position = position;
        this.rotationIndex = 0;
        this.stepTime = Time.time + this.stepDelay;
        this.lockTime = 0f;
        this.canHold = true;

        if(this.cells == null)
        {
            this.cells = new Vector3Int[data.cells.Length];
        }

        for(int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update()
    {
        this.board.Clear(this);

        this.lockTime += Time.deltaTime;

        if(Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.UpArrow)){
            rotate(-1);
        }
        else if(Input.GetKeyDown(KeyCode.E)){
            rotate(1);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow)){
            Move(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)){
            Move(Vector2Int.right);
        }

        if(Input.GetKeyDown(KeyCode.DownArrow)){
            Move(Vector2Int.down);
        }

        if(Input.GetKeyDown(KeyCode.Space)){
            HardDrop();
        }
        if(Input.GetKeyDown(KeyCode.C)){
            if(canHold)
                HoldBlack();
        }

        if (Time.time >= this.stepTime){
            Step();
        }

        this.board.Set(this);
    }

    private void HoldBlack()
    {
        if(this.holdTetromino.Count == 0)
        {
            for(int i = 0; i < this.cells.Length; i++)
            {
                Vector3Int tilePosition = this.cells[i] + this.board.holdPiecePostition;
                this.board.holdTile.SetTile(tilePosition, this.data.tile);
            }
            this.holdTetromino.Add(data);
            this.board.SpawnPiece();
            canHold = false;
        }
        else if(this.holdTetromino[0].tetromino != data.tetromino)
        {   
            this.board.holdTile.ClearAllTiles();
            for(int i = 0; i < this.cells.Length; i++)
            {
                Vector3Int tilePosition = this.cells[i] + this.board.holdPiecePostition;
                this.board.holdTile.SetTile(tilePosition, this.data.tile);
            }
            TetrominoData holdTetrominoTemp = this.holdTetromino[0];
            this.holdTetromino[0] = data;
            this.board.activePiece.Initialize(this.board, this.board.spawnPostition, holdTetrominoTemp);
            canHold = false;
        }
    }

    private void Step()
    {
        this.stepTime = Time.time + this.stepDelay / this.board.level;

        Move(Vector2Int.down);

        if(this.lockTime >= this.lockDelay){
            Lock();
        }
    }

    private void Lock()
    {
        this.board.Set(this);
        this.board.ClearLines();
        this.board.SpawnPiece();
    }

    private void HardDrop(){
        while(Move(Vector2Int.down)){
            this.board.score += 2;
            continue;
        }

        Lock();
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = this.position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = this.board.IsValidPosition(this ,newPosition);

        if(valid)
        {
            this.position = newPosition;
            this.lockTime = 0f;
        }

        return valid;
    }

    private void rotate(int direction)
    {
        int originalRotation = this.rotationIndex;
        this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction);

        if(!TestWallKicks(this.rotationIndex, direction))
        {
            this.rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < this.data.cells.Length; i++)
            {
                Vector3 cell = this.cells[i];

                int x, y;

                switch(this.data.tetromino)
                {
                    case Tetromino.I:
                    case Tetromino.O:
                        cell.x -= 0.5f;
                        cell.y -= 0.5f;
                        x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                        y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                        break;
                    default:
                        x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                        y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                        break;
                }

                this.cells[i] = new Vector3Int(x, y, 0);
            }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);
        
        for (int i = 0; i < this.data.wallKicks.GetLength(1); i++)
        {
            Vector2Int tranlation = this.data.wallKicks[wallKickIndex, i];

            if(Move(tranlation))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, this.data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if(input < min){
            return max - (min - input) % (max - min);
        } else{
            return min + (input - min) % (max - min);
        }
    }
}
