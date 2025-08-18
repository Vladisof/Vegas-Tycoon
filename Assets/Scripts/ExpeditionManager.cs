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
    public int duration; // в секундах
    public int goldReward;
    public bool isActive;
    public float startTime;
    public GameObject expeditionLocationObject; // объект на сцене для этой экспедиции
}

public class ExpeditionManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject expeditionPanel;
    public GameObject expeditionSelectionPanel; // панель со списком экспедиций
    public Transform expeditionButtonParent; // родительский объект для кнопок экспедиций
    public GameObject expeditionButtonPrefab; // префаб кнопки экспедиции
    public GameObject lockedExpeditionPrefab; // префаб заблокированной экспедиции
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
        
        // Инициализируем экспедиции
        InitializeExpeditions();
        
        // Загружаем сохраненные экспедиции ТОЛЬКО если есть сохранения
        LoadExpeditionProgress();
        
        // Настраиваем UI
        SetupUI();
        
        // Проверяем активные экспедиции при загрузке ТОЛЬКО если что-то загружено
        CheckActiveExpeditions();
        
        Debug.Log("ExpeditionManager запущен. Активных экспедиций: " + (currentActiveExpedition != null ? currentActiveExpedition.name : "нет"));
    }
    
    void InitializeExpeditions()
    {
        // Очищаем список если он не пустой
        if (expeditions.Count == 0)
        {
            expeditions.Add(new ExpeditionData 
            { 
                name = "Малая экспедиция", 
                requiredLevelsCompleted = 0, 
                duration = 120, // 2 минуты
                goldReward = 100,
                isActive = false
            });
            
            expeditions.Add(new ExpeditionData 
            { 
                name = "Средняя экспедиция", 
                requiredLevelsCompleted = 2, 
                duration = 180, // 3 минуты
                goldReward = 200,
                isActive = false
            });
            
            expeditions.Add(new ExpeditionData 
            { 
                name = "Большая экспедиция", 
                requiredLevelsCompleted = 4, 
                duration = 300, // 5 минут
                goldReward = 350,
                isActive = false
            });
            
            expeditions.Add(new ExpeditionData 
            { 
                name = "Легендарная экспедиция", 
                requiredLevelsCompleted = 6, 
                duration = 600, // 10 минут
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
            // Изначально показываем кнопку, но делаем неинтерактивной
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
        // Возвращаемся к выбору персонажей через GameManager
        if (gameManager != null)
        {
            gameManager.OpenSelectionPanel();
        }
    }
    
    void UpdateExpeditionButtons()
    {
        // Удаляем старые кнопки
        foreach (Transform child in expeditionButtonParent)
        {
            Destroy(child.gameObject);
        }
        
        int completedLevels = gameManager.GetCompletedLevels().Count;
        
        // Создаем кнопки для доступных экспедиций
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
        
        // Находим текстовые компоненты кнопки
        TextMeshProUGUI[] texts = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length > 0)
        {
            texts[0].text = expedition.name;
        }
        if (texts.Length > 1)
        {
            texts[1].text = $"Награда: {expedition.goldReward} золота";
        }
        if (texts.Length > 2)
        {
            texts[2].text = $"Время: {expedition.duration / 60} мин";
        }
        
        // Проверяем, можно ли начать экспедицию
        bool canStart = !IsAnyExpeditionActive() && !expedition.isActive;
        button.interactable = canStart;
        
        if (expedition.isActive)
        {
            if (texts.Length > 0)
            {
                texts[0].text += " (В процессе)";
            }
        }
        else if (IsAnyExpeditionActive())
        {
            if (texts.Length > 0)
            {
                texts[0].text += " (Недоступно)";
            }
        }
        
        button.onClick.AddListener(() => StartExpedition(expedition));
    }
    
    void CreateLockedExpeditionButton(ExpeditionData expedition)
    {
        GameObject buttonObj = Instantiate(lockedExpeditionPrefab, expeditionButtonParent);
        Button button = buttonObj.GetComponent<Button>();
        
        // Находим текстовые компоненты кнопки
        TextMeshProUGUI[] texts = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length > 0)
        {
            texts[0].text = expedition.name;
        }
        if (texts.Length > 1)
        {
            texts[1].text = $"Требуется уровней: {expedition.requiredLevelsCompleted}";
        }
        
        button.interactable = false;
    }
    
    public void StartExpedition(ExpeditionData expedition)
    {
        if (IsAnyExpeditionActive() || expedition.isActive)
        {
            Debug.Log("Нельзя начать экспедицию - уже есть активная");
            return;
        }
        
        expedition.isActive = true;
        expedition.startTime = Time.time;
        currentActiveExpedition = expedition;
        
        // Сохраняем время начала экспедиции в реальном времени
        SaveExpeditionProgress(expedition);
        
        // Активируем объект локации экспедиции
        if (expedition.expeditionLocationObject != null)
        {
            expedition.expeditionLocationObject.SetActive(true);
        }
        
        // Оставляем кнопку видимой, но делаем неинтерактивной
        if (collectRewardButton != null)
        {
            collectRewardButton.gameObject.SetActive(true);
            collectRewardButton.interactable = false;
        }
        
        StartCoroutine(ExpeditionTimer(expedition));
        
        UpdateExpeditionButtons();
        UpdateActiveExpeditionUI();
        
        Debug.Log($"Начата экспедиция: {expedition.name}");
    }
    
    IEnumerator ExpeditionTimer(ExpeditionData expedition)
    {
        // Вычисляем оставшееся время
        float timeElapsed = Time.time - expedition.startTime;
        float timer = expedition.duration - timeElapsed;
        
        // Если времени уже не осталось, сразу завершаем
        if (timer <= 0)
        {
            CompleteExpedition(expedition);
            yield break;
        }
        
        while (timer > 0 && expedition.isActive)
        {
            timer -= Time.deltaTime;
            
            // Обновляем UI таймера
            UpdateTimerUI(timer);
            
            yield return null;
        }
        
        if (expedition.isActive)
        {
            // Экспедиция завершена
            CompleteExpedition(expedition);
        }
    }
    
    void UpdateTimerUI(float timeLeft)
    {
        if (activeExpeditionTimerText != null && currentActiveExpedition != null)
        {
            int minutes = Mathf.FloorToInt(timeLeft / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);
            activeExpeditionTimerText.text = $"Осталось: {minutes:00}:{seconds:00}";
        }
    }
    
    void CompleteExpedition(ExpeditionData expedition)
    {
        Debug.Log($"Экспедиция {expedition.name} завершена! Награда: {expedition.goldReward} золота");
        
        // Показываем кнопку сбора награды и делаем её интерактивной
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
        
        // Добавляем золото
        if (moneyController != null)
        {
            moneyController.AddMoney(currentActiveExpedition.goldReward);
        }
        
        // Деактивируем объект локации
        if (currentActiveExpedition.expeditionLocationObject != null)
        {
            currentActiveExpedition.expeditionLocationObject.SetActive(false);
        }
        
        // Сбрасываем экспедицию
        currentActiveExpedition.isActive = false;
        currentActiveExpedition = null;
        
        // Очищаем сохраненные данные экспедиции
        ClearExpeditionProgress();
        
        // Скрываем UI активной экспедиции
        if (activeExpeditionUI != null)
        {
            activeExpeditionUI.SetActive(false);
        }
        
        if (collectRewardButton != null)
        {
            collectRewardButton.gameObject.SetActive(false);
        }
        
        UpdateExpeditionButtons();
        
        Debug.Log("Награда собрана!");
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
                activeExpeditionNameText.text = $"Активна: {currentActiveExpedition.name}";
            }
            
            // Проверяем, завершена ли экспедиция
            float timeElapsed = Time.time - currentActiveExpedition.startTime;
            if (timeElapsed >= currentActiveExpedition.duration)
            {
                // Экспедиция завершена - показываем кнопку и делаем интерактивной
                if (collectRewardButton != null)
                {
                    collectRewardButton.gameObject.SetActive(true);
                    collectRewardButton.interactable = true;
                }
                
                if (activeExpeditionTimerText != null)
                {
                    activeExpeditionTimerText.text = "Завершено! Соберите награду";
                }
            }
            else
            {
                // Экспедиция еще идет - показываем кнопку но делаем неинтерактивной
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
            
            // Когда нет активной экспедиции - СКРЫВАЕМ кнопку полностью
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
                // Проверяем, не истекло ли время
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
        Debug.Log("CheckActiveExpeditions вызван");
        
        foreach (ExpeditionData expedition in expeditions)
        {
            Debug.Log($"Проверяем экспедицию: {expedition.name}, isActive: {expedition.isActive}");
            
            if (expedition.isActive)
            {
                Debug.Log($"Найдена активная экспедиция: {expedition.name}");
                
                float timeElapsed = Time.time - expedition.startTime;
                if (timeElapsed < expedition.duration)
                {
                    currentActiveExpedition = expedition;
                    // НЕ запускаем новый таймер - он уже правильно настроен в LoadExpeditionProgress
                    // StartCoroutine(ExpeditionTimer(expedition)); // УБИРАЕМ ЭТУ СТРОКУ
                    
                    // Активируем объект локации
                    if (expedition.expeditionLocationObject != null)
                    {
                        expedition.expeditionLocationObject.SetActive(true);
                    }
                }
                else
                {
                    // Экспедиция уже должна была завершиться
                    CompleteExpedition(expedition);
                }
                break;
            }
        }
        
        Debug.Log($"После CheckActiveExpeditions, currentActiveExpedition: {(currentActiveExpedition != null ? currentActiveExpedition.name : "нет")}");
        UpdateActiveExpeditionUI();
    }
    
    void Update()
    {
        // Постоянно обновляем UI активной экспедиции
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
                // Время истекло - завершаем экспедицию
                CompleteExpedition(currentActiveExpedition);
            }
        }
    }
    
    // Методы для сохранения и загрузки прогресса экспедиций
    void SaveExpeditionProgress(ExpeditionData expedition)
    {
        // Сохраняем название активной экспедиции
        PlayerPrefs.SetString("ActiveExpeditionName", expedition.name);
        
        // Сохраняем реальное время начала экспедиции
        string startTimeString = System.DateTime.Now.ToBinary().ToString();
        PlayerPrefs.SetString("ExpeditionStartTime", startTimeString);
        
        // Сохраняем длительность экспедиции
        PlayerPrefs.SetInt("ExpeditionDuration", expedition.duration);
        
        PlayerPrefs.Save();
        
        Debug.Log($"Сохранена экспедиция: {expedition.name} в {System.DateTime.Now}");
    }
    
    void LoadExpeditionProgress()
    {
        string savedExpeditionName = PlayerPrefs.GetString("ActiveExpeditionName", "");
        
        Debug.Log($"LoadExpeditionProgress: savedExpeditionName = '{savedExpeditionName}'");
        
        if (string.IsNullOrEmpty(savedExpeditionName))
        {
            Debug.Log("Нет сохраненной экспедиции - пропускаем загрузку");
            return; // Нет сохраненной экспедиции
        }
        
        // Получаем сохраненное время начала
        string startTimeString = PlayerPrefs.GetString("ExpeditionStartTime", "");
        if (string.IsNullOrEmpty(startTimeString))
        {
            return;
        }
        
        try
        {
            // Конвертируем время обратно
            long startTimeBinary = System.Convert.ToInt64(startTimeString);
            System.DateTime startTime = System.DateTime.FromBinary(startTimeBinary);
            
            // Получаем длительность экспедиции
            int expeditionDuration = PlayerPrefs.GetInt("ExpeditionDuration", 0);
            
            // Вычисляем сколько времени прошло с начала экспедиции
            System.TimeSpan timeElapsed = System.DateTime.Now - startTime;
            double elapsedSeconds = timeElapsed.TotalSeconds;
            
            // Находим нужную экспедицию
            ExpeditionData savedExpedition = expeditions.Find(exp => exp.name == savedExpeditionName);
            if (savedExpedition != null)
            {
                Debug.Log($"Загружена экспедиция: {savedExpeditionName}. Прошло времени: {elapsedSeconds:F1} сек из {expeditionDuration}");
                
                if (elapsedSeconds >= expeditionDuration)
                {
                    // Экспедиция уже завершена - НЕ запускаем таймер заново!
                    savedExpedition.isActive = true;
                    currentActiveExpedition = savedExpedition;
                    
                    // Устанавливаем время так, чтобы экспедиция была завершена
                    savedExpedition.startTime = Time.time - expeditionDuration - 1; // -1 чтобы точно было завершено
                    
                    // Активируем объект локации если нужно
                    if (savedExpedition.expeditionLocationObject != null)
                    {
                        savedExpedition.expeditionLocationObject.SetActive(true);
                    }
                    
                    Debug.Log("Экспедиция завершена пока игрок был оффлайн!");
                }
                else
                {
                    // Экспедиция все еще идет - восстанавливаем правильное время
                    savedExpedition.isActive = true;
                    currentActiveExpedition = savedExpedition;
                    
                    // Правильно вычисляем startTime для оставшегося времени
                    savedExpedition.startTime = Time.time - (float)elapsedSeconds;
                    
                    // Активируем объект локации
                    if (savedExpedition.expeditionLocationObject != null)
                    {
                        savedExpedition.expeditionLocationObject.SetActive(true);
                    }
                    
                    // Запускаем таймер для продолжающейся экспедиции
                    StartCoroutine(ExpeditionTimer(savedExpedition));
                    
                    float remainingTime = expeditionDuration - (float)elapsedSeconds;
                    Debug.Log($"Экспедиция продолжается. Осталось: {remainingTime:F1} сек");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при загрузке экспедиции: {e.Message}");
            // Очищаем поврежденные данные
            ClearExpeditionProgress();
        }
    }
    
    void ClearExpeditionProgress()
    {
        PlayerPrefs.DeleteKey("ActiveExpeditionName");
        PlayerPrefs.DeleteKey("ExpeditionStartTime");
        PlayerPrefs.DeleteKey("ExpeditionDuration");
        PlayerPrefs.Save();
        
        Debug.Log("Прогресс экспедиции очищен");
    }
}
