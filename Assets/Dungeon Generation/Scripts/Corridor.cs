using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "RPG/Corridor", order = 1)]
public class Corridor : ScriptableObject
{
    public GameObject CorridorEnterPrefab;

    public GameObject CorridorMiddlePrefab;

    public GameObject CorridorExitPrefab;

    [Range(0.0f, 1.0f)]
    public float SpawnProbability;
}