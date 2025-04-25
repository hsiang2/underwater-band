using System;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CharacterPrefabMapping {
    public string characterID; // 角色唯一ID，例如 "character_fish"
    public GameObject prefab;  // 對應的 prefab（例如存放在 Assets/Prefabs/Character_fish）
}


public class StageManager : MonoBehaviour {
    public static StageManager Instance;

    [Header("Stage Settings")]

    public Transform[] stagePositions; // 固定舞台上可放置角色的位置
    // public GameObject characterOnStagePrefab; // 舞台上顯示角色的 prefab（可與 Character 相同或簡化版本）
     // 透過 Inspector 設置 List，之後轉換成 Dictionary
    public List<CharacterPrefabMapping> characterPrefabMappings;

    // 內部字典，key 為角色ID，value 為對應的 prefab
    private Dictionary<string, GameObject> characterPrefabs = new Dictionary<string, GameObject>();
    // 用來記錄當前上台的角色，key 為 characterID
    private Dictionary<string, GameObject> activeStageCharacters = new Dictionary<string, GameObject>();

    void Awake() {
        if (Instance == null) Instance = this;

         // 從 List 將映射轉換到 Dictionary 中
        foreach (var mapping in characterPrefabMappings) {
            if (!characterPrefabs.ContainsKey(mapping.characterID)) {
                characterPrefabs.Add(mapping.characterID, mapping.prefab);
            } else {
                Debug.LogWarning("重複的 characterID 映射：" + mapping.characterID);
            }
        }
    }

    public void AddOrUpdateCharacterOnStage(string characterID, string state, string costume, float duration) {
        Debug.Log($"🎭 AddOrUpdateCharacterOnStage called: {characterID}, state={state}");
         if (activeStageCharacters.ContainsKey(characterID)) {
            UpdateCharacterOnStage(characterID, state, costume, duration);
        } else {
            CreateStageCharacter(characterID, state, costume, duration);
        }
    }

    private void CreateStageCharacter(string characterID, string state, string costume, float duration) {
        if (!characterPrefabs.TryGetValue(characterID, out var prefab)) {
            Debug.LogError("❌ 找不到對應的角色 prefab: " + characterID);
            return;
        }

        Vector3 position = GetAvailablePosition();
        GameObject newChar = Instantiate(prefab, position, Quaternion.identity, transform);

        Character charComp = newChar.GetComponent<Character>();
        if (charComp == null) {
            Debug.LogError("❌ 生成角色時找不到 Character component！");
            return;
        }
        charComp.characterID = characterID;
        charComp.SelectCostume(costume);

        // charComp.state = Enum.TryParse(state, out CharacterState parsedState) ? parsedState : CharacterState.AtHome;
        // charComp.UpdateAnimation();

        if (!Enum.TryParse(state, out CharacterState parsedState)) {
            Debug.LogWarning("⚠️ 無法解析角色狀態，預設為 AtHome");
            parsedState = CharacterState.AtHome;
        }
         // ✅ 不直接呼叫 UpdateAnimation，改用協程延遲
        StartCoroutine(DeferredAnimation(charComp, parsedState, duration));

        activeStageCharacters[characterID] = newChar;

        Debug.Log($"✅ 已新增角色到舞台：{characterID} / {parsedState}");
        // charComp.state = parsedState;

        // // 安全檢查 Animator 再更新動畫
        // var animator = newChar.GetComponent<Animator>();
        // if (animator == null) {
        //     Debug.LogWarning($"⚠️ {characterID} 的 Animator 不存在，跳過動畫更新");
        // } else {
        //     charComp.UpdateAnimation();
        // }


        // if (parsedState == CharacterState.Performing) {
        //     charComp.StartPerformance(duration);
        // }

        // activeStageCharacters[characterID] = newChar;
    }

    private System.Collections.IEnumerator DeferredAnimation(Character charComp, CharacterState state, float duration) {
        yield return null; // 等一幀，讓 Start() 裡的 animator 初始化完畢

        if (charComp == null) yield break;

        charComp.state = state;
        charComp.UpdateAnimation();

        if (state == CharacterState.Performing) {
            charComp.StartPerformance(duration);
        }
    }


