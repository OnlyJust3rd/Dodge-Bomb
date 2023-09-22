using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int id;

    private void OnDestroy()
    {
        GameManager.instance.itemSpawnOccupied[id] = false;
    }
}
