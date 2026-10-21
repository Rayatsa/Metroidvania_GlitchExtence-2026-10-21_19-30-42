using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;

    [Header("Panels Utama")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject saveLoadPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Panels Status")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject congratsPanel;

    [Header("Save/Load References")]
    [SerializeField] private TextMeshProUGUI saveLoadTitleText;
    [SerializeField] private TextMeshProUGUI slot1Text;
    [SerializeField] private TextMeshProUGUI slot2Text;
    [SerializeField] private TextMeshProUGUI slot3Text;

    [Header("Options References")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle fullscreenToggle;

    private bool isPaused = false;
    private bool isGameOver = false;
    private bool isSavingMode = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        CloseAllPanels();

        if (musicSlider != null) musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        if (sfxSlider != null) sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        if (fullscreenToggle != null) fullscreenToggle.isOn = Screen.fullScreen;
    }

    private void Update()
    {
        // Mencegah pause saat player mati atau menang
        if (isGameOver) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // --- PAUSE & RESUME ---
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        CloseAllPanels();
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        CloseAllPanels();
    }

    private void CloseAllPanels()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (saveLoadPanel != null) saveLoadPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (congratsPanel != null) congratsPanel.SetActive(false);
    }

    // --- GAME OVER & CONGRATS ---
    public void ShowGameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        CloseAllPanels();
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void ShowCongrats()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        CloseAllPanels();
        if (congratsPanel != null) congratsPanel.SetActive(true);
    }

    public void RestartCurrentStage()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- NAVIGASI SUB-MENU ---
    public void OpenSaveMenu()
    {
        isSavingMode = true;
        if (saveLoadTitleText != null) saveLoadTitleText.text = "SIMPAN GAME";
        UpdateSlotUI();
        if (pausePanel != null) pausePanel.SetActive(false);
        if (saveLoadPanel != null) saveLoadPanel.SetActive(true);
    }

    public void OpenLoadMenu()
    {
        isSavingMode = false;
        if (saveLoadTitleText != null) saveLoadTitleText.text = "MUAT GAME";
        UpdateSlotUI();
        if (pausePanel != null) pausePanel.SetActive(false);
        if (saveLoadPanel != null) saveLoadPanel.SetActive(true);
    }

    public void OpenOptionsMenu()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        CloseAllPanels();
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void LoadMainMenuScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // --- LOGIKA SAVE & LOAD SLOT ---
    private void UpdateSlotUI()
    {
        UpdateSingleSlotUI(1, slot1Text);
        UpdateSingleSlotUI(2, slot2Text);
        UpdateSingleSlotUI(3, slot3Text);
    }

    private void UpdateSingleSlotUI(int slotIndex, TextMeshProUGUI slotText)
    {
        if (slotText == null) return;

        if (PlayerPrefs.HasKey("SaveSlot_" + slotIndex + "_Stage"))
        {
            string timeSaved = PlayerPrefs.GetString("SaveSlot_" + slotIndex + "_Time", "00:00");
            slotText.text = "SLOT " + slotIndex + "\n" + timeSaved;
        }
        else
        {
            slotText.text = "SLOT " + slotIndex + "\nKOSONG";
        }
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (isSavingMode)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            string currentTime = System.DateTime.Now.ToString("mm:ss");

            PlayerPrefs.SetString("SaveSlot_" + slotIndex + "_Stage", currentSceneName);
            PlayerPrefs.SetString("SaveSlot_" + slotIndex + "_Time", currentTime);
            PlayerPrefs.SetInt("HasSaveData", 1);
            PlayerPrefs.SetInt("LastUsedSlot", slotIndex);
            PlayerPrefs.Save();

            UpdateSlotUI();
        }
        else
        {
            if (PlayerPrefs.HasKey("SaveSlot_" + slotIndex + "_Stage"))
            {
                string targetScene = PlayerPrefs.GetString("SaveSlot_" + slotIndex + "_Stage");
                Time.timeScale = 1f;
                SceneManager.LoadScene(targetScene);
            }
        }
    }

    // --- LOGIKA OPSI ---
    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        AudioListener.volume = value;
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}