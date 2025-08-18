using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class QuestData
{
    public string name;
    public int duration; // в секундах
    public int goldReward;
    public bool isActive;
    public float startTime;
    public bool isCompleted; // завершен ли квест
    public bool rewardCollected; // собрана ли награда
}

public class QuestManager : MonoBehaviour
{
    [Header("Quest UI References")]
    public Transform questButtonParent; // родительский объект для кнопок квестов
    public GameObject questButtonPrefab; // префаб кнопки квеста
    
    [Header("Active Quest UI")]
    public GameObject activeQuestUI;
    public TextMeshProUGUI activeQuestNameText;
    public TextMeshProUGUI activeQuestTimerText;
    public Button collectQuestRewardButton;
    
    [Header("Quest Refresh Timer UI")]
    public TextMeshProUGUI questRefreshTimerText; // таймер до смены квестов
    
    private List<QuestData> currentQuests = new List<QuestData>();
    private QuestData currentActiveQuest;
    private MoneyController moneyController;
    
    // Названия квестов для случайной генерации
    private string[] questNames = {
        "Поиск сокровищ",
        "Разведка местности", 
        "Сбор артефактов",
        "Охота на монстров",
        "Исследование руин",
        "Торговая миссия",
        "Спасательная операция",
        "Патрулирование границ",
        "Добыча ресурсов",
        "Дипломатическое задание",
        "Охрана каравана",
        "Поиск пропавших",
        "Зачистка территории",
        "Доставка посылки",
        "Исследование пещер"
    };
    
