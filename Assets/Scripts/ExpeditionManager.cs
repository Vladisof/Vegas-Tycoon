using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class ExpeditionData
{
    public string name;
    public int requiredLevelsCompleted;
    public int duration; // in seconds
    public int goldReward;
    public bool isActive;
    public float startTime;
    public GameObject expeditionLocationObject; // scene object for this expedition
}

public class ExpeditionManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject expeditionPanel;
    public GameObject expeditionSelectionPanel; // panel with expedition list
    public Transform expeditionButtonParent; // parent object for expedition buttons
    public GameObject expeditionButtonPrefab; // expedition button prefab
    public GameObject lockedExpeditionPrefab; // locked expedition prefab
    public Button backToCharacterSelectionButton;
    
    [Header("Expedition Data")]
    public List<ExpeditionData> expeditions = new List<ExpeditionData>();
    
    [Header("Active Expedition UI")]
    public GameObject activeExpeditionUI;
    public TextMeshProUGUI activeExpeditionNameText;
    public TextMeshProUGUI activeExpeditionTimerText;
    public Button collectRewardButton;
    
    private ExpeditionData currentActiveExpedition;
    private GameManager gameManager;
    private MoneyController moneyController;
    
    void Start()
    {
        gameManager = GameManager.Instance;
        moneyController = FindObjectOfType<MoneyController>();
        
        // Initialize expeditions
        InitializeExpeditions();
        
        // Load saved expeditions ONLY if there are saves
        LoadExpeditionProgress();
        
        // Setup UI
        SetupUI();
        
        // Check active expeditions on load ONLY if something is loaded
        CheckActiveExpeditions();
    }
    
    void InitializeExpeditions()
    {
        // Clear list if not empty
        if (expeditions.Count == 0)
        {
            expeditions.Add(new ExpeditionData 
            { 
                name = "Small Expedition", 
                requiredLevelsCompleted = 0, 
                duration = 120, // 2 minutes
                goldReward = 100,
                isActive = false
            });
            
            expeditions.Add(new ExpeditionData 
            { 
                name = "Medium Expedition", 
                requiredLevelsCompleted = 2, 
                duration = 180, // 3 minutes
                goldReward = 200,
                isActive = false
            });
            
            expeditions.Add(new ExpeditionData 
            { 
                name = "Large Expedition", 
                requiredLevelsCompleted = 4, 
                duration = 300, // 5 minutes
                goldReward = 350,
                isActive = false
            });
            
            expeditions.Add(new ExpeditionData 
            { 
                name = "Legendary Expedition", 
                requiredLevelsCompleted = 6, 
                duration = 600, // 10 minutes
                goldReward = 500,
                isActive = false
            });
        }
    }
    
    void SetupUI()
    {
        if (backToCharacterSelectionButton != null)
        {
            backToCharacterSelectionButton.onClick.AddListener(BackToCharacterSelection);
        }
        
        if (collectRewardButton != null)
        {
            collectRewardButton.onClick.AddListener(CollectReward);
            // Initially show button but make it non-interactive
            collectRewardButton.gameObject.SetActive(true);
            collectRewardButton.interactable = false;
        }
        
        UpdateExpeditionButtons();
    }
    
    public void ShowExpeditionPanel()
    {
        expeditionPanel.SetActive(true);
        UpdateExpeditionButtons();
        UpdateActiveExpeditionUI();
    }
    
    public void HideExpeditionPanel()
    {
        expeditionPanel.SetActive(false);
    }
    
    void BackToCharacterSelection()
    {
        HideExpeditionPanel();
        // Return to character selection through GameManager
        if (gameManager != null)
        {
            gameManager.OpenSelectionPanel();
        }
    }
    
    void UpdateExpeditionButtons()
    {
        // Remove old buttons
        foreach (Transform child in expeditionButtonParent)
        {
            Destroy(child.gameObject);
        }
        
        int completedLevels = gameManager.GetCompletedLevels().Count;
        
        // Create buttons for available expeditions
        foreach (ExpeditionData expedition in expeditions)
        {
            if (completedLevels >= expedition.requiredLevelsCompleted)
            {
                CreateExpeditionButton(expedition);
            }
            else
            {
                CreateLockedExpeditionButton(expedition);
            }
        }
    }
    
    void CreateExpeditionButton(ExpeditionData expedition)
    {
        GameObject buttonObj = Instantiate(expeditionButtonPrefab, expeditionButtonParent);
        Button button = buttonObj.GetComponent<Button>();
        
        // Find text components of the button
        TextMeshProUGUI[] texts = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length > 0)
        {
            texts[0].text = expedition.name;
        }
        if (texts.Length > 1)
        {
            texts[1].text = $"Reward: {expedition.goldReward} gold";
        }
        if (texts.Length > 2)
        {
            texts[2].text = $"Time: {expedition.duration / 60} min";
        }
        
        // Check if expedition can be started
        bool canStart = !IsAnyExpeditionActive() && !expedition.isActive;
        button.interactable = canStart;
        
        if (expedition.isActive)
        {
            if (texts.Length > 0)
            {
                texts[0].text += " (In Progress)";
            }
        }
        else if (IsAnyExpeditionActive())
        {
            if (texts.Length > 0)
            {
                texts[0].text += " (Unavailable)";
            }
        }
        
        button.onClick.AddListener(() => StartExpedition(expedition));
    }
    
    void CreateLockedExpeditionButton(ExpeditionData expedition)
    {
        GameObject buttonObj = Instantiate(lockedExpeditionPrefab, expeditionButtonParent);
        Button button = buttonObj.GetComponent<Button>();
        
        // Find text components of the button
        TextMeshProUGUI[] texts = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length > 0)
        {
            texts[0].text = expedition.name;
        }
        if (texts.Length > 1)
        {
            texts[1].text = $"Required levels: {expedition.requiredLevelsCompleted}";
        }
        
        button.interactable = false;
    }
    
    public void StartExpedition(ExpeditionData expedition)
    {
        if (IsAnyExpeditionActive() || expedition.isActive)
        {
            return;
        }
        
        expedition.isActive = true;
        expedition.startTime = Time.time;
        currentActiveExpedition = expedition;
        
        // Save expedition start time in real time
        SaveExpeditionProgress(expedition);
        
        // Activate expedition location object
        if (expedition.expeditionLocationObject != null)
        {
            expedition.expeditionLocationObject.SetActive(true);
        }
        
        // Keep button visible but make it non-interactive
        if (collectRewardButton != null)
        {
            collectRewardButton.gameObject.SetActive(true);
            collectRewardButton.interactable = false;
        }
        
        StartCoroutine(ExpeditionTimer(expedition));
        
        UpdateExpeditionButtons();
        UpdateActiveExpeditionUI();
    }
    
    IEnumerator ExpeditionTimer(ExpeditionData expedition)
    {
        // Calculate remaining time
        float timeElapsed = Time.time - expedition.startTime;
        float timer = expedition.duration - timeElapsed;
        
        // If no time left, complete immediately
        if (timer <= 0)
        {
            CompleteExpedition(expedition);
            yield break;
        }
        
        while (timer > 0 && expedition.isActive)
        {
            timer -= Time.deltaTime;
            
            // Update timer UI
            UpdateTimerUI(timer);
            
            yield return null;
        }
        
        if (expedition.isActive)
        {
            // Expedition completed
            CompleteExpedition(expedition);
        }
    }
    
    void UpdateTimerUI(float timeLeft)
    {
        if (activeExpeditionTimerText != null && currentActiveExpedition != null)
        {
            int minutes = Mathf.FloorToInt(timeLeft / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);
            activeExpeditionTimerText.text = $"Remaining: {minutes:00}:{seconds:00}";
        }
    }
    
    void CompleteExpedition(ExpeditionData expedition)
    {
        // Show reward collection button and make it interactive
        if (collectRewardButton != null)
        {
            collectRewardButton.gameObject.SetActive(true);
            collectRewardButton.interactable = true;
        }
        
        UpdateActiveExpeditionUI();
    }
    
    public void CollectReward()
    {
        if (currentActiveExpedition == null) return;
        
        // Add gold
        if (moneyController != null)
        {
            moneyController.AddMoney(currentActiveExpedition.goldReward);
        }
        
        // Deactivate location object
        if (currentActiveExpedition.expeditionLocationObject != null)
        {
            currentActiveExpedition.expeditionLocationObject.SetActive(false);
        }
        
        // Reset expedition
        currentActiveExpedition.isActive = false;
        currentActiveExpedition = null;
        
        // Clear saved expedition data
        ClearExpeditionProgress();
        
        // Hide active expedition UI
        if (activeExpeditionUI != null)
        {
            activeExpeditionUI.SetActive(false);
        }
        
        if (collectRewardButton != null)
        {
            collectRewardButton.gameObject.SetActive(false);
        }
        
        UpdateExpeditionButtons();
    }
    
    void UpdateActiveExpeditionUI()
    {
        if (currentActiveExpedition != null && currentActiveExpedition.isActive)
        {
            if (activeExpeditionUI != null)
            {
                activeExpeditionUI.SetActive(true);
            }
            
            if (activeExpeditionNameText != null)
            {
                activeExpeditionNameText.text = $"Active: {currentActiveExpedition.name}";
            }
            
            // Check if expedition is completed
            float timeElapsed = Time.time - currentActiveExpedition.startTime;
            if (timeElapsed >= currentActiveExpedition.duration)
            {
                // Expedition completed - show button and make it interactive
                if (collectRewardButton != null)
                {
                    collectRewardButton.gameObject.SetActive(true);
                    collectRewardButton.interactable = true;
                }
                
                if (activeExpeditionTimerText != null)
                {
                    activeExpeditionTimerText.text = "Completed! Collect your reward";
                }
            }
            else
            {
                // Expedition still ongoing - show button but make it non-interactive
                if (collectRewardButton != null)
                {
                    collectRewardButton.gameObject.SetActive(true);
                    collectRewardButton.interactable = false;
                }
            }
        }
        else
        {
            if (activeExpeditionUI != null)
            {
                activeExpeditionUI.SetActive(false);
            }
            
            // When no active expedition - HIDE button completely
            if (collectRewardButton != null)
            {
                collectRewardButton.gameObject.SetActive(false);
            }
        }
    }
    
    bool IsAnyExpeditionActive()
    {
        foreach (ExpeditionData expedition in expeditions)
        {
            if (expedition.isActive)
            {
                // Check if time hasn't expired
                float timeElapsed = Time.time - expedition.startTime;
                if (timeElapsed < expedition.duration)
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    void CheckActiveExpeditions()
    {
        foreach (ExpeditionData expedition in expeditions)
        {
            if (expedition.isActive)
            {
                float timeElapsed = Time.time - expedition.startTime;
                if (timeElapsed < expedition.duration)
                {
                    currentActiveExpedition = expedition;
                    
                    // Activate location object
                    if (expedition.expeditionLocationObject != null)
                    {
                        expedition.expeditionLocationObject.SetActive(true);
                    }
                }
                else
                {
                    // Expedition should have completed already
                    CompleteExpedition(expedition);
                }
                break;
            }
        }
        
        UpdateActiveExpeditionUI();
    }
    
    void Update()
    {
        // Constantly update active expedition UI
        if (currentActiveExpedition != null && currentActiveExpedition.isActive)
        {
            float timeElapsed = Time.time - currentActiveExpedition.startTime;
            float timeLeft = currentActiveExpedition.duration - timeElapsed;
            
            if (timeLeft > 0)
            {
                UpdateTimerUI(timeLeft);
            }
            else
            {
                // Time expired - complete expedition
                CompleteExpedition(currentActiveExpedition);
            }
        }
    }
    
    // Methods for saving and loading expedition progress
    void SaveExpeditionProgress(ExpeditionData expedition)
    {
        // Save active expedition name
        PlayerPrefs.SetString("ActiveExpeditionName", expedition.name);
        
        // Save real start time of expedition
        string startTimeString = System.DateTime.Now.ToBinary().ToString();
        PlayerPrefs.SetString("ExpeditionStartTime", startTimeString);
        
        // Save expedition duration
        PlayerPrefs.SetInt("ExpeditionDuration", expedition.duration);
        
        PlayerPrefs.Save();
    }
    
    void LoadExpeditionProgress()
    {
        string savedExpeditionName = PlayerPrefs.GetString("ActiveExpeditionName", "");
        
        if (string.IsNullOrEmpty(savedExpeditionName))
        {
            return; // No saved expedition
        }
        
        // Get saved start time
        string startTimeString = PlayerPrefs.GetString("ExpeditionStartTime", "");
        if (string.IsNullOrEmpty(startTimeString))
        {
            return;
        }
        
        try
        {
            // Convert time back
            long startTimeBinary = System.Convert.ToInt64(startTimeString);
            System.DateTime startTime = System.DateTime.FromBinary(startTimeBinary);
            
            // Get expedition duration
            int expeditionDuration = PlayerPrefs.GetInt("ExpeditionDuration", 0);
            
            // Calculate how much time has passed since expedition start
            System.TimeSpan timeElapsed = System.DateTime.Now - startTime;
            double elapsedSeconds = timeElapsed.TotalSeconds;
            
            // Find the required expedition
            ExpeditionData savedExpedition = expeditions.Find(exp => exp.name == savedExpeditionName);
            if (savedExpedition != null)
            {
                if (elapsedSeconds >= expeditionDuration)
                {
                    // Expedition already completed - DON'T start timer again!
                    savedExpedition.isActive = true;
                    currentActiveExpedition = savedExpedition;
                    
                    // Set time so expedition is completed
                    savedExpedition.startTime = Time.time - expeditionDuration - 1; // -1 to ensure completion
                    
                    // Activate location object if needed
                    if (savedExpedition.expeditionLocationObject != null)
                    {
                        savedExpedition.expeditionLocationObject.SetActive(true);
                    }
                }
                else
                {
                    // Expedition still ongoing - restore correct time
                    savedExpedition.isActive = true;
                    currentActiveExpedition = savedExpedition;
                    
                    // Correctly calculate startTime for remaining time
                    savedExpedition.startTime = Time.time - (float)elapsedSeconds;
                    
                    // Activate location object
                    if (savedExpedition.expeditionLocationObject != null)
                    {
                        savedExpedition.expeditionLocationObject.SetActive(true);
                    }
                    
                    // Start timer for ongoing expedition
                    StartCoroutine(ExpeditionTimer(savedExpedition));
                }
            }
        }
        catch (System.Exception e)
        {
            // Clear corrupted data
            ClearExpeditionProgress();
        }
    }
    
    void ClearExpeditionProgress()
    {
        PlayerPrefs.DeleteKey("ActiveExpeditionName");
        PlayerPrefs.DeleteKey("ExpeditionStartTime");
        PlayerPrefs.DeleteKey("ExpeditionDuration");
        PlayerPrefs.Save();
    }
}
