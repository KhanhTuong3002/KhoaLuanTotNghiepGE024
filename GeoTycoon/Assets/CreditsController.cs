using UnityEngine;
using UnityEngine.UI;

public class CreditsController : MonoBehaviour
{
    public RectTransform creditsText;
    public Button backButton2;
    public float scrollSpeed = 50f;
    public GameObject creditsPanel;

    private Vector2 initialPosition;
    private float endPositionY;

    void Start()
    {
        backButton2.onClick.AddListener(OnBackButtonClicked);
        initialPosition = creditsText.anchoredPosition;
        endPositionY = creditsText.rect.height + Screen.height;
    }
    void OnBackButtonClicked()
    {
        creditsPanel.SetActive(false);
        creditsText.anchoredPosition = initialPosition;
    }
    void Update()
    {
        if (creditsText.anchoredPosition.y < endPositionY)
        {
            creditsText.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
        }
        else
        {
            creditsText.anchoredPosition = initialPosition;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            creditsPanel.SetActive(false);
        }
    }
}
