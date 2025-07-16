using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public LevelHistoryManager levelHistoryManager;
    public PerformersManager performersManager;
    public PerformersSelectionManager performersSelectionManager;
    public MoneyController moneyController;
    public LevelChoiceManager levelChoiceManager;
    public AudioManager audioManager;

    [Header("Levels")]
    public List<LevelData> levels = new List<LevelData>();
    public int currentLevelIndex = 0;
    private HashSet<int> completedLevels = new HashSet<int>();
    
    [Header("Level History")]
    private Dictionary<int, int> levelStars = new Dictionary<int, int>();
    
    [Header("UI")]
    public TextMeshProUGUI currentLevelText;
    public TextMeshProUGUI currentLevelTextIntro;
    public TextMeshProUGUI levelIntroText;

    public TextMeshProUGUI flavorTextField;
    public TextMeshProUGUI summaryTextField;
    public GameObject summaryPL;
    public TextMeshProUGUI moneyTextField;
    public TextMeshProUGUI starsTextField;

    [Header("Gameplay")]
    public List<PerformerData> chosenPerformers = new List<PerformerData>();
    public GameObject stars0TextPrefab;
    public GameObject stars1TextPrefab;
    public GameObject stars2TextPrefab;
    public GameObject stars3TextPrefab;
    public GameObject performersSelectionPanel;
    public GameObject simulationPanel;
    public GameObject selectLevelPanel;
    public GameObject MenuPanel;
    public GameObject introPanel;
    

    void Awake()
    {
        Instance = this;
        // Завантаження рівнів можна організувати через JSON або створити в інспекторі
    }
    void Start()
    {
        levels = GetPredefinedLevels();
        LoadProgress();
        SetCurrentLevelBasedOnProgress();
        if (levels.Count == 0)
        {
            Debug.LogError("No levels defined!");
            return;
        }
        if (completedLevels.Count == 0)
        {
            currentLevelIndex = 0;
        }
        currentLevelText.text = GetCurrentLevelText();
    }
    private void SetCurrentLevelBasedOnProgress()
    {
        if (completedLevels.Count == 0)
        {
            // Якщо жодного рівня не пройдено, починаємо з першого
            currentLevelIndex = 0;
        }
        else
        {
            // Знаходимо наступний непройдений рівень
            int nextLevel = 0;
            for (int i = 0; i < levels.Count; i++)
            {
                if (!completedLevels.Contains(i))
                {
                    nextLevel = i;
                    break;
                }
            }
        
            // Якщо всі рівні пройдені, залишаємося на останньому
            if (nextLevel == 0 && completedLevels.Contains(0))
            {
                currentLevelIndex = levels.Count - 1;
            }
            else
            {
                currentLevelIndex = nextLevel;
            }
        }
    }
    public void OpenSelectionPanel()
    {
        performersSelectionPanel.SetActive(true);
        performersSelectionManager.ShowPerformers(performersManager.performers);
    }
    
    public void StartLoading()
    {
        StartCoroutine(Loading());
    }
    
    private System.Collections.IEnumerator Loading()
    {
        MenuPanel.gameObject.SetActive(false);
        introPanel.gameObject.SetActive(true);
        // Load and display the current level intro
        LoadAndDisplayCurrentLevelIntro();
        StartSelection();
        // Initialize the current level text
        currentLevelText.text = GetCurrentLevelText();
        yield return 0;
    }
    
