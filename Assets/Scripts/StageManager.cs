using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public static StageManager instance;

    [Header("Stage Settings")]
    [SerializeField] private string nextStageName = "Stage2"; // Nama Scene Stage 2 yang dikerjakan teman Anda
    [SerializeField] private float transitionDelay = 1.5f;   // Jeda detik sebelum pindah stage

    private int currentEnemyCount;

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
        // Otomatis menghitung berapa banyak objek di scene yang memiliki Tag "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        currentEnemyCount = enemies.Length;
        Debug.Log("Total Musuh di Stage 1: " + currentEnemyCount);
    }

    public void EnemyKilled()
    {
        currentEnemyCount--;
        Debug.Log("Musuh terbunuh! Sisa musuh: " + currentEnemyCount);

        // Jika semua musuh di stage sudah habis terbunuh
        if (currentEnemyCount <= 0)
        {
            Debug.Log("Semua musuh kalah! Memulai perpindahan ke " + nextStageName);
            Invoke(nameof(LoadNextStage), transitionDelay);
        }
    }

    private void LoadNextStage()
    {
        // Memuat scene Stage 2
        SceneManager.LoadScene(nextStageName);
    }
}