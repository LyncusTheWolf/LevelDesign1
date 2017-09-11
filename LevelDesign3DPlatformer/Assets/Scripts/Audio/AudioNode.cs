using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioNode : MonoBehaviour {

    private AudioSource nodeAudio;

    public AudioSource Audio {
        get { return nodeAudio; }
    }

    public void OnValidate() {
        nodeAudio = GetComponent<AudioSource>();
    }

    [ContextMenu("Validate Sound Node")]
    private void ValidateSoundNode() {
        nodeAudio = GetComponent<AudioSource>();
    }
}
