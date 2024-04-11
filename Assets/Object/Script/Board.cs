using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public Tilemap tilemap {get; private set; }
    public Tilemap holdTile;
    public Tilemap nextTile;
    public Piece activePiece {get; private set;}
    public TetrominoData[] tetrominos;
    public List<TetrominoData> tempTetrominos;
    public List<TetrominoData> nextTetrominos;
    public Vector3Int spawnPostition;
    public Vector3Int nextPiecePostition;
    public Vector3Int holdPiecePostition;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public int score;
    public int totalLine {get; private set;}
    public int level {get; private set;}
    public bool backToBack {get; private set;}
    public int combo;
    public TMP_Text scoreText;
    public TMP_Text LinesText;
    public TMP_Text LevelText;
    public GameObject gameoverUi;

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();
        this.score = 0;
        this.totalLine = 0;
        this.level = 1;
        this.backToBack = false;
        this.combo = 0;

        for (int i = 0; i < this.tetrominos.Length; i++)
        {
            this.tetrominos[i].Initialize();
        }
    }

    public void SpawnPiece()
    {
        if(this.nextTetrominos.Count == 0)
        {
            int startrandom = Random.Range(0, this.tetrominos.Length);
            this.tempTetrominos.Add(this.tetrominos[startrandom]);
            TetrominoData startdata = this.tetrominos[startrandom];
            this.activePiece.Initialize(this, this.spawnPostition, startdata);
            int nextRandom = Random.Range(0, this.tetrominos.Length);
            while(this.tempTetrominos.Contains(this.tetrominos[nextRandom]))
            {
                nextRandom = Random.Range(0, this.tetrominos.Length);
            }
            this.nextTetrominos.Add(this.tetrominos[nextRandom]);
            for(int i = 0; i < this.nextTetrominos[0].cells.Length; i++)
            {
                Vector3Int position = new Vector3Int(this.nextTetrominos[0].cells[i].x, this.nextTetrominos[0].cells[i].y, 0);
                Vector3Int tilePosition = position + this.nextPiecePostition;
                this.nextTile.SetTile(tilePosition, this.nextTetrominos[0].tile);
            }
        }
        else
        {
            if(this.tempTetrominos.Count == this.tetrominos.Length)
            {
                this.tempTetrominos.Clear();
            }

            int random = Random.Range(0, this.tetrominos.Length);
            while(this.tempTetrominos.Contains(this.tetrominos[random]))
            {
                random = Random.Range(0, this.tetrominos.Length);
            }
            
            this.tempTetrominos.Add(this.tetrominos[random]);
            TetrominoData data = this.nextTetrominos[0];
            this.nextTetrominos[0] = this.tetrominos[random];

            this.activePiece.Initialize(this, this.spawnPostition, data);
            this.nextTile.ClearAllTiles();
            for(int i = 0; i < this.nextTetrominos[0].cells.Length; i++)
            {
                Vector3Int position = new Vector3Int(this.nextTetrominos[0].cells[i].x, this.nextTetrominos[0].cells[i].y, 0);
                Vector3Int tilePosition = position + this.nextPiecePostition;
                this.nextTile.SetTile(tilePosition, this.nextTetrominos[0].tile);
            }
            }

        if(IsValidPosition(this.activePiece, this.spawnPostition))
        {
            Set(this.activePiece);
        } else{
            GameOver();
        }
    }

    private void GameOver()
    {
        gameoverUi.SetActive(true);
        activePiece.enabled = false;
    }

    public void Restart()
    {
        this.tilemap.ClearAllTiles();
        this.holdTile.ClearAllTiles();
        this.nextTile.ClearAllTiles();
        this.score = 0;
        this.totalLine = 0;
        this.level = 1;
        this.backToBack = false;
        this.activePiece.holdTetromino.Clear();
        this.nextTetrominos.Clear();
        this.tempTetrominos.Clear();
        this.scoreText.text = score.ToString();
        this.LinesText.text = totalLine.ToString();
        this.LevelText.text = level.ToString();
        SpawnPiece();
    }

    public void Set(Piece piece)
    {
        for(int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for(int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;

        for(int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if(!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            if(this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;
        int line = 0;

        while (row < bounds.yMax)
        {
            if(IsLineFull(row)){
                LineClear(row);
                line++;
            }
            else{
                row++;
            }
        }
        int addScore = Score(line, level);
        score = score + addScore;
        int newTotalLine = totalLine + line;
        scoreText.text = score.ToString();
        if(newTotalLine != totalLine)
        {
            totalLine = newTotalLine;
            LinesText.text = totalLine.ToString();
            if(totalLine / 10 == level)
            {
                level++;
                LevelText.text = level.ToString();
            }
        }
    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;

        for(int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if(!this.tilemap.HasTile(position)){
                return false;
            }
        }

        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;

        for(int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax)
        {
            for(int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = this.tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(position, above);
            }

            row++;
        }
    }

    private int Score(int line, int level)
    {
        switch(line)
        {
            case 1:
                if(combo > 0)
                {
                    backToBack = false;
                    combo++;
                    return 100 * level + 50 * combo * level;
                }
                else
                {
                    backToBack = false;
                    combo++;
                    return 100 * level;
                }
            case 2:                if(combo > 0)
                {
                    backToBack = false;
                    combo++;
                    return 300 * level + 50 * combo * level;
                }
                else
                {
                    backToBack = false;
                    combo++;
                    return 300 * level;
                }
            case 3:
                if(combo > 0)
                {
                    backToBack = false;
                    combo++;
                    return 500 * level + 50 * combo * level;
                }
                else
                {
                    backToBack = false;
                    combo++;
                    return 500 * level;
                }
            case 4:
                if(combo > 0)
                {
                    if(backToBack)
                    {
                        double backToBackScore = 800 * level * 1.5 + 50 * combo * level;
                        combo++;
                        return (int)backToBackScore;
                    }
                    else
                    {
                        backToBack = true;
                        combo++;
                        return 800 * level + 50 * combo * level;
                    }
                }
                else
                {
                    backToBack = true;
                    combo++;
                    return 800 * level + 50 * combo * level;
                }
            default:
                combo = 0;
                return 0;
        }
    }
}
