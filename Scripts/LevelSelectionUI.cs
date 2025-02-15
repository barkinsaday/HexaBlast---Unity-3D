using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionUI : MonoBehaviour
{
    public Button level0Button;
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    public Button level4Button;

    void Start()
    {
        level0Button.onClick.AddListener(() => SelectLevel(0));
        level1Button.onClick.AddListener(() => SelectLevel(1));
        level2Button.onClick.AddListener(() => SelectLevel(2));
        level3Button.onClick.AddListener(() => SelectLevel(3));
        level4Button.onClick.AddListener(() => SelectLevel(4));
    }

    void SelectLevel(int level)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager not found! Ensure GameManager exists before selecting a level.");
            return;
        }
        
        GameManager.Instance.RestartLevel(level);
    }
}
