using UnityEditor;
using UnityEngine;
using TMPro;

public class TMPFontGenerator
{
    [MenuItem("Tools/Generate TMP Font (Safe)")]
    static void Generate()
    {
        string fontPath = "Assets/Font/KOREAN/NotoSerifKR-Black.ttf"; // 너의 폰트 경로로 바꿔줘
        string savePath = "Assets/Font/NotoTMP.asset";

        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        TMP_FontAsset tmpFont = TMP_FontAsset.CreateFontAsset(sourceFont);

        AssetDatabase.CreateAsset(tmpFont, savePath);
        AssetDatabase.SaveAssets();

        Debug.Log("✅ TMP 폰트 생성 완료 (스크립트 방식)");
    }
}
