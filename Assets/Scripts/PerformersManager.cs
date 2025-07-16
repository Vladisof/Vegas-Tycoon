using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PerformersManager : MonoBehaviour
{
    public MoneyController moneyController;
    public GameObject performerUIPrefab;
    public Transform performersListParent;

    public List<PerformerData> performers = new List<PerformerData>();
    private const int INITIAL_ACTIVE_PERFORMERS = 8;

    void Start()
    {
        LoadPerformersFromJson();
        LoadPerformersLevels();
        LoadPerformersPurchaseStatus();
        RenderPerformers();
    }

    void LoadPerformersFromJson()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("mr_vegas_performers"); // без .json
        if (textAsset == null)
        {
            Debug.LogError("Failed to load mr_vegas_performers.json from Resources");
            return;
        }

        string json = "{ \"items\": " + textAsset.text + "}";

        PerformerListWrapper wrapper = JsonUtility.FromJson<PerformerListWrapper>(json);
        performers = new List<PerformerData>(wrapper.items);
    }


    [System.Serializable]
    public class PerformerListWrapper
    {
        public PerformerData[] items;
    }

    void LoadPerformersLevels()
    {
        for (int i = 0; i < performers.Count; i++)
            performers[i].localLevel = PlayerPrefs.GetInt("PerformerLevel_" + performers[i].name, 1);
    }

    void LoadPerformersPurchaseStatus()
    {
        for (int i = 0; i < performers.Count; i++)
        {
            bool isPurchased = i < INITIAL_ACTIVE_PERFORMERS || PlayerPrefs.GetInt("PerformerPurchased_" + performers[i].name, 0) == 1;
            performers[i].isPurchased = isPurchased;
        }
    }

    void SavePerformerLevel(PerformerData performer)
    {
        PlayerPrefs.SetInt("PerformerLevel_" + performer.name, performer.localLevel);
        PlayerPrefs.Save();
    }

    void SavePerformerPurchaseStatus(PerformerData performer)
    {
        PlayerPrefs.SetInt("PerformerPurchased_" + performer.name, performer.isPurchased ? 1 : 0);
        PlayerPrefs.Save();
    }

    void RenderPerformers()
    {
        foreach (Transform child in performersListParent) Destroy(child.gameObject);
        
        for (int i = 0; i < performers.Count; i++)
        {
            int idx = i;
            var p = performers[i];
            var go = Instantiate(performerUIPrefab, performersListParent);

            if (p.isPurchased)
            {
                // Show purchased performer info
                go.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = p.name + " (" + p.role + ")";
                go.transform.Find("LevelText").GetComponent<TextMeshProUGUI>().text = "Level: " + p.localLevel + " | Skill: " + p.GetSkill();
                go.transform.Find("UpgradeCostText").GetComponent<TextMeshProUGUI>().text = "" + p.GetUpgradeCost();

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

                Button upgradeBtn = go.transform.Find("UpgradeButton").GetComponent<Button>();
                upgradeBtn.onClick.RemoveAllListeners();
                upgradeBtn.onClick.AddListener(() => UpgradePerformer(idx));
                
                // Hide unlock GameObject
                Transform unlockObj = go.transform.Find("Unlock");
                if (unlockObj != null) unlockObj.gameObject.SetActive(false);
            }
            else
            {
                // Show locked performer with buy option
                go.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = "Locked Performer";
                go.transform.Find("LevelText").GetComponent<TextMeshProUGUI>().text = "";
                go.transform.Find("UpgradeCostText").GetComponent<TextMeshProUGUI>().text = "";
                go.transform.Find("QuirksText").GetComponent<TextMeshProUGUI>().text = "";

                Button upgradeBtn = go.transform.Find("UpgradeButton").GetComponent<Button>();
                upgradeBtn.onClick.RemoveAllListeners();
                upgradeBtn.onClick.AddListener(() => BuyPerformer(idx));
                
                // Change button text to BUY
                Transform buttonText = upgradeBtn.transform.Find("Text");
                if (buttonText != null)
                    buttonText.GetComponent<TextMeshProUGUI>().text = "BUY";

                // Show unlock GameObject
                Transform unlockObj = go.transform.Find("Unlock");
                if (unlockObj != null) unlockObj.gameObject.SetActive(true);
            }
        }
    }

    public void UpgradePerformer(int index)
    {
        var p = performers[index];
        if (!p.isPurchased) return;
        
        int cost = p.GetUpgradeCost();
        if (moneyController.SubtractMoney(cost))
        {
            p.localLevel++;
            SavePerformerLevel(p);
            RenderPerformers();
        }
    }

    public void BuyPerformer(int index)
    {
        var p = performers[index];
        if (p.isPurchased) return;
        
        int cost = p.GetPurchaseCost(); // You'll need to add this method to PerformerData
        if (moneyController.SubtractMoney(cost))
        {
            p.isPurchased = true;
            SavePerformerPurchaseStatus(p);
            RenderPerformers();
        }
    }
}