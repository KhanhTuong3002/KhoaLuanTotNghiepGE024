using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuController : MonoBehaviour
{
    public Button startButton;
    public Button tutorialButton;
    public Button creditButton;
    public Button exitButton;
    public Button backButton1;
    public Button settingButton;
    public Button volumeUpButton;
    public Button volumeDownButton;
    public GameObject MainPanel;
    public GameObject creditsPanel;
    public GameObject settingPanel;

    private string tutorialURL = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

    void Start()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        tutorialButton.onClick.AddListener(OnTutorialButtonClicked);
        creditButton.onClick.AddListener(OnCreditButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
        settingButton.onClick.AddListener(OnSettingButtonClicked);
        backButton1.onClick.AddListener(OnBackButtonClicked);
        volumeUpButton.onClick.AddListener(OnVolumeUpButtonClicked);
        volumeDownButton.onClick.AddListener(OnVolumeDownButtonClicked);

        creditsPanel.SetActive(false);
        settingPanel.SetActive(false);
    }

    void OnStartButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void OnSettingButtonClicked()
    {
        MainPanel.SetActive(false);
        settingPanel.SetActive(true);
    }

    void OnBackButtonClicked()
    {
        settingPanel.SetActive(false);
        creditsPanel.SetActive(false);
        MainPanel.SetActive(true);
    }

    void OnTutorialButtonClicked()
    {
        Application.OpenURL(tutorialURL);
    }

    void OnCreditButtonClicked()
    {
        creditsPanel.SetActive(true);
    }

    void OnExitButtonClicked()
    {
        Application.Quit();
    }

    void OnVolumeUpButtonClicked()
    {
        float newVolume = Mathf.Clamp(AudioPlayer.BGMVolume + 0.1f, 0f, 1f);
        AudioPlayer.BGMVolume = newVolume;
        ApplyVolumeToAllAudioPlayers();
    }

    void OnVolumeDownButtonClicked()
    {
        float newVolume = Mathf.Clamp(AudioPlayer.BGMVolume - 0.1f, 0f, 1f);
        AudioPlayer.BGMVolume = newVolume;
        ApplyVolumeToAllAudioPlayers();
    }

    void ApplyVolumeToAllAudioPlayers()
    {
        AudioPlayer[] audioPlayers = FindObjectsOfType<AudioPlayer>();
        foreach (AudioPlayer audioPlayer in audioPlayers)
        {
            audioPlayer.SetBGMVolume(AudioPlayer.BGMVolume);
        }
    }
}
