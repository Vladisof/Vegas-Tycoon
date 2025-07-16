using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelHistoryManager : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public GameObject levelHistoryUIPrefab; // Prefab for completed level display
    public Transform levelHistoryParent; // Layout group for history items

    [Header("UI")]
    public GameObject levelHistoryPanel;

    void Start()
    {
        // Автоматично знайти GameManager якщо не призначено
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }
    }

    public void ShowLevelHistory()
{
    // Clear existing history items
    foreach (Transform child in levelHistoryParent)
    {
        Destroy(child.gameObject);
    }

    var completedLevels = gameManager.GetCompletedLevels();

    if (completedLevels.Count == 0)
    {
        CreateNoLevelsMessage();
        return;
    }

    foreach (int levelIndex in completedLevels)
    {
        CreateLevelHistoryItem(levelIndex);
    }
}

private void CreateLevelHistoryItem(int levelIndex)
{
    
    if (levelIndex >= gameManager.levels.Count) 
    {
        return;
    }

    var levelData = gameManager.levels[levelIndex];
    int stars = gameManager.GetLevelStars(levelIndex);


    var historyItem = Instantiate(levelHistoryUIPrefab, levelHistoryParent);

    // Set level name
    var nameText = historyItem.transform.Find("LevelNameText").GetComponent<TextMeshProUGUI>();
    if (nameText == null)
    {
        return;
    }
    nameText.text = $"Level {levelIndex + 1}: {levelData.levelName}";

    // Set stars display
    var starsText = historyItem.transform.Find("StarsText").GetComponent<TextMeshProUGUI>();
    if (starsText == null)
    {
        return;
    }
    
    string starsDisplay = "";
    for (int i = 0; i < stars; i++)
    {
        starsDisplay += "⭐";
    }
    for (int i = stars; i < 3; i++)
    {
        starsDisplay += "☆";
    }
    starsText.text = starsDisplay;

}

    private void CreateNoLevelsMessage()
    {
        var messageItem = Instantiate(levelHistoryUIPrefab, levelHistoryParent);
        
        var nameText = messageItem.transform.Find("LevelNameText").GetComponent<TextMeshProUGUI>();
        nameText.text = "Жодного рівня ще не пройдено";
        
        var starsText = messageItem.transform.Find("StarsText").GetComponent<TextMeshProUGUI>();
        starsText.text = "";
        
        var requirementsText = messageItem.transform.Find("RequirementsText")?.GetComponent<TextMeshProUGUI>();
        if (requirementsText != null)
        {
            requirementsText.text = "Пройдіть рівні щоб побачити історію";
        }
    }

    public void OpenLevelHistory()
    {
        levelHistoryPanel.SetActive(true);
        ShowLevelHistory();
    }

    public void CloseLevelHistory()
    {
        levelHistoryPanel.SetActive(false);
    }
}