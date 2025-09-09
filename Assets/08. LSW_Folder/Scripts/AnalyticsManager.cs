using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;

public class AnalyticsManager : Singleton<AnalyticsManager>
{
    public void LogLevelComplete(int level)
    {
        var parameters = new List<Parameter>
        {
            new Parameter("level", level)
        };
        
        FirebaseAnalytics.LogEvent("level_complete", parameters.ToArray());
        Debug.Log($"[Analytics] : {level} level complete event logged");
    }

    public void LogLevelStart(int level)
    {
        FirebaseAnalytics.LogEvent("level_start", new Parameter("level", level));
        Debug.Log($"[Analytics] : {level} level start event logged");
    }
}
