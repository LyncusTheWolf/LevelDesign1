using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteUI : MonoBehaviour {

    [SerializeField]
    public float tallyTime;

    [SerializeField]
    private Text timerText;
    [SerializeField]
    public Text secretsResults;

	// Use this for initialization
	void Start () {
        StartCoroutine(TallyInternal());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public IEnumerator TallyInternal() {
        yield return new WaitForSeconds(1.5f);

        timerText.enabled = true;

        timerText.text = "Time taken:\n     " + GameManager.Instance.GetTimeFormatted();

        yield return new WaitForSeconds(1.0f);

        secretsResults.enabled = true;

        for (int i = 0; i < 30; i++) {
            secretsResults.text = "Secrets found:\n     " + Random.Range(0, 9) + " / " + GameManager.Instance.SecretsOnLevel;
            yield return new WaitForSeconds(tallyTime / 30.0f);
        }

        secretsResults.text = "Secrets found:\n     " + GameManager.Instance.SecretsFound + " / " + GameManager.Instance.SecretsOnLevel;

        yield return new WaitForSeconds(2.0f);

        while (true) {
            if (Input.GetButtonDown("Jump")) {
                LevelManager.Instance.LoadStartScreen();
                break;
            }
            yield return null;
        }
    }
}
