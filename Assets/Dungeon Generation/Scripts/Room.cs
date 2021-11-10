using UnityEngine;

public enum RoomType { Loot, Fight, Spawn, Portal}

[CreateAssetMenu(fileName = "Data", menuName = "RPG/Room", order = 1)]
public class Room : ScriptableObject
{
    public RoomType RoomType;

    public GameObject RoomPrefab;

    [Range(0.0f, 1.0f)]
    public float SpawnProbability;
    public int NumberOfEntrances;
}