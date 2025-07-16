using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Serialization;

public class ShopDonatController : MonoBehaviour
{
    
    public TextMeshProUGUI rewardText;
    public TextMeshProUGUI money1Text;
    public GameObject purschaisePanel;
    [SerializeField] private MoneyController _wallet;
    
  

    public string donat = "road.jumper.coinspack";



public void UpdateMoney1(Product product)
    {
        money1Text.text = product.metadata.localizedPrice + " " + product.metadata.isoCurrencyCode;
    }

    
    public void OnPurchaseComplete(Product product)
    {
        Debug.Log("Покупка прошла успешно");
        if (product.definition.id == donat)
        {
           _wallet.AddMoney(20000);
            rewardText.text = "20000";
            purschaisePanel.SetActive(true);
        }
    }

}
