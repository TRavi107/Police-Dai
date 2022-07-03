using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RamailoGames;

[System.Serializable]
public class SpawnPos
{
    public Transform final_position;
    public Transform spawn_position;
    public bool isSpawned;
}

public class GameManager : MonoBehaviour
{
    [Header("UIS")]
    #region UIS

    public TMP_Text GameOverScoreText;
    public TMP_Text GameOverhighscoreText;
    public TMP_Text scoreText;
    public TMP_Text gamePlayhighscoreText;
    public Image bulletImage;

    #endregion

    [Header("")]
    #region Singleton
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }


    #endregion

    [Header("Transforms")]
    #region Transforms
    public Transform[] livesGameObject;
    public Transform bulletHolderTransform;
    public Canvas effectCanvas;
    #endregion

    [Header("Prefabs")]
    #region Prefabs
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject hostagePrefab;
    [SerializeField] GameObject comboPrefab;
    #endregion

    [Header("")]
    #region List of objects
    [SerializeField] List<SpawnPos> spawnPos;
    #endregion

    [Header("")]
    #region Private Serialized Fields
    [SerializeField] int score;

    #endregion

    [Header("")]
    #region Private Fields

    bool paused;
    float startTime;
    int objectCount;
    int lives;
    float fireInterval = 0.5f;
    float lastFired;
    GameObject comboSpawned;

    #endregion

    [Header("")]
    #region Public Fields
    public int bulletAmount;
    #endregion

    #region MonoBehaviour Functions

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        ScoreAPI.GameStart((bool s) => {
        });
        startTime = Time.time;
        StartCoroutine(nameof(spawnCharacters));
        lives = 3;
        bulletAmount = 7;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - lastFired > fireInterval)
            {
                if (bulletAmount > 0){
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                    if (hit.collider != null)
                    {
                        ICharcterInteface hitCharacter = hit.collider.gameObject.GetComponent<ICharcterInteface>();
                        if (hitCharacter != null)
                        {
                            hitCharacter.TakeHit(hit.point);
                            lastFired = Time.time;
                            DecreaseBullet();
                        }
                    }
                }
                else
                {
                    ShowReloadText();
                    //Tung wala sound effects
                }
                
            }
        }
    }

    #endregion

    #region Public Functions

    public void ReloadGun()
    {
        bulletAmount = 7;
        foreach (Transform item in bulletHolderTransform)
        {
            Destroy(item.gameObject);
        }
        for (int i = 0; i < 7; i++)
        {
            Instantiate(bulletImage, bulletHolderTransform);
        }
    }

    public void ClearSpawnIndex(int index)
    {
        spawnPos[index].isSpawned = false;
    }

    public void TakeDamage()
    {
        //show damage effect;
        DecreaseLife();
    }

    public Vector3 GetMoveAwayPos(int index)
    {
        return spawnPos[index].spawn_position.position;
    }

    public void DecreaseLife()
    {
        Destroy(livesGameObject[lives - 1].gameObject);
        lives--;
        if (lives <= 0)
        {
            Debug.Log("Game Over");
            GameOver();
        }
    }
    public void SlowTime(float amount,float duration)
    {
        Time.timeScale = amount;
        Time.fixedDeltaTime = Time.timeScale * Time.deltaTime;
    }
    public void PauseGame()
    {
        //UIManager.instance.DisableCombo();
        //setHighScore(pausehighscoreText);
        //int min = (int)gameTimer / 60;
        //int sec = (int)gameTimer % 60;
        //pauseMenugameTimerText.text = min.ToString() + ":" + sec.ToString();
        Time.timeScale = 0;
        paused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        paused = false;
    }

    public void AddScore(int amount)
    {
        score += amount ;
        scoreText.text = score.ToString();
        setHighScore(gamePlayhighscoreText);
    }

    #endregion

    #region Private Functions

    private void ShowReloadText()
    {
        StopCoroutine("AutoDisableCombo");
        if (comboSpawned == null)
            comboSpawned = Instantiate(comboPrefab, effectCanvas.transform);
        EnableCombo();
        comboSpawned.transform.position = Vector2.zero;
        StartCoroutine("AutoDisableCombo");
    }
    void DisableCombo()
    {
        if (comboSpawned != null)
            comboSpawned.SetActive(false);
    }
    void EnableCombo()
    {
        if (comboSpawned != null)
            comboSpawned.SetActive(true);
    }

    IEnumerator AutoDisableCombo()
    {
        yield return new WaitForSecondsRealtime(1);
        DisableCombo();
        // Code to execute after the delay
    }

    void DecreaseBullet()
    {
        bulletAmount -= 1;
        if (bulletAmount <= 0)
            bulletAmount = 0;
        Destroy(bulletHolderTransform.GetChild(0).gameObject);
    }
    
    void GameOver()
    {
        PauseGame();
        UIManager.instance.SwitchCanvas(UIPanelType.GameOver);
        UIManager.instance.SwitchCanvas(UIPanelType.GameOver);
        //fruitsCutText.text = "Fruits Cut :  " + fruitscut.ToString();
        GameOverScoreText.text = "Score:            " + score.ToString();
        //MaxComboText.text = "Max Combo:  " + maxCombo.ToString();
        int playTime = (int)(Time.unscaledTime - startTime);
        ScoreAPI.SubmitScore(score, playTime, (bool s, string msg) => { });
        GetHighScore();
    }

    void setHighScore(TMP_Text highscroreTextUI)
    {
        ScoreAPI.GetData((bool s, Data_RequestData d) => {
            if (s)
            {
                if (score >= d.high_score)
                {
                    highscroreTextUI.text = score.ToString();

                }
                else
                {
                    highscroreTextUI.text = d.high_score.ToString();
                }

            }
        });
    }
    
    void GetHighScore()
    {
        ScoreAPI.GetData((bool s, Data_RequestData d) => {
            if (s)
            {
                if (score >= d.high_score)
                {
                    GameOverhighscoreText.text = score.ToString();

                }
                else
                {
                    GameOverhighscoreText.text =d.high_score.ToString();
                }

            }
        });

    }

    #endregion

    #region Coroutines

    IEnumerator spawnCharacters()
    {
        while (true)
        {

            int index = Random.Range(0, 3);
            int spawnIndex = Random.Range(0, spawnPos.Count);
            if (!spawnPos[spawnIndex].isSpawned)
            {
                //Enemy
                if (index == 0 || index==1)
                {
                    GameObject tempCharacter = Instantiate(enemyPrefab, spawnPos[spawnIndex].spawn_position.position, Quaternion.identity);
                    tempCharacter.GetComponent<ThugController>().spawnIndex = spawnIndex;
                    tempCharacter.GetComponent<ThugController>().MoveTowardsScreen(spawnPos[spawnIndex].final_position.position);
                    if (spawnIndex == 5)
                    {
                        tempCharacter.GetComponent<SpriteRenderer>().sortingOrder = 4;
                    }
                }
                else
                {
                    GameObject tempCharacter = Instantiate(hostagePrefab, spawnPos[spawnIndex].spawn_position.position, Quaternion.identity);
                    tempCharacter.GetComponent<HostageController>().spawnIndex = spawnIndex;
                    tempCharacter.GetComponent<HostageController>().MoveTowardsScreen(spawnPos[spawnIndex].final_position.position);
                    if (spawnIndex == 5)
                    {
                        tempCharacter.GetComponent<SpriteRenderer>().sortingOrder = 4;
                    }

                }
                spawnPos[spawnIndex].isSpawned = true;
                objectCount++;
                yield return new WaitForSeconds(1);

            }
            else
            {
                yield return new WaitForSeconds(0.01f);
            }
        }
    }

    #endregion
}
