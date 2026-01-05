using System;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
using System.Reflection.Emit; using System.Collections;
using System.Collections.Generic; using System.Runtime.CompilerServices;
using Unity.VisualScripting; using System.Linq; using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class GenerateMaze : MonoBehaviour 
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] Material matChange;
    [SerializeField] Material matPath;
    [HideInInspector] public int width;
    [HideInInspector] public int length;
    [SerializeField] private float spacing;
    [SerializeField] private Slider widthInput; 
    [SerializeField] private Slider lengthInput;
    [HideInInspector] public Grid currentGrid;
    [SerializeField] private int undoCount;
    List<GameObject> cellsAvailable;
    private List<Vector2Int> allDirections = new List<Vector2Int>()
    {
        new Vector2Int(1, 0), 
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0), 
        new Vector2Int(0, 1),
    }; 
    
    // Contains all the movement directions of the path
    // To prevent the path from walking backwards on itself.
    Stack<Vector2Int> pathForward = new Stack<Vector2Int>();
    
    private int stepsPerFrame = 0;

    // Creates a new grid when the player presses the confirm button on the creation screen
    public void GenerateGrid()
    {
        StopAllCoroutines();
        
        cellsAvailable = new List<GameObject>();
        pathForward.Clear();
        
        width = Convert.ToInt32(widthInput.value);
        length = Convert.ToInt32(lengthInput.value);
        currentGrid = new Grid(width, length);

        // Clear previous grid if needed
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        // Instantiates a Grid of Game Objects relating to the dimensions inputed by the user.
        for (int x = 0; x < width; x++) 
        {
            for (int y = 0; y < length; y++)
            {
                Vector3 position = new Vector3( x * spacing, 0f, y * spacing );
                GameObject t = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                currentGrid.gridMap[x, y] = t;
                cellsAvailable.Add(t);
            }
        }
        
        int gridSize = width * length;
        stepsPerFrame = Mathf.Max(1, gridSize / 10);
        
        SelectTwoRandomTiles(true);
        
        StaticVariables.uiManager.SetSpawnPlayerButton(false);
        StaticVariables.uiManager.SetPointMarkerCanvas(false); 
    }
    
    // Sets the tile where the algorithm starts from.
    // Also sets an end tile if this is the first walk path of the algorithm
    private void SelectTwoRandomTiles(bool isFirst)
    {
        GameObject startTile;
        GameObject endTile;
        
        int startRan = UnityEngine.Random.Range(0, cellsAvailable.Count);
        startTile = cellsAvailable[startRan];
        cellsAvailable.RemoveAt(startRan);
        startTile.GetComponentInChildren<Renderer>().material = matPath;
        
        if (isFirst)
        {
            int endRan = UnityEngine.Random.Range(0, cellsAvailable.Count);
            endTile = cellsAvailable[endRan];
            cellsAvailable.RemoveAt(endRan);
            currentGrid.tileStack.Add(endTile);
            
            endTile.GetComponentInChildren<Renderer>().material = matPath;
        }
        StartCoroutine(WilsonAlgorithm(startTile));
    }

    #region
    private IEnumerator WilsonAlgorithm(GameObject start)
    {
        Stack<GameObject> currentStack = new Stack<GameObject>();
        bool hasEnded = false;
        int xStart, yStart;
        int xCur, yCur;

        FindTileInGrid(start, out xStart, out yStart);
        GameObject currentTile = currentGrid.gridMap[xStart, yStart];
        currentStack.Push(currentTile);

        SetNextTile(xStart, yStart, out xCur, out yCur);
        currentTile = currentGrid.gridMap[xCur, yCur];

        while (!hasEnded)
        {
            for (int step = 0; step < stepsPerFrame && !hasEnded; step++)
            {
                if (!currentGrid.tileStack.Contains(currentTile) || currentGrid.tileStack.Count <= 0)
                {
                    // Rare bug : Could cause gaps in maze
                    // Revisit later.
                    if (!currentStack.Contains(currentTile))
                    {
                        currentStack.Push(currentTile);
                        currentTile.SetActive(false);

                        SetNextTile(xCur, yCur, out xCur, out yCur);
                        currentTile = currentGrid.gridMap[xCur, yCur];
                    }
                    else
                    {
                        #region
                        GameObject check = null;
                        int c = currentStack.Count;
                        for (int i = 0; i < undoCount; i++)
                        {
                            if (currentStack.Count > 0)
                            {
                                check = currentStack.Pop();
                                check.SetActive(true);

                                check.GetComponent<TileObjectScript>().ResetWalls();
                                pathForward.Pop();
                            }
                            else break;
                        }
                        if (check != null)
                        {
                            currentTile = check;
                            FindTileInGrid(currentTile, out xCur, out yCur);
                            if (pathForward.Count > 0)
                                SetUpWalls(xCur, yCur, allDirections.IndexOf(pathForward.Peek()), true);
                        }
                        else currentTile = start;
                        #endregion
                    }
                }
                else
                {
                    BuildPath(currentStack);
                    RemoveFromCellsAvailable(currentStack); hasEnded = true;
                }
            }
            yield return null;
        }
        if (cellsAvailable.Count > 0) SelectTwoRandomTiles(false);
        else
        {
            currentGrid.SetStartAndEndTiles();
            StaticVariables.uiManager.SetSpawnPlayerButton(true);
            StaticVariables.uiManager.SetPositionMarkers(currentGrid.startTile.transform.position, currentGrid.endTile.transform.position);
            StaticVariables.uiManager.SetPointMarkerCanvas(true);
        }
        yield return null;
    }

    #endregion

    // Gets the value of the coordinates of a tile, and then
    // Sets a new tile next to it to continue the walkway
    private void SetNextTile(int xCur, int yCur, out int xNext, out int yNext) 
    {
        xNext = 0; 
        yNext = 0; 

        int ran = 99;
        Vector2Int dir; 
        List<Vector2Int> possibleDirections = new List<Vector2Int>(allDirections); 
        if (pathForward.Count > 0) 
        {
            possibleDirections.Remove(-pathForward.Peek());
        } 
        while (true) 
        {
            ran = UnityEngine.Random.Range(0, possibleDirections.Count);
            dir = possibleDirections[ran];
            
            xNext = dir.x + xCur;
            yNext = dir.y + yCur;
            
            if ((xNext >= 0 && xNext < currentGrid.gridMap.GetLength(0)) && (yNext >= 0 && yNext < currentGrid.gridMap.GetLength(1)))
            {
                pathForward.Push(dir);
                int wall = allDirections.IndexOf(dir);
                SetUpWalls(xCur, yCur, wall, false);
                SetUpWalls(xNext, yNext, wall, true);
                return;
            } 
        }
    }
    
    private void FindTileInGrid(GameObject target, out int x, out int y)
    {
        x = 0; 
        y = 0; 
        
        for (int i = 0; i < currentGrid.width; i++)
        { 
            for (int j = 0; j < currentGrid.length; j++)
            {
                if (target == currentGrid.gridMap[i, j])
                { 
                    x = i; y = j; 
                    return; 
                }
            } 
        } 
    } 
    
    private void SetUpWalls(int x, int y, int dir, bool opposite)
    { 
        TileObjectScript _tos = currentGrid.gridMap[x, y].GetComponent<TileObjectScript>(); 
        if (opposite) { dir += 2; if (dir > 3) dir -= 4; } 
        
        _tos.SetWalls(dir); 
    } 
    
    private void BuildPath(Stack<GameObject> stack) 
    {
        foreach (GameObject obj in stack) 
        { 
            currentGrid.tileStack.Add(obj);
            obj.SetActive(true); 
            obj.GetComponentInChildren<Renderer>().material = matPath;
            obj.GetComponent<TileObjectScript>().EnableWalls(); 
        } 
    } 
    
    private void RemoveFromCellsAvailable(Stack<GameObject> stack) 
    { 
        foreach (GameObject obj in stack) 
        {
            cellsAvailable.Remove(obj); 
        } 
    } 
    
    public void SpawnPlayer()
    {
        Transform spawnPos = FindFirstObjectByType<StartTile>().transform;
        GameObject player = StaticVariables.player; 
        player.transform.position = new Vector3(spawnPos.position.x, (spawnPos.position.y + player.transform.localScale.y) / 1.5f, spawnPos.position.z);
        player.SetActive(true); 
        
        player.GetComponent<ControllablePlayer>().enabled = true; 
    } 
    public class Grid : MonoBehaviour 
    { 
        public int width; 
        public int length; 
        public GameObject[,] gridMap; 
        
        public List<GameObject> tileStack = new List<GameObject>(); 
        public GameObject startTile; 
        public GameObject endTile; 
        
        public Grid(int w, int l) 
        { 
            width = w;
            length = l;
            gridMap = new GameObject[width, length]; 
        } 
        
        public void SetStartAndEndTiles() 
        { 
            startTile = GetRandomTileEdge(); 

            do { endTile = GetRandomTileEdge(); } 
            while (endTile == startTile); 
            
            startTile.AddComponent<StartTile>(); 
            endTile.AddComponent<EndTile>();
        }
        private GameObject GetRandomTileEdge() 
        { 
            int edge = UnityEngine.Random.Range(0, 4); 
            return edge switch 
            { 
                0 => gridMap[0, UnityEngine.Random.Range(0, length)], 
                1 => gridMap[UnityEngine.Random.Range(0, width), 0],
                2 => gridMap[width - 1, UnityEngine.Random.Range(0, length)], 
                3 => gridMap[UnityEngine.Random.Range(0, width), length - 1],
                _ => null 
            };
        } 
    } 
}