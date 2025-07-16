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
            Debug.Log("Операция выполнена успешно.");
            return true;
        }
        else
        {
            Debug.LogWarning("Недостаточно монет для выполнения операции.");
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

    private void OnApplicationQuit()
    {
        SaveMoney();
    }
}