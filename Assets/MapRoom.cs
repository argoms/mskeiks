//file used for map generation related objects
using System.Collections;
public class MapRoom
{
  public MapRoom()
  {
  }
  public MapRoom(Point _entrance, Point _exit, Point _location, Point _size, string _roomType, int _roomNumber)
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
}

public class Point
{
  public Point()
  {
  }
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
    return (point1.x == point2.x) && (point1.y == point2.y);
  }

  public static bool operator !=(Point point1, Point point2) //not equals check
  {
    return !((point1.x == point2.x) && (point1.y == point2.y));
  }

  public int x;
  public int y;
}

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
