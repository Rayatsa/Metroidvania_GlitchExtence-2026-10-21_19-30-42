using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainButtonsPanel;
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Continue Button")]
    [SerializeField] private Button continueButton;

    [Header("Load Slot References")]
    [SerializeField] private TextMeshProUGUI slot1Text;
    [SerializeField] private TextMeshProUGUI slot2Text;
    [SerializeField] private TextMeshProUGUI slot3Text;

    private void Start()
    {
        if (continueButton != null)
        {
            continueButton.interactable = PlayerPrefs.HasKey("HasSaveData");
        }

        BackToMain();
    }

    public void OnContinueClicked()
    {
        int lastSlot = PlayerPrefs.GetInt("LastUsedSlot", 1);
        if (PlayerPrefs.HasKey("SaveSlot_" + lastSlot + "_Stage"))
        {
            string targetStage = PlayerPrefs.GetString("SaveSlot_" + lastSlot + "_Stage");
            SceneManager.LoadScene(targetStage);
        }
    }

    public void OnNewGameClicked()
    {
        SceneManager.LoadScene("Stage1");
    }

    public void OpenLoadPanel()
    {
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);
        if (loadPanel != null) loadPanel.SetActive(true);
        UpdateSlotUI();
    }

    public void OpenOptionsPanel()
    {
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void BackToMain()
    {
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
        if (loadPanel != null) loadPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (PlayerPrefs.HasKey("SaveSlot_" + slotIndex + "_Stage"))
        {
            string targetStage = PlayerPrefs.GetString("SaveSlot_" + slotIndex + "_Stage");
            SceneManager.LoadScene(targetStage);
        }
    }

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

    public void ExitGame()
    {
        Application.Quit();

        Debug.Log("Keluar Game...");

        #if UNITY_EDITOR
        // Menghentikan Play Mode saat testing di Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // Mematikan aplikasi saat dijalankan di Build (.exe)
        Application.Quit();
        #endif
    }
}