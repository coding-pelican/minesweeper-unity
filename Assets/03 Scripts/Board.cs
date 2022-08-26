using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class Board : MonoBehaviour {
    public Tilemap tilemap { get; private set; }
    public Tile tileUnknown;
    public Tile tileEmpty;
    public Tile tileMine;
    public Tile tileExploded;
    public Tile tileFlag;
    public Tile[] tileNums;

    private void Awake() {
        tilemap = GetComponent<Tilemap>();
    }

    public void Draw(Cell[,] state) {
        int width = state.GetLength(0);
        int height = state.GetLength(1);

        for (var x = 0; x < width; x++) {
            for (var y = 0; y < height; y++) {
                Cell cell = state[x, y];
                cell.number = Mathf.Clamp(cell.number, 0, 8);
                tilemap.SetTile(cell.position, GetTile(cell));
            }
        }
    }

    private Tile GetTile(Cell cell) {
        if (cell.revealed) {
            return GetRevealedTile(cell);
        } else if (cell.flagged) {
            return tileFlag;
        } else {
            return tileUnknown;
        }
    }

    private Tile GetRevealedTile(Cell cell) {
        return cell.type switch {
            Cell.Type.Empty => tileEmpty,
            Cell.Type.Mine => cell.exploded ? tileExploded : tileMine,
            Cell.Type.Number => GetNumberTile(cell),
            _ => null,
        };
    }

    private Tile GetNumberTile(Cell cell) => tileNums[cell.number - 1];
}
