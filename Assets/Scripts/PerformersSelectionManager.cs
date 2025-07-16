using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerformersSelectionManager : MonoBehaviour
{
    public GameManager gameManager; // Для chosenPerformers
    public GameObject performerUIPrefab; // Префаб для одного персонажа
    public Transform performersListParent; // LayoutGroup
    public AudioManager audioManager;
    [Header("Cost Settings")]
    public MoneyController moneyController;
    public int selectionCost = 100;
    public GameObject costTextPrefab;

    public int maxCastSize = 5;

    public void ShowPerformers(List<PerformerData> performers)
    {
        foreach (Transform child in performersListParent) Destroy(child.gameObject);

        // Filter to only show purchased performers
        var purchasedPerformers = performers.Where(p => p.isPurchased).ToList();

        for (int i = 0; i < purchasedPerformers.Count; i++)
        {
            int originalIdx = performers.IndexOf(purchasedPerformers[i]); // Get original index
            var p = purchasedPerformers[i];
            var go = Instantiate(performerUIPrefab, performersListParent);

            go.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = $"{p.name} ({p.role})";
            go.transform.Find("SkillText").GetComponent<TextMeshProUGUI>().text = $"Skill: {p.GetSkill()}";

            // Display quirks
            string quirksText = "Quirks: ";
            if (p.quirks != null && p.quirks.Length > 0)
            {
                quirksText += string.Join(", ", p.quirks.Select(q => q.ToString()).ToArray());
            }
            else
            {
                quirksText += "None";
            }
            go.transform.Find("QuirksText").GetComponent<TextMeshProUGUI>().text = quirksText;

            Button selectBtn = go.transform.Find("SelectButton").GetComponent<Button>();

            selectBtn.onClick.RemoveAllListeners();
            selectBtn.onClick.AddListener(() =>
            {
                SelectPerformer(originalIdx, go); // Use original index
            });

            // Highlight if already selected
            bool isSelected = gameManager.chosenPerformers.Contains(p);

            go.GetComponent<Image>().color = isSelected ? Color.black : Color.white;
            selectBtn.interactable = gameManager.chosenPerformers.Count < maxCastSize;
        }
    }

    void SelectPerformer(int performerIndex, GameObject prefabGo)
    {
        var p = gameManager.performersManager.performers[performerIndex];
        bool isAlreadySelected = gameManager.chosenPerformers.Contains(p);

        if (isAlreadySelected)
        {
            // Deselecting - refund money
            moneyController.AddMoney(selectionCost);
            ShowRefundText(prefabGo, selectionCost);
            gameManager.TogglePerformerInCast(p);
        }
        else if (gameManager.chosenPerformers.Count < maxCastSize)
        {
            // Selecting - subtract money
            audioManager.PlaySound(2);
            moneyController.SubtractMoney(selectionCost);
            ShowCostText(prefabGo, selectionCost);
            gameManager.TogglePerformerInCast(p);
        }

        // Update display
        ShowPerformers(gameManager.performersManager.performers);
    }
    
    void ShowRefundText(GameObject performerUI, int cost)
    {
        if (costTextPrefab != null)
        {
            Debug.Log($"Showing refund text: +{cost}");

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas not found!");
                return;
            }

            GameObject costText = Instantiate(costTextPrefab, canvas.transform);
            costText.GetComponent<TextMeshProUGUI>().text = $"+{cost}";
            costText.GetComponent<TextMeshProUGUI>().color = Color.green; // Green for refund

            RectTransform performerRect = performerUI.GetComponent<RectTransform>();
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            Vector3 worldPos = performerRect.TransformPoint(Vector3.zero);
            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPos),
                canvas.worldCamera,
                out canvasPos
            );

            RectTransform rectTransform = costText.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = canvasPos + new Vector2(0, 200);

            StartCoroutine(AnimateCostText(costText));
        }
    }
    void ShowCostText(GameObject performerUI, int cost)
    {
        if (costTextPrefab != null)
        {
            Debug.Log($"Showing cost text: -{cost}");

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas not found!");
                return;
            }

            GameObject costText = Instantiate(costTextPrefab, canvas.transform);
            costText.GetComponent<TextMeshProUGUI>().text = $"-{cost}";
            costText.GetComponent<TextMeshProUGUI>().color = Color.red;

            RectTransform performerRect = performerUI.GetComponent<RectTransform>();
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            Vector3 worldPos = performerRect.TransformPoint(Vector3.zero);
            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPos),
                canvas.worldCamera,
                out canvasPos
            );

            RectTransform rectTransform = costText.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = canvasPos + new Vector2(0, 200);

            // Start animation coroutine
            StartCoroutine(AnimateCostText(costText));
        }
    }
    
    System.Collections.IEnumerator AnimateCostText(GameObject costText)
    {
        if (costText == null) yield break;

        float duration = 1f;
        float elapsed = 0f;
    
        RectTransform rectTransform = costText.GetComponent<RectTransform>();
        TextMeshProUGUI textComponent = costText.GetComponent<TextMeshProUGUI>();
    
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, 100); // Move up 100 pixels
        Color startColor = textComponent.color;

        while (elapsed < duration && costText != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Move upward
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
        
            // Fade out
            textComponent.color = Color.Lerp(startColor, Color.clear, t);

            yield return null;
        }

        if (costText != null)
            Destroy(costText);
    }
    public void ClearSelection()
    {
        gameManager.chosenPerformers.Clear();
        ShowPerformers(gameManager.performersManager.performers);
    }
}