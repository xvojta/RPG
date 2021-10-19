using System.Collections.Generic;
using UnityEngine;

public class DungeonGeneration : MonoBehaviour
{
    public Room[] rooms;
    public Corridor[] corridors;
    public int NumberOfRooms;
    public GameObject DungeonParentObject;

    Vector3 nextRoomSpawnPos;
    EntryPoint lastEntryPoint;
    Vector2 lastPosition;
    int rotation = 0;
    bool[][] map;
    Vector2 startPos;

    public void GenerateDungeonTest()
    {
        foreach (Transform child in DungeonParentObject.transform) Destroy(child.gameObject);
        GenerateDungeon(NumberOfRooms, 3, 10);
    }

    private void OnDestroy()
    {
        Debug.LogError("Destroy");
    }

    void Init(int numberOfRooms, int minCorridor, int maxCorridor)
    {
        rotation = 0;

        startPos = new Vector2(numberOfRooms * 2 - 1, numberOfRooms * 2 - 1);
        int mapSize = (int)startPos.x*2;
        map = new bool[mapSize][];
        for (int i = 0; i < map.Length; i++) map[i] = new bool[mapSize];
        map[(int)startPos.x][(int)startPos.y] = true;
    }

    public void GenerateDungeon(int numberOfRooms, int minCorridor, int maxCorridor)
    {
        Init(numberOfRooms, minCorridor, maxCorridor);

        nextRoomSpawnPos = DungeonParentObject.transform.position;

        Debug.Log(SpawnDungeon(1, 0, RoomType.Spawn, startPos, DungeonParentObject.GetComponent<EntryPoint>(), true));

        Debug.Log(SpawnDungeon(numberOfRooms - 1, 1, RoomType.Loot, lastPosition, lastEntryPoint));

        Debug.Log(SpawnDungeon(1, 1, RoomType.Portal, lastPosition, lastEntryPoint, true));
    }

    GameObject SpawnCorridor(Corridor corridor, int lenght, EntryPoint entryPoint)
    {
        Vector3 nextSpawnPos = new Vector3();

        GameObject newCorridor = Instantiate(corridor.CorridorEnterPrefab, DungeonParentObject.transform, false);
        EntryPoint[] allEntryPoints = newCorridor.GetComponentsInChildren<EntryPoint>();

        var mainEntryPoint = getMainEntryPoint(allEntryPoints);
        mainEntryPoint.transform.position = entryPoint.transform.position;
        rotation = entryPoint.RotationOffset;
        mainEntryPoint.transform.Rotate(0, 180 + rotation, 0);

        foreach (EntryPoint point in allEntryPoints) if (!point.IsMain) nextSpawnPos = point.transform.position;


        for (int i = 0; i < lenght; i++)
        {
            nextSpawnPos = spawnCorridorPiece(corridor.CorridorMiddlePrefab, nextSpawnPos, newCorridor.transform);
        }

        nextRoomSpawnPos = spawnCorridorPiece(corridor.CorridorExitPrefab, nextSpawnPos, newCorridor.transform);
        return newCorridor;
    }

    GameObject SpawnRoom(Room room)
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

