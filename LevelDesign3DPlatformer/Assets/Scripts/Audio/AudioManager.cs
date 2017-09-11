using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    private static AudioManager instance;

    [SerializeField]
    private GameObject audioNodePrefab;

    private AudioNode currentAudioNode;

    public static AudioManager Instance {
        get { return instance; }
    }

    public void Awake() {
        if(instance != null) {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    
}
