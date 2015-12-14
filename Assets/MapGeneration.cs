using UnityEngine;
using System.Collections;

public class MapGeneration : Photon.MonoBehaviour
{
  private bool isFirst;
  private int mapHeight = 64;
  private int mapWidth = 64;
  private Point mapSize;
  private int seed;

  /*
  Load states
 -1 not started
  0 generating map
  5 done
  */
  private int loadState = -1;
  /*
    Map objects
    info for a given tile
    0: background
   -1: floor
    1: basic wall
    2: alternate wall (for debugging?)
    3: door
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
    public static Point operator + (Point point1, Point point2)
    {
      return new Point(point1.x + point2.x, point1.y + point2.y);
    }
    public int x;
    public int y;
  };

  struct Room
  {
  };

  MapObject[,] mapArray;

  void Start()
  {
    TextMesh loadingText = GameObject.Find("GameManager").GetComponent<TextMesh>();
    isFirst = PhotonNetwork.isMasterClient;
    seed = (int)PhotonNetwork.room.customProperties["seed"];
    mapSize = Vec2(mapWidth, mapHeight);

    Debug.Log("generating map with seed " +  seed + (isFirst ? " as master" : " as client"));

    loadingText.text = "generating map";
    GenerateMapArray();
    SpawnMap();

    loadingText.text = "load complete";
    loadState = 5; //done loading

    //SpawnObjectAtPosition(new Point(0, 0), Resources.Load("GameLevel/BasicTile"));
  }

  //map gen:
  void GenerateMapArray() //creates map array
  {
    loadState = 0;
    mapArray = new MapObject[mapHeight, mapWidth];
    Random.seed = seed;

    //make starting 5x5 room:
    FillBorders(Vec2(mapHeight/2 - 3, mapWidth/2 - 3), Vec2(7, 7), Tile(1, -1, 1), false, true);

    //make a bunch of other rooms:
    int i = 0; //index
    int j = 0; //index
    while (i++ < 64)
    {
      FillRandomRoom(Vec2(5, 5), Vec2(16, 16), i);
    }


    

    //fill in room edges
    for (i = 1; i < mapWidth - 1; i++) //looping through every item
    {
      for (j = 1; j < mapHeight - 1; j++)
      {
        //actual stuff goes here
        if (mapArray[i,j].type == 0 && CheckAdjacent(Vec2(i, j), -1))
        {
          mapArray[i, j] = Tile(1, 1, 1);
        }
        //actual stuff stops here
      }
    }

    CrissCrossHalls();

    //fill in map edge
    FillBorders(Vec2(0, 0), mapSize, Tile(1, 2, 1), false, false);
  }

  void SpawnMap() //actually instantiates objects according to array
  {
    //index vars:
    int i, j;

    Point startingPoint = Vec2(-mapWidth / 2, -mapHeight / 2);

    for (i = 0; i < mapWidth; i++) //looping through every item
    {
      for (j = 0; j < mapHeight; j++)
      {
        //actual stuff goes here
        switch(mapArray[i, j].type)
        {
          case 1:
            SpawnObjectAtPosition(Vec2(i + startingPoint.x, j + startingPoint.y), Resources.Load("GameLevel/BasicTile"), 0);
            break;
          case 2:
          case 0: //debug fallthrough
            SpawnObjectAtPosition(Vec2(i + startingPoint.x, j + startingPoint.y), Resources.Load("GameLevel/BasicTile2"), 0);
            break;
          case -1:
            SpawnObjectAtPosition(Vec2(i + startingPoint.x, j + startingPoint.y), Resources.Load("GameLevel/BasicTile2"), 2);
            break;
            /*
            case 0:
              Debug.Log("null test");
              break;*/
        }
        //actual stuff stops here
      }
    }
  }

  
  void SpawnObjectAtPosition(Point location, Object thing, float height)
  {
    //note that location is doubled in magnitude because tiles are 2x2 and not 1x1
    Vector3 worldPosition = new Vector3(location.x * 2, location.y * 2, height);
    Instantiate(thing, worldPosition, Quaternion.identity);
  }

  //map gen tool funcs:

  void CrissCrossHalls()
  {
    //make a bunch of stupid fucking lines
    int i = 1;
    while (i < mapWidth - 2)
    {
      int random = Random.Range(0, 5);
      if (random == 0)
      {
        ReplaceArea(Vec2(i - 1, 0), Vec2(3, mapHeight), Tile(1, 0, 0), 0);
        ReplaceArea(Vec2(i, 0), Vec2(1, mapHeight), Tile(-1, 0, 0), 1);
      }
      else if (random == 6)
      {
        if (i > 2 && i < mapWidth - 6)
        {
          ReplaceArea(Vec2(i - 2, 0), Vec2(5, mapHeight), Tile(1, 0, 0), 0);
          ReplaceArea(Vec2(i - 1, 0), Vec2(3, mapHeight), Tile(-1, 0, 0), 1);
          i += 6;
        }
        else
        {
          break;
        }
      }
      i += 2;
    }

    i = 1;
    while (i < mapHeight - 2)
    {
      int random = Random.Range(0, 5);
      if (random == 0)
      {
        ReplaceArea(Vec2(0, i - 1), Vec2(mapWidth, 3), Tile(1, 0, 0), 0);
        ReplaceArea(Vec2(0, i), Vec2(mapWidth, 1), Tile(-1, 0, 0), 1);
      }
      else if (random == 1)
      {
        if (i > 2 && i < mapHeight - 6)
        {
          ReplaceArea(Vec2(0, i - 2), Vec2(mapWidth, 5), Tile(1, 0, 0), 0);
          ReplaceArea(Vec2(0, i - 1), Vec2(mapWidth, 3), Tile(-1, 0, 0), 1);
          i += 6;
        }
        else
        {
          break;
        }
      }
      i += 2;
    }
  }
  void FillRandomRoom(Point sizeMin, Point sizeMax, int tileMeta) //tilemeta sets subtype of tile object
  {
    Point size = Vec2(//random room size within boundaries
      Random.Range(sizeMin.x, sizeMax.x), 
      Random.Range(sizeMin.y, sizeMax.y)); 

    Point position = Vec2(//random position within boundaries, size subtracted to avoid going out of bounds
      Random.Range(1, mapWidth - size.x),
      Random.Range(1, mapHeight - size.y));

    if (!CheckAreaFor(position, size, -1)) //if nothing's there
    {
      FillBorders(position, size, Tile(1, tileMeta, 1), false, true);
    }
  }

  /*
  FillArea
    Fills given rectangular area with given map object
  */
  void FillArea(Point position, Point dimensions, MapObject newObject, bool ignoreDoors)
  {
    int height = dimensions.y ;
    int width = dimensions.x;
    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        if (ignoreDoors || mapArray[position.x + i, position.y + j].type != 3)
        {
          mapArray[position.x + i, position.y + j] = new MapObject(newObject.type, newObject.subtype, newObject.height);
        }
        //Debug.Log(mapArray[position.x + width, position.y + height].type + "a" + (position.x + width) + "b" + (position.y + height));
      }
    }
    //huh, I guess I don't have to free() stuff in c#
  }

  /*
  ReplaceArea
    Replaces tiles in given rectangular area with given map object
  */
  void ReplaceArea(Point position, Point dimensions, MapObject newObject, int oldType)
  {
    int height = dimensions.y;
    int width = dimensions.x;
    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        if (mapArray[position.x + i, position.y + j].type == oldType)
        {
          mapArray[position.x + i, position.y + j] = new MapObject(newObject.type, newObject.subtype, newObject.height);
        }
        //Debug.Log(mapArray[position.x + width, position.y + height].type + "a" + (position.x + width) + "b" + (position.y + height));
      }
    }
    //huh, I guess I don't have to free() stuff in c#
  }

  /*
  CheckAreaFor
    returns whether or not a given tile type is in an area
  */
  bool CheckAreaFor(Point position, Point dimensions, int type)
  {
    int height = dimensions.y;
    int width = dimensions.x;
    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        if (mapArray[position.x + i, position.y + j].type == type)
        {
          return true;
        }
      }
    }
    return false;  
  }

  /*
  CheckAdjacent
    returns whether or not a given tile type is in a 3x3 area
  */
  bool CheckAdjacent(Point position, int type)
  {
    int height = 3;
    int width = 3;
    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        if (mapArray[position.x + i - 1, position.y + j - 1].type == type)
        {
          return true;
        }
      }
    }
    return false;
  }

  /*
  FillBorders
    Replaces the borders of a given rectangular area with given map object
    More efficient than filling an area and then filling the inside with empty space
    Ignore doors specifies whether or not tile type 3 should be ignored
  */
  void FillBorders(Point position, Point dimensions, MapObject newObject, bool ignoreDoors, bool createFloor)
  {
    int height = dimensions.y;
    int width = dimensions.x;
    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        if (ignoreDoors || mapArray[position.x + i, position.y + j].type != 3)
        {
          if ((i == 0 || i == width - 1 || j == 0 || j == height - 1))
          {
            mapArray[position.x + i, position.y + j] = Tile(newObject.type, newObject.subtype, newObject.height);
          } else {
            if (createFloor)
            {
              mapArray[position.x + i, position.y + j] = Tile(-1, 0, 0); //floor is -1
            }
          }
        }
      }
    }
    //huh, I guess I don't have to free() stuff in c#
  }


  /*misc stuff/shortcuts*/

  Point Vec2(int x, int y) //shortcut for "new Point()"
  {
    return new Point(x, y);
  }

  MapObject Tile(int _type, int _subtype, int _height) //shortcut for "new MapObject()"
  {
    return new MapObject(_type, _subtype, _height);
  }
}