    bool SpawnDungeon(int numberOfrooms, int potencialRooms, RoomType type, Vector2 mapPos, EntryPoint entryPoint,
        bool canSpawn1Entrance = false, int corridorLenght = 5)
    {
        if (numberOfrooms < 1) return false;

        var maxNumberOfEntrances = numberOfrooms - potencialRooms + 1;
        float allProbabilities = 0;
        foreach (Room room in rooms)
        {
            allProbabilities += room.SpawnProbability;
        }

        float random = Random.Range(0, allProbabilities * 1000) / 1000;

        int indexOffset = 0;
        for (int i = rooms.Length - 1; i >= 0; i--)
        {
            if (random > allProbabilities - rooms[i].SpawnProbability)
            {
                indexOffset = i;
                break;
            }
            allProbabilities -= rooms[i].SpawnProbability;
        }

        EntryPoint[] entryPoints = new EntryPoint[0];

        for (int i = 0; i < rooms.Length; i++)
        {
            var newIndex = (i + indexOffset) % rooms.Length;
            if (rooms[newIndex].RoomType == type && rooms[newIndex].NumberOfEntrances <= maxNumberOfEntrances
                && (rooms[newIndex].NumberOfEntrances > 1 || canSpawn1Entrance))
            {
                entryPoints = rooms[newIndex].RoomPrefab.GetComponentsInChildren<EntryPoint>();
                List<Vector2> newPos = new List<Vector2>();
                foreach(EntryPoint point in entryPoints)
                {
                    if (!point.IsMain || type == RoomType.Spawn)
                    {
                        var testPos = topologyCheck(point, mapPos);
                        if (!testPos.HasValue) goto Fail;
                        newPos.Add(testPos.Value);
                    }
                }
                GameObject newCorridor = null;
                GameObject newRoom = null;
                for (int p = 0; p < entryPoints.Length; p++)
                {
                    if (!entryPoints[p].IsMain || type == RoomType.Spawn)
                    {
                        var thisNewPos = newPos[p % newPos.Count];
                        if(newRoom == null)
                        {
                            if (type != RoomType.Spawn)
                            {
                                newCorridor = SpawnCorridor(corridors[0], corridorLenght, entryPoint);
                            }
                            else
                            {
                                nextRoomSpawnPos = entryPoint.transform.position;
                            }
                            newRoom = SpawnRoom(rooms[newIndex]);

                            lastEntryPoint = newRoom.GetComponentsInChildren<EntryPoint>()[p];
                            lastPosition = thisNewPos;
                        }
                        else if(p > 1)
                        {
                            lastEntryPoint = newRoom.GetComponentsInChildren<EntryPoint>()[p];
                            lastPosition = thisNewPos;
                        }


                        if (numberOfrooms == 1 ||
                        SpawnDungeon(numberOfrooms - 1, potencialRooms + rooms[newIndex].NumberOfEntrances - 2, type,
                        thisNewPos, lastEntryPoint, potencialRooms > 1 || numberOfrooms-1 < 2, corridorLenght))
                        {
                            if(p >= entryPoints.Length - 1)return true;
                        }
                        else
                        {
                            if (newRoom != null) Destroy(newRoom);
                            if (newCorridor != null) Destroy(newCorridor);
                            goto Fail;
                        }
                    }
                    else if(rooms[newIndex].NumberOfEntrances < 2)
                    {
                        if (type != RoomType.Spawn) SpawnCorridor(corridors[0], corridorLenght, entryPoint);
                        SpawnRoom(rooms[newIndex]);
                        return true;
                    }
                }
            Fail:
                foreach (EntryPoint point in entryPoints)
                {
                    if (!point.IsMain) setMap(point, mapPos, false);
                }
            }
        }
        //Debug.LogError("Missing room with: " + maxNumberOfEntrances + ". Or topology problem");
        return false;
    }

    Vector2? topologyCheck(EntryPoint point, Vector2 mapPosition)
    {
        var direction = point.RotationOffset + rotation;
        //if (entryPointStack.Count > 0) direction += entryPointStack[0].RotationOffset;
        Vector2 move = Vector2.right.Rotate(direction);
        for(int i = 1; i < 3; i++)
        {
            var newPos = mapPosition + move*i;
            if (!map[(int)newPos.x][(int)newPos.y])
            {
                map[(int)newPos.x][(int)newPos.y] = true;
                if (i == 2) return newPos;
            }
            else
            {
                return null;
            }
        }
        return null;
    }

    void setMap(EntryPoint point, Vector2 mapPosition, bool value)
    {
        var direction = point.RotationOffset + rotation;
        //if (entryPointStack.Count > 0) direction += entryPointStack[0].RotationOffset;
        Vector2 move = Vector2.right.Rotate(direction);
        for (int i = 1; i < 3; i++)
        {
            var newPos = mapPosition + move * i;
            map[(int)newPos.x][(int)newPos.y] = value;
        }
    }

    Vector3 spawnCorridorPiece(GameObject piece, Vector3 spawnPos, Transform parent)
    {
        var newCorridor = Instantiate(piece, parent, false);
        var allEntryPoints = newCorridor.GetComponentsInChildren<EntryPoint>();
        var mainEntryPoint = getMainEntryPoint(allEntryPoints);
        mainEntryPoint.transform.position = spawnPos;
        mainEntryPoint.transform.Rotate(0, 180 + rotation, 0);
        foreach (EntryPoint point in allEntryPoints) if (!point.IsMain) return point.transform.position;
        return Vector3.zero;
    }
}
