using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public Transform player;
    public Camera mainCamera;

    public GameObject groundPrefab;
    public GameObject ceilingPrefab;
    public List<GameObject> backgroundsPrefab;
    public GameObject background1Prefab;

    public Transform groundParent;
    public Transform ceilingParent;
    public Transform backgroundParent;

    public ConfigHandler configHandler;
    public List<Transform> parents;

    [Header("UI Settings")]
    public TMP_Text scoreTxt;
    public TMP_Text coinsTxt;
    public Transform gameOverPanel;
    public Transform pausePanel;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public TMP_Text loseScoreTxt;
    public TMP_Text loseCoinsTxt;
    public Button loseWatchAdButton;
    public Transform adLoadFailedPanel;
    public Button closeAdLoadFailedButton;
    public Animator crossfade;

    [Header("Revive UI")]
    public Transform reviveMenu;
    public Button buyRevive;
    public Button watchAdRevive;
    public Slider reviveSlider;

    [Header("Audio Source")]
    public AudioSource backgroundMusic;
    public AudioSource buttonClickSfx;
    public AudioSource loseSfx;
    public AudioSource collectCoinSfx;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    private List<Transform> grounds;
    private List<Transform> ceilings;
    private List<List<Transform>> backgrounds;
    private GameObject background1;
    private List<List<Transform>> tilesPool;

    private Dictionary<char, Mapping> blockMapping;
    private List<int> changes;
    private PlayerData playerData;
    private GameState gameState = GameState.PLAYING;
    private ToastManager toastManager;
    private InterstitialAdManager interstitialAdManager;
    private RewardedAdManager rewardedAdManager;
    private Action adLoadFailedPanelClose;
    private BlockPoolManager blockPoolManager;

    private Sprite cubeSprite;
    private Sprite shipSprite;
    private Sprite ballSprite;
    private Sprite ufoSprite;
    private Sprite waveSprite;

    private int gameCoins = 0;
    private float gameTime = 0f;

    private int obstacleIndex = -1;
    private float backgroundScale;

    private PlayerMode playerMode = PlayerMode.CUBE;
    private bool isFlip = false;

    private bool isReviveActive = false;
    private bool isRevivedPaused = false;
    private int reviveChances = 2;
    private float reviveTimer = 0f;

    private List<int> playQuest = new List<int>() { 3, 6, 9 };
    private List<int> coinsQuest = new List<int>() { 150, 300, 450 };
    private List<int> scoreQuest = new List<int>() { 1500, 3000, 4500 };

    private string[] playQuestTitles = new string[] { "Warming Up", "Getting the Hang of It", "Game Veteran" };
    private string[] coinsQuestTitles = new string[] { "Coin Collector", "Treasure Hunter", "Gold Hoarder" };
    private string[] scoreQuestTitles = new string[] { "Point Chaser", "Score Master", "Legendary Scorer" };

    class Mapping
    {
        public int index;
        public int parentIndex;

        public Mapping(int index, int parentIndex)
        {
            this.index = index;
            this.parentIndex = parentIndex;
        }
    }

    private void Start()
    {
        playerData = PlayerData.LoadData();
        toastManager = GetComponent<ToastManager>();
        interstitialAdManager = InterstitialAdManager.GetInstance();
        rewardedAdManager = RewardedAdManager.GetInstance();
        blockPoolManager = GetComponent<BlockPoolManager>();

        tilesPool = new List<List<Transform>>();

        cubeSprite = configHandler.cubeShopItems[Mathf.Max(playerData.icons[0].IndexOf('2'), 0)].itemSprite;
        shipSprite = configHandler.shipShopItems[Mathf.Max(playerData.icons[1].IndexOf('2'), 0)].itemSprite;
        ballSprite = configHandler.ballShopItems[Mathf.Max(playerData.icons[2].IndexOf('2'), 0)].itemSprite;
        ufoSprite = configHandler.ufoShopItems[Mathf.Max(playerData.icons[3].IndexOf('2'), 0)].itemSprite;
        waveSprite = configHandler.waveShopItems[Mathf.Max(playerData.icons[4].IndexOf('2'), 0)].itemSprite;

        blockMapping = new Dictionary<char, Mapping>
        {
            { '0', new Mapping(-1, -1) },
            { '1', new Mapping(0, 0) },
            { '2', new Mapping(1, 0) },
            { '3', new Mapping(2, 0) },
            { '4', new Mapping(3, 0) },
            { '5', new Mapping(4, 0) },
            { '6', new Mapping(5, 0) },
            { '7', new Mapping(6, 0) },
            { '8', new Mapping(7, 0) },
            { '9', new Mapping(8, 0) },
            { 'A', new Mapping(9, 0) },
            { 'B', new Mapping(10, 0) },
            { 'C', new Mapping(11, 0) },
            { 'D', new Mapping(12, 0) },
            { 'E', new Mapping(13, 1) },
            { 'F', new Mapping(14, 1) },
            { 'G', new Mapping(15, 1) },
            { 'H', new Mapping(16, 1) },
            { 'I', new Mapping(17, 2) },
            { 'J', new Mapping(18, 2) },
            { 'K', new Mapping(19, 1) },
            { 'L', new Mapping(20, 1) },
            { 'M', new Mapping(21, 2) },
            { 'N', new Mapping(22, 2) },
            { 'O', new Mapping(23, 2) },
            { 'P', new Mapping(24, 2) },
            { 'Q', new Mapping(25, 2) },
            { 'R', new Mapping(26, 2) },
            { 'S', new Mapping(27, 2) },
            { 'T', new Mapping(28, 2) },
            { 'U', new Mapping(29, 0) },
            { 'V', new Mapping(30, 0) },
            { 'W', new Mapping(31, 0) },
            { 'X', new Mapping(32, 0) },
            { 'Y', new Mapping(33, 0) },
            { 'Z', new Mapping(34, 0) },
            { 'a', new Mapping(35, 0) },
            { 'b', new Mapping(36, 0) },
        };

        changes = new List<int> { 0, 0, 0, 1, 1, 1, 2, 3, 4, 5 };

        grounds = new List<Transform>();
        ceilings = new List<Transform>();
        backgrounds = new List<List<Transform>>
        {
            new List<Transform>(), new List<Transform>(), new List<Transform>(), new List<Transform>()
        };

        float worldHeight = mainCamera.orthographicSize * 2f;
        float worldWidth = worldHeight * mainCamera.aspect;

        Vector2 backgroundSpriteSize = backgroundsPrefab[0].GetComponent<SpriteRenderer>().bounds.size;

        float backgroundScaleX = worldWidth / backgroundSpriteSize.x;
        float backgroundScaleY = worldHeight / backgroundSpriteSize.y;

        backgroundScale = Mathf.Max(backgroundScaleX, backgroundScaleY);

        background1 = Instantiate(background1Prefab, Vector2.zero, Quaternion.identity, backgroundParent);

        background1.transform.localScale = new Vector2(backgroundScale, backgroundScale);

        for (int i = -1; i < 2; i++)
        {
            GameObject ground = Instantiate(groundPrefab, new Vector2(backgroundScale * i, -5.5f), Quaternion.identity, groundParent);
            GameObject ceiling = Instantiate(ceilingPrefab, new Vector2(backgroundScale * i, 5.5f), Quaternion.identity, ceilingParent);

            ground.transform.localScale = new Vector2(backgroundScale / 25f, 1f);
            ceiling.transform.localScale = new Vector2(backgroundScale / 25f, 1f);

            grounds.Add(ground.transform);
            ceilings.Add(ceiling.transform);

            for (int j = 0; j < backgroundsPrefab.Count; j++)
            {
                GameObject background = Instantiate(backgroundsPrefab[j], new Vector2(backgroundScale * i, 0), Quaternion.identity, backgroundParent);
                background.transform.localScale = new Vector2(backgroundScale, backgroundScale);
                backgrounds[j].Add(background.transform);
            }
        }

        player.GetComponent<Player>().SetSafePosition(new Vector2((obstacleIndex * 21) + 7.5f, 0f));

        TextAsset startChunk = Resources.Load<TextAsset>("start_obstacle");

        MapObstacle(startChunk);

        for (int i = 0; i < 3; i++)
        {
            GenerateObstacle();
        }

        coinsTxt.text = $"COINS: {gameCoins}";

        UpdateMusicVolume();
        UpdateSfxVolume();

        Input.multiTouchEnabled = false;
    }

    public void SetColors(ParticleSystem particles, TrailRenderer trail, SpriteRenderer child, SpriteRenderer grandChild)
    {
        Color primaryColor = configHandler.colorShopItems[Mathf.Max(playerData.colors.IndexOf('3'), 0)].color;
        Color secondaryColor = configHandler.colorShopItems[Mathf.Max(playerData.colors.IndexOf('2'), 0)].color;

        var main = particles.main;
        main.startColor = primaryColor;

        trail.startColor = secondaryColor;

        child.material.SetColor("_PrimaryColor", primaryColor);
        child.material.SetColor("_SecondaryColor", secondaryColor);

        grandChild.material.SetColor("_PrimaryColor", primaryColor);
        grandChild.material.SetColor("_SecondaryColor", secondaryColor);
    }

    public Sprite CubeSprite() => cubeSprite;

    public Sprite ShipSprite() => shipSprite;

    public Sprite BallSprite() => ballSprite;

    public Sprite UFOSprite() => ufoSprite;

    public Sprite WaveSprite() => waveSprite;

    private void Update()
    {
        scoreTxt.text = $"SCORE: {Mathf.Max(0, Mathf.RoundToInt(player.transform.position.x))}";
        gameTime += Time.deltaTime;

        float cameraXSize = mainCamera.orthographicSize * mainCamera.aspect;

        mainCamera.transform.position = new Vector3(Mathf.Max(0f, (player.position.x + cameraXSize) - 6f), mainCamera.transform.position.y, mainCamera.transform.position.z);
        background1.transform.position = new Vector2(mainCamera.transform.position.x, mainCamera.transform.position.y);
        
        float cameraX = mainCamera.transform.position.x;

        if (cameraX > grounds[0].position.x + backgroundScale)
        {
            Transform topGround = grounds[0];
            Transform topCeiling = ceilings[0];

            grounds.RemoveAt(0);
            ceilings.RemoveAt(0);

            grounds.Add(topGround);
            ceilings.Add(topCeiling);

            topGround.position = new Vector2(topGround.position.x + (backgroundScale * grounds.Count), topGround.position.y);
            topCeiling.position = new Vector2(topCeiling.position.x + (backgroundScale * ceilings.Count), topCeiling.position.y);
        }
        else if (cameraX < grounds.Last().position.x - (backgroundScale * 2))
        {
            Transform bottomGround = grounds.Last();
            Transform bottomCeiling = ceilings.Last();

            grounds.RemoveAt(grounds.Count - 1);
            ceilings.RemoveAt(ceilings.Count - 1);

            grounds.Insert(0, bottomGround);
            ceilings.Insert(0, bottomCeiling);

            bottomGround.position = new Vector2(bottomGround.position.x - (backgroundScale * grounds.Count), bottomGround.position.y);
            bottomCeiling.position = new Vector2(bottomCeiling.position.x - (backgroundScale * ceilings.Count), bottomCeiling.position.y);
        }

        float[] backgroundMovements = new float[] { 4f, 3f, 2f, 1f };

        for (int i = 0; i < backgrounds.Count; i++)
        {
            List<Transform> background = backgrounds[i];

            if (cameraX > background[0].position.x + backgroundScale)
            {
                Transform topBackground = background[0];
                background.RemoveAt(0);
                background.Add(topBackground);
                topBackground.position = new Vector2(topBackground.position.x + (backgroundScale * background.Count), topBackground.position.y);
            }

            else if (cameraX < background.Last().position.x - (backgroundScale * 2))
            {
                Transform bottomBackground = background.Last();
                background.RemoveAt(background.Count - 1);
                background.Insert(0, bottomBackground);
                bottomBackground.position = new Vector2(bottomBackground.position.x - (backgroundScale * background.Count), bottomBackground.position.y);
            }

            if (mainCamera.transform.position.x > 0f)
            {
                foreach (Transform bg in background)
                {
                    bg.position = new Vector2(bg.position.x + Time.deltaTime * backgroundMovements[i], bg.position.y);
                }
            }
        }

        if (cameraX > (obstacleIndex - 1) * 21)
        {
            DestroyObstacles();
            GenerateObstacle();
        }

        if (isReviveActive && !isRevivedPaused)
        {
            reviveTimer -= Time.unscaledDeltaTime;

            reviveSlider.value = reviveTimer / 5f;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.phase == TouchPhase.Began)
                {
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        PointerEventData pointerData = new PointerEventData(EventSystem.current);
                        pointerData.position = touch.position;

                        List<RaycastResult> results = new List<RaycastResult>();
                        EventSystem.current.RaycastAll(pointerData, results);

                        bool clickedButton = results.Exists(r => r.gameObject.GetComponent<Button>() != null);

                        if (clickedButton) continue;
                    }

                    reviveTimer -= 2f;
                }
            }

            if (reviveTimer <= 0f)
            {
                reviveMenu.gameObject.SetActive(false);

                isReviveActive = false;

                GameOver();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (adLoadFailedPanel.gameObject.activeSelf)
            {
                adLoadFailedPanelClose?.Invoke();
            }
            else
            {
                switch (gameState)
                {
                    case GameState.PLAYING:
                        Pause();
                        break;
                    case GameState.PAUSED:
                        HomeButton();
                        break;
                    case GameState.LOSE:
                        HomeButton();
                        break;
                    case GameState.REVIVE:
                        reviveTimer = 0f;
                        break;
                }
            }
        }
    }

    private void GenerateObstacle()
    {
        int randomChanges = changes[UnityEngine.Random.Range(0, changes.Count)];

        Dictionary<PlayerMode, List<int>> mapping = new Dictionary<PlayerMode, List<int>>
        {
            { PlayerMode.CUBE, new List<int> { 1, 2, 3, 4 } },
            { PlayerMode.SHIP, new List<int> { 0, 2, 3, 4 } },
            { PlayerMode.BALL, new List<int> { 0, 1, 3, 4 } },
            { PlayerMode.UFO, new List<int> { 0, 1, 2, 4 } },
            { PlayerMode.WAVE, new List<int> { 0, 1, 2, 3 } }
        };

        Dictionary<PlayerMode, string> modeNames = new Dictionary<PlayerMode, string>
        {
            { PlayerMode.CUBE, "Cube" },
            { PlayerMode.SHIP, "Ship" },
            { PlayerMode.BALL, "Ball" },
            { PlayerMode.UFO, "UFO" },
            { PlayerMode.WAVE, "Wave" }
        };

        string pathOne = modeNames[playerMode];
        string pathTwo = GetChunkDifficulty();
        string pathThree = isFlip ? "Flip" : "Normal";
        string pathFour = (UnityEngine.Random.Range(0, randomChanges < 2 ? 6 : 2) + 1).ToString();

        if (randomChanges == 1)
        {
            isFlip = !isFlip;
            pathThree = isFlip ? "Normal to Flip" : "Flip to Normal";
        }
        if (randomChanges >= 2 && randomChanges <= 5)
        {
            playerMode = (PlayerMode)mapping[playerMode][randomChanges - 2];
            pathThree += " to " + modeNames[playerMode];
        }

        TextAsset chunk = Resources.Load<TextAsset>(Path.Combine(pathOne, pathTwo, pathThree, pathFour));

        MapObstacle(chunk);
    }

    private void MapObstacle(TextAsset chunk)
    {
        string[] lines = chunk.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        List<Transform> tiles = new List<Transform>();

        for (int i = 0; i < lines.Length; i++)
        {
            for (int j = 0; j < lines[i].Length; j++)
            {
                Mapping blockMap = blockMapping[lines[i][j]];
                int index = blockMap.index;
                int parentIndex = blockMap.parentIndex;
                if (index >= 0)
                {
                    GameObject prefab = configHandler.objectPrefab[index];
                    float xSpawnPoint = (j + 10.5f) + (obstacleIndex * 21);
                    float ySpawnPoint = -i + 4.5f;

                    Transform parent = parentIndex >= 0 ? parents[parentIndex] : null;

                    //GameObject instance = blockPoolManager.Get(prefab, parent);
                    GameObject instance = Instantiate(prefab, parent);
                    instance.transform.position = new Vector3(xSpawnPoint, ySpawnPoint);
                    instance.transform.rotation = prefab.transform.rotation;

                    tiles.Add(instance.transform);
                }
            }
        }

        tilesPool.Add(tiles);

        obstacleIndex++;
    }

    private void DestroyObstacles()
    {
        List<Transform> topObstacles = tilesPool[0];
        tilesPool.RemoveAt(0);

        foreach (Transform t in topObstacles)
        {
            //blockPoolManager.Release(t.gameObject);
            if (t != null) Destroy(t.gameObject);
        }
    }

    private string GetChunkDifficulty()
    {
        if (obstacleIndex >= 0 && obstacleIndex <= 3)
        {
            return "Easy";
        }
        else if (obstacleIndex >= 4 && obstacleIndex <= 6)
        {
            return "Normal";
        }
        else if (obstacleIndex >= 7)
        {
            return "Hard";
        }
        else
        {
            return "Easy";
        }
    }

    public GameState GetGameState()
    {
        return gameState;
    }

    public int GetObstacleIndex()
    {
        return obstacleIndex;
    }

    private void SetDataWhenLose()
    {
        playerData.highscore = Mathf.Max(playerData.highscore, Mathf.Max(0, Mathf.RoundToInt(player.transform.position.x)));
        playerData.coins += gameCoins;
        playerData.totalCoins += gameCoins;
        playerData.highestGameCoins = Mathf.Max(playerData.highestGameCoins, gameCoins);
        playerData.totalScore += Mathf.Max(0, Mathf.RoundToInt(player.transform.position.x));
        playerData.totalTime += gameTime;
        playerData.gamesPlayed++;

        UpdatePlayQuest();
        UpdateCoinsQuest(gameCoins);
        UpdateScoreQuest(Mathf.Max(0, Mathf.RoundToInt(player.transform.position.x)));

        playerData.SaveData();
    }

    public void CollectCoin(Collider2D collision)
    {
        //blockPoolManager.Release(collision.gameObject);
        Destroy(collision.gameObject);

        gameCoins++;

        coinsTxt.text = $"COINS: {gameCoins}";

        collectCoinSfx.Play();
    }

    private void UpdateMusicVolume()
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(playerData.musicVolume) * 20);
    }

    private void UpdateSfxVolume()
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(playerData.sfxVolume) * 20);
    }

    public void OnMusicVolumeChange(float value)
    {
        playerData.musicVolume = value;
        UpdateMusicVolume();
    }

    public void OnSfxVolumeChange(float value)
    {
        playerData.sfxVolume = value;
        UpdateSfxVolume();
    }

    public void OnRevive()
    {
        if (reviveChances > 0)
        {
            Time.timeScale = 0f;

            isReviveActive = true;
            reviveTimer = 5f;

            reviveMenu.gameObject.SetActive(true);

            gameState = GameState.REVIVE;

            buyRevive.interactable = playerData.coins >= 200;

            PauseAllSfx();
        }
        else
        {
            GameOver();
        }
    }

    public void BuyRevive()
    {
        buttonClickSfx.Play();

        playerData.coins -= 200;
        playerData.revivesDone++;
        reviveChances--;
        isReviveActive = false;

        gameState = GameState.PLAYING;

        playerData.SaveData();

        player.GetComponent<Player>().SetPlayerAfterRevive();

        UnpauseAllSfx();

        reviveMenu.gameObject.SetActive(false);

        Time.timeScale = 1f;
    }

    public void WatchAdRevive()
    {
        watchAdRevive.interactable = false;

        buttonClickSfx.Play();
        isRevivedPaused = true;
        toastManager.PauseToasts();

        rewardedAdManager.ShowRewardedAd(() => { }, () =>
        {
            watchAdRevive.interactable = true;

            playerData.revivesDone++;
            reviveChances--;
            isReviveActive = false;
            isRevivedPaused = false;

            gameState = GameState.PLAYING;

            playerData.SaveData();

            player.GetComponent<Player>().SetPlayerAfterRevive();

            toastManager.ResumeToasts();

            UnpauseAllSfx();

            reviveMenu.gameObject.SetActive(false);

            Time.timeScale = 1f;
        }, () =>
        {
            watchAdRevive.interactable = true;

            toastManager.ResumeToasts();

            OpenAdLoadFailedPanel(() =>
            {
                isRevivedPaused = false;
            });

            StartCoroutine(SetPauseAfterAd());
        });
    }

    private void GameOver()
    {
        gameState = GameState.LOSE;

        StopAllSfx();

        SetDataWhenLose();

        Time.timeScale = 0f;

        if (gameCoins > 0)
        {
            loseWatchAdButton.interactable = true;

            loseWatchAdButton.onClick.RemoveAllListeners();

            loseWatchAdButton.onClick.AddListener(() =>
            {
                loseWatchAdButton.interactable = false;

                buttonClickSfx.Play();
                toastManager.PauseToasts();

                rewardedAdManager.ShowRewardedAd(() => { }, () =>
                {
                    playerData.coins += gameCoins;
                    playerData.totalCoins += gameCoins;

                    playerData.SaveData();

                    toastManager.ResumeToasts();

                    StartCoroutine(AnimationManager.AnimateCoinText(loseCoinsTxt, gameCoins, gameCoins * 2, "", " <sprite index=0>"));

                    loseWatchAdButton.interactable = false;
                    loseWatchAdButton.GetComponentInChildren<TMP_Text>().text = "Watched";

                    StartCoroutine(SetPauseAfterAd());
                }, () =>
                {
                    loseWatchAdButton.interactable = true;

                    OpenAdLoadFailedPanel(() => { });
                    toastManager.ResumeToasts();
                    StartCoroutine(SetPauseAfterAd());
                });
            });
        }
        else
        {
            loseWatchAdButton.interactable = false;
        }

        toastManager.PauseToasts();

        interstitialAdManager.ShowInterstitial(() =>
        {
            toastManager.ResumeToasts();

            StartCoroutine(SetPauseAfterAd());
            StartCoroutine(ShowGameOver());
        });
    }

    private IEnumerator ShowGameOver()
    {
        loseSfx.Play();

        yield return new WaitForSecondsRealtime(1f);

        gameOverPanel.gameObject.SetActive(true);
        gameOverPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", true);

        yield return new WaitForSecondsRealtime(0.2f);

        StartCoroutine(AnimationManager.AnimateCoinText(loseCoinsTxt, 0, gameCoins, "", " <sprite index=0>"));
        StartCoroutine(AnimationManager.AnimateCoinText(loseScoreTxt, 0, Mathf.Max(0, Mathf.RoundToInt(player.transform.position.x)), "SCORE: ", ""));
    }

    private void PauseAllSfx()
    {
        backgroundMusic.Pause();

        if (collectCoinSfx.isPlaying)
        {
            collectCoinSfx.Pause();
        }
    }

    private void UnpauseAllSfx()
    {
        backgroundMusic.UnPause();

        if (!collectCoinSfx.isPlaying && collectCoinSfx.time > 0f && collectCoinSfx.time < collectCoinSfx.clip.length)
        {
            collectCoinSfx.UnPause();
        }
    }

    private void StopAllSfx()
    {
        backgroundMusic.Stop();

        if (collectCoinSfx.isPlaying)
        {
            collectCoinSfx.Stop();
        }
    }

    public void Pause()
    {
        pausePanel.gameObject.SetActive(true);
        pausePanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", true);
        PauseAllSfx();
        buttonClickSfx.Play();
        gameState = GameState.PAUSED;

        musicVolumeSlider.value = playerData.musicVolume;
        sfxVolumeSlider.value = playerData.sfxVolume;

        Time.timeScale = 0f;
    }

    public void Continue()
    {
        pausePanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", false);
        playerData.SaveData();
        buttonClickSfx.Play();
        StartCoroutine(DelayedUnpause());
    }

    private IEnumerator DelayedUnpause()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        pausePanel.gameObject.SetActive(false);
        UnpauseAllSfx();
        gameState = GameState.PLAYING;
        Time.timeScale = 1f;
    }

    public void HomeButton()
    {
        buttonClickSfx.Play();

        if (gameState == GameState.PAUSED)
        {
            SetDataWhenLose();
        }

        StartCoroutine(SwitchScene("Menu"));
    }

    public void RetryButton()
    {
        buttonClickSfx.Play();

        if (gameState == GameState.PAUSED)
        {
            SetDataWhenLose();
        }

        StartCoroutine(SwitchScene("Game"));
    }

    private void UpdatePlayQuest()
    {
        if (playerData.playQuestTotal > 0 && playerData.playQuestProgress < playerData.playQuestTotal)
        {
            playerData.playQuestProgress++;

            if (playerData.playQuestProgress >= playerData.playQuestTotal)
            {
                int index = playQuest.IndexOf(Mathf.Abs(playerData.playQuestTotal));
                toastManager.ShowToast("Quest Completed: " + playQuestTitles[index]);
            }
        }
    }

    private void UpdateCoinsQuest(int coinsReceived)
    {
        if (playerData.coinsQuestTotal > 0 && playerData.coinsQuestProgress < playerData.coinsQuestTotal)
        {
            playerData.coinsQuestProgress += coinsReceived;

            if (playerData.coinsQuestProgress >= playerData.coinsQuestTotal)
            {
                int index = coinsQuest.IndexOf(Mathf.Abs(playerData.coinsQuestTotal));
                toastManager.ShowToast("Quest Completed: " + coinsQuestTitles[index]);
            }
        }
    }

    private void UpdateScoreQuest(int score)
    {
        if (playerData.scoreQuestTotal > 0 && playerData.scoreQuestProgress < playerData.scoreQuestTotal)
        {
            playerData.scoreQuestProgress += score;
            if (playerData.scoreQuestProgress >= playerData.scoreQuestTotal)
            {
                int index = scoreQuest.IndexOf(Mathf.Abs(playerData.scoreQuestTotal));
                toastManager.ShowToast("Quest Completed: " + scoreQuestTitles[index]);
            }
        }
    }

    private IEnumerator SetPauseAfterAd()
    {
        yield return null; // Wait 1 frame so SDK finishes its reset
        Time.timeScale = 0f;
    }

    public void OpenAdLoadFailedPanel(Action onPanelCloseExtraAction)
    {
        adLoadFailedPanel.gameObject.SetActive(true);
        adLoadFailedPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", true);

        void PanelCloseAction()
        {
            onPanelCloseExtraAction?.Invoke();

            adLoadFailedPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", false);
            buttonClickSfx.Play();
            StartCoroutine(DelayedPanelClose(adLoadFailedPanel));
        }

        adLoadFailedPanelClose = PanelCloseAction;

        closeAdLoadFailedButton.onClick.RemoveAllListeners();
        closeAdLoadFailedButton.onClick.AddListener(() => PanelCloseAction());
    }

    private IEnumerator DelayedPanelClose(Transform panel)
    {
        yield return new WaitForSecondsRealtime(0.2f);
        panel.gameObject.SetActive(false);
    }

    private IEnumerator SwitchScene(string name)
    {
        crossfade.GetComponent<CanvasGroup>().blocksRaycasts = true;
        crossfade.SetBool("isOpen", true);
        yield return new WaitForSecondsRealtime(0.3f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(name);
    }
}
