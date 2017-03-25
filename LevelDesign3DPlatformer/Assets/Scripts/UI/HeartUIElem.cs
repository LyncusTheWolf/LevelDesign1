using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartUIElem : MonoBehaviour {

    [SerializeField]
    private GameObject filledImage;
    [SerializeField]
    private GameObject emptyImage;

    public void SetHealthFilled() {
        filledImage.SetActive(true);
        emptyImage.SetActive(false);
    }

    public void SetHealthEmpty() {
        filledImage.SetActive(false);
        emptyImage.SetActive(true);
    }
}
