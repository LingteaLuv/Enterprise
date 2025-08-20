#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace JHT
{
    [CustomEditor(typeof(ItemWeaponSO))]
    public class TestEditor : Editor
    {
        private ItemWeaponSO weaponItem;
        private SerializedProperty weaponTypeProperty;
        private SerializedProperty characterWeaponTypeProperty;
        private SerializedProperty weaponClassesProperty;
        private SerializedProperty starsProperty;

        // 기본 ItemSO 프로퍼티들
        private SerializedProperty itemNameProperty;
        private SerializedProperty itemDescriptionProperty;
        private SerializedProperty itemIconProperty;
        private SerializedProperty itemIDProperty;
        private SerializedProperty itemTypeProperty;
        private SerializedProperty itemRarityProperty;

        private bool showWeaponClasses = true;
        private bool showStars = true;

        private void OnEnable()
        {
            weaponItem = (ItemWeaponSO)target;

            // ItemWeaponSO 프로퍼티들
            weaponTypeProperty = serializedObject.FindProperty("weaponType");
            characterWeaponTypeProperty = serializedObject.FindProperty("characterWeaponType");
            weaponClassesProperty = serializedObject.FindProperty("weaponClasses");
            starsProperty = serializedObject.FindProperty("stars");

            // 기본 ItemSO 프로퍼티들 (실제 필드명에 맞게 수정해주세요)
            itemNameProperty = serializedObject.FindProperty("itemName");
            itemDescriptionProperty = serializedObject.FindProperty("itemDescription");
            itemIconProperty = serializedObject.FindProperty("itemIcon");
            itemIDProperty = serializedObject.FindProperty("itemID");
            itemTypeProperty = serializedObject.FindProperty("itemType");
            itemRarityProperty = serializedObject.FindProperty("itemRarity");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();

            // 왼쪽: 아이콘 표시
            DrawIconSection();

            // 오른쪽: 기본 정보
            EditorGUILayout.BeginVertical();
            DrawBasicInfo();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // 무기 타입 섹션
            DrawWeaponTypeSection();

            EditorGUILayout.Space(10);

            // 무기 클래스 섹션
            DrawWeaponClassesSection();

            EditorGUILayout.Space(10);

            // 별 UI 섹션
            DrawStarsSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawIconSection()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(80));

            // 아이콘 표시
            Rect iconRect = GUILayoutUtility.GetRect(64, 64, GUILayout.Width(64), GUILayout.Height(64));

            if (itemIconProperty?.objectReferenceValue != null)
            {
                Sprite iconSprite = itemIconProperty.objectReferenceValue as Sprite;
                if (iconSprite != null)
                {
                    Texture2D iconTexture = iconSprite.texture;
                    GUI.DrawTextureWithTexCoords(iconRect, iconTexture,
                        new Rect(iconSprite.textureRect.x / iconTexture.width,
                                iconSprite.textureRect.y / iconTexture.height,
                                iconSprite.textureRect.width / iconTexture.width,
                                iconSprite.textureRect.height / iconTexture.height));
                }
            }
            else
            {
                EditorGUI.DrawRect(iconRect, Color.gray);
                GUI.Label(iconRect, "No Icon", EditorStyles.centeredGreyMiniLabel);
            }

            // 아이콘 선택 필드
            EditorGUILayout.Space(5);
            if (itemIconProperty != null)
            {
                EditorGUILayout.PropertyField(itemIconProperty, GUIContent.none, GUILayout.Width(64));
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawBasicInfo()
        {
            EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);

            if (itemNameProperty != null)
                EditorGUILayout.PropertyField(itemNameProperty, new GUIContent("아이템 이름"));
            if (itemIDProperty != null)
                EditorGUILayout.PropertyField(itemIDProperty, new GUIContent("아이템 ID"));
            if (itemTypeProperty != null)
                EditorGUILayout.PropertyField(itemTypeProperty, new GUIContent("아이템 타입"));
            if (itemRarityProperty != null)
                EditorGUILayout.PropertyField(itemRarityProperty, new GUIContent("희귀도"));
            if (itemDescriptionProperty != null)
                EditorGUILayout.PropertyField(itemDescriptionProperty, new GUIContent("설명"));
        }

        private void DrawWeaponTypeSection()
        {
            EditorGUILayout.LabelField("무기 타입 설정", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.PropertyField(weaponTypeProperty, new GUIContent("무기 타입"));

            EditorGUILayout.Space(5);

            // CharacterWeaponType을 Flags로 처리
            EditorGUILayout.LabelField("캐릭터 무기 타입 (복수 선택 가능)");

            CharacterWeponType currentFlags = (CharacterWeponType)characterWeaponTypeProperty.intValue;

            EditorGUI.indentLevel++;
            bool swordSelected = EditorGUILayout.Toggle("Sword", (currentFlags & CharacterWeponType.Sword) != 0);
            bool bowSelected = EditorGUILayout.Toggle("Bow", (currentFlags & CharacterWeponType.Bow) != 0);
            bool axSelected = EditorGUILayout.Toggle("Ax", (currentFlags & CharacterWeponType.Ax) != 0);
            EditorGUI.indentLevel--;

            CharacterWeponType newFlags = 0;
            if (swordSelected) newFlags |= CharacterWeponType.Sword;
            if (bowSelected) newFlags |= CharacterWeponType.Bow;
            if (axSelected) newFlags |= CharacterWeponType.Ax;

            characterWeaponTypeProperty.intValue = (int)newFlags;

            EditorGUILayout.EndVertical();
        }

        private void DrawWeaponClassesSection()
        {
            showWeaponClasses = EditorGUILayout.Foldout(showWeaponClasses, "무기 클래스 설정", true);

            if (showWeaponClasses)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"클래스 수: {weaponClassesProperty.arraySize}");
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("+", GUILayout.Width(30)))
                {
                    weaponClassesProperty.arraySize++;
                }

                if (GUILayout.Button("-", GUILayout.Width(30)) && weaponClassesProperty.arraySize > 0)
                {
                    weaponClassesProperty.arraySize--;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                for (int i = 0; i < weaponClassesProperty.arraySize; i++)
                {
                    SerializedProperty classElement = weaponClassesProperty.GetArrayElementAtIndex(i);
                    SerializedProperty starProperty = classElement.FindPropertyRelative("star");
                    SerializedProperty levelsProperty = classElement.FindPropertyRelative("levels");

                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"클래스 {i + 1}", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(starProperty, new GUIContent("별 등급"));

                    // Levels 배열 관리
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"레벨 수: {levelsProperty.arraySize}");

                    if (GUILayout.Button("+", GUILayout.Width(25)))
                    {
                        levelsProperty.arraySize++;
                    }

                    if (GUILayout.Button("-", GUILayout.Width(25)) && levelsProperty.arraySize > 0)
                    {
                        levelsProperty.arraySize--;
                    }
                    EditorGUILayout.EndHorizontal();

                    // 레벨들을 한 줄에 여러 개씩 표시
                    int levelsPerRow = 4;
                    for (int j = 0; j < levelsProperty.arraySize; j += levelsPerRow)
                    {
                        EditorGUILayout.BeginHorizontal();
                        for (int k = 0; k < levelsPerRow && (j + k) < levelsProperty.arraySize; k++)
                        {
                            SerializedProperty levelElement = levelsProperty.GetArrayElementAtIndex(j + k);
                            EditorGUILayout.PropertyField(levelElement, new GUIContent($"Lv{j + k + 1}"));
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawStarsSection()
        {
            showStars = EditorGUILayout.Foldout(showStars, "별 UI 이미지", true);

            if (showStars)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"별 이미지 수: {starsProperty.arraySize}");
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("+", GUILayout.Width(30)))
                {
                    starsProperty.arraySize++;
                }

                if (GUILayout.Button("-", GUILayout.Width(30)) && starsProperty.arraySize > 0)
                {
                    starsProperty.arraySize--;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                for (int i = 0; i < starsProperty.arraySize; i++)
                {
                    SerializedProperty starElement = starsProperty.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(starElement, new GUIContent($"별 {i + 1}"));
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();
            }
        }

        // Context menu for quick setup
        [MenuItem("Assets/Create/JHT/Item Weapon SO")]
        public static void CreateItemWeaponSO()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (System.IO.Path.GetExtension(path) != "")
            {
                path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New ItemWeaponSO.asset");

            ItemWeaponSO asset = CreateInstance<ItemWeaponSO>();
            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}
#endif