    void Start()
    {
        moneyController = FindObjectOfType<MoneyController>();
        
        // Загружаем квесты или генерируем новые
        LoadOrGenerateQuests();
        
        // Настраиваем UI
        SetupUI();
        
        // Проверяем активные квесты при загрузке
        CheckActiveQuest();
        
        // Запускаем таймер обновления UI
        StartCoroutine(UpdateQuestRefreshTimer());
        
        Debug.Log("QuestManager запущен. Квестов сгенерировано: " + currentQuests.Count);
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
        // Проверяем, нужно ли генерировать новые квесты
        string lastQuestGenerationTime = PlayerPrefs.GetString("LastQuestGenerationTime", "");
        
        if (string.IsNullOrEmpty(lastQuestGenerationTime))
        {
            // Первый запуск - генерируем квесты
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
                    // Прошло 24 часа - сбрасываем все и генерируем новые квесты
                    ClearAllQuestProgress();
                    GenerateNewQuests();
                }
                else
                {
                    // Загружаем существующие квесты
                    LoadExistingQuests();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка при загрузке времени генерации квестов: {e.Message}");
                GenerateNewQuests();
            }
        }
    }
    
    void GenerateNewQuests()
    {
        currentQuests.Clear();
        
        // Генерируем 3 случайных квеста
        for (int i = 0; i < 3; i++)
        {
            QuestData newQuest = new QuestData();
            
            // Случайное название
            newQuest.name = questNames[Random.Range(0, questNames.Length)];
            
            // Случайная продолжительность: 5 минут, 30 минут, 1 час или 3 часа
            float[] durations = { 300f, 1800f, 3600f, 10800f }; // в секундах
            int[] rewards = { 150, 500, 1200, 4000 }; // соответствующие награды
            
            int durationIndex = Random.Range(0, durations.Length);
            newQuest.duration = (int)durations[durationIndex];
            newQuest.goldReward = rewards[durationIndex];
            
            // Добавляем небольшую случайность к награде (±20%)
            float rewardMultiplier = Random.Range(0.8f, 1.2f);
            newQuest.goldReward = Mathf.RoundToInt(newQuest.goldReward * rewardMultiplier);
            
            newQuest.isActive = false;
            newQuest.isCompleted = false;
            newQuest.rewardCollected = false;
            
            currentQuests.Add(newQuest);
        }
        
        // Сохраняем время генерации
        SaveQuestGenerationTime();
        SaveQuests();
        
        Debug.Log("Сгенерированы новые квесты");
    }
    
    void LoadExistingQuests()
    {
        currentQuests.Clear();
        
        // Загружаем количество квестов
        int questCount = PlayerPrefs.GetInt("QuestCount", 0);
        
        Debug.Log($"LoadExistingQuests: загружается {questCount} квестов");
        
        for (int i = 0; i < questCount; i++)
        {
            QuestData quest = new QuestData();
            quest.name = PlayerPrefs.GetString($"Quest{i}_Name", "");
            quest.duration = PlayerPrefs.GetInt($"Quest{i}_Duration", 300);
            quest.goldReward = PlayerPrefs.GetInt($"Quest{i}_Reward", 150);
            quest.isActive = PlayerPrefs.GetInt($"Quest{i}_IsActive", 0) == 1;
            quest.isCompleted = PlayerPrefs.GetInt($"Quest{i}_IsCompleted", 0) == 1;
            quest.rewardCollected = PlayerPrefs.GetInt($"Quest{i}_RewardCollected", 0) == 1;
            
            Debug.Log($"Квест {i}: {quest.name}, isActive: {quest.isActive}, isCompleted: {quest.isCompleted}, rewardCollected: {quest.rewardCollected}");
            
            if (quest.isActive && !quest.rewardCollected)
            {
                // Загружаем время начала активного квеста
                string startTimeString = PlayerPrefs.GetString($"Quest{i}_StartTime", "");
                Debug.Log($"Время начала квеста {quest.name}: {startTimeString}");
                
                if (!string.IsNullOrEmpty(startTimeString))
                {
                    try
                    {
                        long startTimeBinary = System.Convert.ToInt64(startTimeString);
                        System.DateTime startTime = System.DateTime.FromBinary(startTimeBinary);
                        System.TimeSpan timeElapsed = System.DateTime.Now - startTime;
                        double elapsedSeconds = timeElapsed.TotalSeconds;
                        
                        Debug.Log($"Загружается квест: {quest.name}. Прошло времени: {elapsedSeconds:F1} сек из {quest.duration}");
                        
                        if (elapsedSeconds >= quest.duration)
                        {
                            // Квест уже завершен - устанавливаем правильное время и отмечаем как завершенный
                            quest.startTime = Time.time - quest.duration - 1; // -1 чтобы точно было завершено
                            quest.isCompleted = true;
                            currentActiveQuest = quest;
                            Debug.Log($"Квест {quest.name} завершен пока игрок был оффлайн!");
                        }
                        else
                        {
                            // Квест все еще идет - правильно вычисляем startTime для оставшегося времени
                            quest.startTime = Time.time - (float)elapsedSeconds;
                            currentActiveQuest = quest;
                            // НЕ запускаем таймер здесь - он будет запущен в CheckActiveQuest()
                            Debug.Log($"Квест {quest.name} продолжается. Осталось: {quest.duration - elapsedSeconds:F1} сек");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Ошибка при загрузке времени квеста: {e.Message}");
                        quest.isActive = false;
                        quest.isCompleted = false;
                    }
                }
                else
                {
                    // Нет времени начала - сбрасываем квест
                    Debug.LogWarning($"У квеста {quest.name} нет времени начала - сбрасываем");
                    quest.isActive = false;
                    quest.isCompleted = false;
                }
            }
            
            currentQuests.Add(quest);
        }
        
        Debug.Log($"Загружены существующие квесты: {questCount}, currentActiveQuest: {(currentActiveQuest != null ? currentActiveQuest.name : "нет")}");
        
        // Важно: сохраняем обновленное состояние квестов
        if (currentActiveQuest != null)
        {
            SaveQuests();
        }
    }
    
    void UpdateQuestButtons()
    {
        // Удаляем старые кнопки
        foreach (Transform child in questButtonParent)
        {
            Destroy(child.gameObject);
        }
        
        // Создаем кнопки для квестов
        for (int i = 0; i < currentQuests.Count; i++)
        {
            CreateQuestButton(currentQuests[i], i);
        }
    }
    
    void CreateQuestButton(QuestData quest, int index)
    {
        GameObject buttonObj = Instantiate(questButtonPrefab, questButtonParent);
        Button button = buttonObj.GetComponent<Button>();
        
        // Находим текстовые компоненты кнопки
        TextMeshProUGUI[] texts = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length > 0)
        {
            texts[0].text = quest.name;
        }
        if (texts.Length > 1)
        {
            texts[1].text = $"Награда: {quest.goldReward} золота";
        }
        if (texts.Length > 2)
        {
            int minutes = quest.duration / 60;
            if (minutes < 60)
            {
                texts[2].text = $"Время: {minutes} мин";
            }
            else
            {
                int hours = minutes / 60;
                texts[2].text = $"Время: {hours} ч";
            }
        }
        
        // Определяем состояние кнопки
        bool canStart = currentActiveQuest == null && !quest.isActive && !quest.rewardCollected;
        button.interactable = canStart;
        
        if (quest.isActive && quest == currentActiveQuest)
        {
            if (texts.Length > 0)
            {
                texts[0].text += " (В процессе)";
            }
        }
        else if (quest.rewardCollected)
        {
            if (texts.Length > 0)
            {
                texts[0].text += " (Выполнен)";
            }
        }
        else if (currentActiveQuest != null)
        {
            if (texts.Length > 0)
            {
                texts[0].text += " (Недоступно)";
            }
        }
        
        button.onClick.AddListener(() => StartQuest(quest));
    }
    
    public void StartQuest(QuestData quest)
    {
        if (currentActiveQuest != null || quest.isActive || quest.rewardCollected)
        {
            Debug.Log("Нельзя начать квест - уже есть активный или квест выполнен");
            return;
        }
        
        quest.isActive = true;
        quest.startTime = Time.time;
        currentActiveQuest = quest;
        
        Debug.Log($"Начинаем квест: {quest.name}, устанавливаем isActive = true");
        
        // Сохраняем прогресс СРАЗУ после изменения состояния
        SaveActiveQuestProgress(quest);
        SaveQuests(); // Важно: сохраняем общее состояние квестов
        
        Debug.Log($"Квест сохранен. Проверяем: isActive = {PlayerPrefs.GetInt($"Quest{currentQuests.IndexOf(quest)}_IsActive", -1)}");
        
        // Запускаем таймер
        StartCoroutine(QuestTimer(quest));
        
        UpdateQuestButtons();
        UpdateActiveQuestUI();
        
        Debug.Log($"Начат квест: {quest.name}");
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
                activeQuestTimerText.text = $"Осталось: {hours:00}:{minutes:00}:{seconds:00}";
            }
            else
            {
                activeQuestTimerText.text = $"Осталось: {minutes:00}:{seconds:00}";
            }
        }
    }
    
    void CompleteQuest(QuestData quest)
    {
        quest.isCompleted = true;
        
        // Показываем кнопку сбора награды
        if (collectQuestRewardButton != null)
        {
            collectQuestRewardButton.gameObject.SetActive(true);
            collectQuestRewardButton.interactable = true;
        }
        
        UpdateActiveQuestUI();
        SaveQuests();
        
        Debug.Log($"Квест {quest.name} завершен! Награда: {quest.goldReward} золота");
    }
    
    public void CollectQuestReward()
    {
        if (currentActiveQuest == null || !currentActiveQuest.isCompleted) return;
        
        // Добавляем золото
        if (moneyController != null)
        {
            moneyController.AddMoney(currentActiveQuest.goldReward);
        }
        
        // Отмечаем награду как собранную
        currentActiveQuest.rewardCollected = true;
        currentActiveQuest.isActive = false;
        
        // Скрываем UI активного квеста
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
        
        Debug.Log("Награда за квест собрана!");
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
                activeQuestNameText.text = $"Активен: {currentActiveQuest.name}";
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
                    activeQuestTimerText.text = "Завершено! Соберите награду";
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
        Debug.Log("CheckActiveQuest вызван");
        
        // Если активный квест уже установлен в LoadExistingQuests, не переустанавливаем его
        if (currentActiveQuest != null)
        {
            Debug.Log($"Активный квест уже найден: {currentActiveQuest.name}");
            
            if (!currentActiveQuest.isCompleted)
            {
                float timeElapsed = Time.time - currentActiveQuest.startTime;
                if (timeElapsed >= currentActiveQuest.duration)
                {
                    CompleteQuest(currentActiveQuest);
                }
                else
                {
                    // Запускаем таймер для продолжающегося квеста
                    StartCoroutine(QuestTimer(currentActiveQuest));
                    Debug.Log($"Запущен таймер для продолжающегося квеста: {currentActiveQuest.name}");
                }
            }
        }
        else
        {
            // Ищем активный квест если он не был установлен
            foreach (QuestData quest in currentQuests)
            {
                if (quest.isActive && !quest.rewardCollected)
                {
                    currentActiveQuest = quest;
                    Debug.Log($"Найден активный квест: {quest.name}");
                    
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
        Debug.Log($"После CheckActiveQuest, currentActiveQuest: {(currentActiveQuest != null ? currentActiveQuest.name : "нет")}");
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
            questRefreshTimerText.text = "Новые квесты: скоро";
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
                // Время вышло - обновляем квесты
                ClearAllQuestProgress();
                GenerateNewQuests();
                UpdateQuestButtons();
                questRefreshTimerText.text = "Квесты обновлены!";
            }
            else
            {
                int hours = (int)timeUntilRefresh.TotalHours;
                int minutes = timeUntilRefresh.Minutes;
                int seconds = timeUntilRefresh.Seconds;
                
                questRefreshTimerText.text = $"Новые квесты через: {hours:00}:{minutes:00}:{seconds:00}";
            }
        }
        catch
        {
            questRefreshTimerText.text = "Новые квесты: скоро";
        }
    }
    
    void Update()
    {
        // Обновляем таймер активного квеста
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
    
    // Методы сохранения и загрузки
    void SaveQuestGenerationTime()
    {
        string timeString = System.DateTime.Now.ToBinary().ToString();
        PlayerPrefs.SetString("LastQuestGenerationTime", timeString);
        PlayerPrefs.Save();
    }
    
    void SaveActiveQuestProgress(QuestData quest)
    {
        string startTimeString = System.DateTime.Now.ToBinary().ToString();
        
        // Найдем индекс квеста
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
    
    void ClearAllQuestProgress()
    {
        // Очищаем текущие квесты
        currentActiveQuest = null;
        currentQuests.Clear();
        
        // Очищаем все сохраненные данные квестов
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
        
        Debug.Log("Все данные квестов очищены");
    }
}
