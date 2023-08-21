using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects", order = 1)]
public class GameData : ScriptableObject
{
    // coins
    public int totalCoins;
    public int currentCollectedCoins;

    // Deaths
    public int currentDeaths;

    // depth
    public float bestDepth;
    public float currentDepth;
    public float maxDepth;
    public float minDepth;

    // gameTime
    public float hou;
    public float min;
    public float sec;
}
