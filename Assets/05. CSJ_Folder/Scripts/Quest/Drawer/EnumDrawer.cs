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
        float h = EditorGUIUtility.singleLineHeight; // 1) kind 라인
        h += EditorGUIUtility.standardVerticalSpacing;

        var kind = (KeyKind)kindProp.enumValueIndex;

        if (kind == KeyKind.Achieve)
        {
            // TextArea 높이(필드 속성의 TextArea를 존중)
            h += EditorGUI.GetPropertyHeight(achieveProp, true);
        }
        else
        {
            // 2) 선택 enum 라인 한 줄
            h += EditorGUIUtility.singleLineHeight;
        }

        return h;
        // // kind 1줄 + 선택 enum 1줄 = 2줄
        // return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var kindProp    = property.FindPropertyRelative("kind");
        var monsterProp = property.FindPropertyRelative("monster");
        var MoneyProp    = property.FindPropertyRelative("Money");
        var itemTypeProp= property.FindPropertyRelative("itemType");
        var upgradeProp = property.FindPropertyRelative("upgrade");
        var achieveProp = property.FindPropertyRelative("achieveText");

        Rect baseRect = EditorGUI.IndentedRect(position);
        float line  = EditorGUIUtility.singleLineHeight;
        float vpad  = EditorGUIUtility.standardVerticalSpacing;
        float labW  = EditorGUIUtility.labelWidth;

        // 1) 첫 줄: kind (라벨 포함)
        Rect r1 = new Rect(baseRect.x, baseRect.y, baseRect.width, line);
        EditorGUI.PropertyField(r1, kindProp, label);

        // 2) 두 번째 줄: "값"만 그리되, 첫 줄의 '값 영역'과 동일한 x/폭으로 정렬
        Rect valueRect = new Rect(baseRect.x + labW, r1.y + line + vpad, baseRect.width - labW, line);


        // 2) 선택된 종류만 노출
        var kind = (KeyKind)kindProp.enumValueIndex;
        switch (kind)
        {
            case KeyKind.Monster:     EditorGUI.PropertyField(valueRect, monsterProp, GUIContent.none); break;
            case KeyKind.Money:       EditorGUI.PropertyField(valueRect, MoneyProp,   GUIContent.none); break;
            case KeyKind.ItemType:    EditorGUI.PropertyField(valueRect, itemTypeProp,GUIContent.none); break;
            case KeyKind.UpgradeType: EditorGUI.PropertyField(valueRect, upgradeProp, GUIContent.none); break;
            case KeyKind.Achieve:     valueRect.height = EditorGUI.GetPropertyHeight(achieveProp, true);
                                      EditorGUI.PropertyField(valueRect, achieveProp, GUIContent.none, true);
                                      break;
        }
    }
}
#endif