    private void UpdateCharacterOnStage(string characterID, string state, string costume, float duration) {
        var charObj = activeStageCharacters[characterID];
        var charComp = charObj.GetComponent<Character>();

        charComp.SelectCostume(costume);
        // charComp.state = Enum.TryParse(state, out CharacterState parsedState) ? parsedState : CharacterState.OnStage;
        if (!Enum.TryParse(state, out CharacterState parsedState)) {
            Debug.LogWarning("⚠️ 無法解析角色狀態，預設為 OnStage");
            parsedState = CharacterState.OnStage;
        }

        charComp.UpdateAnimation();

        if (parsedState == CharacterState.Performing) {
            charComp.StartPerformance(duration);
        }

         Debug.Log($"🟡 已更新角色狀態：{characterID} / {parsedState}");
    }


    // public void AddOrUpdateCharacterOnStage(string characterID, string state, string costume, float duration) {
    //      // 若角色已存在，這裡可以更新其資訊
    //     if (activeStageCharacters.ContainsKey(characterID)) {
    //             // 角色已存在：更新角色資訊（例如服裝、動畫狀態）
    //         GameObject existingChar = activeStageCharacters[characterID];
    //         Character characterComp = existingChar.GetComponent<Character>();
    //         // 更新狀態（假設狀態是由 Firebase 同步，不必額外更新）
    //         // 更新服裝：例如呼叫 UpdateCostume 方法來更新外觀
    //         characterComp.UpdateCostume(costume);
    //         // 如有其他屬性需要更新，也可以在這裡補充
    //         return;
    //     }

    //      GameObject prefab;
    //     if (characterPrefabs.TryGetValue(characterID, out prefab)) {
    //         Vector3 pos = GetAvailablePosition();
    //         GameObject newChar = Instantiate(prefab, pos, Quaternion.identity, transform);
    //         Character charComp = newChar.GetComponent<Character>();
    //         charComp.characterID = characterID;
    //         charComp.UpdateAnimation();
    //         activeStageCharacters.Add(characterID, newChar);
    //     } else {
    //         Debug.LogError("找不到角色 " + characterID + " 的專屬 prefab！");
    //     }
    // }

    // // 移除上台角色，當角色返回洗衣機時呼叫
    // public void RemoveCharacterFromStage(string characterID) {
    //     if (activeStageCharacters.ContainsKey(characterID)) {
    //         Destroy(activeStageCharacters[characterID]);
    //         activeStageCharacters.Remove(characterID);
    //     }
    // }

    public void RemoveCharacterFromStage(string characterID) {
        if (activeStageCharacters.TryGetValue(characterID, out var obj)) {
            Destroy(obj);
            activeStageCharacters.Remove(characterID);
        }
    }

    //  // 若需要將舞台角色切換到表演狀態（例如觸發表演動畫）可以使用此方法
    // public void SetCharacterPerforming(string characterID) {
    //     if (activeStageCharacters.ContainsKey(characterID)) {
    //         // 假設 prefab 內有 Character component
    //         activeStageCharacters[characterID].GetComponent<Character>().StartPerformance(10f); // 此處以10秒為範例
    //     }
    // }

    // // 從預設位置中取得一個空位
    // private Vector3 GetAvailablePosition() {
    //     foreach (Transform pos in stagePositions) {
    //         if (IsPositionEmpty(pos.position))
    //             return pos.position;
    //     }
    //     return stagePositions[0].position;
    // }
    private Vector3 GetAvailablePosition() {
        foreach (Transform pos in stagePositions) {
            if (IsPositionEmpty(pos.position)) return pos.position;
        }
        return stagePositions.Length > 0 ? stagePositions[0].position : Vector3.zero;
    }

     private bool IsPositionEmpty(Vector3 position) {
        foreach (var obj in activeStageCharacters.Values) {
            if (Vector3.Distance(obj.transform.position, position) < 1f)
                return false;
        }
        return true;
    }

    public bool IsCharacterOnStage(string characterID) {
        return activeStageCharacters.ContainsKey(characterID);
    }

}
