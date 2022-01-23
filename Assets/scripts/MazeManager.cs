using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeManager : MonoBehaviour
{

    [SerializeField] private int 
        maxLength = 100, minLength = 10, 
        maxDeadEnd = 5, minDeadEnd = 0, 
        maxDeadEndLength = 20, minDeadEndLength = 10;
    [SerializeField] private Vector3 start;
    [SerializeField]
    private GameObject floorTile;
    [SerializeField] BoxCollider mazeStart;
    [SerializeField] BoxCollider mazeEnd;
    private List<GameObject> floorTiles = new List<GameObject>();

    private int mazeLength;
    private int deadEnds;
    private float tileWidth;

    private int minHallwayLength, maxHallwayLength, currentHallwayLength;
    // TODO looping paths/returning paths

    void Start()
    {
        this.determineMazeProperties();
        this.GenerateMaze();
    }
    private void GenerateMaze()
    {
        this.generateHappyPath();
        this.placeMazeCollider(mazeStart, floorTiles[0]);
        this.placeMazeCollider(mazeEnd, floorTiles[floorTiles.Count-1]);
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
        List<List<GameObject>> hallways = splitHappyPathTilesIntoHallways();
        positionHappyPath(hallways);
    }

    private void positionHappyPath(List<List<GameObject>> hallways)
    {
        for (int i = 0; i < hallways.Count; i++)
        {
            GameObject hallwayContainer = new GameObject("Hallway " + i);
            Instantiate(hallwayContainer, this.transform);
            Vector3 previousPosition = start;
            // loop over each hallway
            DIRECTION hallwayDireciton = getDirection(previousPosition);
            for (int j = 0; j < hallways[i].Count; j++)
            {
                hallways[i][j].GetComponent<Transform>().SetParent(hallwayContainer.GetComponent<Transform>());
                hallways[i][j].GetComponent<Transform>().SetParent(this.transform);
                // if at the first element of a hallway, the previous position is the position of the last element
                // of the previous hallway
                // otherwise proceed as normal
                if (i == 0 && j == 0)
                {
                    // first hallway
                    previousPosition = start;
                }
                else if (j != 0)
                {
                    // any hallway, not first element
                    // previous position is current hallway, previous element position
                    previousPosition = hallways[i][j - 1].GetComponent<Transform>().position;
                }
                else if (i != 0 && j == 0)
                {
                    // any subsequent hallway, where j is first element
                    // position will be position of last element of previous hallway
                    previousPosition = hallways[i - 1][hallways[i - 1].Count - 1].GetComponent<Transform>().position;
                }
                hallways[i][j].GetComponent<Transform>().position = this.getNextFloorTilePosition(previousPosition, hallwayDireciton);
            }
        }
    }

    private List<List<GameObject>> splitHappyPathTilesIntoHallways()
    {
        List<List<GameObject>> hallways = new List<List<GameObject>>();
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

        return hallways;
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
        }
        return DIRECTION.FRONT;
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
        //return noTilePlacedHere(nextPosition) ? nextPosition : getNextFloorTilePosition(previousPosition, direction);
        return nextPosition;
    }

    bool noTilePlacedHere(Vector3 position)
    {
        for (int i = 0; i < floorTiles.Count; i++)
        {
            if(floorTiles[i].GetComponent<Transform>().position == position)
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
        minHallwayLength = Random.Range(3, 6);
        maxHallwayLength = Random.Range(6, 12);
    }
}

enum DIRECTION
{
    FRONT,
    RIGHT,
    LEFT
}
