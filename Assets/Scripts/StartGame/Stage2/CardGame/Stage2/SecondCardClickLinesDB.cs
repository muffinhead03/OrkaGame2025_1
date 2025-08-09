using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardClickEntry
{
    [Tooltip("이 슬롯(또는 카드)의 식별용 이름. 정답 이름과 맞추면 찾기 편해요.")]
    public string cardName;

    [Tooltip("이 카드의 언어별 6줄 대사(SecondCardClickKeyedLinesMB 컴포넌트).")]
    public SecondCardClickKeyedLinesMB keyedLines; // ← 실제 사용하는 MB 타입으로 맞춰주세요
}

public class SecondCardClickLinesDB : MonoBehaviour
{
    [Header("슬롯 10개에 대응하는 카드 대사 데이터")]
    public CardClickEntry[] entries = new CardClickEntry[10];

    [Header("옵션")]
    [Tooltip("LanguageManager가 내보낸 값을 정규화해서 넘기고 싶을 때 사용(권장)")]
    public bool normalizeLanguage = true;

    public string GetLineBySlot(int slotIndex, int clickIndex)
    {
        if (entries == null || slotIndex < 0 || slotIndex >= entries.Length)
        {
            Debug.LogWarning($"[SecondCardClickLinesDB] 잘못된 slotIndex={slotIndex}");
            return string.Empty;
        }

        var e = entries[slotIndex];
        if (e == null || e.keyedLines == null)
        {
            Debug.LogWarning($"[SecondCardClickLinesDB] slotIndex={slotIndex}에 keyedLines가 비어있음");
            return string.Empty;
        }

        // 어떤 인스턴스를 쓰는지 확인
        Debug.Log($"[LinesDB] slot={slotIndex}, name='{e.cardName}', keyed='{e.keyedLines.name}', id={e.keyedLines.GetInstanceID()}");

        string lang = LanguageManager.GetLanguage();
        if (normalizeLanguage) lang = Normalize(lang);

        int idx = Mathf.Clamp(clickIndex, 0, 5);
        return e.keyedLines.GetLine(lang, idx);
    }

    /// <summary>카드 이름으로 검색(대소문자/공백 무시)</summary>
    public string GetLineByCardName(string cardName, int clickIndex)
    {
        if (string.IsNullOrWhiteSpace(cardName) || entries == null) return string.Empty;

        string target = cardName.Trim();
        for (int i = 0; i < entries.Length; i++)
        {
            var ent = entries[i];
            if (ent != null && !string.IsNullOrWhiteSpace(ent.cardName))
            {
                if (string.Equals(ent.cardName.Trim(), target, StringComparison.OrdinalIgnoreCase))
                    return GetLineBySlot(i, clickIndex);
            }
        }
        Debug.LogWarning($"[SecondCardClickLinesDB] cardName='{cardName}'을(를) 찾지 못함");
        return string.Empty;
    }

    /// <summary>(선택) 1~10 카드번호 기준으로 검색하고 싶을 때</summary>
    public string GetLineByCardNumber(int cardNumberOneBased, int clickIndex)
    {
        int slotIndex = cardNumberOneBased - 1;
        return GetLineBySlot(slotIndex, clickIndex);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 배열 길이 보정
        if (entries == null) entries = new CardClickEntry[10];

        // 슬롯 간 같은 KeyedLines 인스턴스를 공유하면 경고
        var seen = new Dictionary<int, int>(); // instanceID -> firstSlot
        for (int i = 0; i < entries.Length; i++)
        {
            var kl = entries[i]?.keyedLines;
            if (kl == null) continue;
            int id = kl.GetInstanceID();
            if (seen.TryGetValue(id, out int prev))
            {
                Debug.LogWarning(
                    $"[SecondCardClickLinesDB] slot {prev}와 slot {i}가 같은 KeyedLines('{kl.name}', id={id})를 공유합니다."
                );
            }
            else seen[id] = i;
        }
    }

    [ContextMenu("DBG/Print Mapping")]
    private void DBG_PrintMapping()
    {
        if (entries == null) { Debug.Log("[SecondCardClickLinesDB] entries=null"); return; }

        Debug.Log("=== [SecondCardClickLinesDB] Slot → CardName / KeyedLines / InstanceID ===");
        for (int i = 0; i < entries.Length; i++)
        {
            var e = entries[i];
            if (e == null)
            {
                Debug.Log($"- slot {i}: (null)");
                continue;
            }
            string name = string.IsNullOrWhiteSpace(e.cardName) ? "(no name)" : e.cardName.Trim();
            string kl   = e.keyedLines ? e.keyedLines.name : "(null)";
            int id      = e.keyedLines ? e.keyedLines.GetInstanceID() : 0;
            Debug.Log($"- slot {i}: cardName='{name}', keyed='{kl}', id={id}");
        }
    }
#endif

    // --- 내부 유틸 ---
    private static string Normalize(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "english";
        raw = raw.ToLowerInvariant();

        if (raw == "chinese" || raw == "zh" || raw.StartsWith("zh-") || raw.Contains("chinese") || raw == "c1") return "chinese";
        if (raw.StartsWith("ko") || raw == "korean" || raw == "k1")                                           return "korean";
        if (raw.StartsWith("ja") || raw == "japanese" || raw == "j1")                                         return "japanese";
        if (raw.StartsWith("ka") || raw == "kazakh" || raw == "kazakhstan")                                   return "kazakh";
        if (raw.StartsWith("en") || raw == "english" || raw == "e1")                                          return "english";
        return "english";
    }
}
