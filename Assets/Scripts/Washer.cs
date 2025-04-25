using UnityEngine;

public class Washer : MonoBehaviour
{
    public string washerID;  // 洗衣機的唯一識別碼
    public GameObject characterOverlay; // 這是用來顯示角色在家的 Image Target overlay
    // private Animator animator; // 若你希望洗衣機有背景動畫，可以利用 Animator
    private void Start()
    {
        Debug.Log("🚩Washer.cs 開始執行 Start()");
        Character childCharacter = GetComponentInChildren<Character>();
        if (childCharacter != null)
        {
            characterOverlay = childCharacter.gameObject;
            Debug.Log("✅ 成功自動抓到角色：" + characterOverlay.name);
        }
        else
        {
            Debug.LogWarning("❌ 沒找到角色！");
        }

        UpdateWasherVisual(true);

        // animator = GetComponent<Animator>();
        // 初始狀態：角色在家，overlay 顯示
        // UpdateWasherVisual(true);
    }

    // 控制洗衣機 overlay 是否顯示角色動畫
    public void  UpdateWasherVisual(bool showCharacter) {

        if (characterOverlay != null) {
            characterOverlay.SetActive(showCharacter);
        }

        // 若有背景動畫，也可依此決定播放什麼動畫
        // if (animator != null) {
        //     if (showCharacter) {
        //         animator.Play("Washer_Background"); // 背景動畫，包含角色在家畫面（背景含角色動畫已整合在AtHome動畫中）
        //     } else {
        //          animator.Play("Washer_Idle"); // 角色不在家，背景保持靜止
        //     }
        // }
    }
}
