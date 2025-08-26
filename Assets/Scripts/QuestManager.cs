using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class QuestData
{
    public string name;
    public int duration; // in seconds
    public int goldReward;
    public bool isActive;
    public float startTime;
    public bool isCompleted; // whether the quest is completed
    public bool rewardCollected; // whether the reward is collected
}

public class QuestManager : MonoBehaviour
{
    [Header("Quest UI References")]
    public Transform questButtonParent; // parent object for quest buttons
    public GameObject questButtonPrefab; // quest button prefab
    
    [Header("Active Quest UI")]
    public GameObject activeQuestUI;
    public TextMeshProUGUI activeQuestNameText;
    public TextMeshProUGUI activeQuestTimerText;
    public Button collectQuestRewardButton;
    
    [Header("Quest Refresh Timer UI")]
    public TextMeshProUGUI questRefreshTimerText; // timer until quest refresh
    
    private List<QuestData> currentQuests = new List<QuestData>();
    private QuestData currentActiveQuest;
    private MoneyController moneyController;
    
    // Quest names for random generation
    private string[] questNames = {
        "Treasure Hunt",
        "Territory Reconnaissance", 
        "Artifact Collection",
        "Monster Hunt",
        "Ruins Exploration",
        "Trade Mission",
        "Rescue Operation",
        "Border Patrol",
        "Resource Gathering",
        "Diplomatic Mission",
        "Caravan Guard",
        "Search for Missing",
        "Area Clearing",
        "Package Delivery",
        "Cave Exploration"
    };
    
    void Start()
    {
        moneyController = FindObjectOfType<MoneyController>();
        
        // Load quests or generate new ones
        LoadOrGenerateQuests();
        
        // Setup UI
        SetupUI();
        
        // Check active quests on load
        CheckActiveQuest();
        
        // Start UI update timer
        StartCoroutine(UpdateQuestRefreshTimer());
    }
    
    void SetupUI()
    {
        if (collectQuestRewardButton != null)
        {
            collectQuestRewardButton.onClick.AddListener(CollectQuestReward);
            collectQuestRewardButton.gameObject.SetActive(false);
        }
        
        UpdateQuestButtons();
    }
    
    void LoadOrGenerateQuests()
    {
        // Check if we need to generate new quests
        string lastQuestGenerationTime = PlayerPrefs.GetString("LastQuestGenerationTime", "");
        
        if (string.IsNullOrEmpty(lastQuestGenerationTime))
        {
            // First launch - generate quests
            GenerateNewQuests();
        }
        else
        {
            try
            {
                long lastGenerationBinary = System.Convert.ToInt64(lastQuestGenerationTime);
                System.DateTime lastGeneration = System.DateTime.FromBinary(lastGenerationBinary);
                System.TimeSpan timeSinceLastGeneration = System.DateTime.Now - lastGeneration;
                
                if (timeSinceLastGeneration.TotalHours >= 24)
                {
                    // 24 hours have passed - reset everything and generate new quests
                    ClearAllQuestProgress();
                    GenerateNewQuests();
                }
                else
                {
                    // Load existing quests
                    LoadExistingQuests();
                }
            }
            catch (System.Exception e)
            {
                GenerateNewQuests();
            }
        }
    }
    
    void GenerateNewQuests()
    {
        currentQuests.Clear();
        
        // Generate 3 random quests
        for (int i = 0; i < 3; i++)
        {
            QuestData newQuest = new QuestData();
            
            // Random name
            newQuest.name = questNames[Random.Range(0, questNames.Length)];
            
            // Random duration: 5 minutes, 30 minutes, 1 hour or 3 hours
            float[] durations = { 300f, 1800f, 3600f, 10800f }; // in seconds
            int[] rewards = { 150, 500, 1200, 4000 }; // corresponding rewards
            
            int durationIndex = Random.Range(0, durations.Length);
            newQuest.duration = (int)durations[durationIndex];
            newQuest.goldReward = rewards[durationIndex];
            
            // Add small randomness to reward (±20%)
            float rewardMultiplier = Random.Range(0.8f, 1.2f);
            newQuest.goldReward = Mathf.RoundToInt(newQuest.goldReward * rewardMultiplier);
            
            newQuest.isActive = false;
            newQuest.isCompleted = false;
            newQuest.rewardCollected = false;
            
            currentQuests.Add(newQuest);
        }
        
        // Save generation time
        SaveQuestGenerationTime();
        SaveQuests();
    }
    
