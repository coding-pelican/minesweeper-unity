using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour {
    public int width = 16;
    public int height = 16;
    public int mineCount = 32;

    private Board _board;
    private Cell[,] _state;
    private bool _gameover;

    private readonly KeyCode _flagKey = KeyCode.Mouse1;
    private readonly KeyCode _revealKey = KeyCode.Mouse0;
    private readonly KeyCode _newgameKey = KeyCode.R;

    private void OnValidate() {
        mineCount = Mathf.Clamp(mineCount, 0, width * height);
    }

    private void Awake() {
        Application.targetFrameRate = 60;
        _board = GetComponentInChildren<Board>();
    }

    private void Start() {
        NewGame();
    }

    private void NewGame() {
        _state = new Cell[width, height];
        _gameover = false;
        GenerateCells();
        GenerateMines();
        GenerateNumbers();
        SetCamera();
        _board.Draw(_state);
    }

    private void SetCamera() {
        var cameraStartPosZ = Camera.main.transform.position.z;
        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, cameraStartPosZ);
        Camera.main.orthographicSize = (width > height ? width : height) * 0.5f + 2;
    }

    private void GenerateCells() {
        for (var x = 0; x < width; x++) {
            for (var y = 0; y < height; y++) {
                var cell = new Cell {
                    position = new Vector3Int(x, y, 0),
                    type = Cell.Type.Empty
                };
                _state[x, y] = cell;
            }
        }
    }

    private void GenerateMines() {
        for (var i = 0; i < mineCount; i++) {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            while (_state[x, y].type == Cell.Type.Mine) {
                x++;
                if (x >= width) {
                    x = 0;
                    y++;
                    if (y >= height) {
                        y = 0;
                    }
                }
            }
            _state[x, y].type = Cell.Type.Mine;
        }
    }

    private void GenerateNumbers() {
        for (var x = 0; x < width; x++) {
            for (var y = 0; y < height; y++) {
                Cell cell = _state[x, y];

                if (cell.type == Cell.Type.Mine) {
                    continue;
                }
                cell.number = CountMines(x, y);

                if (cell.number > 0) {
                    cell.type = Cell.Type.Number;
                }
                _state[x, y] = cell;
            }
        }
    }

    private int CountMines(int cellX, int cellY) {
        int count = 0;

        for (var adjacentX = -1; adjacentX <= 1; adjacentX++) {
            for (var adjacentY = -1; adjacentY <= 1; adjacentY++) {
                if ((adjacentX == 0) && (adjacentY == 0)) {
                    continue;
                }
                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

                if (x < 0 || x >= width || y < 0 || y >= height) {
                    continue;
                }
                if (GetCell(x, y).type == Cell.Type.Mine) {
                    count++;
                }
            }
        }
        return count;
    }

    private void Update() {
        if (Input.GetKeyDown(_newgameKey)) {
            NewGame();
        } else if (!_gameover) {
            if (Input.GetKeyDown(_flagKey)) {
                Flag();
            } else if (Input.GetKeyDown(_revealKey)) {
                Reveal();
            }
        }
    }

    private void Reveal() {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = _board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged) {
            return;
        }

        switch (cell.type) {
            case Cell.Type.Mine:
                Explode(cell);
                break;
            case Cell.Type.Empty:
                Flood(cell);
                CheckWinCondition();
                break;
            default:
                cell.revealed = true;
                _state[cellPosition.x, cellPosition.y] = cell;
                CheckWinCondition();
                break;
        }
        _board.Draw(_state);
    }

    private void CheckWinCondition() {
        for (var x = 0; x < width; x++) {
            for (var y = 0; y < height; y++) {
                Cell cell = _state[x, y];

                // All non-mine cells must be revealed to have won
                if (cell.type != Cell.Type.Mine && !cell.revealed) {
                    return; // no win
                }
            }
        }

        Debug.Log("Winner!");
        _gameover = true;

        // Flag all the mines
        for (var x = 0; x < width; x++) {
            for (var y = 0; y < height; y++) {
                Cell cell = _state[x, y];

                if (cell.type == Cell.Type.Mine) {
                    cell.flagged = true;
                    _state[x, y] = cell;
                }
            }
        }
    }

    private void Explode(Cell cell) {
        Debug.Log("Game Over!");
        _gameover = true;
        cell.exploded = true; // Set the mine as exploded
        cell.revealed = true;
        _state[cell.position.x, cell.position.y] = cell;

        // Reveal all other mines
        for (var x = 0; x < width; x++) {
            for (var y = 0; y < height; y++) {
                cell = _state[x, y];

                if (cell.type == Cell.Type.Mine) {
                    cell.revealed = true;
                    _state[x, y] = cell;
                }
            }
        }
    }

    private void Flood(Cell cell) {
        if (cell.revealed) {
            return;
        }
        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) {
            return;
        }
        cell.revealed = true;
        _state[cell.position.x, cell.position.y] = cell;

        if (cell.type == Cell.Type.Empty) {
            Flood(GetCell(cell.position.x - 1, cell.position.y));
            Flood(GetCell(cell.position.x + 1, cell.position.y));
            Flood(GetCell(cell.position.x, cell.position.y - 1));
            Flood(GetCell(cell.position.x, cell.position.y + 1));
        }
    }

    private void Flag() {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = _board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed) {
            return;
        }
        cell.flagged = !cell.flagged;
        _state[cellPosition.x, cellPosition.y] = cell;
        _board.Draw(_state);
    }

    private Cell GetCell(int x, int y) {
        if (IsValidTile(x, y)) {
            return _state[x, y];
        } else {
            return new Cell();
        }
    }

    private bool IsValidTile(int x, int y) {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}
