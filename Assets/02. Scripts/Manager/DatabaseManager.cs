using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _05._CSJ_Folder.Scripts.Quest.Data;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class DatabaseManager : Singleton<DatabaseManager>
{
    private FirebaseUser _user;
    private string _uid;
    
    public event Action OnChangedNickname;
    public event Action OnChangedCreditData;
    
    public void Init()
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
    
    /// <summary>
    /// Firebase RTDB에 단일 데이터를 저장하는 메서드
    /// </summary>
    public async Task SaveFieldAsync(string tempPath, object value)
    {
        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        string path = $"{_uid}/" + tempPath;
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
    public async Task LoadFieldAsync<T>(string path, Action<T> callback, bool init = false, T value = default)
    {
        DatabaseReference dataRef = FirebaseManager.DataReference.Child(path);
        DataSnapshot snapshot = await dataRef.GetValueAsync();

        if (!snapshot.Exists || snapshot.Value == null)
        {
            if (init)
            {
                await dataRef.SetValueAsync(value);
                DataSnapshot newSnapshot = await dataRef.GetValueAsync();
                T newData = (T)Convert.ChangeType(newSnapshot.Value, typeof(T));
                callback(newData);
            }
            return;
        }
        
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

    public async Task<bool> CheckFieldAsync<T>(string subPath, Action<T> callback)
    {
        string path = $"{_uid}/" + subPath;
        
        DatabaseReference dataRef = FirebaseManager.DataReference.Child(path);
        DataSnapshot snapshot = await dataRef.GetValueAsync();

        if (!snapshot.Exists) return false;
        
        T newData = (T)Convert.ChangeType(snapshot.Value, typeof(T));
        callback(newData);
        return true;
    }
    
    #region Nickname
    
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

        await SaveFieldAsync($"PublicData/Nickname",nickname);

        OnChangedNickname?.Invoke();
    }
    
    /// <summary>
    /// FFirebase DB - PublicData에서 현재 유저 닉네임을 불러오는 메서드
    /// 반환값: DB에 저장된 유저 Nickname
    /// </summary>
    public async Task LoadNicknameAsync(Action<string> callback)
    {
        _uid = FirebaseManager.Auth.CurrentUser.UserId;
        DatabaseReference nicknameRef = FirebaseManager.DataReference.Child(_uid).Child("PublicData").Child("Nickname");
        Debug.Log($"{_uid}");    
        DataSnapshot snapshot = await nicknameRef.GetValueAsync();
        //string nickname = snapshot.Value.ToString();

        if (snapshot.Exists)
        {
            string nickname = snapshot.Value.ToString();
            Debug.Log($"닉네임 로드 성공 : {nickname}");
            callback(nickname);
        }
        else
        {
            Debug.LogWarning("닉네임 데이터 없음");
            callback(null);
        }
    }
    
    public void LoadNickname(Action<string> callback)
    {
        _uid = FirebaseManager.Auth.CurrentUser.UserId;
        DatabaseReference nicknameRef = FirebaseManager.DataReference.Child(_uid).Child("PublicData").Child("Nickname");
        Debug.Log($"{_uid}");    
        nicknameRef.GetValueAsync().ContinueWithOnMainThread((task)=>
        {
            if (task.IsCanceled || task.IsFaulted) return;

            if (task.Result == null)
            {
                Debug.LogWarning("닉네임 데이터 없음");
                callback(null);
                return;
            }
            
            string nickname = task.Result.ToString();
            Debug.LogWarning($"닉네임 로드 성공 : {nickname}");
            callback(nickname);
        });
    }
    
    #endregion

    #region Time

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
    public async Task<long> LoadLogOutTime()
    {
        var snapshot = await FirebaseManager.DataReference.Child(_uid).Child("UserData").Child("LogOutTime")
            .GetValueAsync();
        if (snapshot.Exists)
        {
            return Convert.ToInt64(snapshot.Value);
        }
        return 0;
    }

    public async Task<long> CurrentServerTime()
    {
        DatabaseReference tempRef = FirebaseManager.DataReference.Child("CurrentTime").Push();
        await tempRef.SetValueAsync(ServerValue.Timestamp);
        DataSnapshot timeSnapshot = await tempRef.GetValueAsync();
        long serverTime = Convert.ToInt64(timeSnapshot.Value);
        await tempRef.RemoveValueAsync();
        return serverTime;
    }
    
    public async Task<long> CheckOfflineTime()
    {
        long logOutTime = await LoadLogOutTime();
        long currentTime = await CurrentServerTime();
        return currentTime / 1000 - logOutTime;
    }
    
    #endregion
    
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
    
    public void LoadPackageData(string packageId, Action<int, Dictionary<string, object>> callback)
    {
        DatabaseReference packageRef = FirebaseManager.DataReference
            .Child("SharedData").Child("PackageData").Child(packageId);
        packageRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                var data = task.Result;
                int price = Convert.ToInt32(data.Child("Price").Value);
                Dictionary<string, object> reward = (Dictionary<string, object>)data.Child("Reward").Value;
                callback(price, reward);
            }
        });
    }
    
    #endregion
    
    #region Currency
    
    private void CreditValueChanged(object sender, ValueChangedEventArgs args)
    {
        DataSnapshot snapshot = args.Snapshot;

        if (snapshot.Exists && snapshot.HasChild("Gold"))
        {
            CurrencyManager.Instance.AddCurrencyFromInspectorString(CurrencyType.Gold,
                snapshot.Child("Gold").Value.ToString());
        }
        if (snapshot.Exists && snapshot.HasChild("Gem"))
        {
            CurrencyManager.Instance.AddCurrencyFromInspectorString(CurrencyType.Gem,
                snapshot.Child("Gem").Value.ToString());
        }
        if (snapshot.Exists && snapshot.HasChild("EnhancementStone"))
        {
            CurrencyManager.Instance.AddCurrencyFromInspectorString(CurrencyType.EnhancementStone,
                snapshot.Child("EnhancementStone").Value.ToString());
        }
        OnChangedCreditData?.Invoke();
    }
    
    public void DisplayCreditData()
    {
        DatabaseReference creditRef = FirebaseManager.DataReference.Child(_uid).Child("CreditData");
        creditRef.ValueChanged += CreditValueChanged;
    }
    
    public void AddCurrency(string type, int value)
    {
        DatabaseReference goldRef = FirebaseManager.DataReference.Child(_uid).Child("CreditData").Child(type);
        goldRef.RunTransaction(data =>
        {
            if (data.Value == null)
            {
                data.Value = value;
            }
            else
            {
                int currentGold = Convert.ToInt32(data.Value);
                data.Value = currentGold + value;
            }
            return TransactionResult.Success(data);
        });
    }
    
    public void SpendCurrency(string type, int value)
    {
        DatabaseReference goldRef = FirebaseManager.DataReference.Child(_uid).Child("CreditData").Child(type);
        goldRef.RunTransaction(data =>
        {
            if (data.Value == null)
            {
                data.Value = 0;
            }
            else
            {
                int currentGold = Convert.ToInt32(data.Value);
                if (currentGold >= value)
                {
                    Debug.LogWarning($"[DataBaseManager] : SpendCurrency 재화 소모 진입, 소모 재화 : {value}");
                    data.Value = currentGold - value;
                }
                else
                {
                    return TransactionResult.Abort();
                }
            }
            return TransactionResult.Success(data);
        });
    }
    #endregion

    #region Attendance

    public void Attendance(Action<int> callback)
    {
        DatabaseReference goldRef = FirebaseManager.DataReference.Child(_uid).Child("UserData").Child("Date");
        goldRef.RunTransaction(data =>
        {
            if (data.Value == null)
            {
                data.Value = 1;
                callback(1);
            }
            else
            {
                int curDate = Convert.ToInt32(data.Value);
                data.Value = curDate + 1;
                callback(curDate + 1);
            }
            
            return TransactionResult.Success(data);
        });
        FirebaseManager.DataReference.Child(_uid).Child("UserData").Child("RewardTime")
            .SetValueAsync(ServerValue.Timestamp);
    }

    private async Task<bool> CheckAttendance()
    {
        var snapshot = await FirebaseManager.DataReference.Child(_uid).Child("UserData").Child("RewardTime")
            .GetValueAsync();

        long rewardTime = snapshot.Exists ? long.Parse(snapshot.Value.ToString()) : 0;

        long serverTime = await CurrentServerTime();
        
        DateTime currentTime = DateTimeOffset.FromUnixTimeMilliseconds(serverTime).UtcDateTime;
        DateTime currentTimeKor = currentTime.AddHours(9);

        DateTime resetTime = new DateTime(currentTimeKor.Year, currentTimeKor.Month, currentTimeKor.Day, 6, 0, 0);
        if (currentTimeKor.Hour < 6)
        {
            resetTime = resetTime.AddDays(-1);
        }
        
        DateTime lastRewardTime = DateTimeOffset.FromUnixTimeMilliseconds(rewardTime).UtcDateTime;

        return lastRewardTime < resetTime;
    }

    public async Task CheckTodayReward(Action callback)
    {
        if (await CheckAttendance())
        {
            Debug.Log("출석 가능");
            callback();
        }
        else
        {
            Debug.Log("출석 불가");
        }
    }

    #endregion

    public async Task SaveCrewDataAsync(string subPath, PlayerCharacterData data)
    {
        Init();
        var root = $"{_uid}/{subPath}";
        var crewData = new Dictionary<string, object>();
        
        crewData[$"{root}/Level"] = data.Level;
        crewData[$"{root}/Star"] = data.Star;
            
        await SaveFieldsAsync(crewData);
    }

    #region QuestCheck (AddedByCSJ)
    
    // reset 기준 UTC 기준 시간선; 기본인 9는 Kst인 UTC+09:00 기준입니다.
    private const int _stdResetTime = 9;
    private long _serverOffset = long.MinValue;
    private long _offsetUpdatedLocal;

    private const string LocalDailyKey = "DailyQuestTime";
    private const string LocalWeeklyKey = "WeeklyQuestTime";
    # region Daily
    /// <summary>
    /// 일일 퀘스트 초기화가 필요한지 체크합니다. 
    /// </summary>
    /// <param name="resetTime">기본 기준 시간입니다. </param>
    /// <returns>일일 초기화가 필요한지 bool 값으로 반환합니다.</returns>
    public async Task<bool> DailyCheckIn(int resetTime = 6)
    {
        Init();
        
        // server 시간을 받아와서 기준 시간대에 맞춥니다.
        long serverTime = await CurrentServerTime();
        DateTime _stdNow = GetResetTime(serverTime);

        // 서버 시간을 기준으로 초기화 경계를 받아옵니다.
        var dailyBoundary = GetDailyBoundary(_stdNow, resetTime);

        // 마지막으로 일일 퀘스트를 진행한 시간을 기준 시간으로 받아옵니다.
        DateTime lastTime = await GetDailyQuestTimeToStd();
        
        // 만약 마지막으로 진행한 기록이 없다면 ≒ 클리어 횟수가 0회인 경우 true를 반환합니다.
        if (lastTime == DateTime.MinValue) return true;
        
        // 진행한 기록이 있다면 현재 초기화 경계가 마지막 진행한 날을 넘었는지 확인하여 값을 반환합니다.
        return lastTime < dailyBoundary;
    }

    /// <summary>
    /// 일일 초기화 경계를 받아옵니다.
    /// </summary>
    private DateTime GetDailyBoundary(DateTime time,int resetTime)
    {
        // 현재의 날짜까지와 시는 일일 퀘스트 초기화 기준 시로 DateTime을 생성합니다.
        var dailyBoundary = new DateTime(time.Year, time.Month, time.Day, resetTime, 0, 0, DateTimeKind.Utc);
        
        // 만약 현재 시간의 시가 아직 초기화 기준 시(resetTime)을 넘지 못한 경우 전날의 경계로 넘깁니다. 
        if (time < dailyBoundary)
        {
            dailyBoundary = dailyBoundary.AddDays(-1);
        }

        // 경계를 반환합니다.
        return dailyBoundary;
    }

    /// <summary>
    /// 현재 서버 시각을 일일 퀘스트 초기화 시간으로 저장합니다.
    /// </summary>
    public async Task SetDailyQuestTime()
    {
        Init();
        long serverTime = await CurrentServerTime();
        
        // 유저 데이터의 DailyQuestTime에 현재의 서버 시간을 저장합니다.
        await FirebaseManager.DataReference.Child(_uid).Child("UserData")
            .Child("DailyQuestTime").SetValueAsync(serverTime);
        
        // 현재 Local에서도 server 시간을 저장합니다. (서버와의 통신을 줄이기 위해)
        SetLocalDaily(serverTime);
    }

    /// <summary>
    /// 마지막 일일 초기화 시각을 ms로 반환합니다.
    /// </summary>
    /// <returns>최종 퀘스트 초기화 시점. ms로 반환, 없으면 0으로 반환</returns>
    private async Task<long> GetDailyQuestTime()
    {
        Init();
        // 유저 데이터의 DailyQuestTime에서 데이터를 받아옵니다.
        var snapshot = await FirebaseManager.DataReference.Child(_uid).Child("UserData")
            .Child("DailyQuestTime").GetValueAsync();
        // 값이 존재한다면 int값으로 반환합니다.
        if (snapshot.Exists)
        {
            return Convert.ToInt64(snapshot.Value);
        }
        return 0;
    }

    /// <summary>
    /// 마지막 일일 퀘스트 초기화 시점을 UTC 초기화 기준 시간으로 반환합니다.
    /// 기본 UTC+09
    /// </summary>
    /// <returns>최종 퀘스트 초기화 시점, 없으면 DateTime.MinValue</returns>
    private async Task<DateTime> GetDailyQuestTimeToStd()
    {
        // 마지막 클리어 시간을 long 값으로 받습니다.
        var ms = await GetDailyQuestTime();
        
        // 만약 ms의 값이 없다면 Min값을 반환하고 존재한다면 해당 시간을 기준 시간으로 반환합니다.
        return ms <= 0 ? DateTime.MinValue : GetResetTime(ms);
    }
    #endregion
    
    /// <summary>
    /// long 값을 받아서 유저의 UTC 초기화 기준 시간에 맞춰 DateTime값을 반환합니다.
    /// 기본 UTC+09
    /// </summary>
    private DateTime GetResetTime(long time)
    {
        // time 값을 UtcDateTime에 현재 유저의 UTC 초기화 기준 시를 더해서 DateTimeOffset으로 반환합니다. 
        return DateTimeOffset.FromUnixTimeMilliseconds(time).UtcDateTime.AddHours(_stdResetTime);
    }
    
    #region Weekly

    /// <summary>
    /// 주간 퀘스트 초기화 여부를 확인합니다.
    /// </summary>
    /// <param name="resetDay">초기화 기준 요일입니다. 기본 값은 월요일입니다.</param>
    /// <param name="resetTime">일일 초기화 기준 시각입니다. </param>
    /// <returns></returns>
    public async Task<bool> WeeklyCheckIn(DayOfWeek resetDay = DayOfWeek.Monday, int resetTime = 6)
    {
        Init();
        
        // 서버 시간을 기준 시로 반환 받습니다.
        var serverTime = await CurrentServerTime();
        var _stdNow = GetResetTime(serverTime);
        
        // 주간 퀘스트 경계를 반환 받습니다.
        DateTime boundary = GetWeeklyBoundary(_stdNow, resetDay, resetTime);
        
        // 마지막 주간 퀘스트 수행 시간을 반환 받습니다.
        var lastTime = await GetWeeklyQuestTimeToStd();
        // 수행 기록이 없다면 초기화를 해야한다 => true로 반환합니다.
        if (lastTime == DateTime.MinValue) return true;
        
        // 퀘스트 초기화 경계가 마지막 수행 시간보다 큰지 반환합니다.
        return lastTime < boundary;
    }

    /// <summary>
    /// 주간 초기화 시간을 추가합니다 현재의 시간과 요일을 입력합니다
    /// </summary>
    public async Task SetWeeklyQuestTime()
    {
        Init(); 
        
        var userData = FirebaseManager.DataReference.Child(_uid).Child("UserData");
        var serverTime = await CurrentServerTime();
        
        // WeeklyQuestTime에 현재 서버 시간을 저장합니다.
        await userData.Child("WeeklyQuestTime").SetValueAsync(serverTime);
        DateTime _stdNow = GetResetTime(serverTime);
        
        // WeeklyQuestDay에 현재 요일 값을 저장합니다.
        await userData.Child("WeeklyQuestDay").SetValueAsync((int)_stdNow.DayOfWeek);
        
        // Local에도 데이터 처리를 위해 server 시간을 저장합니다.
        SetLocalWeekly(serverTime);
    }

    /// <summary>
    /// 주간 퀘스트의 클리어 시간을 받아옵니다. 
    /// </summary>
    private async Task<long> GetWeeklyQuestTime()
    {
        Init();
        // 퀘스트 클리어 시간을 데이터에서 받아옵니다.
        var snapshot = await FirebaseManager.DataReference.Child(_uid).Child("UserData")
            .Child("WeeklyQuestTime").GetValueAsync();
        // 만일 값이 존재한다면 반환합니다.
        if (snapshot.Exists)
        {
            return Convert.ToInt64(snapshot.Value);
        }
        return 0;
    }
    
    /// <summary>
    /// 마지막 주간 퀘스트 초기화 시점을 UTC 초기화 기준 시간으로 반환합니다.
    /// 기본 UTC+09
    /// </summary>
    /// <returns>최종 퀘스트 초기화 시점, 없으면 DateTime.MinValue</returns>
    private async Task<DateTime> GetWeeklyQuestTimeToStd()
    {
        // 마지막 클리어 시간을 long 값으로 받습니다.
        var ms = await GetWeeklyQuestTime();
        
        // 만약 ms의 값이 없다면 Min값을 반환하고 존재한다면 해당 시간을 기준 시간으로 반환합니다.
        return ms <= 0 ? DateTime.MinValue : GetResetTime(ms);
    }

    /// <summary>
    /// 마지막 퀘스트 수행 요일을 받아옵니다.
    /// </summary>
    // 추후 사용할 수도 있어 만들었습니다. 현재는 미사용
    private async Task<DayOfWeek> GetWeeklyQuestDay()
    {
        Init();
        
        var snapshot = await FirebaseManager.DataReference.Child(_uid).Child("UserData")
            .Child("WeeklyQuestDay").GetValueAsync();
        if (snapshot.Exists)
        {
            return (DayOfWeek)Convert.ToInt32(snapshot.Value);
        }
        return DayOfWeek.Monday;
    }

    /// <summary>
    /// 주간 퀘스트가 초기화되는 경계를 받아옵니다. (현재 시간을 기점으로 마지막으로 초기화된 경계)
    /// </summary>
    private static DateTime GetWeeklyBoundary(DateTime now, DayOfWeek resetDay, int resetTime)
    {
        // 경계 기본 값을 지정합니다.
        var BaseToday = new DateTime(now.Year, now.Month, now.Day, resetTime, 0, 0, DateTimeKind.Utc);
      
        // 오늘의 요일에서 초기화 요일을 빼서 초기화 요일 기준으로 경계를 지정합니다.
        var delta = ((int)now.DayOfWeek - (int)resetDay + 7) % 7;
        var Boundary = BaseToday.AddDays(-delta);
        
        // 만일 초기화 날짜는 지났지만 시간이 지나지 못했다면 저번주의 경계로 바꿉니다. 
        if(now < Boundary)
            Boundary = Boundary.AddDays(-7);
        
        return Boundary;
    }
    #endregion

    #region Offset


    /// <summary>
    /// 서버의 Offset을 설정합니다
    /// </summary>
    /// <param name="maxAge"></param>
    public async Task EnsureServerOffset(int maxAge = 300)
    {
       long nowLocalMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (_serverOffset == long.MinValue || _offsetUpdatedLocal + maxAge * 1000L < nowLocalMs)
        {
            long serverMs = await CurrentServerTime();
            _serverOffset = serverMs - nowLocalMs;
            _offsetUpdatedLocal = nowLocalMs;
        } 
    }

    public long GetApproxServerTime()
    {
        if(_serverOffset == long.MinValue) return 0;
        return DateTimeOffset.Now.ToUnixTimeMilliseconds() + _serverOffset;
    }

    public void SetLocalDaily(long ms)
    {
        PlayerPrefs.SetString(LocalDailyKey, ms.ToString());
        PlayerPrefs.Save();
    }
    
    private void SetLocalWeekly(long ms)
    {
        PlayerPrefs.SetString(LocalWeeklyKey, ms.ToString());
        PlayerPrefs.Save();
    }

    public long GetLocalDaily()
    {
        if (long.TryParse(PlayerPrefs.GetString(LocalDailyKey, "0"), out var v)) return v;
        return 0;
    }
    public long GetLocalWeekly()
    {
        if (long.TryParse(PlayerPrefs.GetString(LocalWeeklyKey, "0"), out var v)) return v;
        return 0;
    }

    public bool QuickDailyCheck(int resetTime = 6)
    {
        long approxMs = GetApproxServerTime();
        if (approxMs == 0) return true;
        
        DateTime nowStd = GetResetTime(approxMs);
        DateTime dailyBoundary = GetDailyBoundary(nowStd, resetTime);
        
        long lastMs = GetLocalDaily();
        if (lastMs <= 0) return true;
        DateTime lastStd = GetResetTime(lastMs);
        return lastStd < dailyBoundary;
    }

    public bool QuickWeeklyCheck(DayOfWeek resetDay = DayOfWeek.Monday, int resetTime = 6)
    {
        long approxMs = GetApproxServerTime();
        if (approxMs == 0) return true;
        
        DateTime nowStd = GetResetTime(approxMs);
        DateTime boundary = GetWeeklyBoundary(nowStd, resetDay, resetTime);
        
        long lastMs = GetLocalWeekly();
        if (lastMs <= 0) return true;
        DateTime lastStd = GetResetTime(lastMs);
        return lastStd < boundary;
    }

    /// <summary>
    /// {_uid}/{subPath}에 데이터를 저장합니다.
    /// </summary>
    public async Task SaveQuestDataAsync(string subPath, QuestData data)
    {
        Init();
        var root = $"{_uid}/{subPath}";
        var payload = new Dictionary<string, object>();

        // General Quest 칸이 null이 아닌 경우, 해당 General의 내용을 딕셔너리화
        if (data?.General != null)
        {
            payload[$"{root}/General/ActiveQuestId"] = data.General.ActiveQuestId ?? string.Empty;
            payload[$"{root}/General/ActiveState"] = data.General.ActiveState;
            payload[$"{root}/General/ActiveProgress"] = data.General.ActiveProgress;
            
            payload[$"{root}/General/ClearedQuestCount"] = data.General.ClearedQuestCount;
            payload[$"{root}/General/CurrentQuestStage"] = data.General.CurrentQuestStage; 
        }

        // Temporary 칸이 null이 아닌 경우 딕셔너리화
        // Temporary는 Dictionary<string (questId), TemporaryQuestData>
        if (data?.Temporary != null)
        {
            foreach (var kv in data.Temporary)
            {
                //questId는 해당 데이터의 key 값으로 등록
                string questId = kv.Key ?? "";
                var t = kv.Value;
                if (string.IsNullOrEmpty(questId) || t == null) continue;

                payload[$"{root}/Temporary/{questId}/QuestType"] = t.QuestType;
                payload[$"{root}/Temporary/{questId}/State"] = t.State;
                payload[$"{root}/Temporary/{questId}/Progress"] = t.Progress;
            }

        }

        if (data?.Tutorial is not null)
        {
            payload[$"{root}/Tutorial/Cleared"] = data.Tutorial.ClearedTutorialQuestIds;
        }
        
        // 해당 payload들을 딕셔너리로 저장
        await SaveFieldsAsync(payload);
    }
    
    /// <summary>
    /// {_uid}/{subPath}에서 데이터를 로드합니다.
    /// </summary>
    public async Task<QuestData> LoadQuestDatatAsync(string subPath)
    {
        Init();
        var dataRef = FirebaseManager.DataReference.Child(_uid).Child(subPath);
        var snapshot = await dataRef.GetValueAsync();
        if (!snapshot.Exists || snapshot.Value == null) return null;
        
        // 반환할 QuestData 객체 생성
        var result = new QuestData
        {
            General = new GeneralQuestData(),
            Temporary = new Dictionary<string, TemporaryQuestData>(),
            Tutorial = new TutorialQuestData(),
        };

        // 반환 받은 데이터에서 General의 자식들을 받아옴
        var g = snapshot.Child("General");
        // General 데이터가 존재한다면 result에 해당 값을 등록
        if (g.Exists)
        {
            result.General.ActiveQuestId = g.Child("ActiveQuestId").Value?.ToString() ?? string.Empty;
            result.General.ActiveState = ToInt(g.Child("ActiveState").Value, 0);
            result.General.ActiveProgress = ToInt(g.Child("ActiveProgress").Value, 0);
            
            result.General.ClearedQuestCount = ToInt(g.Child("ClearedQuestCount").Value, 0);
            result.General.CurrentQuestStage = ToInt(g.Child("CurrentQuestStage").Value, 1);
        }
        Debug.Log($"Load QuestID : {result.General.ActiveQuestId}");

        // 반환 받은 데이터에서 Temporary의 자식들을 받아옴
        var tNode = snapshot.Child("Temporary");
        // 해당 데이터를 result에 등록
        if (tNode.Exists)
        {
            foreach (var child in tNode.Children)
            {
                string questId = child.Key;
                if (string.IsNullOrEmpty(questId)) continue;

                var t = new TemporaryQuestData
                {
                    QuestType = ToInt(child.Child("QuestType").Value, 0),
                    State = ToInt(child.Child("State").Value, 0),
                    Progress = ToInt(child.Child("Progress").Value, 0),
                };
                result.Temporary[questId] = t;
            }
        }
        
        var TutoNode = snapshot.Child("Tutorial");
        if (TutoNode.Exists)
        {
            var clearedNode = TutoNode.Child("Cleared");
            result.Tutorial.ClearedTutorialQuestIds = ReadStringListFromSnapshot(clearedNode);
        }
        else
        {
            result.Tutorial.ClearedTutorialQuestIds = new List<string>();
        }


        return result;
    }
    
    /// <summary>
    /// 데이터베이스에서 받은 Object 값을 int 값으로 변환해서 반환해주는 메서드
    /// </summary>
    /// <param name="value"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    private static int ToInt(object value, int defaultValue)
    {
        if (value == null) return defaultValue;
        return Convert.ToInt32(value);
    }

    private static List<string> ReadStringListFromSnapshot(DataSnapshot snapshot)
    {
        var result = new List<string>();
        if (snapshot is null || !snapshot.Exists) return result;

        if (snapshot.Value is IList list)
        {
            foreach (var item in list)
            {
                var s = item as string ?? item?.ToString();
                if (!string.IsNullOrEmpty(s))
                    result.Add(s);
            }

            return result;
        }

        var indexs = new List<(int idx, string val)>();
        foreach (var child in snapshot.Children)
        {
            if (int.TryParse(child.Key, out var idx))
            {
                var s = child.Value as string ?? child.Value?.ToString();
                if (!string.IsNullOrEmpty(s))
                    indexs.Add((idx, s));
            }
        }
        if (indexs.Count > 0)
        {
            indexs.Sort((a, b) => a.idx.CompareTo(b.idx));
            result.AddRange(indexs.Select(p => p.val));
            return result;
        }

        foreach (var child in snapshot.Children)
        {
            var s = child.Value as string ?? child.Value?.ToString();
            if (!string.IsNullOrEmpty(s))
                result.Add(s);
        }
        return result;
    }
    #endregion
    
    
    #endregion
}