    void LoadExistingQuests()
    {
        currentQuests.Clear();
        
        // Load quest count
        int questCount = PlayerPrefs.GetInt("QuestCount", 0);
        
        for (int i = 0; i < questCount; i++)
        {
            QuestData quest = new QuestData();
            quest.name = PlayerPrefs.GetString($"Quest{i}_Name", "");
            quest.duration = PlayerPrefs.GetInt($"Quest{i}_Duration", 300);
            quest.goldReward = PlayerPrefs.GetInt($"Quest{i}_Reward", 150);
            quest.isActive = PlayerPrefs.GetInt($"Quest{i}_IsActive", 0) == 1;
            quest.isCompleted = PlayerPrefs.GetInt($"Quest{i}_IsCompleted", 0) == 1;
            quest.rewardCollected = PlayerPrefs.GetInt($"Quest{i}_RewardCollected", 0) == 1;
            
            if (quest.isActive && !quest.rewardCollected)
            {
                // Load start time of active quest
                string startTimeString = PlayerPrefs.GetString($"Quest{i}_StartTime", "");
                
                if (!string.IsNullOrEmpty(startTimeString))
                {
                    try
                    {
                        long startTimeBinary = System.Convert.ToInt64(startTimeString);
                        System.DateTime startTime = System.DateTime.FromBinary(startTimeBinary);
                        System.TimeSpan timeElapsed = System.DateTime.Now - startTime;
                        double elapsedSeconds = timeElapsed.TotalSeconds;
                        
                        if (elapsedSeconds >= quest.duration)
                        {
                            // Quest is already completed - set correct time and mark as completed
                            quest.startTime = Time.time - quest.duration - 1; // -1 to ensure completion
                            quest.isCompleted = true;
                            currentActiveQuest = quest;
                        }
                        else
                        {
                            // Quest is still ongoing - correctly calculate startTime for remaining time
                            quest.startTime = Time.time - (float)elapsedSeconds;
                            currentActiveQuest = quest;
                        }
                    }
                    catch (System.Exception e)
                    {
                        quest.isActive = false;
                        quest.isCompleted = false;
                    }
                }
                else
                {
                    quest.isActive = false;
                    quest.isCompleted = false;
                }
            }
            
            currentQuests.Add(quest);
        }
        
        // Important: save updated quest state
        if (currentActiveQuest != null)
        {
            SaveQuests();
        }
    }
    
    void UpdateQuestButtons()
    {
        // Remove old buttons
        foreach (Transform child in questButtonParent)
        {
            Destroy(child.gameObject);
        }
        
        // Create buttons for quests
        for (int i = 0; i < currentQuests.Count; i++)
        {
            CreateQuestButton(currentQuests[i], i);
        }
    }
    
    void CreateQuestButton(QuestData quest, int index)
    {
        GameObject buttonObj = Instantiate(questButtonPrefab, questButtonParent);
        Button button = buttonObj.GetComponent<Button>();
        
        // Find text components of the button
        TextMeshProUGUI[] texts = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length > 0)
        {
            texts[0].text = quest.name;
        }
        if (texts.Length > 1)
        {
            texts[1].text = $"Reward: {quest.goldReward} gold";
        }
        if (texts.Length > 2)
        {
            int minutes = quest.duration / 60;
            if (minutes < 60)
            {
                texts[2].text = $"Time: {minutes} min";
            }
            else
            {
                int hours = minutes / 60;
                texts[2].text = $"Time: {hours} h";
            }
        }
        
        // Determine button state
        bool canStart = currentActiveQuest == null && !quest.isActive && !quest.rewardCollected;
        button.interactable = canStart;
        
        if (quest.isActive && quest == currentActiveQuest)
        {
            if (texts.Length > 0)
            {
                texts[0].text += " (In Progress)";
            }
        }
        else if (quest.rewardCollected)
        {
            if (texts.Length > 0)
            {
                texts[0].text += " (Completed)";
            }
        }
        else if (currentActiveQuest != null)
        {
            if (texts.Length > 0)
            {
                texts[0].text += " (Unavailable)";
            }
        }
        
