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
        int deadEndLength = Random.Range(minDeadEndLength, maxDeadEndLength);
        this.placeFloorTiles();
        this.placeMazeCollider( mazeStart, floorTiles[0]);
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

    private void placeFloorTiles()
    {
        floorTiles.Add(floorTile);
        // generate all floor tiles
        Vector3 instantiationPos = new Vector3(start.x, start.y, start.z);
        instantiationPos.x -= tileWidth;
        instantiationPos.y -= tileWidth;
        instantiationPos.z -= tileWidth;
        for (int i = 0; i < mazeLength-1; i++)
        {
            floorTiles.Add(Instantiate(floorTile, instantiationPos, Quaternion.identity));
        }
        // place first one at start position
        floorTiles[0].GetComponent<Transform>().position = this.start;
       
        // split list into "hallways"
        // place each hallway...
        for (int i = 1; i < floorTiles.Count; i++)
        {
            floorTiles[i].GetComponent<Transform>().SetParent(this.transform);
            floorTiles[i].GetComponent<Transform>().position = this.getNextFloorTilePosition(floorTiles[i-1].GetComponent<Transform>().position);
        }
    }

    private Vector3 getNextFloorTilePosition(Vector3 previousPosition)
    {
        Vector3 nextPosition = new Vector3();
        switch (Random.Range(0, 3))
        {
            case 0:
                nextPosition = positionToFront(previousPosition);
                break;
            case 1:
                nextPosition = positionToRight(previousPosition);
                break;
            case 2:
                nextPosition = positionToLeft(previousPosition);
                break;
        }
        return noTilePlacedHere(nextPosition) ? nextPosition : getNextFloorTilePosition(previousPosition);
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

enum POSITION
{
    FRONT,
    RIGHT,
    LEFT
}
