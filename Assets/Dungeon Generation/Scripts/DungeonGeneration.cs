using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomReturn
{
    private bool valid = false;

    private List<GameObject> spawnedObjects;
    private List<Vector2> mapPositions;

    public RoomReturn(bool Valid, List<GameObject> SpawnedObjects, List<Vector2> MapPositions)
    {
        valid = Valid;
        spawnedObjects = SpawnedObjects;
        mapPositions = MapPositions;
    }

    public bool Valid
    {
        get { return valid; }
        set { valid = value; }
    }
    public List<GameObject> SpawnedObjects
    {
        get { return spawnedObjects; }
        set { spawnedObjects = value; }
    }
    public List<Vector2> MapPositions
    {
        get { return mapPositions; }
        set { mapPositions = value; }
    }
}

public class DungeonGeneration : MonoBehaviour
{
    public Room[] rooms;
    public Corridor[] corridors;
    public int NumberOfRooms;
    public GameObject DungeonParentObject;
    public MinimapGeneration minimapGeneration;

    Vector3 nextRoomSpawnPos;
    EntryPoint lastEntryPoint;
    bool[][] map;
    Vector2 startPos;

    public void Start()
    {
        testRun();
    }

    public void testRun()
    {
        foreach (Transform child in DungeonParentObject.transform) Destroy(child.gameObject);
        GenerateDungeon(NumberOfRooms, 3, 10);
        if (minimapGeneration != null) minimapGeneration.Generate(map);
    }

    void Init(int numberOfRooms, int minCorridor, int maxCorridor)
    {
        startPos = new Vector2(numberOfRooms * 2 - 1, numberOfRooms * 2 - 1);
        int mapSize = (int)startPos.x*2;
        map = new bool[mapSize][];
        for (int i = 0; i < map.Length; i++) map[i] = new bool[mapSize];
        map[(int)startPos.x][(int)startPos.y] = true;
    }

    public void GenerateDungeon(int numberOfRooms, int minCorridor, int maxCorridor)
    {
        Init(numberOfRooms, minCorridor, maxCorridor);

        #region spawnSpawn
        nextRoomSpawnPos = DungeonParentObject.transform.position;

        List<Room> spawnRooms = new List<Room>();
        foreach (Room room in rooms) if (room.RoomType == RoomType.Spawn) spawnRooms.Add(room);
        var spawnRoom = SpawnRoom(randomRoom(spawnRooms), 0);
        var mainEntry = getMainEntryPoint(spawnRoom.GetComponentsInChildren<EntryPoint>());
        mainEntry.transform.position = DungeonParentObject.transform.position;
        #endregion

        var dungeon = SpawnDungeon(numberOfRooms - 1, RoomType.Loot, startPos, 0, DungeonParentObject.GetComponent<EntryPoint>());

        #region spawnPortal
        List<Room> portalRooms = new List<Room>();
        foreach (Room room in rooms) if (room.RoomType == RoomType.Portal) portalRooms.Add(room);
        var portalRoom = SpawnRoom(randomRoom(portalRooms), 0);
        var lastRoom = dungeon.SpawnedObjects[dungeon.SpawnedObjects.Count - 1];
        mainEntry = getMainEntryPoint(portalRoom.GetComponentsInChildren<EntryPoint>());
        mainEntry.transform.position = lastRoom.GetComponentInChildren<EntryPoint>().transform.position;
        mainEntry.transform.rotation = lastRoom.GetComponentInChildren<EntryPoint>().transform.rotation;
        Destroy(lastRoom);
        #endregion
    }

    GameObject SpawnCorridor(Corridor corridor, int lenght, EntryPoint entryPoint, int rotation)
    {
        Vector3 nextSpawnPos = new Vector3();

        GameObject newCorridor = Instantiate(corridor.CorridorEnterPrefab, DungeonParentObject.transform, false);
        EntryPoint[] allEntryPoints = newCorridor.GetComponentsInChildren<EntryPoint>();

        var mainEntryPoint = getMainEntryPoint(allEntryPoints);
        mainEntryPoint.transform.position = entryPoint.transform.position;
        mainEntryPoint.transform.Rotate(0, 180 + rotation, 0);

        foreach (EntryPoint point in allEntryPoints) if (!point.IsMain) nextSpawnPos = point.transform.position;


        for (int i = 0; i < lenght; i++)
        {
            nextSpawnPos = spawnCorridorPiece(corridor.CorridorMiddlePrefab, nextSpawnPos, newCorridor.transform, rotation);
        }

        nextRoomSpawnPos = spawnCorridorPiece(corridor.CorridorExitPrefab, nextSpawnPos, newCorridor.transform, rotation);
        return newCorridor;
    }

    GameObject SpawnRoom(Room room, int rotation)
    {
        Debug.Log(room.ToString());

        GameObject spawnedRoom = Instantiate(room.RoomPrefab, DungeonParentObject.transform, false);

        EntryPoint[] allEntryPoints = spawnedRoom.GetComponentsInChildren<EntryPoint>();
        EntryPoint mainEntryPoint = getMainEntryPoint(allEntryPoints);
        mainEntryPoint.transform.position = nextRoomSpawnPos;
        mainEntryPoint.transform.Rotate(0, 180 - rotation, 0);
        mainEntryPoint.name += " " + nextRoomSpawnPos;

        return spawnedRoom;
    }