        button.onClick.AddListener(() => StartQuest(quest));
    }
    
    public void StartQuest(QuestData quest)
    {
        if (currentActiveQuest != null || quest.isActive || quest.rewardCollected)
        {
            return;
        }
        
        quest.isActive = true;
        quest.startTime = Time.time;
        currentActiveQuest = quest;

        
        // Save progress IMMEDIATELY after state change
        SaveActiveQuestProgress(quest);
        SaveQuests(); // Important: save general quest state

        
        // Start timer
        StartCoroutine(QuestTimer(quest));
        
        UpdateQuestButtons();
        UpdateActiveQuestUI();

    }
    
    IEnumerator QuestTimer(QuestData quest)
    {
        float timeElapsed = Time.time - quest.startTime;
        float timer = quest.duration - timeElapsed;
        
        if (timer <= 0)
        {
            CompleteQuest(quest);
            yield break;
        }
        
        while (timer > 0 && quest.isActive && !quest.isCompleted)
        {
            timer -= Time.deltaTime;
            UpdateQuestTimerUI(timer);
            yield return null;
        }
        
        if (quest.isActive && !quest.isCompleted)
        {
            CompleteQuest(quest);
        }
    }
    
    void UpdateQuestTimerUI(float timeLeft)
    {
        if (activeQuestTimerText != null && currentActiveQuest != null)
        {
            int hours = Mathf.FloorToInt(timeLeft / 3600);
            int minutes = Mathf.FloorToInt((timeLeft % 3600) / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);
            
            if (hours > 0)
            {
                activeQuestTimerText.text = $"Remaining: {hours:00}:{minutes:00}:{seconds:00}";
            }
            else
            {
                activeQuestTimerText.text = $"Remaining: {minutes:00}:{seconds:00}";
            }
        }
    }
    
    void CompleteQuest(QuestData quest)
    {
        quest.isCompleted = true;
        
        // Show reward collection button
        if (collectQuestRewardButton != null)
        {
            collectQuestRewardButton.gameObject.SetActive(true);
            collectQuestRewardButton.interactable = true;
        }
        
        UpdateActiveQuestUI();
        SaveQuests();

    }
    
    public void CollectQuestReward()
    {
        if (currentActiveQuest == null || !currentActiveQuest.isCompleted) return;
        
        // Add gold
        if (moneyController != null)
        {
            moneyController.AddMoney(currentActiveQuest.goldReward);
        }
        
        // Mark reward as collected
        currentActiveQuest.rewardCollected = true;
        currentActiveQuest.isActive = false;
        
        // Hide active quest UI
        if (activeQuestUI != null)
        {
            activeQuestUI.SetActive(false);
        }
        
        if (collectQuestRewardButton != null)
        {
            collectQuestRewardButton.gameObject.SetActive(false);
        }
        
        currentActiveQuest = null;
        
        UpdateQuestButtons();
        SaveQuests();

    }
    
    void UpdateActiveQuestUI()
    {
        if (currentActiveQuest != null && currentActiveQuest.isActive)
        {
            if (activeQuestUI != null)
            {
                activeQuestUI.SetActive(true);
            }
            
            if (activeQuestNameText != null)
            {
                activeQuestNameText.text = $"Active: {currentActiveQuest.name}";
            }
            
            if (currentActiveQuest.isCompleted)
            {
                if (collectQuestRewardButton != null)
                {
                    collectQuestRewardButton.gameObject.SetActive(true);
                    collectQuestRewardButton.interactable = true;
                }
                
                if (activeQuestTimerText != null)
                {
                    activeQuestTimerText.text = "Completed! Collect your reward";
                }
            }
            else
            {
                if (collectQuestRewardButton != null)
                {
                    collectQuestRewardButton.gameObject.SetActive(true);
                    collectQuestRewardButton.interactable = false;
                }
            }
        }
        else
        {
            if (activeQuestUI != null)
            {
                activeQuestUI.SetActive(false);
            }
            
            if (collectQuestRewardButton != null)
            {
                collectQuestRewardButton.gameObject.SetActive(false);
            }
        }
    }
    
    void CheckActiveQuest()
    {
        // If active quest is already set in LoadExistingQuests, do not reset it
        if (currentActiveQuest != null)
        {
            
            if (!currentActiveQuest.isCompleted)
            {
                float timeElapsed = Time.time - currentActiveQuest.startTime;
                if (timeElapsed >= currentActiveQuest.duration)
                {
                    CompleteQuest(currentActiveQuest);
                }
                else
                {
                    // Start timer for ongoing quest
                    StartCoroutine(QuestTimer(currentActiveQuest));
                }
            }
        }
        else
        {
            // Find active quest if it was not set
            foreach (QuestData quest in currentQuests)
            {
                if (quest.isActive && !quest.rewardCollected)
                {
                    currentActiveQuest = quest;
                    Debug.Log($"Active quest found: {quest.name}");
                    
                    if (!quest.isCompleted)
                    {
                        float timeElapsed = Time.time - quest.startTime;
                        if (timeElapsed >= quest.duration)
                        {
                            CompleteQuest(quest);
                        }
                        else
                        {
                            StartCoroutine(QuestTimer(quest));
                        }
                    }
                    break;
                }
            }
        }
        
        UpdateActiveQuestUI();
    }
    
    IEnumerator UpdateQuestRefreshTimer()
    {
        while (true)
        {
            UpdateRefreshTimerUI();
            yield return new WaitForSeconds(1f);
        }
    }
    
    void UpdateRefreshTimerUI()
    {
        if (questRefreshTimerText == null) return;
        
        string lastGenerationTimeString = PlayerPrefs.GetString("LastQuestGenerationTime", "");
        if (string.IsNullOrEmpty(lastGenerationTimeString))
        {
            questRefreshTimerText.text = "New quests: soon";
            return;
        }
        
        try
        {
            long lastGenerationBinary = System.Convert.ToInt64(lastGenerationTimeString);
            System.DateTime lastGeneration = System.DateTime.FromBinary(lastGenerationBinary);
            System.DateTime nextGeneration = lastGeneration.AddHours(24);
            System.TimeSpan timeUntilRefresh = nextGeneration - System.DateTime.Now;
            
            if (timeUntilRefresh.TotalSeconds <= 0)
            {
                // Time's up - refresh quests
                ClearAllQuestProgress();
                GenerateNewQuests();
                UpdateQuestButtons();
                questRefreshTimerText.text = "Quests refreshed!";
            }
            else
            {
                int hours = (int)timeUntilRefresh.TotalHours;
                int minutes = timeUntilRefresh.Minutes;
                int seconds = timeUntilRefresh.Seconds;
                
                questRefreshTimerText.text = $"New quests in: {hours:00}:{minutes:00}:{seconds:00}";
            }
        }
        catch
        {
            questRefreshTimerText.text = "New quests: soon";
        }
    }
    
    void Update()
    {
        // Update active quest timer
        if (currentActiveQuest != null && currentActiveQuest.isActive && !currentActiveQuest.isCompleted)
        {
            float timeElapsed = Time.time - currentActiveQuest.startTime;
            float timeLeft = currentActiveQuest.duration - timeElapsed;
            
            if (timeLeft > 0)
            {
                UpdateQuestTimerUI(timeLeft);
            }
            else
            {
                CompleteQuest(currentActiveQuest);
            }
        }
    }
    
    // Save and load methods
    void SaveQuestGenerationTime()
    {
        string timeString = System.DateTime.Now.ToBinary().ToString();
        PlayerPrefs.SetString("LastQuestGenerationTime", timeString);
        PlayerPrefs.Save();
    }
    
    void SaveActiveQuestProgress(QuestData quest)
    {
        string startTimeString = System.DateTime.Now.ToBinary().ToString();
        
        // Find quest index
        int questIndex = currentQuests.IndexOf(quest);
        if (questIndex >= 0)
        {
            PlayerPrefs.SetString($"Quest{questIndex}_StartTime", startTimeString);
            PlayerPrefs.Save();
        }
    }
    
    void SaveQuests()
    {
        PlayerPrefs.SetInt("QuestCount", currentQuests.Count);
        
        for (int i = 0; i < currentQuests.Count; i++)
        {
            QuestData quest = currentQuests[i];
            PlayerPrefs.SetString($"Quest{i}_Name", quest.name);
            PlayerPrefs.SetInt($"Quest{i}_Duration", quest.duration);
            PlayerPrefs.SetInt($"Quest{i}_Reward", quest.goldReward);
            PlayerPrefs.SetInt($"Quest{i}_IsActive", quest.isActive ? 1 : 0);
            PlayerPrefs.SetInt($"Quest{i}_IsCompleted", quest.isCompleted ? 1 : 0);
            PlayerPrefs.SetInt($"Quest{i}_RewardCollected", quest.rewardCollected ? 1 : 0);
        }
        
        PlayerPrefs.Save();
    }
    
    public void ClearAllQuestProgress()
    {
        // Clear current quests
        currentActiveQuest = null;
        currentQuests.Clear();
        
        // Clear all saved quest data
        int questCount = PlayerPrefs.GetInt("QuestCount", 0);
        for (int i = 0; i < questCount; i++)
        {
            PlayerPrefs.DeleteKey($"Quest{i}_Name");
            PlayerPrefs.DeleteKey($"Quest{i}_Duration");
            PlayerPrefs.DeleteKey($"Quest{i}_Reward");
            PlayerPrefs.DeleteKey($"Quest{i}_IsActive");
            PlayerPrefs.DeleteKey($"Quest{i}_IsCompleted");
            PlayerPrefs.DeleteKey($"Quest{i}_RewardCollected");
            PlayerPrefs.DeleteKey($"Quest{i}_StartTime");
        }
        
        PlayerPrefs.DeleteKey("QuestCount");
        PlayerPrefs.Save();
    }
}
