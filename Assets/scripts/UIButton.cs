using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum ButtonType
{
    none,
    pauseButton,
    changeScene,
    backButton,
}
public class UIButton : MonoBehaviour
{
    public UIPanelType nextPanel;
    private Button mybtn;
    public ButtonType mytype;

    public SoundType soundType;
    public int sceneIndex;
    private void Start()
    {
        mybtn = GetComponent<Button>();
        if (mybtn != null)
        {
            mybtn.onClick.AddListener(OnClicked);
        }
    }

    void OnClicked()
    {
        UIManager.instance.SwitchCanvas(nextPanel);
        switch (mytype)
        {
            case ButtonType.none:
                break;
            case ButtonType.pauseButton:
                GameManager.instance.PauseGame();
                break;
            case ButtonType.changeScene:
                Time.timeScale = 1;
                SceneManager.LoadScene(sceneIndex);
                break;
            case ButtonType.backButton:
                GameManager.instance.ResumeGame();
                break;
            default:
                break;
        }

        if (soundType != SoundType.none)
        {
            soundManager.instance.PlaySound(soundType);
        }
        
    }
}
