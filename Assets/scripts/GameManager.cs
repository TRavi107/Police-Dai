using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RamailoGames;
using FirstGearGames.SmoothCameraShaker;

[System.Serializable]
public class SpawnPos
{
    public Transform final_position;
    public Transform spawn_position;
    public bool isSpawned;
}

public class GameManager : MonoBehaviour
{
    public ShakeData shakeData;
    public ShakeData MinishakeData;
    [Header("UIS")]
    #region UIS

    public TMP_Text GameOverScoreText;
    public TMP_Text GameOverhighscoreText;
    public TMP_Text scoreText;
    public TMP_Text gamePlayhighscoreText;
    public Image bulletImage;
    public Image HealthImage;

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
    public Transform livesHoldertransform;
    public Transform bulletHolderTransform;
    public Canvas effectCanvas;
    public GameObject mainMenuPanel;
    #endregion

    [Header("Prefabs")]
    #region Prefabs
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject hostagePrefab;
    [SerializeField] GameObject comboPrefab;
    [SerializeField] GameObject bloodPrefab;
    [SerializeField] GameObject hitEffectPrefab;
    [SerializeField] GameObject[] PowerUpsPrefab;
    #endregion

    [Header("")]
    #region List of objects
    [SerializeField] List<SpawnPos> spawnPos;
    #endregion

    [Header("Private Serialized Fields")]
    #region Private Serialized Fields
    [SerializeField] int score;
    [SerializeField] float enemySpawnDuration;
    [SerializeField] float LevelIncreaseDuration;
    [Range(1,100)]
    [SerializeField] int powerUpSpawnChance;

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
    bool ghostMode;
    float lastLevelUpdated;
    #endregion

    [Header("")]
    #region Public Fields
    public int bulletAmount;
    public float enemyWaitBeforeShootDuration;
    public float MaxcenemyWaitBeforeShootDuration;
    #endregion

    #region MonoBehaviour Functions

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        ScoreAPI.GameStart((bool s) => {
        });
        startTime = Time.time;
        StartCoroutine(nameof(SpawnCharacters));
        bulletAmount = 7;
        for (int i = 0; i < 3; i++)
        {
            AddHealth();
        }
        PauseGame();
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
                            soundManager.instance.PlaySound(SoundType.pistolShoot);
                        }
                        else if (hit.collider.CompareTag("PowerUp"))
                        {
                            lastFired = Time.time;
                            DecreaseBullet();
                            soundManager.instance.PlaySound(SoundType.pistolShoot);
                            AddHealth();
                            Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
                            Destroy(hit.collider.gameObject,0.1f);
                        }
                    }
                }
                else
                {
                    ShowReloadText();
                    //Tung wala sound effects
                }
                
            }

            if(UIManager.instance.activeUIPanel.uiPanelType == UIPanelType.howToplay)
            {
                UIManager.instance.SwitchCanvas(UIPanelType.mainGame);
                ResumeGame();
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
        soundManager.instance.PlaySound(SoundType.ReloadShoot);
    }

    public void ClearSpawnIndex(int index)
    {
        spawnPos[index].isSpawned = false;
    }

    public void TakeDamage()
    {
        //show damage effect;
        if (!ghostMode)
        {
            Destroy(Instantiate(bloodPrefab), 0.3f);
            mainMenuPanel.GetComponent<Image>().enabled = true;
            Invoke(nameof(DisableBloodEffect), 0.3f);
            DecreaseLife();
            Camera.main.GetComponent<CameraShaker>().Shake(shakeData);
            ghostMode = true;
        }
    }

    private void DisableBloodEffect()
    {
        mainMenuPanel.GetComponent<Image>().enabled = false;
        ghostMode = false;
    }

    public Vector3 GetMoveAwayPos(int index)
    {
        return spawnPos[index].spawn_position.position;
    }

    public void DecreaseLife()
    {
        lives--;
        if (lives <= 0)
        {
            Debug.Log("Game Over");
            GameOver();
            return;
        }
        Destroy(livesHoldertransform.GetChild(0).gameObject);

    }
    public void SlowTime(float amount,float duration)
    {
        Time.timeScale = amount;
        Time.fixedDeltaTime = Time.timeScale * Time.deltaTime;
    }
    public void PauseGame()
    {
        DisableCombo();
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
        if (Time.time - lastLevelUpdated > LevelIncreaseDuration)
        {
            lastLevelUpdated = Time.time;
            enemyWaitBeforeShootDuration -= 0.1f;
            if (enemyWaitBeforeShootDuration <= MaxcenemyWaitBeforeShootDuration)
                enemyWaitBeforeShootDuration = MaxcenemyWaitBeforeShootDuration;
        }
    }

    #endregion

    #region Private Functions

    void AddHealth()
    {
        if (lives < 3)
        {
            Instantiate(HealthImage, livesHoldertransform);
            lives++;
        }
    }

    void ShowReloadText()
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
        //Camera.main.GetComponent<CameraShaker>().Shake(MinishakeData);
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

    IEnumerator SpawnCharacters()
    {
        while (true)
        {
            int spawnIndex = Random.Range(0, spawnPos.Count);

            
            if (!spawnPos[spawnIndex].isSpawned)
            {
                int index = Random.Range(0, 3);

                if (Random.Range(0, 100) < powerUpSpawnChance)
                {
                    int powerUpspawnIndex = Random.Range(0, PowerUpsPrefab.Length);
                    GameObject powerupTemp =Instantiate(PowerUpsPrefab[powerUpspawnIndex],
                        new( 
                        spawnPos[spawnIndex].final_position.position.x,
                        spawnPos[spawnIndex].final_position.position.y+0.4f
                        ), 
                        Quaternion.identity);

                    powerupTemp.GetComponent<PowerUpController>().spawnIndex = spawnIndex;
                    Destroy(powerupTemp, 2);
                }
                //Enemy
                else if (index == 0 || index==1)
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
                yield return new WaitForSeconds(enemySpawnDuration);

            }
            else
            {
                yield return new WaitForSeconds(0.01f);
            }
        }
    }

    #endregion
}
