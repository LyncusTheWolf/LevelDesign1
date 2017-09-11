using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Driver : MonoBehaviour {

    private static UI_Driver instance;

    public delegate void OnExit();
    public delegate void OnFade();

    public event OnExit onExit;

    [SerializeField]
    private Transform viewerNode;

    private ScreenSet currentScreenSet;
    private UI_Panel activeUIPanel;

    [SerializeField]
    private Image fadeImage;

    public static UI_Driver Instance {
        get {
            #if UNITY_EDITOR
            if (instance == null) {
                Debug.Log("Scene does not contain a UI_Driver but an object is trying to access it. Remove the object trying to access a UI_Driver or add one to the scene");
            }

            return instance;
            #endif
        }
    }

    private void Awake() {
        if(instance != null && instance != this) {
            Destroy(this.gameObject);
            Debug.Log("This scene already contains a UI_Driver, merge over all UI elements into a single driver");
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadPanelSet(ScreenSet newSet) {
        if(currentScreenSet != null) {
            Destroy(currentScreenSet.gameObject);
        }
        
        newSet.transform.SetParent(viewerNode);
        currentScreenSet = newSet;
        activeUIPanel = newSet.InitialPanel;
        RectTransform screenRect = newSet.GetComponent<RectTransform>();
        screenRect.anchorMin = new Vector2(0, 0);
        screenRect.anchorMax = new Vector2(1, 1);
        screenRect.pivot = new Vector2(0.5f, 0.5f);
        screenRect.offsetMin = new Vector2(0.0f, 0.0f);
        screenRect.offsetMax = new Vector2(0.0f, 0.0f);
        newSet.InitialPanel.Init(this);
    }

    public void ToggleUIPanel(UI_Panel newPanel) {
        activeUIPanel.Close();
        newPanel.Init(this);
        activeUIPanel = newPanel;
    }

    public void FadeOut(float duration, OnFade onFadeCB) {
        StartCoroutine(FadeOutInternal(duration, onFadeCB));
    }

    private IEnumerator FadeOutInternal(float duration, OnFade onFadeCB) {
        float currentDuration = 0.0f;

        while(currentDuration < duration) {
            fadeImage.color = new Color(0.0f, 0.0f, 0.0f, currentDuration/duration);
            currentDuration += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        onFadeCB();
    }

    public void FadeIn(float duration, OnFade onFadeCB) {
        StartCoroutine(FadeInInternal(duration, onFadeCB));
    }

    private IEnumerator FadeInInternal(float duration, OnFade onFadeCB) {
        float currentDuration = 0.0f;

        while (currentDuration < duration) {
            fadeImage.color = new Color(0.0f, 0.0f, 0.0f, 1 - (currentDuration / duration));
            currentDuration += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        onFadeCB();
    }
}
