using UnityEngine;
using Firebase.Database;

public enum CharacterState { AtHome, OnStage, Performing }

public class Character: MonoBehaviour
{
    public string characterID;
    public string washerID;
    public CharacterState state = CharacterState.AtHome;
    private Animator animator;
    private string currentCostume = "Default";

    public AudioClip guitarClip;    // 為 fish 用
    public AudioClip drumsClip;     // 為 octopus 用

    private AudioSource audioSource;

    

    void Start() {
        // homeWasher = GetComponentInParent<Washer>();

        animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();


        if (animator == null) {
            Debug.LogError($"❌ Animator is missing on: {gameObject.name}");
            return;
        }

        Debug.Log("動畫初始化成功：" + gameObject.name + " / " + animator?.runtimeAnimatorController?.name);
        UpdateAnimation();

        // UpdateState(CharacterState.AtHome);

        // 你可以在這裡新增 Firebase 監聽（若希望角色自動更新狀態）
        // DatabaseReference charRef = FirebaseDatabase.DefaultInstance.GetReference("Characters").Child(characterID);
        // charRef.ChildChanged += OnCharacterDataChanged;
    }

    void OnMouseDown() {
        
    // 防止 UI 點擊穿透 好像沒用？
    if (Application.isMobilePlatform) {
        if (Input.touchCount > 0 && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            return;
    } else {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;
    }

    if (state == CharacterState.AtHome) {
        // 在洗衣機區域點擊角色時呼叫
        UIManager.Instance.OpenWasherPanel(characterID);
    } else if (state == CharacterState.OnStage) {
        // 在舞台上點擊角色時呼叫
        UIManager.Instance.OpenSelectionPanel(this);
    }
    }
    // public void HandleTouch()
    // {
    //     if (state == CharacterState.AtHome) {
    //         UIManager.Instance.OpenWasherPanel(characterID);
    //     } else if (state == CharacterState.OnStage) {
    //         UIManager.Instance.OpenSelectionPanel(this);
    //     }
    // }


    public void UpdateAnimation()
    {
        if (animator == null) {
        animator = GetComponent<Animator>();
        if (animator == null) {
                Debug.LogError($"❌ {characterID} 上沒有 Animator，無法播放動畫！");
                return;
            }
        }

        switch (state)
        {
            case CharacterState.AtHome:
                animator.Play("AtHome");
                break;
            case CharacterState.OnStage:
                animator.Play("Idle");
                break;
            case CharacterState.Performing:
                animator.Play("Perform_" + currentCostume); // 根據服裝播放對應表演動畫
                break;
        }
    }

      //  當玩家在 UI 中選擇服裝後呼叫
    public void SelectCostume(string costume)
    {
        currentCostume = costume;
        FirebaseManager.Instance.UpdateCharacterCostume(characterID, costume); 
        // 若希望同步更新動畫，可以呼叫 UpdateAnimation()（但通常在狀態改變時更新）
        if(state == CharacterState.Performing)
            UpdateAnimation();
    }

    // 角色上台，通常在對話後決定上台時呼叫
    public void GoOnStage() {
        if (state != CharacterState.AtHome) return;
        state = CharacterState.OnStage;
        FirebaseManager.Instance.UpdateCharacterState(characterID, "OnStage");
        UpdateAnimation();
    }
    
    // 玩家在舞台上設定表演時長後開始表演
    public void StartPerformance(float duration) {
        if (state != CharacterState.OnStage) return;

        state = CharacterState.Performing;
        FirebaseManager.Instance.UpdateCharacterState(characterID, "Performing"); // 🟢 同步到 Firebase
        FirebaseManager.Instance.UpdateCharacterDuration(characterID, duration);

        UpdateAnimation();

        // 播放角色對應音樂
        if (audioSource != null) {
            switch (characterID) {
                case "fish":
                    audioSource.clip = guitarClip;
                    break;
                case "octopus":
                    audioSource.clip = drumsClip;
                    break;
            }
            audioSource.Play();
        }

        Invoke(nameof(EndPerformance), duration);
    }

    // 角色結束表演
    private void EndPerformance() {
        // 1. 改狀態
        state = CharacterState.AtHome;

        // 2. Firebase 更新狀態
        FirebaseManager.Instance.UpdateCharacterState(characterID, "AtHome");

        // 3. 切換動畫
        UpdateAnimation();

        if (audioSource != null && audioSource.isPlaying) {
            audioSource.Stop();
        }

        // 4. 顯示回對應的 Washer（立即處理，無須等 Firebase 回調）
        Washer[] washers = GameObject.FindObjectsOfType<Washer>();
        foreach (var washer in washers) {
            if (washer.washerID == characterID) {
                washer.UpdateWasherVisual(true);
                break;
            }
        }

        // 5. 通知 StageManager 移除自己（視情況可省略，因為 Firebase 也會觸發這件事）
        StageManager.Instance.RemoveCharacterFromStage(characterID);
    }

     // 角色回到洗衣機
    public void ReturnToWasher()
    {
        state = CharacterState.AtHome;
        FirebaseManager.Instance.UpdateCharacterState(characterID, "AtHome"); // 🟢 同步到 Firebase
        UpdateAnimation();
    }

     // 可供 Firebase 監聽時使用：更新服裝
    public void UpdateCostume(string costume) {
        currentCostume = costume;
        UpdateAnimation();
    }

    // public void UpdateState(CharacterState newState) {
    //     currentState = newState;

    //     switch (newState) {
    //         case CharacterState.AtHome:
    //             animator.Play("WasherIdle");
    //             break;
    //         case CharacterState.OnStage:
    //             animator.Play("Idle");
    //             break;
    //         case CharacterState.Performing:
    //             animator.Play("Performing");
    //             break;
    //     }
    // }

    // public void SetOutfit() {
    //     currentOutfit = outfit;
    // }
}
