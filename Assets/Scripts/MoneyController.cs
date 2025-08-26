using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoneyController : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    private float money;

    private void Start()
    {
        money = PlayerPrefs.GetFloat("Money", 2000);
        UpdateMoneyText();
    }

    private void Update()
    {
        UpdateMoneyText();
    }

    public void AddMoney(float amount)
    {
        money += amount;
        SaveMoney();
        UpdateMoneyText();
    }
    public bool SubtractMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            SaveMoney();
            UpdateMoneyText();
            Debug.Log("Operation completed successfully.");
            return true;
        }
        else
        {
            Debug.LogWarning("Not enough coins to complete the operation.");
            return false;
        }
    }

    private void UpdateMoneyText()
    {
        if (money < 5)
        {
            money = 5;
            SaveMoney();
        }
        moneyText.text = "" + money.ToString("F0");
    }

    private void SaveMoney()
    {
        PlayerPrefs.SetFloat("Money", money);
        PlayerPrefs.Save();
    }
    
    // Метод для сброса денег к начальному значению
    public void ResetMoney()
    {
        money = 2000; // Initial amount of money
        SaveMoney();
        UpdateMoneyText();
        Debug.Log("Money reset to initial value: " + money);
    }

    private void OnApplicationQuit()
    {
        SaveMoney();
    }
}