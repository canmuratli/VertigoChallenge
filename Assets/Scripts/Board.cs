using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;



public class Board : MonoBehaviour
{
    public int width;
    public int height;

    public int borderSize;

    public float swapSpeed = 0.5f;

    public GameObject tilePrefab;
    public Color[] gamePieceColors;

    public GameObject gamePiecePrefab;

    public Tile[,] m_allTiles;
    GamePiece[,] m_allGamePieces;

    Tile m_clickedTile;
    Tile m_targetTile;
    Tile m_targetTile_2;

    public Text tScore;
    public int clickCount = 0;
    public int Score=0;
    public int bombMakerValue = 1000;
    public int bombMakerValueTinker = 1;
    public bool m_playerInputEnabled = true;


    public int moveCount = 0;

    public int normalizedMoveCount;

    // Start is called before the first frame update
    void Start()
    {
        m_allTiles = new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];
        SetupTiles();
        SetupCamera();
        FillBoard(10, 0.5f);
        Score = 0;
       


    }
    private void Update()
    {
        tScore.text = "Score: " + Score;
        normalizedMoveCount = moveCount / 90;

    }




    void SetupTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (i % 2 == 1)
                {
                    GameObject tile = (GameObject)Instantiate(tilePrefab, new Vector3(i * 0.78f, (j + 0.45f) * 0.9f, 0), Quaternion.identity, transform);

                    tile.name = "Tile (" + i + "," + j + ")";

                    m_allTiles[i, j] = tile.GetComponent<Tile>();
                    m_allTiles[i, j].Init(i, j, this);
                }
                else
                {
                    GameObject tile = (GameObject)Instantiate(tilePrefab, new Vector3(i * 0.78f, j * 0.9f, 0), Quaternion.identity, transform);

                    tile.name = "Tile (" + i + "," + j + ")";

                    m_allTiles[i, j] = tile.GetComponent<Tile>();
                    m_allTiles[i, j].Init(i, j, this);
                }


            }
        }
    }
    void SetupCamera()
    {
        Camera.main.transform.position = new Vector3((float)(width - 1) / 2f, (float)(height - 1) / 2f, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;

        float verticalSize = (float)height / 2f + (float)borderSize;

        float horizontalSize = ((float)width / 2f + (float)borderSize) / aspectRatio;

        //Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;

        if (verticalSize > horizontalSize)
        {
            Camera.main.orthographicSize = verticalSize;
        }
        else
        {
            Camera.main.orthographicSize = horizontalSize;
        }

    }
    Color GetRandomGamePieceColor()
    {
        int randomId = Random.Range(0, gamePieceColors.Length);

        if (gamePieceColors[randomId] == null)
        {
            Debug.LogWarning("Board: " + randomId + "does not contain a valid Game Piece prefab....");

        }

        return gamePieceColors[randomId];

    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("Board: Invalid Game Piece...");
            return;
        }
        gamePiece.transform.position = m_allTiles[x, y].transform.position;
        gamePiece.transform.rotation = Quaternion.identity;
        if (IsWithinBounds(x, y))
        {
            m_allGamePieces[x, y] = gamePiece;
        }
        gamePiece.SetCoord(x, y);
    }

    bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }


    GamePiece FillRandomAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        GameObject randomPiece = (GameObject)Instantiate(gamePiecePrefab, Vector3.zero, Quaternion.identity);

        randomPiece.GetComponent<SpriteRenderer>().color = GetRandomGamePieceColor();

        if (randomPiece != null)
        {

                randomPiece.GetComponent<GamePiece>().Init(this);
                PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), x, y);


                if (falseYOffset != 0)
                {
                    randomPiece.transform.position = new Vector3(m_allTiles[x, y].transform.position.x, m_allTiles[x, y].transform.position.y + falseYOffset, 0);
                    randomPiece.GetComponent<GamePiece>().Move(x, y, moveTime);
                }


            if(Score > 0 && Score/(bombMakerValue*bombMakerValueTinker)>=1)
            {
                randomPiece.GetComponent<GamePiece>().isBomb = true;
                randomPiece.GetComponent<GamePiece>().Timer = 8;
                bombMakerValueTinker++;
                moveCount = 0;
            }
            randomPiece.transform.parent = transform;
            return randomPiece.GetComponent<GamePiece>();
        }
        return null;
    }
    void FillBoard(int falseYoffset = 0, float moveTime = 0.1f)
    {
        for (int i = 0; i < width; i++)
        {
            int maxIterations = 100;
            int iterations = 0;
            for (int j = 0; j < height; j++)
            {
                if (m_allGamePieces[i, j] == null)
                {
                    GamePiece piece = FillRandomAt(i, j, falseYoffset, moveTime);
                    iterations = 0;

                    while (HasMatchOnFill(i, j))
                    {
                        ClearPieceAt(i, j);
                        piece = FillRandomAt(i, j, falseYoffset, moveTime);
                        iterations++;
                        if (iterations >= maxIterations)
                        {

                            break;
                        }

                    }
                }
            }
        }
    }
    bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        List<GamePiece> allMatches = FindMatches(x, y, minLength);


        if (allMatches == null)
        {
            allMatches = new List<GamePiece>();
        }
        
        return (allMatches.Count > 0);


    }


    public void ClikTile(Tile tile)
    {
        int x = tile.xIndex;
        int y = tile.yIndex;
        Tile neighborTileA = null;
        Tile neighborTileB = null;
        Tile neighborTileC = null;
        Tile neighborTileD = null;
        Tile neighborTileE = null;
        Tile neighborTileF = null;

        if (x % 2 == 0)
        {
            if (IsWithinBounds(x - 1, y - 1))
            {
                neighborTileA = m_allTiles[x - 1, y - 1];
            }
            if (IsWithinBounds(x - 1, y))
            {
                neighborTileB = m_allTiles[x - 1, y];
            }
            if (IsWithinBounds(x, y + 1))
            {
                neighborTileC = m_allTiles[x, y + 1];
            }
            if (IsWithinBounds(x + 1, y))
            {
                neighborTileD = m_allTiles[x + 1, y];
            }
            if (IsWithinBounds(x + 1, y - 1))
            {
                neighborTileE = m_allTiles[x + 1, y - 1];
            }
            if (IsWithinBounds(x, y - 1))
            {
                neighborTileF = m_allTiles[x, y - 1];
            }
        }
        else
        {
            if (IsWithinBounds(x - 1, y))
            {
                neighborTileA = m_allTiles[x - 1, y];
            }
            if (IsWithinBounds(x - 1, y + 1))
            {
                neighborTileB = m_allTiles[x - 1, y + 1];
            }
            if (IsWithinBounds(x, y + 1))
            {
                neighborTileC = m_allTiles[x, y + 1];
            }
            if (IsWithinBounds(x + 1, y + 1))
            {
                neighborTileD = m_allTiles[x + 1, y + 1];
            }
            if (IsWithinBounds(x + 1, y))
            {
                neighborTileE = m_allTiles[x + 1, y];
            }
            if (IsWithinBounds(x, y - 1))
            {
                neighborTileF = m_allTiles[x, y - 1];
            }
        }
        if (m_clickedTile == null)
        {
            m_clickedTile = tile;
            m_clickedTile.color = new Color(m_clickedTile.color.r, m_clickedTile.color.g, m_clickedTile.color.b, 1);
            if(neighborTileA != null && neighborTileB != null)
            {
                m_targetTile = neighborTileA;
                m_targetTile_2 = neighborTileB;

                m_targetTile.color = new Color(m_targetTile.color.r, m_targetTile.color.g, m_targetTile.color.b, 1);
                m_targetTile_2.color = new Color(m_targetTile_2.color.r, m_targetTile_2.color.g, m_targetTile_2.color.b, 1);
                clickCount = 1;
            }
            else if(neighborTileB != null && neighborTileC != null)
            {
                m_targetTile = neighborTileB;
                m_targetTile_2 = neighborTileC;

                m_targetTile.color = new Color(m_targetTile.color.r, m_targetTile.color.g, m_targetTile.color.b, 1);
                m_targetTile_2.color = new Color(m_targetTile_2.color.r, m_targetTile_2.color.g, m_targetTile_2.color.b, 1);
                clickCount = 2;
            }
            else if(neighborTileC != null && neighborTileD != null)
            {
                m_targetTile = neighborTileC;
                m_targetTile_2 = neighborTileD;

                m_targetTile.color = new Color(m_targetTile.color.r, m_targetTile.color.g, m_targetTile.color.b, 1);
                m_targetTile_2.color = new Color(m_targetTile_2.color.r, m_targetTile_2.color.g, m_targetTile_2.color.b, 1);
                clickCount = 3;
            }
            else if (neighborTileD != null && neighborTileE != null)
            {
                m_targetTile = neighborTileD;
                m_targetTile_2 = neighborTileE;

                m_targetTile.color = new Color(m_targetTile.color.r, m_targetTile.color.g, m_targetTile.color.b, 1);
                m_targetTile_2.color = new Color(m_targetTile_2.color.r, m_targetTile_2.color.g, m_targetTile_2.color.b, 1);
                clickCount = 4;
            }
            else if (neighborTileE != null && neighborTileF != null)
            {
                m_targetTile = neighborTileE;
                m_targetTile_2 = neighborTileF;

                m_targetTile.color = new Color(m_targetTile.color.r, m_targetTile.color.g, m_targetTile.color.b, 1);
                m_targetTile_2.color = new Color(m_targetTile_2.color.r, m_targetTile_2.color.g, m_targetTile_2.color.b, 1);
                clickCount = 5;
            }else if(neighborTileF != null && neighborTileA != null)
            {
                m_targetTile = neighborTileF;
                m_targetTile_2 = neighborTileA;

                m_targetTile.color = new Color(m_targetTile.color.r, m_targetTile.color.g, m_targetTile.color.b, 1);
                m_targetTile_2.color = new Color(m_targetTile_2.color.r, m_targetTile_2.color.g, m_targetTile_2.color.b, 1);
                clickCount = 6;
            }
        }
        else if(m_clickedTile == tile)
        {
            if(m_targetTile != null && m_targetTile_2 != null)
            {
                m_targetTile.color = new Color(m_targetTile.color.r, m_targetTile.color.g, m_targetTile.color.b, 0);
                m_targetTile_2.color = new Color(m_targetTile_2.color.r, m_targetTile_2.color.g, m_targetTile_2.color.b, 0);
            }
            if (neighborTileB != null && neighborTileC != null && clickCount == 1)
            {
                m_targetTile = neighborTileB;
                m_targetTile_2 = neighborTileC;

                m_targetTile.color = new Color(m_targetTile.color.r, m_targetTile.color.g, m_targetTile.color.b, 1);
                m_targetTile_2.color = new Color(m_targetTile_2.color.r, m_targetTile_2.color.g, m_targetTile_2.color.b, 1);


            }
            else if(neighborTileC == null && clickCount == 1)
            {
                clickCount=2;
            }

            if (neighborTileC != null && neighborTileD != null && clickCount == 2)
            {
                m_targetTile = neighborTileC;
                m_targetTile_2 = neighborTileD;

                m_targetTile.color = new Color(m_targetTile.color.r, m_targetTile.color.g, m_targetTile.color.b, 1);
                m_targetTile_2.color = new Color(m_targetTile_2.color.r, m_targetTile_2.color.g, m_targetTile_2.color.b, 1);

            }
            else if (neighborTileD == null && clickCount == 2)
            {
                clickCount=3;
            }

            if (neighborTileD != null && neighborTileE != null && clickCount == 3)
            {
                m_targetTile = neighborTileD;
                m_targetTile_2 = neighborTileE;

                m_targetTile.color = new Color(m_targetTile.color.r, m_targetTile.color.g, m_targetTile.color.b, 1);
                m_targetTile_2.color = new Color(m_targetTile_2.color.r, m_targetTile_2.color.g, m_targetTile_2.color.b, 1);

            }
            else if (neighborTileE == null && clickCount == 3)
            {
                clickCount=4;
            }

            if (neighborTileE != null && neighborTileF != null && clickCount == 4)
            {
                m_targetTile = neighborTileE;
                m_targetTile_2 = neighborTileF;

                m_targetTile.color = new Color(m_targetTile.color.r, m_targetTile.color.g, m_targetTile.color.b, 1);
                m_targetTile_2.color = new Color(m_targetTile_2.color.r, m_targetTile_2.color.g, m_targetTile_2.color.b, 1);

            }
            else if (neighborTileF == null && clickCount == 4)
            {
                clickCount=5;
            }

            if (neighborTileF != null && neighborTileA != null && clickCount == 5)
            {
                m_targetTile = neighborTileF;
                m_targetTile_2 = neighborTileA;

                m_targetTile.color = new Color(m_targetTile.color.r, m_targetTile.color.g, m_targetTile.color.b, 1);
                m_targetTile_2.color = new Color(m_targetTile_2.color.r, m_targetTile_2.color.g, m_targetTile_2.color.b, 1);

            }
            else if (neighborTileA == null && clickCount == 5)
            {
                clickCount=6;
            }
            clickCount++;
            if (clickCount > 6)
            {
                m_targetTile.color = new Color(m_targetTile.color.r, m_targetTile.color.g, m_targetTile.color.b, 0);
                m_targetTile_2.color = new Color(m_targetTile_2.color.r, m_targetTile_2.color.g, m_targetTile_2.color.b, 0);
                m_targetTile = null;
                m_targetTile_2 = null;
                m_clickedTile.color = new Color(m_clickedTile.color.r, m_clickedTile.color.g, m_clickedTile.color.b, 0);
                m_clickedTile = null;
                clickCount = 0;
            }



        }
        else if (m_clickedTile != tile)
        {
            m_targetTile.color = new Color(m_targetTile.color.r, m_targetTile.color.g, m_targetTile.color.b, 0);
            m_targetTile_2.color = new Color(m_targetTile_2.color.r, m_targetTile_2.color.g, m_targetTile_2.color.b, 0);
            m_targetTile = null;
            m_targetTile_2 = null;
            m_clickedTile.color = new Color(m_clickedTile.color.r, m_clickedTile.color.g, m_clickedTile.color.b, 0); 
            m_clickedTile = null;
        }


    }

    public void DragToTile(Tile tile)
    {
        if (m_clickedTile != null && IsNextTo(tile, m_clickedTile))
        {
            m_targetTile = tile;
        }
    }

    public void ReleaseTile(int rotation)
    {
        if (m_clickedTile != null && m_targetTile != null && m_targetTile_2 != null)
        {
            SwitchTiles(m_clickedTile, m_targetTile, m_targetTile_2,rotation);


        }



    }

    void SwitchTiles(Tile clickedTile, Tile targetTile, Tile targetTile_2, int rotation)
    {

        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile, targetTile_2, rotation));


    }
    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile, Tile targetTile_2, int rotation)
    {
        if (m_playerInputEnabled)
        {
            GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
            GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];
            GamePiece targetPiece_2 = m_allGamePieces[targetTile_2.xIndex, targetTile_2.yIndex];

            if (targetPiece != null && clickedPiece != null && targetPiece_2 != null && rotation != 0)
            {
                if(rotation == 1)
                {
                    clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapSpeed);
                    targetPiece.Move(targetTile_2.xIndex, targetTile_2.yIndex, swapSpeed);
                    targetPiece_2.Move(clickedTile.xIndex, clickedTile.yIndex, swapSpeed); 
                     yield return new WaitForSeconds(swapSpeed);

                    List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                    List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);
                    List<GamePiece> targetPiece_2Matches = FindMatchesAt(targetTile_2.xIndex, targetTile_2.yIndex);

                    if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0 && targetPiece_2Matches.Count == 0)
                    {
                        clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapSpeed);
                        targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapSpeed);
                        targetPiece_2.Move(targetTile_2.xIndex, targetTile_2.yIndex, swapSpeed);
                        clickedTile.color = new Color(clickedTile.color.r, clickedTile.color.g, clickedTile.color.b, 0);
                        targetTile.color = new Color(targetTile.color.r, targetTile.color.g, targetTile.color.b, 0);
                        targetTile_2.color = new Color(targetTile_2.color.r, targetTile_2.color.g, targetTile_2.color.b, 0);
                        m_clickedTile = null;
                        m_targetTile = null;
                        m_targetTile_2 = null;
                        clickCount = 0;
                       
                    }
                    else
                    {
                        yield return new WaitForSeconds(swapSpeed);

                        ClearAndRefillBoard((clickedPieceMatches.Union(targetPieceMatches).ToList()).Union(targetPiece_2Matches).ToList());
                        clickedTile.color = new Color(clickedTile.color.r, clickedTile.color.g, clickedTile.color.b, 0);
                        targetTile.color = new Color(targetTile.color.r, targetTile.color.g, targetTile.color.b, 0);
                        targetTile_2.color = new Color(targetTile_2.color.r, targetTile_2.color.g, targetTile_2.color.b, 0);
                        m_clickedTile = null;
                        m_targetTile = null;
                        m_targetTile_2 = null;
                        clickCount = 0;
                        moveCount++;

                    }
                }
                if(rotation == 2)
                {
                    clickedPiece.Move(targetTile_2.xIndex, targetTile_2.yIndex, swapSpeed);
                    targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapSpeed);
                    targetPiece_2.Move(targetTile.xIndex, targetTile.yIndex, swapSpeed);

                    yield return new WaitForSeconds(swapSpeed);

                    List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                    List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);
                    List<GamePiece> targetPiece_2Matches = FindMatchesAt(targetTile_2.xIndex, targetTile_2.yIndex);

                    if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0 && targetPiece_2Matches.Count == 0)
                    {
                        clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapSpeed);
                        targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapSpeed);
                        targetPiece_2.Move(targetTile_2.xIndex, targetTile_2.yIndex, swapSpeed);
                        clickedTile.color = new Color(clickedTile.color.r, clickedTile.color.g, clickedTile.color.b, 0);
                        targetTile.color = new Color(targetTile.color.r, targetTile.color.g, targetTile.color.b, 0);
                        targetTile_2.color = new Color(targetTile_2.color.r, targetTile_2.color.g, targetTile_2.color.b, 0);
                        m_clickedTile = null;
                        m_targetTile = null;
                        m_targetTile_2 = null;
                        clickCount = 0;
                       
                    }
                    else
                    {
                        yield return new WaitForSeconds(swapSpeed);
                         
                        ClearAndRefillBoard((clickedPieceMatches.Union(targetPieceMatches).ToList()).Union(targetPiece_2Matches).ToList());
                        clickedTile.color = new Color(clickedTile.color.r, clickedTile.color.g, clickedTile.color.b, 0);
                        targetTile.color = new Color(targetTile.color.r, targetTile.color.g, targetTile.color.b, 0);
                        targetTile_2.color = new Color(targetTile_2.color.r, targetTile_2.color.g, targetTile_2.color.b, 0);
                        m_clickedTile = null;
                        m_targetTile = null;
                        m_targetTile_2 = null;
                        clickCount = 0;
                        moveCount++;

                    }
                }
            }
        }
    }


    bool IsNextTo(Tile start, Tile end)
    {
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }

        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
        {
            return true;
        }

        return false;
    }

    List<GamePiece> FindMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;
        GamePiece neighborA = null;
        GamePiece neighborB = null;
        GamePiece neighborC = null;
        GamePiece neighborD = null;
        GamePiece neighborE = null;
        GamePiece neighborF = null;
        if (IsWithinBounds(startX, startY))
        {
            startPiece = m_allGamePieces[startX, startY];
        }
        if (startPiece != null)
        {
            matches.Add(startPiece);

        }
        else
        {
            return null;
        }
        if (startX % 2 == 0)
        {
            if (IsWithinBounds(startX - 1, startY - 1))
            {
                neighborA = m_allGamePieces[startX - 1, startY - 1];
            }
            if (IsWithinBounds(startX - 1, startY))
            {
                neighborB = m_allGamePieces[startX - 1, startY];
            }
            if (IsWithinBounds(startX, startY + 1))
            {
                neighborC = m_allGamePieces[startX, startY + 1];
            }
            if (IsWithinBounds(startX + 1, startY))
            {
                neighborD = m_allGamePieces[startX + 1, startY];
            }
            if (IsWithinBounds(startX + 1, startY - 1))
            {
                neighborE = m_allGamePieces[startX + 1, startY - 1];
            }
            if (IsWithinBounds(startX, startY - 1))
            {
                neighborF = m_allGamePieces[startX, startY - 1];
            }
        }
        else
        {
            if (IsWithinBounds(startX - 1, startY))
            {
                neighborA = m_allGamePieces[startX - 1, startY];
            }
            if (IsWithinBounds(startX - 1, startY + 1))
            {
                neighborB = m_allGamePieces[startX - 1, startY + 1];
            }
            if (IsWithinBounds(startX, startY + 1))
            {
                neighborC = m_allGamePieces[startX, startY + 1];
            }
            if (IsWithinBounds(startX + 1, startY + 1))
            {
                neighborD = m_allGamePieces[startX + 1, startY + 1];
            }
            if (IsWithinBounds(startX + 1, startY))
            {
                neighborE = m_allGamePieces[startX + 1, startY];
            }
            if (IsWithinBounds(startX, startY - 1))
            {
                neighborF = m_allGamePieces[startX, startY - 1];
            }
        }



        if (neighborA != null)
        {
            if (neighborF != null && neighborF.color == neighborA.color && neighborA.color == startPiece.color && !matches.Contains(neighborA))
            {
                matches.Add(neighborA);
            }
            if (neighborB != null && neighborB.color == neighborA.color && neighborA.color == startPiece.color && !matches.Contains(neighborA))
            {
                matches.Add(neighborA);
            }
        }
        if (neighborB != null)
        {
            if (neighborA != null && neighborA.color == neighborB.color && neighborB.color == startPiece.color && !matches.Contains(neighborB))
            {
                matches.Add(neighborB);
            }
            if (neighborC != null && neighborC.color == neighborB.color && neighborB.color == startPiece.color && !matches.Contains(neighborB))
            {
                matches.Add(neighborB);
            }
        }
        if (neighborC != null)
        {
            if (neighborB != null && neighborB.color == neighborC.color && neighborC.color == startPiece.color && !matches.Contains(neighborC))
            {
                matches.Add(neighborC);
            }
            if (neighborD != null && neighborD.color == neighborC.color && neighborC.color == startPiece.color && !matches.Contains(neighborC))
            {
                matches.Add(neighborC);
            }
        }
        if (neighborD != null)
        {
            if (neighborC != null && neighborC.color == neighborD.color && neighborD.color == startPiece.color && !matches.Contains(neighborD))
            {
                matches.Add(neighborD);
            }
            if (neighborE != null && neighborE.color == neighborD.color && neighborD.color == startPiece.color && !matches.Contains(neighborD))
            {
                matches.Add(neighborD);
            }
        }
        if (neighborE != null)
        {
            if (neighborD != null && neighborD.color == neighborE.color && neighborE.color == startPiece.color && !matches.Contains(neighborE))
            {
                matches.Add(neighborE);
            }
            if (neighborF != null && neighborF.color == neighborE.color && neighborE.color == startPiece.color && !matches.Contains(neighborE))
            {
                matches.Add(neighborE);
            }
        }
        if (neighborF != null)
        {
            if (neighborE != null && neighborE.color == neighborF.color && neighborF.color == startPiece.color && !matches.Contains(neighborF))
            {
                matches.Add(neighborF);
            }
            if (neighborA != null && neighborA.color == neighborF.color && neighborF.color == startPiece.color && !matches.Contains(neighborF))
            {
                matches.Add(neighborF);
            }
        }


        if (matches.Count >= minLength)
        {
            return matches;
        }

        return null;
    }

    List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
    {
        List<GamePiece> combinedMatches = FindMatches(x, y, minLength);
        if(combinedMatches == null)
        {
            combinedMatches = new List<GamePiece>();
        }
        return combinedMatches;
    }
    List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLegth = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLegth)).ToList();
        }

        return matches;
    }
    List<GamePiece> FindAllMatches()
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                List<GamePiece> matches = FindMatches(i, j,3);
                if(matches == null)
                {
                    matches = new List<GamePiece>();
                }
                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }
        return combinedMatches;
    }


    void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = m_allGamePieces[x, y];

        if (pieceToClear != null)
        {
            m_allGamePieces[x, y] = null;
            Destroy(pieceToClear.gameObject);
            Score += 5;
            //tScore.text = "Score: " + Score;
        }



    }

    void ClearBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                ClearPieceAt(i, j);
            }
        }
    }

    void ClearPieceAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                ClearPieceAt(piece.xIndex, piece.yIndex);
            }

        }
    }

    List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for (int i = 0; i < height - 1; i++)
        {
            if (m_allGamePieces[column, i] == null)
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (m_allGamePieces[column, j] != null)
                    {
                        m_allGamePieces[column, j].Move(column, i, collapseTime * (j - 1));

                        m_allGamePieces[column, i] = m_allGamePieces[column, j];

                        m_allGamePieces[column, i].SetCoord(column, i);

                        if (!movingPieces.Contains(m_allGamePieces[column, i]))
                        {
                            movingPieces.Add(m_allGamePieces[column, i]);
                        }
                        m_allGamePieces[column, j] = null;
                        break;
                    }
                }
            }
        }
        return movingPieces;
    }

    List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> columnsToCollapse = GetColumns(gamePieces);

        foreach (int column in columnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }

        return movingPieces;
    }

    List<int> GetColumns(List<GamePiece> gamePieces)
    {
        List<int> columns = new List<int>();

        foreach (GamePiece piece in gamePieces)
        {

                if (!columns.Contains(piece.xIndex))
                {
                    
                    columns.Add(piece.xIndex);
                    
                }
            

        }
        return columns;
    }

    void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }
    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {

        m_playerInputEnabled = false;
        List<GamePiece> matches = gamePieces;

        do
        {
            //clear and collapse
            yield return (ClearAndCollapseRoutine(matches));
            yield return null;

            //refill
            yield return StartCoroutine(RefillRoutine());

            matches = FindAllMatches();

            yield return new WaitForSeconds(0.5f);
        }
        while (matches.Count != 0);

        m_playerInputEnabled = true;
    }
    IEnumerator RefillRoutine()
    {
        for (int i = 0; i < width; i++)
        {

            for (int j = 0; j < height; j++)
            {
                if (m_allGamePieces[i, j] == null)
                {
                    GamePiece piece = FillRandomAt(i, j, 10, 0.5f);

                }
            }
        }
        //FillBoard(10, 0.5f);
        yield return null;
    }
    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        yield return new WaitForSeconds(0.5f);

        bool isFinished = false;

        while (!isFinished)
        {
            ClearPieceAt(gamePieces);

            yield return new WaitForSeconds(0.25f);
            movingPieces = CollapseColumn(gamePieces);

            while (!IsCollapsed(movingPieces))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);
            matches = FindMatchesAt(movingPieces);

            if (matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }

        yield return null;
    }

    bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                if (piece.transform.position.y - (float)piece.yIndex > 0.001f)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
