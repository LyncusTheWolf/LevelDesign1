using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverUI : MonoBehaviour {

    [SerializeField]
    public GameObject continueIcon;

    // Use this for initialization
    void Start () {
        StartCoroutine(GameOverInternal());
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public IEnumerator GameOverInternal() {
        yield return new WaitForSeconds(3.0f);

        continueIcon.SetActive(true);

        while (true) {
            if (Input.GetButtonDown("Jump")) {
                GameManager.Instance.LoadStartScreen();
                break;
            }
            yield return null;
        }
    }
}
