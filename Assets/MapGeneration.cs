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
  10 done
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

    public static Point operator +(Point point1, Point point2) //point addition
    {
      return new Point(point1.x + point2.x, point1.y + point2.y);
    }

    public static Point operator -(Point point1, Point point2) //point subtraction
    {
      return new Point(point1.x - point2.x, point1.y - point2.y);
    }

    public static Point operator *(Point input, int scalar) //scalar multiplication
    {
      return new Point(input.x * scalar, input.y * scalar);
    }

    public static Point operator /(Point input, int scalar) //scalar division
    {
      return new Point(input.x / scalar, input.y / scalar);
    }
    public int x;
    public int y;
  };

  struct Room
  {
    public Room(Point _entrance, Point _exit, Point _location, Point _size)
    {
      entrance = _entrance;
      exit = _exit;
      location = _location;
      size = _size;
      doorDirections = new ArrayList();
      //doors = new ArrayList();
      //doors.Add(entrance);
      //doors.Add(exit);
    }
    //ArrayList doors;
    public Point entrance;
    public Point exit;
    public Point location;
    public Point size;
    public ArrayList doorDirections;
  };

  MapObject[,] mapArray;
  private TextMesh loadingText;
  private ArrayList maze;
  private int counter;
  private Hashtable rooms;

  void Start()
  {
    loadingText = GameObject.Find("GameManager").GetComponent<TextMesh>();
    isFirst = PhotonNetwork.isMasterClient;
    seed = (int)PhotonNetwork.room.customProperties["seed"];
    mapSize = Vec2(mapWidth, mapHeight);
    rooms = new Hashtable();

    Debug.Log("generating map with seed " + seed + (isFirst ? " as master" : " as client"));

    loadingText.text = "generating map";
    loadState = 0;

    



    //SpawnObjectAtPosition(new Point(0, 0), Resources.Load("GameLevel/BasicTile"));
  }

  void Update()
  {
    switch (loadState)
    {
      case 10:
        break;
      case 0:
        Debug.Log("beginning mapgen");
        GenerateMapArray();

        loadState = 1;
        counter = 0;

        break;
      case 1:
        counter += MazeGen(ref maze);
        if (counter > 64)
        {
          loadState = 2;
        }
        break;
      case 2:
        counter = 0;
        RandomRoomsGen(8);
        loadState = 3;
        break;
      case 3:
        counter += MazeGen(ref maze);
        if (counter > 64)
        {
          loadState = 4;
        }
        break;
      case 4:
        RoomDoorsGen();
        loadState = 5;
        break;
      case 5:
        GenerateEdges();
        loadState = 6;
        break;
      case 6:
        loadState = 9;
        break;
      case 9:
        SpawnMap();
        loadingText.text = "load complete";
        GameObject newPlayer = PhotonNetwork.Instantiate("Player", new Vector3(0, 0, 0), Quaternion.identity, 0);
        loadState = 10; //done loading
        break;

    }
  }


  void GenerateEdges()
  {
    int i = 0; int j = 0;
    for (i = 4; i < mapWidth - 4; i++) //looping through every item
    {
      for (j = 4; j < mapHeight - 4; j++)
      {
        //actual stuff goes here
        if (mapArray[i, j].type == 0 && CheckAdjacent(Vec2(i, j), -1) < 1)
        {
          mapArray[i, j] = Tile(1, 1, 1);
        }
        //actual stuff stops here
      }
    }

    for (i = 4; i < mapWidth - 4; i++) //looping through every item
    {
      for (j = 4; j < mapHeight - 4; j++)
      {
        if (mapArray[i, j].type == 2)
        {
          SetTileAt(Vec2(i, j), Tile(-1, 0, 0));
        }
      }
    }
  }
  //map gen:
  void GenerateMapArray() //creates map array
  {

    mapArray = new MapObject[mapHeight, mapWidth];
    Random.seed = seed;
    //fill in map edge
    FillBorders(Vec2(0, 0), mapSize, Tile(1, 2, 1), false, false, -1);
    //fill in map edge
    FillBorders(Vec2(1, 1), mapSize - Vec2(2, 2), Tile(1, 2, 1), false, false, -1);
    //fill in map edge
    FillBorders(Vec2(3, 3), mapSize - Vec2(6, 6), Tile(1, 2, 1), false, false, -1);
    //fill in map edge
    FillBorders(Vec2(5, 5), mapSize - Vec2(10, 10), Tile(1, 2, 1), false, false, -1);

    //make starting 5x5 room:
    Room startingRoom = new Room(Vec2(mapWidth / 2 - 3, 10), Vec2(-1, 0), Vec2(mapWidth / 2 - 3, 10), Vec2(7, 7));
    CreateRoom(ref startingRoom, 1, Vec2(0, 1), 0, false);
    rooms.Add(0, false);

    maze = new ArrayList();
    Point mazeStart = startingRoom.exit + ((Point)startingRoom.doorDirections[0]);
    if (((Point)startingRoom.doorDirections[0]).x == -1)
    {
      mazeStart.x -= 1;
    }
    if (((Point)startingRoom.doorDirections[0]).y == -1)
    {
      mazeStart.y -= 1;
    }
    //Debug.Log(mazeStart.x + "aa" + mazeStart.y);
    //SetTileAt(mazeStart, Tile(2, 1, 1));
    FillArea(mazeStart, Vec2(2, 2), Tile(2, 1, 1), true);
    maze.Add(mazeStart);
    //maze.Capacity = 100;

    //FillBorders(Vec2(mapWidth/2 - 3, 10), Vec2(7, 7), Tile(1, -1, 1), false, true);

    //make a bunch of other rooms:
    int i = 0; //index
    int j = 0; //index


    i = 0;
  }

  void RandomRoomsGen(int number)
  {
    int i = 1;
    while (i < number + 1)
    {
      if (FillRandomRoom(Vec2(6, 6), Vec2(12, 12), i))
        i++;
    }
  }

  void RoomDoorsGen()
  {
    //index vars:
    int i, j;

    Point startingPoint = Vec2(-mapWidth / 2, -12);

    for (i = 0; i < mapWidth; i++) //looping through every item
    {
      for (j = 0; j < mapHeight; j++)
      {
        //actual stuff goes here
        /*if (TileAt(Vec2(i, j)).type == 1) //generate doors if adjacent tiles
        {
          if ((TileAt(Vec2(i + 1, j)).type == -1 || TileAt(Vec2(i + 1, j)).type == 2)
            && (TileAt(Vec2(i - 1, j)).type == -1 || TileAt(Vec2(i - 1, j)).type == 2))
          {
            SetTileAt(Vec2(i, j), Tile(3, 0, 0));

          } else if ((TileAt(Vec2(i, j + 1)).type == -1 || TileAt(Vec2(i, j + 1)).type == 2)
            && (TileAt(Vec2(i, j - 1)).type == -1 || TileAt(Vec2(i, j - 1)).type == 2))
          {
            SetTileAt(Vec2(i, j), Tile(3, 0, 0));
          }
        }*/
        if (TileAt(Vec2(i, j)).type == 3)
        {
          //Debug.Log(TileAt(Vec2(i, j)).subtype);
          if ((bool)rooms[TileAt(Vec2(i, j)).subtype])
          {

            //SetTileAt(Vec2(i, j), Tile(1, TileAt(Vec2(i, j)).subtype, 0));
          }
          else
          {
            if (((TileAt(Vec2(i + 1, j)).type == -1 || TileAt(Vec2(i + 1, j)).type == 2)
               && (TileAt(Vec2(i - 1, j)).type == -1 || TileAt(Vec2(i - 1, j)).type == 2))
               || ((TileAt(Vec2(i, j + 1)).type == -1 || TileAt(Vec2(i, j + 1)).type == 2)
               && (TileAt(Vec2(i, j - 1)).type == -1 || TileAt(Vec2(i, j - 1)).type == 2)))
            {
              rooms[TileAt(Vec2(i, j)).subtype] = true;
              //SetTileAt(Vec2(i, j), Tile(3, 0, 0));
            }
            else
            {
              SetTileAt(Vec2(i, j), Tile(1, TileAt(Vec2(i, j)).subtype, 0));
            }
          }
        }
        //actual stuff stops here
      }
    }
  }

  int MazeGen(ref ArrayList maze)
  {
    int blocksCreated = 0;
    /*
    if (maze.Count < 1)
    {
      return 9999;
    }
    for (int j = 0; j < maze.Count; j++)
    {
      int randomX = Random.Range(-1, 2);
      int randomY = randomX == 0 ? Random.Range(-1, 2) : 0;
      Point currentPoint = (Point)maze[j];
      if (currentPoint.x == -1)
      {
        //Debug.Log(currentPoint.x + "a" + currentPoint.y);
        break;
      }
      
      if (CheckAdjacent(currentPoint + Vec2(randomX, randomY), 1) < 4 && TileAt(currentPoint + Vec2(randomX, randomY)).type == 0 && CheckAdjacent(currentPoint + Vec2(randomX, randomY), 2) < 3)
       // && CheckDiagonal(currentPoint + Vec2(randomX, randomY), 2) < 1)
      {
        Debug.Log(CheckDiagonal(currentPoint + Vec2(randomX, randomY), 2));
        SetTileAt(currentPoint + Vec2(randomX, randomY), Tile(2, 1, 1));
        maze.Add(currentPoint + Vec2(randomX, randomY));
      }
      else
      {
        
        if (CheckAdjacent(currentPoint, 1) > 3)
        {
          maze.RemoveAt(j);
          break;
        }
      }
    }*/
    if (maze.Count < 1)
    {
      return 9999;
    }
    for (int j = 0; j < maze.Count; j++)
    {
      int randomX = Random.Range(-1, 2);
      int randomY = randomX == 0 ? Random.Range(-1, 2) : 0;
      randomX *= 2;
      randomY *= 2;
      Point currentPoint = (Point)maze[j];
      if (currentPoint.x == -1)
      {
        //Debug.Log(currentPoint.x + "a" + currentPoint.y);
        break;
      }

      Point testPoint = currentPoint + Vec2(randomX, randomY);
      if (CheckAreaFor(currentPoint + Vec2(randomX - 1, randomY - 1), Vec2(4, 4), 1) < 1
        && CheckAreaFor(currentPoint + Vec2(randomX - 1, randomY - 1), Vec2(4, 4), 2) < 4
        && (((randomX == 2 && (TileAt(testPoint + Vec2(2, 2)).type == 0)) && (TileAt(testPoint + Vec2(2, -1)).type == 0)) //diagonals checking depending on direction
        || ((randomX == -2 && (TileAt(testPoint + Vec2(-1, 2)).type == 0)) && (TileAt(testPoint + Vec2(-1, -1)).type == 0)) //damn that's a clusterfuck
        || ((randomY == -2 && (TileAt(testPoint + Vec2(-1, -1)).type == 0)) && (TileAt(testPoint + Vec2(2, -1)).type == 0)) //I guess it's organized, but that's pretty hard to read.
        || ((randomY == 2 && (TileAt(testPoint + Vec2(-1, 2)).type == 0)) && (TileAt(testPoint + Vec2(2, 2)).type == 0))
        ))
      // && (TileAt(testPoint + Vec2(-1, 2)).type == 0) && (TileAt(testPoint + Vec2(-1, 2)).type == 0) && (TileAt(testPoint + Vec2(-1, -1)).type == 0))
      {

        if (randomX == 0)
        {
          FillArea(testPoint, Vec2(2, 2), Tile(2, 1, 1), true);
          //SetTileAt(currentPoint + Vec2(randomX, randomY), Tile(2, 1, 1));
        }
        else
        {
          FillArea(testPoint, Vec2(2, 2), Tile(2, 1, 1), true);
        }

        maze.Add(testPoint);
        blocksCreated++;
      }
      else
      {
        if (CheckAreaFor(currentPoint, Vec2(4, 4), 2) > 10)
        {
          maze.RemoveAt(j);
          break;
        }
      }
    }
    return blocksCreated;
  }
  void SpawnMap() //actually instantiates objects according to array
  {
    //index vars:
    int i, j;

    Point startingPoint = Vec2(-mapWidth / 2, -12);

    for (i = 0; i < mapWidth; i++) //looping through every item
    {
      for (j = 0; j < mapHeight; j++)
      {
        GameObject tile = this.gameObject;
        //actual stuff goes here
        switch (mapArray[i, j].type)
        {
          case 1:
            tile = SpawnObjectAtPosition(Vec2(i + startingPoint.x, j + startingPoint.y), Resources.Load("GameLevel/BasicTile"), 0);
            break;
          case 2:
            //case 0: //debug fallthrough
            tile =  SpawnObjectAtPosition(Vec2(i + startingPoint.x, j + startingPoint.y), Resources.Load("GameLevel/BasicTile2"), 0);
            break;
          case -1:
          case 3: //debug fallthroguh
            //SpawnObjectAtPosition(Vec2(i + startingPoint.x, j + startingPoint.y), Resources.Load("GameLevel/BasicTile2"), 2);
            break;
            /*
            case 0:
              Debug.Log("null test");
              break;*/
        }
        if (tile != this.gameObject)
        {
          tile.transform.parent = transform;
        }
        //actual stuff stops here
      }
    }
  }


  GameObject SpawnObjectAtPosition(Point location, Object thing, float height)
  {
    //note that location is doubled in magnitude because tiles are 2x2 and not 1x1
    Vector3 worldPosition = new Vector3(location.x * 2, location.y * 2, height);
    return (GameObject)Instantiate(thing, worldPosition, Quaternion.identity);
  }

  bool FillRandomRoom(Point sizeMin, Point sizeMax, int tileMeta) //tilemeta sets subtype of tile object
  {
    Point size = Vec2(//random room size within boundaries
      Random.Range(sizeMin.x, sizeMax.x),
      Random.Range(sizeMin.y, sizeMax.y));

    Point position = Vec2(//random position within boundaries, size subtracted to avoid going out of bounds
      Random.Range(1, mapWidth - size.x),
      Random.Range(1, mapHeight - size.y));

    position = (position / 2) * 2;
    size = (size / 2) * 2;

    if (CheckAreaFor(position, size, -1) == 0 && CheckAreaFor(position, size, 1) == 0) //if nothing's there
    {
      Room newRoom = new Room(Vec2(-1, 0), Vec2(-1, 0), position, size);
      CreateRoom(ref newRoom, 16, Vec2(0, 0), tileMeta, true);

      rooms.Add(tileMeta, false);
      //FillBorders(position, size, Tile(1, tileMeta, 1), false, true, tileMeta);
      return true;
    }
    else
    {
      return false;
    }
  }

  /*
    Generates a room attached to the existing room
  */
  void GenerateAdjacentRoom(ref Room inputRoom)
  {

  }
  /*
    create functions ar emeant to take existing structures as input, generates will spawn and return them
    createdoors specifices number of doors to generate along the edges of the room
    also modifies original room
    doorbias determines what sides doors can spawn in
    deterministic doors currently uses ultra simple functionality 
  */
  void CreateRoom(ref Room inputRoom, int createDoors, Point doorDirectionBias, int meta, bool deterministicDoors)
  {
    Point location = inputRoom.location;
    Point size = inputRoom.size;
    FillBorders(location, size, Tile(1, meta, 0), true, true, meta);

    while (createDoors > 0) //generate doors
    {
      int random = (createDoors % 4) + 1;
      if (!deterministicDoors)
      {
        random = Random.Range(1, 5);
      }
      Point door = Vec2(0, 0);
      Point direction = Vec2(0, 0);
      switch (random) //go along one of the 4 walls
      {
        case 1:
          if (doorDirectionBias.x > -1)
          {
            door.y = (Random.Range(1, size.y - 3) / 2) * 2 + 2;
            door.x = size.x - 1;
            direction.x = 1;
          }
          else
          {
            door.x = -1;
          }
          break;
        case 2:
          if (doorDirectionBias.x < 1)
          {
            door.y = (Random.Range(1, size.y - 3) / 2) * 2 + 2;
            door.x = 0;
            direction.x = -1;
          }
          else
          {
            door.x = -1;
          }
          break;
        case 3:
          if (doorDirectionBias.y > -1)
          {
            door.x = (Random.Range(1, size.x - 3) / 2) * 2 + 2;
            door.y = size.y - 1;
            direction.y = 1;
          }
          else
          {
            door.x = -1;
          }
          break;
        case 4:
          if (doorDirectionBias.y < 1)
          {
            door.x = (Random.Range(1, size.x - 3) / 2) * 2 + 2;
            door.y = 0;
            direction.y = -1;
          }
          else
          {
            door.x = -1;
          }
          break;
      }

      if (door.x != -1 && TileAt(door).type != 3)
      {
        door = door + inputRoom.location;
        inputRoom.doorDirections.Add(direction);

        SetTileAt(door, Tile(3, meta, 0));
        if (inputRoom.entrance.x == -1) //-1 x value means location uninitialized in this context
        {
          inputRoom.entrance = door;
        }
        createDoors--;
      }

      if (inputRoom.exit.x == -1)
      {
        inputRoom.exit = door;
      }
    }
  }
  /*
  FillArea
    Fills given rectangular area with given map object
  */
  void FillArea(Point position, Point dimensions, MapObject newObject, bool ignoreDoors)
  {
    int height = dimensions.y;
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

  void FloodMetaFill(Point startLocation, int newMeta) //flood fills same types' meta values
  {
    int myType = TileAt(startLocation).type;
    ArrayList points = new ArrayList();
    points.Add(startLocation);
    while (points.Count > 0)
    {
      foreach (Point fill in points)
      {
        if (TileAt(fill + Vec2(0, 1)).type == myType && TileAt(fill + Vec2(0, 1)).subtype != newMeta)
        {
          SetTileAt(fill + Vec2(0, 1), Tile(myType, newMeta, 0));
          points.Add(fill + Vec2(0, 1));
        }
        else if (TileAt(fill + Vec2(1, 0)).type == myType && TileAt(fill + Vec2(1, 0)).subtype != newMeta)
        {
          SetTileAt(fill + Vec2(1, 0), Tile(myType, newMeta, 0));
          points.Add(fill + Vec2(1, 0));
        }
        else if (TileAt(fill + Vec2(0, -1)).type == myType && TileAt(fill + Vec2(0, -1)).subtype != newMeta)
        {
          SetTileAt(fill + Vec2(0, -1), Tile(myType, newMeta, 0));
          points.Add(fill + Vec2(0, -1));
        }
        else if (TileAt(fill + Vec2(-1, 0)).type == myType && TileAt(fill + Vec2(-1, 0)).subtype != newMeta)
        {
          SetTileAt(fill + Vec2(-1, 0), Tile(myType, newMeta, 0));
          points.Add(fill + Vec2(-1, 0));
        }
      }
    }
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
  int CheckAreaFor(Point position, Point dimensions, int type)
  {
    int height = dimensions.y;
    int width = dimensions.x;
    int count = 0;
    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        if (mapArray[position.x + i, position.y + j].type == type)
        {
          count++;
        }
      }
    }
    return count;
  }

  /*
  CheckAdjacent
    number of a given tile type is in a 3x3 area
  */
  int CheckAdjacent(Point position, int type)
  {
    int count = 0;
    int height = 3;
    int width = 3;
    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        if (mapArray[position.x + i - 1, position.y + j - 1].type == type)
        {
          count++;
        }
      }
    }
    return count;
  }

  /*
  CheckOrthogonal
    only checks in straight lines (no diagonals) next to given tile
  */
  int CheckOrthogonal(Point position, int type)
  {
    int count = 0;
    count += TileAt(position + Vec2(1, 0)).type == type ? 1 : 0;
    count += TileAt(position + Vec2(-1, 0)).type == type ? 1 : 0;
    count += TileAt(position + Vec2(0, 1)).type == type ? 1 : 0;
    count += TileAt(position + Vec2(0, -1)).type == type ? 1 : 0;
    return count;
  }

  /*
  CheckDiagonal
    only checks diagonally next to given tile
  */
  int CheckDiagonal(Point position, int type)
  {
    int count = 0;
    count += TileAt(position + Vec2(1, 1)).type == type ? 1 : 0;
    count += TileAt(position + Vec2(-1, 1)).type == type ? 1 : 0;
    count += TileAt(position + Vec2(-1, -1)).type == type ? 1 : 0;
    count += TileAt(position + Vec2(1, -1)).type == type ? 1 : 0;
    return count;
  }

  /*
  FillBorders
    Replaces the borders of a given rectangular area with given map object
    More efficient than filling an area and then filling the inside with empty space
    Ignore doors specifies whether or not tile type 3 should be ignored
  */
  void FillBorders(Point position, Point dimensions, MapObject newObject, bool ignoreDoors, bool createFloor, int meta)
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
          }
          else
          {
            if (createFloor)
            {
              mapArray[position.x + i, position.y + j] = Tile(-1, meta, 0); //floor is -1
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

  MapObject TileAt(Point location) //shortcut for mapArray[location.x, location.y]
  {
    //Debug.Log(location.x + "|" + location.y);
    return mapArray[location.x, location.y];
  }

  void SetTileAt(Point location, MapObject tile) //set tile at coordinates given by point
  {
    mapArray[location.x, location.y] = tile;
  }
}
