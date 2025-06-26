#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System.Linq;

public static class AddressablesCleaner
{
    [MenuItem("Tools/Addressables/Cleanup Null Entries")]
    public static void CleanupNullEntries()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogWarning("⚠️ Addressables Settings not found.");
            return;
        }

        int removedCount = 0;

        foreach (var group in settings.groups)
        {
            if (group == null) continue;

            var entries = group.entries.ToList(); // 인덱싱 가능하게 변환

            for (int i = entries.Count - 1; i >= 0; i--)
            {
                var entry = entries[i];
                if (entry == null || entry.TargetAsset == null)
                {
                    group.RemoveAssetEntry(entry);
                    removedCount++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"✅ Addressables 정리 완료: null entry {removedCount}개 제거됨.");
    }
}
#endif