public void LoadAndDisplayCurrentLevelIntro()
{
    if (levels == null || levels.Count == 0 || currentLevelIndex >= levels.Count)
    {
        Debug.LogError("No valid level data available");
        return;
    }

    // Get current level name

    string currentLevelName = levels[currentLevelIndex].levelName;

    TextAsset textAsset = Resources.Load<TextAsset>("mr_vegas_level_intros");
    if (textAsset == null)
    {
        Debug.LogError("Level intros JSON file not found in Resources!");
        return;
    }

    string jsonContent = textAsset.text;

    // Parse each JSON object manually since JsonUtility can't handle arrays directly
    LevelIntroData currentLevelIntro = null;
    
    try
    {
        // Remove the array brackets and split by objects
        jsonContent = jsonContent.Trim();
        if (jsonContent.StartsWith("["))
            jsonContent = jsonContent.Substring(1);
        if (jsonContent.EndsWith("]"))
            jsonContent = jsonContent.Substring(0, jsonContent.Length - 1);

        // Split by objects (look for },{ pattern)
        string[] objects = jsonContent.Split(new string[] { "}," }, System.StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string obj in objects)
        {
            string cleanObj = obj.Trim();
            if (!cleanObj.StartsWith("{"))
                cleanObj = "{" + cleanObj;
            if (!cleanObj.EndsWith("}"))
                cleanObj = cleanObj + "}";

            try
            {
                LevelIntroData intro = JsonUtility.FromJson<LevelIntroData>(cleanObj);
                if (intro.levelName == currentLevelName)
                {
                    currentLevelIntro = intro;
                    break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to parse intro object: {e.Message}");
                continue;
            }
        }
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Failed to parse JSON: {e.Message}");
        return;
    }

    if (currentLevelIntro != null)
    {
        // Update UI elements
        if (currentLevelTextIntro != null)
            currentLevelTextIntro.text = currentLevelName;

        if (levelIntroText != null)
            levelIntroText.text = currentLevelIntro.intro;
    }
    else
    {
        Debug.LogWarning($"No intro found for level: {currentLevelName}");
    }
}
    
    public void SetCurrentLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levels.Count)
        {
            currentLevelIndex = levelIndex;
            currentLevelText.text = GetCurrentLevelText();
        }
    }
    
    public void StartSelection()
    {
        performersSelectionManager.ShowPerformers(performersManager.performers);
    }

    // --- ВИКЛИКАЙ КОЛИ ГРАВЕЦЬ НАЗНАЧИВ УСІХ ПЕРСОНАЖІВ І НАТИСНУВ "ПОЧАТИ" ---
    public void StartShow()
    {
        LevelData level = levels[currentLevelIndex];
        var result = SimulateShow(level, chosenPerformers);
        performersSelectionPanel.SetActive(false);
        simulationPanel.SetActive(true);

        StartCoroutine(ShowResultsSequentially(result));

        if (result.success)
        {
            Debug.Log("Level complete! Stars: " + result.stars + ", Money: " + result.moneyEarned);

            // Save level result with stars
            SaveLevelResult(currentLevelIndex, result.stars);

            // Notify LevelChoiceManager
            if (levelChoiceManager != null)
            {
                levelChoiceManager.OnLevelCompleted(currentLevelIndex);
            }
        }
        else
        {
            Debug.Log("Level failed. Try different performers or upgrades.");
        }

        performersSelectionManager.ClearSelection();
    }
    
    private System.Collections.IEnumerator ShowResultsSequentially(ShowResult result)
    {
        // Initially hide all text fields
        flavorTextField.gameObject.SetActive(false);
        summaryTextField.gameObject.SetActive(false);
        moneyTextField.gameObject.SetActive(false);

        // Show flavor text for 3 seconds
        audioManager.PlaySound(1);
        flavorTextField.text = result.flavorText;
        flavorTextField.gameObject.SetActive(true);
        summaryPL.gameObject.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        flavorTextField.gameObject.SetActive(false);
        audioManager.PlaySound(1);

        // Show summary text for 3 seconds
        summaryTextField.text = result.summaryText;
        summaryTextField.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(3f);
        summaryTextField.gameObject.SetActive(false);
        summaryPL.gameObject.SetActive(false);

        // Show stars earned for 2 seconds
        if (result.stars == 3) stars3TextPrefab.gameObject.SetActive(true);
        else if (result.stars == 2) stars2TextPrefab.gameObject.SetActive(true);
        else if (result.stars == 1) stars1TextPrefab.gameObject.SetActive(true);
        else stars0TextPrefab.gameObject.SetActive(true);
        yield return new WaitForSeconds(4f);
        if (result.stars == 3) stars3TextPrefab.gameObject.SetActive(false);
        else if (result.stars == 2) stars2TextPrefab.gameObject.SetActive(false);
        else if (result.stars == 1) stars1TextPrefab.gameObject.SetActive(false);
        else stars0TextPrefab.gameObject.SetActive(false);
        starsTextField.gameObject.SetActive(false);

        // Show money text and add money
        moneyTextField.text = $"+{result.moneyEarned}$";
        audioManager.PlaySound(1);
        moneyTextField.gameObject.SetActive(true);
        moneyController.AddMoney(result.moneyEarned);
        yield return new WaitForSeconds(3f);
        moneyTextField.gameObject.SetActive(false);
        audioManager.PlaySound(1);
    
        simulationPanel.SetActive(false);
        selectLevelPanel.SetActive(true);
    }

    // --- Основна логіка симуляції шоу і оцінки ---
   public ShowResult SimulateShow(LevelData level, List<PerformerData> cast)
{
    var allRoles = new HashSet<string>(cast.Select(x => x.role));
    var allTags = new HashSet<string>(cast.SelectMany(x => x.tags));

    int totalSkill = 0;
    
    // Calculate skill with penalty for performers without recommended tags
    foreach (var performer in cast)
    {
        int performerSkill = performer.GetSkill();
        
        // Check if performer has any recommended tags
        bool hasRecommendedTag = level.recommendedTags != null && 
                                 level.recommendedTags.Any(tag => performer.tags.Contains(tag));
        
        // Reduce skill by half if no recommended tags
        if (level.recommendedTags != null && level.recommendedTags.Count > 0 && !hasRecommendedTag)
        {
            performerSkill = performerSkill / 3;
        }
        
        totalSkill += performerSkill;
    }

    int synergy = 0;
    int conflict = 0;

    foreach (var p in cast)
    {
        foreach (var q in p.quirks)
        {
            if (!string.IsNullOrEmpty(q.boosts_if_with_tag) && allTags.Contains(q.boosts_if_with_tag))
                synergy++;
            if (!string.IsNullOrEmpty(q.boosts_if_with_role) && allRoles.Contains(q.boosts_if_with_role))
                synergy++;
            if (!string.IsNullOrEmpty(q.conflicts_with_tag) && allTags.Contains(q.conflicts_with_tag))
                conflict++;
            if (!string.IsNullOrEmpty(q.conflicts_with_role) && allRoles.Contains(q.conflicts_with_role))
                conflict++;
        }
    }

    // Recommended tags bonus (keep existing logic)
    int recTagsBonus = 0;
    if (level.recommendedTags != null)
        recTagsBonus = level.recommendedTags.Count(tag => cast.Any(p => p.tags.Contains(tag)));

    int random = Random.Range(-2, 3);

    int score = totalSkill + synergy - conflict + recTagsBonus + random;
    int money = (100 + (score * 50) + (synergy * 50) - (conflict * 200));
    int stars = (score >= 15) ? 3 : (score >= 11) ? 2 : (score >= 6) ? 1 : 0;
    if (money<= stars)
        money = stars * 100; // Ensure minimum money based on stars

    bool success = stars >= level.requiredStars && money >= level.requiredMoney;

    string flavorText = GetFlavorText(stars, synergy, conflict, random, cast, allRoles, allTags);
    string summaryText = $"Skil: {totalSkill}\nSynergies: +{synergy}\nConflicts: -{conflict}\nRecommend tags: +{recTagsBonus}\naccidentally: {((random >= 0) ? "+" : "")}{random}";

    return new ShowResult
    {
        stars = stars,
        moneyEarned = money,
        success = success,
        flavorText = flavorText,
        summaryText = summaryText
    };
}

    private string GetFlavorText(int stars, int synergy, int conflict, int random, List<PerformerData> cast, HashSet<string> allRoles, HashSet<string> allTags)
    {
        if (stars == 3 && conflict == 0)
            return "Mr. Vegas: Brilliant! This show will be remembered for a long time. Your cast worked perfectly!";
        if (conflict >= 2)
            return "Mr. Vegas: Drama! There were disputes among the performers. But the show was a success!";
        if (synergy >= 3)
            return "Mr. Vegas: The team acted like a well-oiled machine! The audience is thrilled!";
        if (random >= 2)
            return "Mr. Vegas: I feel lucky today! The crowd is giving a standing ovation.";
        if (random <= -2)
            return "Mr. Vegas: Not everything is going according to plan, but sometimes risks pay off. Let's learn from this!";
        if (stars == 3)
            return "Mr. Vegas: This is the show of the year! The audience is going wild!";
        if (stars == 2)
            return "Mr. Vegas: Great job! Just a little more and you'll be at the top!";
        return "Mr. Vegas: There's room for improvement. But every experience is a step towards success!";
    }

    
    public string GetCurrentLevelText()
    {
        if (levels == null || levels.Count == 0)
            return "No levels available";
    
        if (currentLevelIndex >= levels.Count)
            return "All levels completed!";
    
        return $"Level {currentLevelIndex + 1}: {levels[currentLevelIndex].levelName}";
    }

    // --- Призначення персонажа на місце у касті ---
    public void AssignPerformerToCast(PerformerData p)
    {
        if (!chosenPerformers.Contains(p) && chosenPerformers.Count < 5)
        {
            chosenPerformers.Add(p);
            // Онови UI
        }
    }
    
    
    public void SaveLevelResult(int levelIndex, int stars)
    {
        levelStars[levelIndex] = stars;
        completedLevels.Add(levelIndex);
        SaveProgress();
    }

    public int GetLevelStars(int levelIndex)
    {
        return levelStars.ContainsKey(levelIndex) ? levelStars[levelIndex] : 0;
    }

    public void RemovePerformerFromCast(PerformerData p)
    {
        if (chosenPerformers.Contains(p))
        {
            chosenPerformers.Remove(p);
            // Онови UI
        }
    }
    
    public void TogglePerformerInCast(PerformerData p)
    {
        if (chosenPerformers.Contains(p))
            RemovePerformerFromCast(p);
        else
            AssignPerformerToCast(p);
    }
    
    // Перевірка чи рівень пройдений
    public bool IsLevelCompleted(int levelIndex)
    {
        return completedLevels.Contains(levelIndex);
    }

    // Отримати всі пройдені рівні
    public HashSet<int> GetCompletedLevels()
    {
        return new HashSet<int>(completedLevels);
    }

    // Збереження прогресу
    public void SaveProgress()
    {
        string completedLevelsJson = string.Join(",", completedLevels);
        PlayerPrefs.SetString("CompletedLevels", completedLevelsJson);
        PlayerPrefs.SetInt("CurrentLevelIndex", currentLevelIndex);
        // Save star ratings
        foreach (var kvp in levelStars)
        {
            PlayerPrefs.SetInt($"LevelStars_{kvp.Key}", kvp.Value);
        }
    
        PlayerPrefs.Save();
    }

    // Завантаження прогресу
    public void LoadProgress()
    {
        string completedLevelsJson = PlayerPrefs.GetString("CompletedLevels", "");
        completedLevels.Clear();
        levelStars.Clear();

        if (!string.IsNullOrEmpty(completedLevelsJson))
        {
            string[] completedArray = completedLevelsJson.Split(',');
            foreach (string levelStr in completedArray)
            {
                if (int.TryParse(levelStr, out int levelIdx))
                {
                    completedLevels.Add(levelIdx);
                    // Load star rating for this level
                    int stars = PlayerPrefs.GetInt($"LevelStars_{levelIdx}", 0);
                    levelStars[levelIdx] = stars;
                }
            }
        }
        currentLevelIndex = PlayerPrefs.GetInt("CurrentLevelIndex", 0);
    }
    public List<LevelData> GetPredefinedLevels()
{
    return new List<LevelData>
    {
        new LevelData
        {
            levelName = "Solo Debut",
            requiredStars = 1,
            requiredMoney = 800,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Singer", count = 1 }
            },
            recommendedTags = new List<string>{ "Charming" },
            restrictions = new List<LevelRestriction>()
        },
        new LevelData
        {
            levelName = "Dance Off",
            requiredStars = 1,
            requiredMoney = 1300,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Dancer", count = 1 },
                new LevelRoleRequirement{ role = "Singer", count = 1 }
            },
            recommendedTags = new List<string>{ "Loud", "Energetic" },
            restrictions = new List<LevelRestriction>()
        },
        new LevelData
        {
            levelName = "Magic Moment",
            requiredStars = 2,
            requiredMoney = 1600,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Magician", count = 1 },
                new LevelRoleRequirement{ role = "Host", count = 1 }
            },
            recommendedTags = new List<string>{ "Weird", "Showy" },
            restrictions = new List<LevelRestriction>()
        },
        new LevelData
        {
            levelName = "Tech & Dance",
            requiredStars = 2,
            requiredMoney = 1900,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Tech", count = 1 },
                new LevelRoleRequirement{ role = "Dancer", count = 1 }
            },
            recommendedTags = new List<string>{ "Futuristic", "Edgy" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ forbiddenRole = "Singer" }
            }
        },
        new LevelData
        {
            levelName = "Classical Night",
            requiredStars = 2,
            requiredMoney = 2100,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Singer", count = 1 },
                new LevelRoleRequirement{ role = "Acrobat", count = 1 }
            },
            recommendedTags = new List<string>{ "Classical" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ mustIncludeTag = "Classical" }
            }
        },
        new LevelData
        {
            levelName = "Futuristic Frenzy",
            requiredStars = 2,
            requiredMoney = 2300,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Tech", count = 1 },
                new LevelRoleRequirement{ role = "Magician", count = 1 }
            },
            recommendedTags = new List<string>{ "Futuristic", "Stylish" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ mustIncludeTag = "Futuristic" }
            }
        },
        new LevelData
        {
            levelName = "Host's Challenge",
            requiredStars = 2,
            requiredMoney = 2450,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Host", count = 1 },
                new LevelRoleRequirement{ role = "Singer", count = 1 }
            },
            recommendedTags = new List<string>{ "Showy" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ forbiddenRole = "Tech" }
            }
        },
        new LevelData
        {
            levelName = "Energetic Mix",
            requiredStars = 2,
            requiredMoney = 2600,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Dancer", count = 1 },
                new LevelRoleRequirement{ role = "Singer", count = 1 },
                new LevelRoleRequirement{ role = "Magician", count = 1 }
            },
            recommendedTags = new List<string>{ "Energetic", "Weird" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ forbiddenRole = "Acrobat" }
            }
        },
        new LevelData
        {
            levelName = "Dramatic Showdown",
            requiredStars = 2,
            requiredMoney = 2750,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Singer", count = 1 },
                new LevelRoleRequirement{ role = "Host", count = 1 },
                new LevelRoleRequirement{ role = "Tech", count = 1 }
            },
            recommendedTags = new List<string>{ "Dramatic", "Charming" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ mustIncludeTag = "Dramatic" }
            }
        },
        new LevelData
        {
            levelName = "Edgy Ensemble",
            requiredStars = 2,
            requiredMoney = 2950,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Tech", count = 1 },
                new LevelRoleRequirement{ role = "Magician", count = 1 },
                new LevelRoleRequirement{ role = "Dancer", count = 1 }
            },
            recommendedTags = new List<string>{ "Edgy", "Loud" },
            restrictions = new List<LevelRestriction>()
        },
        new LevelData
        {
            levelName = "Stylish Affair",
            requiredStars = 2,
            requiredMoney = 3150,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Host", count = 1 },
                new LevelRoleRequirement{ role = "Acrobat", count = 1 },
                new LevelRoleRequirement{ role = "Singer", count = 1 }
            },
            recommendedTags = new List<string>{ "Stylish", "Showy" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ forbiddenRole = "Tech" }
            }
        },
        new LevelData
        {
            levelName = "Animal Parade",
            requiredStars = 2,
            requiredMoney = 3350,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Dancer", count = 1 },
                new LevelRoleRequirement{ role = "Acrobat", count = 1 }
            },
            recommendedTags = new List<string>{ "Animal Lover" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ mustIncludeTag = "Animal Lover" }
            }
        },
        new LevelData
        {
            levelName = "Quiet Night",
            requiredStars = 2,
            requiredMoney = 3550,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Magician", count = 1 },
                new LevelRoleRequirement{ role = "Singer", count = 1 }
            },
            recommendedTags = new List<string>{ "Quiet", "Edgy" },
            restrictions = new List<LevelRestriction>()
        },
        new LevelData
        {
            levelName = "Weird Wonders",
            requiredStars = 2,
            requiredMoney = 3750,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Dancer", count = 1 },
                new LevelRoleRequirement{ role = "Magician", count = 1 },
                new LevelRoleRequirement{ role = "Host", count = 1 }
            },
            recommendedTags = new List<string>{ "Weird", "Showy" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ forbiddenRole = "Tech" }
            }
        },
        new LevelData
        {
            levelName = "Loud Crowd",
            requiredStars = 2,
            requiredMoney = 3950,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Singer", count = 1 },
                new LevelRoleRequirement{ role = "Acrobat", count = 1 }
            },
            recommendedTags = new List<string>{ "Loud", "Classical" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ mustIncludeTag = "Loud" }
            }
        },
        new LevelData
        {
            levelName = "Showy Stunt",
            requiredStars = 2,
            requiredMoney = 4100,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Dancer", count = 1 },
                new LevelRoleRequirement{ role = "Host", count = 1 }
            },
            recommendedTags = new List<string>{ "Showy", "Charming" },
            restrictions = new List<LevelRestriction>()
        },
        new LevelData
        {
            levelName = "Acrobatic Artistry",
            requiredStars = 2,
            requiredMoney = 4250,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Acrobat", count = 2 }
            },
            recommendedTags = new List<string>{ "Classical", "Stylish" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ forbiddenRole = "Magician" }
            }
        },
        new LevelData
        {
            levelName = "Final Countdown",
            requiredStars = 3,
            requiredMoney = 4400,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Singer", count = 1 },
                new LevelRoleRequirement{ role = "Tech", count = 1 },
                new LevelRoleRequirement{ role = "Dancer", count = 1 },
                new LevelRoleRequirement{ role = "Host", count = 1 }
            },
            recommendedTags = new List<string>{ "Futuristic", "Weird" },
            restrictions = new List<LevelRestriction>()
        },
        new LevelData
        {
            levelName = "Gala Premiere",
            requiredStars = 3,
            requiredMoney = 4800,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Acrobat", count = 1 },
                new LevelRoleRequirement{ role = "Dancer", count = 1 },
                new LevelRoleRequirement{ role = "Magician", count = 1 },
                new LevelRoleRequirement{ role = "Host", count = 1 }
            },
            recommendedTags = new List<string>{ "Stylish", "Edgy" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ mustIncludeTag = "Stylish" }
            }
        },
        new LevelData
        {
            levelName = "Vegas Finale",
            requiredStars = 3,
            requiredMoney = 5600,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Singer", count = 1 },
                new LevelRoleRequirement{ role = "Dancer", count = 1 },
                new LevelRoleRequirement{ role = "Acrobat", count = 1 },
                new LevelRoleRequirement{ role = "Tech", count = 1 },
                new LevelRoleRequirement{ role = "Host", count = 1 },
                new LevelRoleRequirement{ role = "Magician", count = 1 }
            },
            recommendedTags = new List<string>{ "Classical", "Futuristic", "Weird" },
            restrictions = new List<LevelRestriction>()
        }
    };
}
}

public class ShowResult
{
    public int stars;
    public int moneyEarned;
    public bool success;
    public string errorMessage;
    public string flavorText;
    public string summaryText;
    public static ShowResult Failed(string msg) => new ShowResult { success = false, errorMessage = msg };
}
