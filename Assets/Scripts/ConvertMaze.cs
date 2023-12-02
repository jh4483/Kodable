using System.Collections;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class MazeConverter : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject solutionPrefab;
    public Canvas canvas;
    private float cellSize = 1.0f;
    private float cellSpace = 20f;
    private float mazeWidth;
    private float mazeHeight;
    public string[] lines;
    private float offsetX;
    private float offsetY;
    private int[,] mazeArray;
    private Vector2 start;
    private Vector2 end;
    public GameObject player;
    private float prevXPos;
    private float prevYPos;

    void Start()
    {
        string filePath = "Assets/MazeRepo/Maze.txt";
        lines = File.ReadAllLines(filePath);
        GenerateMaze(lines);


        FindAndDrawStart();
        FindAndDrawEnd();
        StartCoroutine(DrawPathWithDelay(FindPath(), 0.2f));
    }

    public void GenerateMaze(string[] lines)
    {
        mazeWidth = lines[0].Length * (cellSize + cellSpace);
        mazeHeight = lines.Length * (cellSize + cellSpace);

        offsetX = - mazeWidth / 2f;
        offsetY = mazeHeight / 2f;

        mazeArray = new int[lines.Length, lines[0].Length];

        for (int y = 0; y < lines.Length; y++)
        {
            for (int x = 0; x < lines[y].Length; x++)
            {
                char mc = lines[y][x];

                float xPos = x * (cellSize + cellSpace) + offsetX;
                float yPos = -y * (cellSize + cellSpace) + offsetY;

                if (mc == '+' || mc == '-' || mc == '|')
                {
                    DrawMaze(wallPrefab, xPos, yPos);
                    mazeArray[y, x] = 0;
                }
                else if (mc == ' ')
                {
                    if (y < mazeArray.GetLength(0) && x < mazeArray.GetLength(1))
                    {
                        mazeArray[y, x] = 1;
                        if (start == Vector2.zero && x == 0)
                        {
                            start = new Vector2(x, y);
                        }
                        if (x == mazeArray.GetLength(1) - 1)
                        {
                            end = new Vector2(x, y);
                        }
                    }
                }
            }
        }

    }

    public void FindAndDrawStart()
    {
        DrawMaze(solutionPrefab, start.x * (cellSize + cellSpace) + offsetX, -start.y * (cellSize + cellSpace) + offsetY);
    }

    public void FindAndDrawEnd()
    {
        DrawMaze(solutionPrefab, end.x * (cellSize + cellSpace) + offsetX, -end.y * (cellSize + cellSpace) + offsetY);
    }

    public void DrawMaze(GameObject prefab, float xPos, float yPos)
    {
        GameObject obj = Instantiate(prefab, new Vector3(xPos, yPos, 0), Quaternion.identity);
        obj.transform.SetParent(canvas.transform, false);
    }

    private List<Vector2> FindPath()
    {
        List<Vector2> path = new List<Vector2>();
        HashSet<Vector2> visited = new HashSet<Vector2>();
        bool solutionFound = false;

        DepthFirstSearch(start, path, visited, ref solutionFound);

        return path;
    }

    public void DepthFirstSearch(Vector2 current, List<Vector2> path, HashSet<Vector2> visited, ref bool solutionFound)
    {
        if (current == end)
        {
            path.Add(current);
            solutionFound = true;
            return;
        }

        visited.Add(current);

        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        foreach (Vector2 dir in directions)
        {
            Vector2 neighbour = current + dir;
            if (!solutionFound && IsValidMove(neighbour, visited))
            {
                path.Add(current);
                DepthFirstSearch(neighbour, path, visited, ref solutionFound);
                if (solutionFound)
                    return;
            }
        }
        if (path.Count > 0)
        {
            path.RemoveAt(path.Count - 1);
        }
    }

    public bool IsValidMove(Vector2 pos, HashSet<Vector2> visited)
    {
        return pos.x >= 0 && pos.x < mazeArray.GetLength(1) &&
               pos.y >= 0 && pos.y < mazeArray.GetLength(0) &&
               mazeArray[(int)pos.y, (int)pos.x] == 1 &&
               !visited.Contains(pos);
    }

    private IEnumerator DrawPathWithDelay(List<Vector2> path, float delay)
    {
            foreach (Vector2 pos in path)
            {
                float xPos = pos.x * (cellSize + cellSpace) + offsetX;
                float yPos = -pos.y * (cellSize + cellSpace) + offsetY;

                DrawMaze(solutionPrefab, xPos, yPos);
                UpdatePlayerFacingDirection(xPos, yPos);

                player.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);
                yield return new WaitForSeconds(delay);
                prevXPos = xPos;
                prevYPos = yPos;
            }
        }

    private void UpdatePlayerFacingDirection(float currentXPos, float currentYPos)
    {
        Vector2 direction = new Vector2(currentXPos - prevXPos, currentYPos - prevYPos).normalized;

        if (direction.x < 0)
        {
            player.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (direction.x > 0)
        {
            player.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        if (direction.y < 0)
        {
            player.transform.rotation = Quaternion.Euler(0, 0, -90);
        }
        else if (direction.y > 0)
        {
            player.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
    }

}

