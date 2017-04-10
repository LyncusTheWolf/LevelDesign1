using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour {

    public const int HEALTH_PER_ROW = 8;
    public const int HEALTH_ROWS = GameManager.MAX_PLAYER_HEALTH / HEALTH_PER_ROW;

    [SerializeField]
    private Text coinText;
    [SerializeField]
    private Text keyText;
    [SerializeField]
    private Text livesText;
    [SerializeField]
    private GridLayoutGroup healthPanelgroup;
    [SerializeField]
    private GameObject healthElemPrefab;

    private HeartUIElem[] heartElems;

    private Player playerRef;

    private void Awake() {
        RectTransform rect = healthPanelgroup.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(healthPanelgroup.cellSize.x * HEALTH_PER_ROW + healthPanelgroup.spacing.x * (HEALTH_PER_ROW - 1) + healthPanelgroup.padding.left + healthPanelgroup.padding.right,
            healthPanelgroup.cellSize.y * HEALTH_ROWS + healthPanelgroup.spacing.y * (HEALTH_ROWS - 1) + healthPanelgroup.padding.bottom + healthPanelgroup.padding.top);
        heartElems = new HeartUIElem[GameManager.MAX_PLAYER_HEALTH];

        for (int i = 0; i < GameManager.MAX_PLAYER_HEALTH; i++) {
            GameObject go = Instantiate(healthElemPrefab, healthPanelgroup.transform);
            heartElems[i] = go.GetComponent<HeartUIElem>();
        }
    }

	// Use this for initialization
	void Start () {
        playerRef = Player.Instance;
        playerRef.onDamage += UpdateHealth;
        playerRef.onReset += UpdateHealth;
        UpdateHealth();
        GameManager.Instance.gameOverEvent += GameOverTransition;
    }

    private void OnDisable() {
        GameManager.Instance.gameOverEvent -= GameOverTransition;
    }

    // Update is called once per frame
    void Update () {
        coinText.text = "x " + playerRef.Coins;
        keyText.text = "x " + playerRef.KeyCount;
        livesText.text = "x " + GameManager.Instance.PlayerLives;
	}

    public void UpdateHealth() {
        for (int i = 0; i < GameManager.MAX_PLAYER_HEALTH; i++) {
            if(i >= playerRef.MaxHealth) {
                heartElems[i].gameObject.SetActive(false);
            } else {
                heartElems[i].gameObject.SetActive(true);
                if(i >= playerRef.CurrentHealth) {
                    heartElems[i].SetHealthEmpty();
                } else {
                    heartElems[i].SetHealthFilled();
                }
            }
        }
    }

    public void GameOverTransition() {
        StartCoroutine(GameOverInternal());
    }

    public IEnumerator GameOverInternal() {
        yield return new WaitForSeconds(3.0f);
        LevelManager.Instance.LoadGameOverScreen();
    }
}
