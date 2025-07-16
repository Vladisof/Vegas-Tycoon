using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelChoiceManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject performersSelectionPanel;
    public GameObject selectLevelPanel;

    // Кнопки для вибору рівнів
    public Button button1, button2, button3;
    public TextMeshProUGUI levelText1, levelText2, levelText3;

    // Які індекси рівнів зараз доступні для вибору
    private int[] shownLevelIndices = new int[3];

    // Який рівень у списку наступний (для заміни після проходження)
    private int nextLevelToShow = 5; // Починаємо з 2,3,4 — тобто індекс 5 = 5-й рівень (індексація з 0)

    private HashSet<int> completedLevels = new HashSet<int>();

    void Start()
    {
        // Завантажуємо пройдені рівні з GameManager
        completedLevels = gameManager.GetCompletedLevels();
        UpdateShownLevels();
        UpdateLevelButtons();

        button1.onClick.AddListener(() => OnLevelChosen(0));
        button2.onClick.AddListener(() => OnLevelChosen(1));
        button3.onClick.AddListener(() => OnLevelChosen(2));
    }

    public void OnLevelCompleted(int levelIndex)
    {
        completedLevels.Add(levelIndex);
        UpdateShownLevels();
        UpdateLevelButtons();
    }
    
    void UpdateShownLevels()
    {
        // Знаходимо перші 3 непройдені рівні
        int shownCount = 0;
        for (int i = 0; i < gameManager.levels.Count && shownCount < 3; i++)
        {
            if (!completedLevels.Contains(i))
            {
                shownLevelIndices[shownCount] = i;
                shownCount++;
            }
        }

        // Якщо менше 3 доступних рівнів, заповнюємо -1 (немає рівня)
        for (int i = shownCount; i < 3; i++)
        {
            shownLevelIndices[i] = -1;
        }
    }
    void UpdateLevelButtons()
    {
        levelText1.text = GetLevelNameOrEnd(shownLevelIndices[0]);
        levelText2.text = GetLevelNameOrEnd(shownLevelIndices[1]);
        levelText3.text = GetLevelNameOrEnd(shownLevelIndices[2]);
        // Деактивуємо кнопки для вже пройдених або недоступних рівнів
        button1.interactable = !completedLevels.Contains(shownLevelIndices[0]) && IsLevelExist(shownLevelIndices[0]);
        button2.interactable = !completedLevels.Contains(shownLevelIndices[1]) && IsLevelExist(shownLevelIndices[1]);
        button3.interactable = !completedLevels.Contains(shownLevelIndices[2]) && IsLevelExist(shownLevelIndices[2]);
    }

    string GetLevelNameOrEnd(int idx)
    {
        if (IsLevelExist(idx))
            return gameManager.levels[idx].levelName;
        return "Немає рівня";
    }

    bool IsLevelExist(int idx)
    {
        return idx >= 0 && idx < gameManager.levels.Count;
    }

    void OnLevelChosen(int buttonIdx)
    {
        int levelIdx = shownLevelIndices[buttonIdx];
        if (!IsLevelExist(levelIdx) || completedLevels.Contains(levelIdx))
            return;

        // Встановлюємо обраний рівень в GameManager
        gameManager.SetCurrentLevel(levelIdx);


        // Відмічаємо як пройдений
        completedLevels.Add(levelIdx);

        // Оновлюємо shownLevelIndices:
        // - кнопка, яку вибрали — оновлюється на наступний рівень за списком
        // - решта залишаються
        shownLevelIndices[buttonIdx] = nextLevelToShow;
        nextLevelToShow++;
        UpdateLevelButtons();
        // Показуємо панель вибору виконавців
        gameManager.StartLoading();
        selectLevelPanel.SetActive(false);
    }
    // Метод для збереження прогресу (викликати при закритті гри)
    public void SaveProgress()
    {
        string completedLevelsJson = string.Join(",", completedLevels);
        PlayerPrefs.SetString("CompletedLevels", completedLevelsJson);
        PlayerPrefs.Save();
    }

    // Метод для завантаження прогресу (викликати при запуску гри)
    public void LoadProgress()
    {
        string completedLevelsJson = PlayerPrefs.GetString("CompletedLevels", "");
        if (!string.IsNullOrEmpty(completedLevelsJson))
        {
            string[] completedArray = completedLevelsJson.Split(',');
            foreach (string levelStr in completedArray)
            {
                if (int.TryParse(levelStr, out int levelIdx))
                {
                    completedLevels.Add(levelIdx);
                }
            }
        }
        UpdateShownLevels();
        UpdateLevelButtons();
    }
}
