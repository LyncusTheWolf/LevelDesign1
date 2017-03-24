using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour {

    public const int HEALTH_PER_ROW = 8;
    public const int HEALTH_ROWS = Player.MAX_PLAYER_HEALTH / HEALTH_PER_ROW;

    [SerializeField]
    private Text coinText;
    [SerializeField]
    private Text keyText;
    [SerializeField]
    private GridLayoutGroup healthPanelgroup;

    private Player playerRef;

    private void Awake() {
        RectTransform rect = healthPanelgroup.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(healthPanelgroup.cellSize.x * HEALTH_PER_ROW + healthPanelgroup.spacing.x * (HEALTH_PER_ROW - 1) + healthPanelgroup.padding.left + healthPanelgroup.padding.right,
            healthPanelgroup.cellSize.y * HEALTH_ROWS + healthPanelgroup.spacing.y * (HEALTH_ROWS - 1) + healthPanelgroup.padding.bottom + healthPanelgroup.padding.top);
    }

	// Use this for initialization
	void Start () {
        playerRef = Player.Instance;
    }
	
	// Update is called once per frame
	void Update () {
        coinText.text = "x " + playerRef.Coins;
        keyText.text = "x " + playerRef.KeyCount;
	}
}
