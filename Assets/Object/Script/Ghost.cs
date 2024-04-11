using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour
{
    public Tile tile;
    public Board Mainboard;
    public Piece trackingPiece;

    public Tilemap tilemap{get; private set;}
    public Vector3Int[] cells{get; private set;}
    public Vector3Int position{get; private set;}

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        cells = new Vector3Int[4];
    }

    private void LateUpdate()
    {
        Clear();
        Copy();
        Drop();
        Set();
    }

    private void Clear()
    {
        for(int i = 0; i < cells.Length; i++)
        {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    private void Copy()
    {
        for(int i = 0; i < cells.Length; i++)
        {
            cells[i] = trackingPiece.cells[i];
        }
    }

    private void Drop()
    {
        Vector3Int position = this.trackingPiece.position;

        int current = position.y;
        int bottom = -Mainboard.boardSize.y / 2 - 1;

        Mainboard.Clear(this.trackingPiece);

        for(int row = current; row >= bottom; row--)
        {
            position.y = row;

            if(Mainboard.IsValidPosition(this.trackingPiece, position))
            {
                this.position = position;
            }
            else
            {
                break;
            }
        }

        Mainboard.Set(this.trackingPiece);
    }

    private void Set()
    {
        for(int i = 0; i < cells.Length; i++)
        {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, tile);
        }
    }
}
