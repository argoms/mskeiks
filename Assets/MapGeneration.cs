using UnityEngine;
using System.Collections;

public class MapGeneration : Photon.MonoBehaviour
{
  private bool isFirst;
  private int mapHeight = 32;
  private int mapWidth = 32;
  private Point mapSize;
  private int seed;
  /*
    Map objects
    info for a given tile
    0: floor
    -1: background
    1: basic wall
  */
  struct MapObject
  {
    public MapObject(int _type, int _subtype, int _height)
    {
      type = _type;
      subtype = _subtype;
      height = _height;
    }
    public int type;
    public int subtype;
    public int height;
  };

  struct Point
  {
    public Point(int _x, int _y)
    {
      x = _x;
      y = _y;
    }
    public int x;
    public int y;
  }

  MapObject[,] mapArray;

  void Start()
  {
    isFirst = PhotonNetwork.isMasterClient;
    seed = (int)PhotonNetwork.room.customProperties["seed"];
    mapSize = Vec2(mapWidth, mapHeight);

    Debug.Log("generating map with seed " +  seed + (isFirst ? " as master" : " as client"));

    GenerateMapArray();
    SpawnMap();

    //SpawnObjectAtPosition(new Point(0, 0), Resources.Load("GameLevel/BasicTile"));
  }

  //map gen:
  void GenerateMapArray() //creates map array
  {
    mapArray = new MapObject[mapHeight, mapWidth];
    Random.seed = seed;

    //make starting 5x5 room:
    FillArea(Vec2(mapHeight/2 - 3, mapWidth/2 - 3), Vec2(7, 7), new MapObject(1, 1, 1));
    FillArea(Vec2(mapHeight/2 - 2, mapWidth/2 - 2), Vec2(5, 5), new MapObject(0, 1, 1));
  }

  void SpawnMap() //actually instantiates objects according to array
  {
    //index vars:
    int i, j;

    Point startingPoint = new Point(-mapWidth / 2, -mapHeight / 2);

    for (i = 0; i < mapWidth; i++) //looping through every item
    {
      for (j = 0; j < mapHeight; j++)
      {
        //actual stuff goes here
        switch(mapArray[i, j].type)
        {
          case 1:
            SpawnObjectAtPosition(new Point(i + startingPoint.x, j + startingPoint.y), Resources.Load("GameLevel/BasicTile"));
            break;
          case 2:
            SpawnObjectAtPosition(new Point(i + startingPoint.x, j + startingPoint.y), Resources.Load("GameLevel/BasicTile2"));
            break;
        }
        //
      }
    }
  }

  void SpawnObjectAtPosition(Point location, Object thing)
  {
    Vector3 worldPosition = new Vector3(location.x * 2, location.y * 2, 0);
    Instantiate(thing, worldPosition, Quaternion.identity);
  }

  //map gen tool funcs:

  /*
  FillArea
    Fills given rectangular area with given map object
  */
  void FillArea(Point position, Point dimensions, MapObject newObject)
  {
    int height = dimensions.y ;
    int width = dimensions.x;
    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        mapArray[position.x + i, position.y + j] = new MapObject(newObject.type, newObject.subtype, newObject.height);
        //Debug.Log(mapArray[position.x + width, position.y + height].type + "a" + (position.x + width) + "b" + (position.y + height));
      }
    }
    //huh, I guess I don't have to free() stuff in c#
  }



  /*
	// Update is called once per frame
	void Update ()
  {
	
	}*/

  /*misc stuff*/
  
  Point Vec2(int x, int y) //shortcut for "new Point()"
  {
    return new Point(x, y);
  }
}
