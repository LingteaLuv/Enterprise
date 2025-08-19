#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class ForcePlayFromFirstScene
{
    private const string PREF_KEY = "ForcePlayFromFirstScene_Enabled";

    static ForcePlayFromFirstScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!EditorPrefs.GetBool(PREF_KEY, true))
            return;

        if (state == PlayModeStateChange.ExitingEditMode) {
            string firstScenePath = SceneUtility.GetScenePathByBuildIndex(0);

            if (!EditorSceneManager.GetActiveScene().path.Equals(firstScenePath)) {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                    EditorSceneManager.OpenScene(firstScenePath);
                }
                else {
                    EditorApplication.isPlaying = false;
                }
            }
        }
    }

    [MenuItem("Tools/Force Play From First Scene")]
    private static void ToggleForcePlay()
    {
        bool current = EditorPrefs.GetBool(PREF_KEY, true);
        EditorPrefs.SetBool(PREF_KEY, !current);
    }

    [MenuItem("Tools/Force Play From First Scene", true)]
    private static bool ToggleForcePlayValidate()
    {
        Menu.SetChecked("Tools/Force Play From First Scene", EditorPrefs.GetBool(PREF_KEY, true));
        return true;
    }
}
#endif
