using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class CharacterPortrait {
    public string characterID;
    public Sprite portrait;
}

[System.Serializable]
public class DialogueLine {
    public string speaker;
    public string text;

    public DialogueLine(string speaker, string text) {
        this.speaker = speaker;
        this.text = text;
    }
}

public class DialogueManager : MonoBehaviour {
    public static DialogueManager Instance;

    [Header("UI Element")]
    public GameObject dialoguePanel;

    public GameObject leftBox;  // 角色 UI
    public GameObject rightBox; // 玩家 UI

    public TMP_Text leftNameText;
    public TMP_Text leftDialogueText;

    public TMP_Text rightNameText;
    public TMP_Text rightDialogueText;

    // public TMP_Text nameText;
    // public TMP_Text dialogueText;
    public Image characterImage;
    public Button nextButton;

    [Header("Character Mapping")]
    public List<CharacterPortrait> characterPortraits;

    private Dictionary<string, Sprite> portraitMap = new Dictionary<string, Sprite>();

    private Queue<DialogueLine> dialogueLines = new Queue<DialogueLine>();
    private string characterID;
    private string characterName;


    void Awake() {
        if (Instance == null) Instance = this;
        dialoguePanel.SetActive(false);
        nextButton.onClick.AddListener(DisplayNextLine);

          // 初始化圖片對應表
        foreach (var pair in characterPortraits) {
            if (!portraitMap.ContainsKey(pair.characterID)) {
                portraitMap[pair.characterID] = pair.portrait;
            }
        }
    }

    public void StartDialogue(string id) {
        characterID = id;
        dialoguePanel.SetActive(true);

         // 設定角色名稱（可改為從資料抓）
        switch (characterID) {
            case "fish":
                characterName = "Fish";
                break;
            case "octopus":
                characterName = "Octopus";
                break;
            default:
                characterName = "???";
                break;
        }

        // nameText.text = characterName;
        //  // 設定角色圖片
        // if (portraitMap.ContainsKey(characterID)) {
        //     characterImage.sprite = portraitMap[characterID];
        // } else {
        //     Debug.LogWarning("❌ 沒有對應圖片的角色ID：" + characterID);
        //     characterImage.sprite = null; // 或使用預設圖
        // }

         // 預載角色圖像到對話時會用到的 UI（右側框）
        if (portraitMap.TryGetValue(characterID, out var portrait)) {
           characterImage.sprite = portrait;
        } else {
            characterImage.sprite = null;
            Debug.LogWarning($"❌ 沒有對應圖片的角色ID：{characterID}");
        }

        dialogueLines.Clear();

        // // 自動決定角色名稱
        // string charName = characterID switch {
        //     "fish" => "Fish:",
        //     "octopus" => "Octopus:",
        //     _ => "Creature"
        // };


        dialogueLines.Enqueue(new DialogueLine("You:", "Hello there! I’m a reliable talent scout. I must say, you’ve got star quality. Would you fancy a performance on stage?"));
        dialogueLines.Enqueue(new DialogueLine($"{characterName}:", "Hmm... you seem a little fishy to me. And if I go, who’s going to guard my home? My neighbour’s been eyeing it for ages."));
        dialogueLines.Enqueue(new DialogueLine("You:", "Not to worry! I’ll watch your place while you’re gone."));
        dialogueLines.Enqueue(new DialogueLine($"{characterName}:", "...Alright then. But keep your woollens out of my living room."));

        DisplayNextLine();
    }

    public void DisplayNextLine() {
        if (dialogueLines.Count == 0) {
            EndDialogue();
            return;
        }

        DialogueLine line = dialogueLines.Dequeue();

        bool isPlayer = line.speaker == "You:";

        // 切換顯示哪一個面板
        rightBox.SetActive(isPlayer);
        leftBox.SetActive(!isPlayer);

        if (isPlayer) {
            rightNameText.text = line.speaker;
            rightDialogueText.text = line.text;
      
            // // 顯示角色頭像
            // if (portraitMap.ContainsKey(characterID)) {
            //     rightCharacterImage.sprite = portraitMap[characterID];
            // }
            
        } else {
            leftNameText.text = line.speaker;
            leftDialogueText.text = line.text;
        }


        // nameText.text = line.speaker;
        // dialogueText.text = line.text;
    }

    private void EndDialogue() {
        dialoguePanel.SetActive(false);
        FirebaseManager.Instance.UpdateCharacterState(characterID, "OnStage");

            // 2. 隱藏對應的 Washer 中的角色圖像
        Washer[] washers = FindObjectsOfType<Washer>();
        foreach (Washer washer in washers) {
            if (washer.washerID == characterID) {
                washer.UpdateWasherVisual(false);
                break;
            }
        }

        // 3. 在舞台生成角色（預設服裝和時間）
        string defaultCostume = "Default";
        float defaultDuration = 10f;
        StageManager.Instance.AddOrUpdateCharacterOnStage(characterID, "OnStage", defaultCostume, defaultDuration);

        Debug.Log($"✅ Dialogue 完成，{characterID} 已上台");
    }
}
