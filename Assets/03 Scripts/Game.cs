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

    public Board Board { get => _board; set => _board = value; }

    private void Awake() {
        Application.targetFrameRate = 60;
        Board = GetComponentInChildren<Board>();
    }

    private void Start() {
        NewGame();
    }

    private void NewGame() {
        _state = new Cell[width, height];
        GenerateCells();
        GenerateMines();
        GenerateNumbers();
        SetCamera();
        Board.Draw(_state);
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
        for (int i = 0; i < mineCount; i++) {
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
                // TODO : 51:14
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
                if (_state[x, y].type == Cell.Type.Mine) {
                    count++;
                }
            }
        }
        return count;
    }


}
