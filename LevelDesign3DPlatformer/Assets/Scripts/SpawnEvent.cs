using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEvent : MonoBehaviour {

    [SerializeField]
    private List<GameObject> spawnSet;

    public void SpawnAll() {
        foreach(GameObject go in spawnSet) {
            go.SetActive(true);
        }
    }
}