    EntryPoint getMainEntryPoint(EntryPoint[] entryPoints)
    {
        foreach (EntryPoint entryPoint in entryPoints) if (entryPoint.IsMain) return entryPoint;
        Debug.LogError("World doesn't have main entry point");
        return null;
    }

    RoomReturn SpawnDungeon(int roomsToGenerate, RoomType type, Vector2 mapPos, int rotation,
        EntryPoint entryPoint, int corridorLenght = 5)
    {
        List<Room> ValidRooms = rooms.OfType<Room>().ToList();

        RoomReturn roomReturn = new RoomReturn(false, new List<GameObject>(), new List<Vector2>());

        EntryPoint[] entryPoints = new EntryPoint[0];

        while (ValidRooms.Count > 0)
        {
            Room room = randomRoom(ValidRooms);
            if (room == null) break; //no more valid rooms
            ValidRooms.Remove(room);

            if (room.RoomType == type && room.NumberOfEntrances <= roomsToGenerate
                && (room.NumberOfEntrances > 1 || roomsToGenerate == 1))            
            {
                //all entry points topology check, room does not directly violate rooms topology and save its topology position
                var newPos = topologyCheck(entryPoint, mapPos, rotation);
                if (newPos == null) goto Fail;
                roomReturn.MapPositions.AddRange(newPos);

                roomReturn.SpawnedObjects.Add(SpawnCorridor(corridors[0], corridorLenght, entryPoint, rotation));
                roomReturn.SpawnedObjects.Add(SpawnRoom(room, rotation));

                entryPoints = roomReturn.SpawnedObjects[roomReturn.SpawnedObjects.Count - 1].GetComponentsInChildren<EntryPoint>();
                var newMapPos = newPos[newPos.Count - 1];

                for (int p = 0; p < entryPoints.Length; p++)
                {
                    if (!entryPoints[p].IsMain)
                    {
                        lastEntryPoint = entryPoints[p];

                        var maxRooms = roomsToGenerate - 1 - (room.NumberOfEntrances - 2);
                        int branchRoomsToGenerate = (p < entryPoints.Length-1)? Random.Range(1, maxRooms*2/entryPoints.Length) : maxRooms;

                        var newRoomReturn = SpawnDungeon(branchRoomsToGenerate,
                        type, newMapPos, rotation + lastEntryPoint.RotationOffset, lastEntryPoint, corridorLenght);

                        roomReturn.MapPositions.AddRange(newRoomReturn.MapPositions);
                        roomReturn.SpawnedObjects.AddRange(newRoomReturn.SpawnedObjects);

                        if (roomsToGenerate == 1 || newRoomReturn.Valid)
                        {
                            roomsToGenerate -= newRoomReturn.SpawnedObjects.Count / 2 - 1; //-1 for completed branch
                        }
                        else
                        {                            
                            goto Fail;
                        }
                    }
                }
                roomReturn.Valid = true;
                return roomReturn;

            Fail:
                foreach (GameObject gameObject in roomReturn.SpawnedObjects)
                {
                    Destroy(gameObject);
                }
                foreach (Vector2 position in roomReturn.MapPositions)
                {
                    setMap(position, false);
                }
            }
        }
        return new RoomReturn(false, new List<GameObject>(), new List<Vector2>());
    }

    List<Vector2> topologyCheck(EntryPoint point, Vector2 mapPosition, int rotation)
    {
        List<Vector2> value = new List<Vector2>();
        Vector2 move = Vector2.right.Rotate(rotation);
        for(int i = 1; i < 3; i++)
        {
            var newPos = mapPosition + move*i;
            if (!map[(int)newPos.x][(int)newPos.y])
            {
                map[(int)newPos.x][(int)newPos.y] = true;
                value.Add(newPos);
                if (i == 2) return value;
            }
            else
            {
                return null;
            }
        }
        return null;
    }

    void setMap(Vector2 mapPosition, bool value)
    {
        map[(int)mapPosition.x][(int)mapPosition.y] = value;
    }

    Vector3 spawnCorridorPiece(GameObject piece, Vector3 spawnPos, Transform parent, int rotation)
    {
        var newCorridor = Instantiate(piece, parent, false);
        var allEntryPoints = newCorridor.GetComponentsInChildren<EntryPoint>();
        var mainEntryPoint = getMainEntryPoint(allEntryPoints);
        mainEntryPoint.transform.position = spawnPos;
        mainEntryPoint.transform.Rotate(0, 180 + rotation, 0);
        foreach (EntryPoint point in allEntryPoints) if (!point.IsMain) return point.transform.position;
        return Vector3.zero;
    }

    Room randomRoom(List<Room> rooms)
    {
        float allProbabilities = 0;
        foreach (Room room in rooms)
        {
            allProbabilities += room.SpawnProbability;
        }

        float random = Random.Range(0, allProbabilities);

        for (int i = rooms.Count - 1; i >= 0; i--)
        {
            if (random > allProbabilities - rooms[i].SpawnProbability)
            {
                return rooms[i];
            }
            allProbabilities -= rooms[i].SpawnProbability;
        }
        return null;
    }
}
