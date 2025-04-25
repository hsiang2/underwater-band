using UnityEngine;
using Firebase.Database;

public class FirebaseManager: MonoBehaviour
{
    public static FirebaseManager Instance;
    private DatabaseReference dbReference;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
             Debug.Log("✅ FirebaseManager 已初始化！");
        } else {
            Destroy(gameObject);
        }
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }
    void Start() {
        StartListening();

        if (Firebase.FirebaseApp.DefaultInstance != null) {
            Debug.Log("✅ Firebase App 初始化成功！");
        } else {
            Debug.LogError("❌ Firebase App 尚未初始化！");
        }
    }
     // 更新角色狀態：狀態字串必須為 "AtHome", "OnStage", "Performing"
    public void UpdateCharacterState(string characterID, string state) {
        dbReference.Child("Characters").Child(characterID).Child("state").SetValueAsync(state);
    }

    // 更新角色服裝（例如 "Default", "RockStar", "Classic" 等）
    public void UpdateCharacterCostume(string characterID, string costume) {
        dbReference.Child("Characters").Child(characterID).Child("costume").SetValueAsync(costume);
    }
    
    // 更新表演時長（用來作為表演結束的倒數依據）
    public void UpdateCharacterDuration(string characterID, float duration) {
        dbReference.Child("Characters").Child(characterID).Child("duration").SetValueAsync(duration);
    }

    // 若需要全域監聽（例如讓所有玩家畫面一致），可以在這裡新增 ValueChanged 監聽
    public void StartListening() {
        dbReference.Child("Characters").ValueChanged += OnCharactersChanged;
    }
    
    private void OnCharactersChanged(object sender, ValueChangedEventArgs args) {
        // 可根據需要解析各角色的狀態，這邊示範留空
        if(args.DatabaseError != null) {
            Debug.LogError("Firebase Error: " + args.DatabaseError.Message);
            return;
        }
        // 例如：遍歷 args.Snapshot.Children，讓 StageManager 或 WasherManager 更新畫面
        foreach (var child in args.Snapshot.Children) {
            string characterID = child.Key;
            string state = child.Child("state").Value.ToString();
            string costume = child.Child("costume").Value.ToString();
            string washerID = child.Child("washerID").Value.ToString();
            float duration = 0f;
            if(child.Child("duration").Value != null)
                float.TryParse(child.Child("duration").Value.ToString(), out duration);

            if (state == "AtHome")
            {
                // ✅ 1. 從舞台移除角色（如果有）
                StageManager.Instance.RemoveCharacterFromStage(characterID);

                // ✅ 2. 顯示 Washer 上的角色（回到家了）
                foreach (var washer in GameObject.FindObjectsByType<Washer>(FindObjectsSortMode.None))
                {
                    if (washer.washerID == washerID)
                    {
                        washer.UpdateWasherVisual(true);
                        break;
                    }
                }
            }
            else if (state == "OnStage" || state == "Performing")
            {
                // ✅ 1. 防止角色重複上台（已經在舞台上的就不處理）
                if (!StageManager.Instance.IsCharacterOnStage(characterID))
                {
                    StageManager.Instance.AddOrUpdateCharacterOnStage(characterID, state, costume, duration);
                }

                // ✅ 2. 隱藏 Washer 上角色（離開家）
                foreach (var washer in GameObject.FindObjectsByType<Washer>(FindObjectsSortMode.None))
                {
                    if (washer.washerID == washerID)
                    {
                        washer.UpdateWasherVisual(false);
                        break;
                    }
                }
            }

        }
    }
}
