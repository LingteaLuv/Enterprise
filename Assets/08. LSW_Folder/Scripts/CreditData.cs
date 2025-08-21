using UnityEngine;

public class PlayerData
{
    private PlayerCreditData _creditData = new PlayerCreditData();
    private PlayerPrivateData _privateData = new PlayerPrivateData();
    
    public PlayerCreditData CreditData => _creditData;
    public PlayerPrivateData PrivateData => _privateData;
}

[System.Serializable]
public class PlayerCreditData
{
    public int Gold { get; set; }
    public int Diamond { get; set; }
}

[System.Serializable]
public class PlayerPrivateData
{
    public bool IsPassPurchased { get; set; }
    public int IsPackagePurchased { get; set; }
}

