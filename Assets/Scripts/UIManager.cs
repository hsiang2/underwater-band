using UnityEngine;
using UnityEngine.UI;
using TMPro; 


public class UIManager : MonoBehaviour {
    public static UIManager Instance;

    [Header("Perform UI")]
    public GameObject selectionPanel;  // 包含服裝下拉選單和表演時長輸入欄
    public TMP_Dropdown costumeDropdown;
    public TMP_InputField durationInput;

    [Header("Washer UI")]
    public GameObject washerPanel;
     public TMP_Text washerPanelText;    // 顯示提示文字，例如 "請問要讓角色上台嗎？"

    // private Character selectedCharacter; // 目前選取的角色

    // 當玩家從舞台點選角色時，直接傳入該角色實例
    private Character selectedCharacter;

    // 當玩家從洗衣機區域點選時，僅保留角色 ID（因為原始角色物件可能是隱藏的）
    private string selectedCharacterID;

    public AudioSource uiAudioSource;
    public AudioClip clickSound;

    void Awake() {
        if (Instance == null) Instance = this;
        selectionPanel.SetActive(false);
        washerPanel.SetActive(false);
    }

    // 🎯【當玩家點擊「洗衣機中的角色」時】開啟 UI
    public void OpenWasherPanel(string characterID) {
        selectedCharacterID = characterID;
          if (washerPanelText != null)
            washerPanelText.text = "Do you want him to perform on stage?"; // 或根據 characterID 顯示具體名稱

        washerPanel.SetActive(true);
    }

    // 🚀【按下確認讓角色上台】更新 Firebase 狀態
    public void ConfirmStageSelection() {
        if (string.IsNullOrEmpty(selectedCharacterID)) return;

        washerPanel.SetActive(false);

        // 改為啟動 RPG 對話（後續由 DialogueManager 控管後續流程）
        DialogueManager.Instance.StartDialogue(selectedCharacterID);
        
        // FirebaseManager.Instance.UpdateCharacterState(selectedCharacterID, "OnStage");

        //  // 2. 找出對應的 Washer 並隱藏角色顯示
        // Washer[] washers = FindObjectsOfType<Washer>();
        // foreach (Washer washer in washers) {
        //     if (washer.washerID == selectedCharacterID) {
        //         washer.UpdateWasherVisual(false);
        //         break;
        //     }
        // }

        //  // 3. 新增角色到舞台（使用預設服裝和表演時長）
        // string defaultCostume = "Default";      // 你也可以從 Firebase 或其他 UI 拿到
        // float defaultDuration = 10f;            // 預設時長（可改為 0 或從輸入取得）
        // StageManager.Instance.AddOrUpdateCharacterOnStage(selectedCharacterID, "OnStage", defaultCostume, defaultDuration);

        // washerPanel.SetActive(false);

        // Debug.Log($"✅ [Confirm] washerPanel activeAfter = {washerPanel.activeSelf}");
    }

     // ❌ 關閉 UI
    public void CloseWasherPanel() {
        washerPanel.SetActive(false);
    }
    

    // 當玩家點擊舞台上的角色時（例如角色的 OnMouseDown() 呼叫此方法），打開選單
    public void OpenSelectionPanel(Character character) {
         // 判斷角色狀態是否為 OnStage，否則不開啟 (你可以在這裡進行判斷)
        if (character.state != CharacterState.OnStage) {
            Debug.LogWarning("角色不是在台上狀態，無法開啟表演設定 UI！");
            return;
        }

        selectedCharacter = character;
        selectionPanel.SetActive(true);
    }

    // 玩家在 UI 上確認選擇後呼叫
    public void ConfirmSelection() {
        if (selectedCharacter == null) return;

        string selectedCostume = costumeDropdown.options[costumeDropdown.value].text;
        float duration = 0f;
        if (!float.TryParse(durationInput.text, out duration)) {
            Debug.LogError("請輸入有效的表演時長！");
            return;
        }

        // 更新角色服裝並開始表演
        selectedCharacter.SelectCostume(selectedCostume);
        selectedCharacter.StartPerformance(duration);
        
        selectionPanel.SetActive(false);
    }

    public void PlayClickSound() {
        if (uiAudioSource && clickSound) {
            uiAudioSource.PlayOneShot(clickSound);
        }
    }

}
