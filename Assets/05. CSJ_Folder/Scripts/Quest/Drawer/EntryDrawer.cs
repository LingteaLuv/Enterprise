#if UNITY_EDITOR

using _05._CSJ_Folder.Scripts.Quest;
using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(QuestRewardSO.RewardEntry))]
public class EntryDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var isVar = property.FindPropertyRelative("IsVariableReward").boolValue;
        var lines = isVar ? 5 : 4;
        return EditorGUIUtility.singleLineHeight * lines + EditorGUIUtility.standardVerticalSpacing * (lines - 1);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var line = EditorGUIUtility.singleLineHeight;
        var y = position.y;
        var full = new Rect(position.x, y, position.width, line);
        
        var iconProp = property.FindPropertyRelative("RewardIcon");
        EditorGUI.PropertyField(full, iconProp, new GUIContent("Reward Icon"));
        y += line + EditorGUIUtility.standardVerticalSpacing;
        
        var typeProp = property.FindPropertyRelative("rewardType");
        full.y = y;
        EditorGUI.PropertyField(full, typeProp, new GUIContent("Reward Type"));
        y += line + EditorGUIUtility.standardVerticalSpacing;
        
        var variableProp = property.FindPropertyRelative("IsVariableReward");
        full.y = y;
        EditorGUI.PropertyField(full, variableProp, new GUIContent("Is Variable Value"));
        y += line + EditorGUIUtility.standardVerticalSpacing;

        if (!variableProp.boolValue)
        {
            var fixedProp = property.FindPropertyRelative("FixedAmount");
            full.y = y;
            EditorGUI.PropertyField(full, fixedProp, new GUIContent("Fixed Amount"));
        }
        else
        {
            var valueProp = property.FindPropertyRelative("VariableAmount");
            full.y = y;
            EditorGUI.PropertyField(full, valueProp, new GUIContent("Variable Value"));
            y += line + EditorGUIUtility.standardVerticalSpacing;

            var calculateProp = property.FindPropertyRelative("RewardCalculate");
            full.y = y;
            EditorGUI.PropertyField(full, calculateProp, new GUIContent("Reward Calculate"));
            
        }
        EditorGUI.EndProperty();
    }
}
#endif