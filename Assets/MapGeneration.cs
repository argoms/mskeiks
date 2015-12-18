using UnityEngine;
using System.Collections;

public class MapGeneration : Photon.MonoBehaviour
{
  private bool isFirst;
  private int mapHeight = 128;
  private int mapWidth = 128;
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

    public static bool operator ==(Point point1, Point point2) //equality check
    {
      return (point1.x == point2.x) && (point1.y == point2.y) ;
    }

    public static bool operator !=(Point point1, Point point2) //not equals check
    {
      return !((point1.x == point2.x) && (point1.y == point2.y));
    }

    public int x;
    public int y;
  };

  struct Room
  {
    public Room(Point _entrance, Point _exit, Point _location, Point _size, string _roomType, int _roomNumber)
    {
      entrance = _entrance;
      exit = _exit;
      location = _location;
      size = _size;
      doorDirections = new ArrayList();
      doors = new ArrayList();
      roomType = _roomType;
      roomNumber = _roomNumber;
      //doors.Add(entrance);
      //doors.Add(exit);
    }

    //ArrayList doors;
    public Point entrance;
    public Point exit;
    public Point location;
    public Point size;
    public ArrayList doorDirections;
    public ArrayList doors;
    public string roomType;
    public int roomNumber;
  };

  int mapLength = 0;
  MapObject[,] mapArray;
  private TextMesh loadingText;
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

        loadState = 5;
        break;
      /*case 1:
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
        break;*/
      case 5:
        //GenerateEdges();
        loadState = 6;
        break;
      case 6:
        loadState = 9;
        break;
      case 9:
        SpawnMap();
        loadingText.text = "load complete";
        GameObject newPlayer = PhotonNetwork.Instantiate("Player", new Vector3(0, 0, 0), Quaternion.identity, 0);
        //PhotonNetwork.room.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "plist", (ArrayList)(PhotonNetwork.room.customProperties["plist"]).Add(1) } });
        //PlayerScore.SetPlayerScore.SetCustomProperties( new ExitGames.Client.Photon.Hashtable(){ { "Deaths", (int)PlayerScore.customProperties["Deaths"]+1 } } )
        //this.GetComponent<LevelManager>().playerList.Add(newPlayer);
        this.GetComponent<LevelManager>().UpdateList(); // photonView.RPC("AddPlayer", PhotonTargets.All, newPlayer);
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
    /*
    Room startingRoom = new Room(Vec2(mapWidth / 2 - 3, 10), Vec2(-1, 0), Vec2(mapWidth / 2 - 3, 10), Vec2(7, 7));
    CreateRoom(ref startingRoom, 1, Vec2(0, 1), 0, false);*/
    Room startingRoom = CreateStartRoom(7, "StartRoom");
    startingRoom.roomNumber = 1;
    DrawRoom(startingRoom, Vec2(mapWidth / 2 - 3, 20));

    int i = 0;
    while (i++ < 5)
    {
      DrawNextRoom(mapLength - 1);
    }
  }

  Room CreateSquareRoom(int size, string type)
  {
    Room newRoom = new Room();
    newRoom.doorDirections = new ArrayList();
    newRoom.doors = new ArrayList();

    newRoom.location = Vec2(-1, 0);
    newRoom.size = Vec2(size, size);

    newRoom.doors.Add(Vec2(0, newRoom.size.y / 2)); //door going left
    newRoom.doorDirections.Add(Vec2(-1, 0));

    newRoom.doors.Add(Vec2(newRoom.size.x - 1, newRoom.size.y / 2)); //door going right
    newRoom.doorDirections.Add(Vec2(1, 0));

    newRoom.doors.Add(Vec2(newRoom.size.x / 2, 0)); //door going down
    newRoom.doorDirections.Add(Vec2(0, -1));

    newRoom.doors.Add(Vec2(newRoom.size.x / 2, newRoom.size.y - 1)); //door going up;
    newRoom.doorDirections.Add(Vec2(0, 1));

    newRoom.roomType = type;
    return newRoom;
  }

  Room CreateStartRoom(int size, string type)
  {
    Room newRoom = new Room();
    newRoom.doorDirections = new ArrayList();
    newRoom.doors = new ArrayList();

    newRoom.location = Vec2(-1, 0);
    newRoom.size = Vec2(size, size);

    newRoom.doors.Add(Vec2(0, newRoom.size.y / 2)); //door going left
    newRoom.doorDirections.Add(Vec2(-1, 0));

    newRoom.doors.Add(Vec2(newRoom.size.x - 1, newRoom.size.y / 2)); //door going right
    newRoom.doorDirections.Add(Vec2(1, 0));

    newRoom.doors.Add(Vec2(newRoom.size.x / 2, newRoom.size.y - 1)); //door going up;
    newRoom.doorDirections.Add(Vec2(0, 1));

    newRoom.roomType = type;
    return newRoom;
  }

  void DrawRoom(Room newRoom, Point location)
  {
    
    newRoom.location = location;
    switch (newRoom.roomType)
    {
      case "StartRoom":
        FillBorders(location, newRoom.size, Tile(1, 0, 0), false, true, 1); //starting room's special meta value is 1 because fuck starting at zero right?
        break; //seriously though the default value is 0 so that's why it's 1 here

      case "RegularRoom":
        FillBorders(location, newRoom.size, Tile(1, 0, 0), false, true, newRoom.roomNumber);
        break;
    }

    rooms.Add(mapLength, newRoom);
    mapLength++;
  }

  void DrawNextRoom(int index) //draw a room attached to the index room
  {
    
    bool done = false; //used to repeatedly try things
    bool reallyDone = false; //used to repeatedly try larger scale things
    int counter = 0;
    
    while (!reallyDone)
    {
      Room nextRoom = CreateSquareRoom(Random.Range(2, 5) * 2 + 1, "RegularRoom");
      Debug.Log(Random.Range(2, 5));
      Room currentRoom = (Room)rooms[index];
      int randomDoor = 0;
      Point randomDoorDirection;
      int originalDoor = -1;
      done = false;
      while (!done) //find a connection
      {
        randomDoor = Random.Range(0, (currentRoom.doors.Count)); //pick a random door index
        randomDoorDirection = (Point)currentRoom.doorDirections[randomDoor]; //get direction of that index
        originalDoor = -1; //index of the door on the original room to join to

        for (int i = 0; i < nextRoom.doorDirections.Count; i++)
        {
          //find a door on the original room in the opposite direction of the randomly chosen door on the new room:
          if ((Point)nextRoom.doorDirections[i] == (randomDoorDirection * -1))
          {
            originalDoor = i;
          }
        }

        if (originalDoor != -1)
        {
          done = true; //stop seraching once a connection is found
        }
      }


      Point newRoomLocation = currentRoom.location + (Point)currentRoom.doors[originalDoor] + (Point)currentRoom.doorDirections[originalDoor] - (Point)nextRoom.doors[randomDoor];
      //Debug.Log(newRoomLocation.x + "|" + newRoomLocation.y + "|||" + nextRoom.size.x + "|" + nextRoom.size.y);

      if (CheckAreaFor(newRoomLocation, nextRoom.size, -1) == 0)
      {
        DrawRoom(nextRoom, newRoomLocation);

        Point doorLocation = currentRoom.location + (Point)currentRoom.doors[originalDoor];
        SetTileAt(doorLocation, Tile(3, 0, 0));
        SetTileAt(doorLocation + (Point)currentRoom.doorDirections[originalDoor], Tile(3, 0, 0));

        reallyDone = true;
      }

      counter++;
      if (counter > 50)
        reallyDone = true; //stop that. stop trying things.
    }

    
    //if(CheckAreaFor(currentRoom.location + (Point)currentRoom.doors[originalDoor]

  }

  void SpawnMap() //actually instantiates objects according to array
  {
    //index vars:
    int i, j;

    Point startingPoint = Vec2(-mapWidth / 2, -22);

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
            if (mapArray[i, j].subtype == -2)
            {
              if (isFirst)
              {
                GameObject newEnemy = PhotonNetwork.Instantiate("BasicEnemy", new Vector3((i + startingPoint.x) * 2 - 1, (j + startingPoint.y) * 2 - 1, 0), Quaternion.identity, 0);
                newEnemy.GetComponent<EnemyControl>().level = this.gameObject;
              }
            }
            break;
          case 3: //debug fallthroguh
            SpawnObjectAtPosition(Vec2(i + startingPoint.x, j + startingPoint.y), Resources.Load("GameLevel/BasicTile2"), 2);
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

    if (isFirst)
    {
      GameObject newEnemy = PhotonNetwork.Instantiate("BasicEnemy", new Vector3(0, 13, 0), Quaternion.identity, 0);
      newEnemy.GetComponent<EnemyControl>().level = this.gameObject;
    }
  }


  GameObject SpawnObjectAtPosition(Point location, Object thing, float height)
  {
    //note that location is doubled in magnitude because tiles are 2x2 and not 1x1
    Vector3 worldPosition = new Vector3(location.x * 2, location.y * 2, height);
    return (GameObject)Instantiate(thing, worldPosition, Quaternion.identity);
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
