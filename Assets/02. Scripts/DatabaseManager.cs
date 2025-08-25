using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class DatabaseManager : Singleton<DatabaseManager>
{
    private FirebaseUser _user;
    private string _uid;
    private PlayerData _playerData;
    public PlayerData PlayerData => _playerData;
    
    private void Start()
    {
        _playerData = new PlayerData();
    }
    
    public event Action OnChangedNickname;
    public event Action OnChangedCreditData;
    
    private void Init()
    {
        _user = FirebaseManager.Auth.CurrentUser;
        Debug.Log($"_user : {_user}");
        Debug.Log($"_uid : {_user.UserId}");
        _uid = _user.UserId;
    }

    public async Task DeleteDataAsync()
    {
        Init();
        await FirebaseManager.DataReference.Child(_uid).RemoveValueAsync();
    }

    #region Nickname
    
    /// <summary>
    /// Firebase RTDB에 단일 데이터를 저장하는 메서드
    /// </summary>
    public async Task SaveFieldAsync(string path, object value)
    {
        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        dictionary[path] = value;
        
        var task = FirebaseManager.DataReference.UpdateChildrenAsync(dictionary);
        await task; 

        if (task.IsCompletedSuccessfully)
        {
            Debug.Log("데이터 저장 성공");
        }
        else
        {
            Debug.LogError("데이터 저장 실패");
        }
    }

    /// <summary>
    /// Firebase RTDB에 다중 데이터(Dictionary)를 저장하는 메서드
    /// </summary>
    public async Task SaveFieldsAsync(Dictionary<string, object> dictionary)
    {
        var task = FirebaseManager.DataReference.UpdateChildrenAsync(dictionary);
        await task; 

        if (task.IsCompletedSuccessfully)
        {
            Debug.Log("데이터 저장 성공");
        }
        else
        {
            Debug.LogError("데이터 저장 실패");
        }
    }
    
    /// <summary>
    /// Firebase RTDB에 다중 데이터(Custom class)를 저장하는 메서드
    /// </summary>
    public async Task SaveFieldsAsync<T>(T data) where T : class
    {
        string json = JsonUtility.ToJson(data);
        Dictionary<string, object> dictionary= Utility.JsonToDict(json);
        
        if (dictionary == null) return;
        
        var task = FirebaseManager.DataReference.Child(typeof(T).Name).UpdateChildrenAsync(dictionary);
        await task; 

        if (task.IsCompletedSuccessfully)
        {
            Debug.Log("데이터 저장 성공");
        }
        else
        {
            Debug.LogError("데이터 저장 실패");
        }
    }
    
    /// <summary>
    /// Firebase RTDB에서 단일 데이터를 불러오는 메서드
    /// </summary>
    public async Task LoadFieldAsync<T>(string path, Action<T> callback)
    {
        DatabaseReference dataRef = FirebaseManager.DataReference.Child(path);
        DataSnapshot snapshot = await dataRef.GetValueAsync();

        if (!snapshot.Exists || snapshot.Value == null) return;
        
        T data = (T)Convert.ChangeType(snapshot.Value, typeof(T));
        callback(data);
    }
    
    /// <summary>
    /// Firebase RTDB에서 다중 데이터(Custom class)를 불러오는 메서드
    /// </summary>
    public async Task LoadFieldsAsync<T>(Action<T> callback) where T : class
    {
        DatabaseReference dataRef = FirebaseManager.DataReference.Child(_uid).Child(typeof(T).Name);
        DataSnapshot snapshot = await dataRef.GetValueAsync();

        if (!snapshot.Exists || snapshot.Value == null) return;

        Dictionary<string, object> dictionary = snapshot.Value as Dictionary<string, object>;

        string json = Utility.DictToJson(dictionary);
        T data = JsonUtility.FromJson<T>(json);
        callback(data);
    }
    
    /// <summary>
    /// 유저의 닉네임을 입력받은 값으로 DB에 저장하는 메서드 
    /// </summary>
    /// <param name="newNickname">변경할 닉네임</param>
    public async Task SetNickname(string newNickname = "Guest")
    {
        Init();
        string nickname;
        if (_user.IsAnonymous)
        {
            nickname = $"게스트{UnityEngine.Random.Range(1000, 10000)}";
        }
        else
        {
            nickname = newNickname;
        }

        await SaveFieldAsync($"{_uid}/PublicData/Nickname",nickname);

        OnChangedNickname?.Invoke();
    }
    
    /// <summary>
    /// FFirebase DB - PublicData에서 현재 유저 닉네임을 불러오는 메서드
    /// 반환값: DB에 저장된 유저 Nickname
    /// </summary>
    public async Task LoadNicknameAsync(Action<string> callback)
    {
        DatabaseReference nicknameRef = FirebaseManager.DataReference.Child(_uid).Child("PublicData").Child("Nickname");

        DataSnapshot snapshot = await nicknameRef.GetValueAsync();
        string nickname = snapshot.Value.ToString();
        Debug.Log($"LoadNickname 닉네임 : {nickname}");

        if (snapshot.Exists)
        {
            //string nickname = snapshot.Value.ToString();
            Debug.Log($"닉네임 로드 성공 : {nickname}");
            callback(nickname);
        }
        else
        {
            Debug.LogWarning("닉네임 데이터 없음");
            callback(null);
        }
    }
    #endregion
    
    /// <summary>
    /// 오프라인이 되었을 때 접속 종료 시각을 저장하는 메서드
    /// </summary>
    public void SaveLogOutTime()
    {
        Init();
        
        DatabaseReference userRef = FirebaseManager.DataReference.Child(_uid).Child("UserData");
        long unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        userRef.Child("LogOutTime").SetValueAsync(unixTime);
        //userRef.Child("IsOnline").SetValueAsync(false);
    }

    /// <summary>
    /// 온라인이 되었을 때 보상을 계산하기 위해 최근 종료 시각을 불러오는 메서드
    /// </summary>
    /// <param name="callback"></param>
    public void LoadLogOutTime(Action<long> callback)
    {
        FirebaseManager.DataReference.Child(_uid).Child("UserData").Child("LogOutTime")
            .GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                long savedTime = Convert.ToInt64(task.Result.Value);
                callback(savedTime);
            }
            else
            {
                callback(0);
            }
        });
    }

    #region Package

    public void SavePackage(string packageName)
    {
        Init();
        
        DatabaseReference userRef = FirebaseManager.DataReference.Child(_uid).Child("PrivateData");
        userRef.Child(packageName).SetValueAsync(true);
    }
    
    public void LoadPackage(string packageName, Action<bool> callback)
    {
        Init();
        
        DatabaseReference userRef = FirebaseManager.DataReference.Child(_uid).Child("PrivateData");
        userRef.Child(packageName).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                object result = task.Result.Value;
                if (bool.TryParse(result.ToString(), out bool isPurchase))
                {
                    callback(isPurchase);
                }
            }
            else
            {
                callback(false);
            }
        });
    }

    public void LoadAllPackageData(Action<string> callback)
    {
        DatabaseReference packageRef = FirebaseManager.DataReference
            .Child("SharedData").Child("PackageData");
        packageRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                foreach (var package in task.Result.Children)
                {
                    string packageId = package.Key;
                    Debug.Log($"LoadAllPackageData : packageId = {packageId}");
                    callback(packageId);
                }
            }
        });
    }
    
    public void LoadPackageData(string packageId, Action<string, bool> callback)
    {
        DatabaseReference packageRef = FirebaseManager.DataReference
            .Child("SharedData").Child("PackageData").Child(packageId);
        packageRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                var data = task.Result;
                string price = data.Child("Price").Value.ToString();
                bool isPurchased = Convert.ToBoolean(data.Child("IsPurchased").Value);
                callback(price, isPurchased);
            }
        });
    }
    
    #endregion
    
    
    private void CreditValueChanged(object sender, ValueChangedEventArgs args)
    {
        DataSnapshot snapshot = args.Snapshot;

        if (snapshot.Exists && snapshot.HasChild("Gold"))
        {
            long gold = (long)snapshot.Child("Gold").Value;
            _playerData.CreditData.Gold = (int)gold;
        }
        else
        {
            _playerData.CreditData.Gold = 0;
        }
        OnChangedCreditData?.Invoke();
    }
    
    public void DisplayCreditData()
    {
        DatabaseReference creditRef = FirebaseManager.DataReference.Child(_uid).Child("CreditData");
        creditRef.ValueChanged += CreditValueChanged;
    }

    #region Test

    public void PlusGold()
    {
        DatabaseReference goldRef = FirebaseManager.DataReference.Child(_uid).Child("CreditData").Child("Gold");
        goldRef.RunTransaction(data =>
        {
            if (data.Value == null)
            {
                data.Value = 1;
            }
            else
            {
                int currentGold = Convert.ToInt32(data.Value);
                data.Value = currentGold + 1;
            }
            return TransactionResult.Success(data);
        });
    }
    
    public void MinusGold()
    {
        DatabaseReference goldRef = FirebaseManager.DataReference.Child(_uid).Child("CreditData").Child("Gold");
        goldRef.RunTransaction(data =>
        {
            if (data.Value == null)
            {
                data.Value = 0;
            }
            else
            {
                int currentGold = Convert.ToInt32(data.Value);
                if (currentGold > 0)
                {
                    data.Value = currentGold - 1;
                }
                else
                {
                    data.Value = 0;
                }
            }
            return TransactionResult.Success(data);
        });
    }
    #endregion
}
