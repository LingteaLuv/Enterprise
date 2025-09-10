#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TypedEnumKey))]
public class TypedEnumKeyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var kindProp    = property.FindPropertyRelative("kind");
        var achieveProp = property.FindPropertyRelative("achieveText");
        float h = EditorGUIUtility.singleLineHeight;
        h += EditorGUIUtility.standardVerticalSpacing;

        var kind = (KeyKind)kindProp.enumValueIndex;

        if (kind == KeyKind.Achieve)
        {
            h += EditorGUI.GetPropertyHeight(achieveProp, true);
        }
        else
        {
            h += EditorGUIUtility.singleLineHeight;
        }

        return h;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var kindProp    = property.FindPropertyRelative("kind");
        var monsterProp = property.FindPropertyRelative("monster");
        var MoneyProp    = property.FindPropertyRelative("Money");
        var itemTypeProp= property.FindPropertyRelative("itemType");
        var upgradeProp = property.FindPropertyRelative("upgrade");
        var activeProp  = property.FindPropertyRelative("active");
        var achieveProp = property.FindPropertyRelative("achieveText");

        Rect baseRect = EditorGUI.IndentedRect(position);
        float line  = EditorGUIUtility.singleLineHeight;
        float vpad  = EditorGUIUtility.standardVerticalSpacing;
        float labW  = EditorGUIUtility.labelWidth;
        
        Rect r1 = new Rect(baseRect.x, baseRect.y, baseRect.width, line);
        EditorGUI.PropertyField(r1, kindProp, label);
        
        Rect valueRect = new Rect(baseRect.x + labW, r1.y + line + vpad, baseRect.width - labW, line);

        
        var kind = (KeyKind)kindProp.enumValueIndex;
        switch (kind)
        {
            case KeyKind.Monster:     EditorGUI.PropertyField(valueRect, monsterProp, GUIContent.none); break;
            case KeyKind.Money:       EditorGUI.PropertyField(valueRect, MoneyProp,   GUIContent.none); break;
            case KeyKind.ItemType:    EditorGUI.PropertyField(valueRect, itemTypeProp,GUIContent.none); break;
            case KeyKind.UpgradeType: EditorGUI.PropertyField(valueRect, upgradeProp, GUIContent.none); break;
            case KeyKind.Active:      EditorGUI.PropertyField(valueRect, activeProp,  GUIContent.none); break;
            case KeyKind.Achieve:     valueRect.height = EditorGUI.GetPropertyHeight(achieveProp, true);
                                      EditorGUI.PropertyField(valueRect, achieveProp, GUIContent.none, true);
                                      break;
        }
    }
}
#endif