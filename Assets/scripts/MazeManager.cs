using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeManager : MonoBehaviour
{

    [SerializeField]
    [Range(50, 400)] 
    private int maxLength = 100;

    [SerializeField]
    [Range(20, 200)] 
    private int minLength = 20;

    [SerializeField]
    [Range(10, 40)]
    private int maxHallwayLength = 20;

    [SerializeField]
    [Range(3, 10)]
    private int minHallwayLength = 5;

    [SerializeField]
    [Range(5, 25)]
    private int maxDeadEnd = 5;

    [SerializeField]
    [Range(0, 5)]
    private int minDeadEnd = 0;

    [SerializeField]
    [Range(20, 40)]
    private int maxDeadEndLength = 20;

    [SerializeField]
    [Range(3, 10)]
    private int minDeadEndLength = 10;
    [SerializeField] 
    private Vector3 start;

    [SerializeField]
    private GameObject floorTile;
    
    [SerializeField] 
    BoxCollider mazeStart;
    
    [SerializeField] 
    BoxCollider mazeEnd;
    
    private List<GameObject> floorTiles = new List<GameObject>();

    private int mazeLength;
    private int deadEnds;
    private float tileWidth;

    private int currentHallwayLength;
    List<List<GameObject>> hallways = new List<List<GameObject>>();

    void Start()
    {
        this.determineMazeProperties();
        this.GenerateMaze();
    }
    private void GenerateMaze()
    {
        this.generateHappyPath();
        this.placeMazeCollider(mazeStart, floorTiles[0]);
        this.placeMazeCollider(mazeEnd, floorTiles[floorTiles.Count - 1]);
    }

    private void placeMazeCollider(BoxCollider collider, GameObject targetTile)
    {
        collider.GetComponent<Transform>().position = targetTile.GetComponent<Transform>().position;
        Vector3 newPos = collider.GetComponent<Transform>().position;
        newPos.y = collider.GetComponent<Transform>().lossyScale.y / 2;
        newPos.y += targetTile.GetComponent<Transform>().lossyScale.y / 2;
        collider.GetComponent<Transform>().position = newPos;
    }

    private void generateHappyPath()
    {
        instantiateHappPathTiles();
        splitHappyPathTilesIntoHallways();
        positionHappyPath(hallways);
    }

    private void positionHappyPath(List<List<GameObject>> hallways)
    {
        for (int i = 0; i < hallways.Count; i++)
        {
            GameObject hallwayContainer = new GameObject("Hallway " + i);
            Instantiate(hallwayContainer, this.transform);
            Vector3 previousPosition = new Vector3();
            DIRECTION hallwayDireciton = getDirection(previousPosition);
            for (int j = 0; j < hallways[i].Count; j++)
            {
                GameObject currentTile = hallways[i][j];
                currentTile.GetComponent<Transform>().SetParent(hallwayContainer.GetComponent<Transform>());

                bool isFirstTile = i == 0 && j == 0;
                Vector3 tilePosition = new Vector3();
                if (isFirstTile)
                {
                    tilePosition = start;
                }
                else
                {
                    previousPosition = getPreviousPositionForCurrentTIle(hallways, i, j);
                    tilePosition = this.getNextFloorTilePosition(previousPosition, hallwayDireciton);
                    // check if second last tile position of previous hallway matches tile position
                    if (i > 0 && hallways[i - 1][hallways[i - 1].Count - 2].transform.position == tilePosition)
                    {
                        hallwayDireciton = getDifferentDireciton(hallwayDireciton);
                        tilePosition = this.getNextFloorTilePosition(previousPosition, hallwayDireciton);
                    }
                }
                currentTile.GetComponent<Transform>().position = tilePosition;
            }
        }
    }

    private DIRECTION getDifferentDireciton(DIRECTION hallwayDireciton)
    {
        switch (hallwayDireciton)
        {
            case DIRECTION.FRONT:
                return Random.Range(0, 2) == 0 ? DIRECTION.RIGHT : DIRECTION.LEFT;
            case DIRECTION.RIGHT:
                return Random.Range(0, 2) == 0 ? DIRECTION.LEFT : DIRECTION.FRONT;
            case DIRECTION.LEFT:
                return Random.Range(0, 2) == 0 ? DIRECTION.RIGHT : DIRECTION.FRONT;
            default:
                return DIRECTION.RIGHT;
        }
    }

    private Vector3 getPreviousPositionForCurrentTIle(List<List<GameObject>> hallways, int hallwayIndex, int tileIndex)
    {
        if (tileIndex != 0)
        {
            return hallways[hallwayIndex][tileIndex - 1].GetComponent<Transform>().position;
        }
        else
        {
            List<GameObject> previousHallway = hallways[hallwayIndex - 1];
            GameObject lastTile = previousHallway[previousHallway.Count - 1];
            return lastTile.GetComponent<Transform>().position;
        }
    }

    private void splitHappyPathTilesIntoHallways()
    {
        bool hallwaysLeft = true;
        int hallWayStart = 0;
        int hallwayEndCount;
        while (hallwaysLeft)
        {
            hallwayEndCount = Random.Range(minHallwayLength, maxHallwayLength);
            int targetIndex = hallWayStart + hallwayEndCount;

            if (hallWayStart >= floorTiles.Count)
            {
                Debug.LogWarning("what the hell");
                break;
            }
            if (hallWayStart < floorTiles.Count - 1 && targetIndex < floorTiles.Count - 1)
            {
                List<GameObject> nextHallway = floorTiles.GetRange(hallWayStart, hallwayEndCount);
                hallways.Add(nextHallway);
                hallWayStart = targetIndex + 1;
            }
            else
            {
                hallways.Add(floorTiles.GetRange(hallWayStart, floorTiles.Count - hallWayStart));
                break;
            }
        }
    }

    private void instantiateHappPathTiles()
    {
        floorTiles.Add(floorTile);
        Vector3 instantiationPos = new Vector3(this.start.x, this.start.y, this.start.z);
        instantiationPos.x -= tileWidth;
        instantiationPos.y -= tileWidth;
        instantiationPos.z -= tileWidth;
        for (int i = 0; i < mazeLength - 1; i++)
        {
            floorTiles.Add(Instantiate(floorTile, instantiationPos, Quaternion.identity));
        }
    }

    private DIRECTION getDirection(Vector3 previousPosition)
    {
        switch (Random.Range(0, 3))
        {
            case 0:
                return noTilePlacedHere(previousPosition) ? DIRECTION.FRONT : getDirection(previousPosition);
            case 1:
                return noTilePlacedHere(previousPosition) ? DIRECTION.RIGHT : getDirection(previousPosition);
            case 2:
                return noTilePlacedHere(previousPosition) ? DIRECTION.LEFT : getDirection(previousPosition);
            default:
                return DIRECTION.FRONT;
        }
    }

    private Vector3 getNextFloorTilePosition(Vector3 previousPosition, DIRECTION direction)
    {
        Vector3 nextPosition = new Vector3();
        switch (direction)
        {
            case DIRECTION.FRONT:
                nextPosition = positionToFront(previousPosition);
                break;
            case DIRECTION.RIGHT:
                nextPosition = positionToRight(previousPosition);
                break;
            case DIRECTION.LEFT:
                nextPosition = positionToLeft(previousPosition);
                break;
        }
        return nextPosition;
    }

    bool noTilePlacedHere(Vector3 position)
    {
        for (int i = 0; i < floorTiles.Count; i++)
        {
            if (floorTiles[i].GetComponent<Transform>().position == position)
            {
                return false;
            }
        }
        return true;
    }

    private Vector3 positionToFront(Vector3 previousPosition)
    {
        Vector3 nextPosition = previousPosition;
        nextPosition.z += this.tileWidth;
        return nextPosition;
    }

    private Vector3 positionToRight(Vector3 previousPosition)
    {
        Vector3 nextPosition = previousPosition;
        nextPosition.x += this.tileWidth;
        return nextPosition;
    }

    private Vector3 positionToLeft(Vector3 previousPosition)
    {
        Vector3 nextPosition = previousPosition;
        nextPosition.x -= this.tileWidth;
        return nextPosition;
    }

    private void determineMazeProperties()
    {
        mazeLength = Random.Range(minLength, maxLength);
        deadEnds = Random.Range(minDeadEnd, maxDeadEnd);
        tileWidth = this.floorTile.GetComponent<Transform>().lossyScale.x;
    }
}

enum DIRECTION
{
    FRONT,
    RIGHT,
    LEFT
}
