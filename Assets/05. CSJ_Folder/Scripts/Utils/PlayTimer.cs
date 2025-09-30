using System;
using System.Threading.Tasks;
using UnityEngine;


public class PlayTimer : Singleton<PlayTimer>
{
    public int verifyInervalSec = 60;
    private long serverSessionStartMs;
    
    private double localSec;
    private int verifiedServerSec;
    private int creditedSec;
    
    
    private bool isActive;
    private bool verifying;

    public async Task BeginSessionAsync()
    {
        await DatabaseManager.Instance.EnsureServerOffset();
        serverSessionStartMs = DatabaseManager.Instance.GetApproxServerTime();

        isActive = true;
        verifying = false;
        localSec = 0;
        verifiedServerSec = 0;
        creditedSec = 0;
    }

    private void Update()
    {
        if (!isActive) return;
        
        localSec += Time.unscaledDeltaTime;

        if ((int)localSec - verifiedServerSec >= verifyInervalSec && !verifying)
        {
            _ = VerifyServerAsync();
        }
        
    }
    private async Task VerifyServerAsync(int permitSec = 3)
    {
        if (verifying) return;
        verifying = true;
        try
        {
            long serverNow = DatabaseManager.Instance.GetApproxServerTime();

            int serverElapsedSec = (int)((serverNow - serverSessionStartMs) / 1000);
            int localElapsedSec = (int)Math.Floor(localSec);

            int verifiableSec = Math.Min(localElapsedSec, serverElapsedSec + permitSec);

            int deltaSec = verifiableSec - creditedSec;
            int deltaMin = deltaSec / 60;
            if (deltaMin > 0)
            {
                QuestSignalManager.Instance.Active(ActiveType.AutoCombat, deltaMin);
                creditedSec += deltaMin * 60;
            }

            verifiedServerSec = verifiableSec;

            if (localElapsedSec > serverElapsedSec + permitSec)
            {
                localSec = verifiedServerSec;
            }
        }
        finally
        {
            verifying = false;
        }
    }

    private async void OnApplicationFocus(bool hasFocus)
    {
        isActive = hasFocus;
        if (hasFocus)
        {
            await ReanchorServerTimeAsync();
            _ = VerifyServerAsync();
        }
    }

    private async void OnApplicationPause(bool pauseStatus)
    {
        isActive = !pauseStatus;
        if (!pauseStatus)
        {
            await ReanchorServerTimeAsync();
            _ = VerifyServerAsync();
        }
    }

    private async Task ReanchorServerTimeAsync()
    {
        await DatabaseManager.Instance.EnsureServerOffset();
        long serverNow = DatabaseManager.Instance.GetApproxServerTime();

        serverSessionStartMs = serverNow - verifiedServerSec * 1000L;
    }
}