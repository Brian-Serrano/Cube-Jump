using Newtonsoft.Json.Utilities;
using PimDeWitte.UnityMainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Background")]
    public List<GameObject> backgroundPrefab;
    public GameObject background1Prefab;
    public Transform backgroundParent;
    public Camera mainCamera;

    [Header("Main UI")]
    public TMP_Text bestScoreMainTxt;
    public TMP_Text coinsMainTxt;
    public GameObject coinPrefab;
    public RectTransform coinsIcon;
    public Canvas canvas;
    public Transform adLoadFailedPanel;
    public Animator crossfade;

    [Header("Stats UI")]
    public Transform statsPanel;
    public TMP_Text bestScoreTxt;
    public TMP_Text coinsTxt;
    public TMP_Text totalScoreTxt;
    public TMP_Text gamesPlayedTxt;
    public TMP_Text mostCoinsCollectedTxt;
    public TMP_Text totalTimeTxt;
    public TMP_Text totalCoinsTxt;
    public TMP_Text revivesDoneTxt;
    public TMP_Text cubeIconsOwnedTxt;
    public TMP_Text shipIconsOwnedTxt;
    public TMP_Text ballIconsOwnedTxt;
    public TMP_Text ufoIconsOwnedTxt;
    public TMP_Text waveIconsOwnedTxt;
    public TMP_Text colorsOwnedTxt;

    [Header("Settings UI")]
    public Transform settingsPanel;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Quest UI")]
    public Transform questPanel;
    public TMP_Text newQuestRemTime;

    public Button questOneButton;
    public Button questTwoButton;
    public Button questThreeButton;

    public TMP_Text questOneTitle;
    public TMP_Text questTwoTitle;
    public TMP_Text questThreeTitle;

    public TMP_Text questOneDescription;
    public TMP_Text questTwoDescription;
    public TMP_Text questThreeDescription;

    public TMP_Text noQuestOne;
    public TMP_Text noQuestTwo;
    public TMP_Text noQuestThree;

    [Header("Daily UI")]
    public Transform dailyPanel;

    public Button dailyOneButton;
    public Button dailyTwoButton;
    public Button dailyThreeButton;
    public Button dailyFourButton;

    public Animator chestOneSprite;
    public Animator chestTwoSprite;
    public Animator chestThreeSprite;
    public Animator chestFourSprite;

    public TMP_Text newDailyRemTime;

    [Header("Account UI")]
    public Transform accountPanel;
    public Transform loginPanel;
    public Transform signupPanel;
    public TMP_InputField loginUsernameTxt;
    public TMP_InputField loginPasswordTxt;
    public TMP_Text loginErrorTxt;
    public TMP_InputField signupUsernameTxt;
    public TMP_InputField signupEmailTxt;
    public TMP_InputField signupPasswordTxt;
    public TMP_InputField signupConfirmPasswordTxt;
    public TMP_Text signupErrorTxt;

    public Button signupButton;
    public Button loginButton;
    public Button saveButton;
    public Button loadButton;
    public Button logoutButton;
    public Button signupSubmitButton;
    public Button loginSubmitButton;

    public TMP_Text accountErrorTxt;
    public TMP_Text accountLoginText;
    public GameObject spinnerContainer;

    [Header("Confirm Panel UI")]
    public Transform confirmPanel;
    public TMP_Text confirmPanelText;
    public Button confirmPanelOkButton;

    [Header("Audio Source")]
    public AudioSource backgroundMusic;
    public AudioSource buttonClickSfx;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    private PlayerData playerData;
    private RewardedAdManager rewardedAdManager;
    private CubeJumpHTTPClient client;
    private List<List<Transform>> backgrounds;

    private float backgroundScale;

    private List<int> playQuest = new List<int>() { 3, 6, 9 };
    private List<int> coinsQuest = new List<int>() { 150, 300, 450 };
    private List<int> scoreQuest = new List<int>() { 1500, 3000, 4500 };

    private string[] playQuestTitles = new string[] { "Warming Up", "Getting the Hang of It", "Game Veteran" };
    private string[] coinsQuestTitles = new string[] { "Coin Collector", "Treasure Hunter", "Gold Hoarder" };
    private string[] scoreQuestTitles = new string[] { "Point Chaser", "Score Master", "Legendary Scorer" };

    private void Awake()
    {
        playerData = PlayerData.LoadData();
        rewardedAdManager = RewardedAdManager.GetInstance();

        BannerAdManager.GetInstance().EnsureBannerVisible();

        client = CubeJumpHTTPClient.GetInstance();

        backgrounds = new List<List<Transform>>
        {
            new List<Transform>(), new List<Transform>(), new List<Transform>(), new List<Transform>()
        };

        bestScoreMainTxt.text = $"BEST SCORE: {playerData.highscore}";
        coinsMainTxt.text = playerData.coins.ToString();

        Input.multiTouchEnabled = false;
    }

    private void Start()
    {
        UpdateMusicVolume();
        UpdateSfxVolume();

        CheckForNewQuest();

        float worldHeight = mainCamera.orthographicSize * 2f;
        float worldWidth = worldHeight * mainCamera.aspect;

        Vector2 backgroundSpriteSize = backgroundPrefab[0].GetComponent<SpriteRenderer>().bounds.size;

        float backgroundScaleX = worldWidth / backgroundSpriteSize.x;
        float backgroundScaleY = worldHeight / backgroundSpriteSize.y;

        backgroundScale = Mathf.Max(backgroundScaleX, backgroundScaleY);

        GameObject background1 = Instantiate(background1Prefab, Vector2.zero, Quaternion.identity, backgroundParent);

        background1.transform.localScale = new Vector2(backgroundScale, backgroundScale);

        for (int i = -1; i < 2; i++)
        {
            for (int j = 0; j < backgroundPrefab.Count; j++)
            {
                GameObject backgroundObj = Instantiate(backgroundPrefab[j], new Vector2(backgroundScale * i, 0f), Quaternion.identity, backgroundParent);

                backgroundObj.transform.localScale = new Vector2(backgroundScale, backgroundScale);

                backgrounds[j].Add(backgroundObj.transform);
            }
        }
    }

    private void Update()
    {
        float[] backgroundMoveSpeed = new float[] { -2f, -3f, -4f, -5f };

        for (int i = 0; i < backgrounds.Count; i++)
        {
            List<Transform> background = backgrounds[i];

            if (background[0].transform.position.x < -backgroundScale)
            {
                Transform topBackground = background[0];
                background.RemoveAt(0);
                background.Add(topBackground);
                topBackground.position = new Vector2(topBackground.position.x + (backgroundScale * background.Count), 0f);
            }

            foreach (Transform bg in background)
            {
                bg.position = new Vector2(bg.position.x + Time.deltaTime * backgroundMoveSpeed[i], 0f);
            }
        }

        if (questPanel.gameObject.activeSelf)
        {
            newQuestRemTime.text = "NEW QUESTS IN: " + GetTimeRemaining();
        }
        if (dailyPanel.gameObject.activeSelf)
        {
            newDailyRemTime.text = "NEW REWARDS IN: " + GetTimeRemaining();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!spinnerContainer.activeSelf)
            {
                if (questPanel.gameObject.activeSelf)
                {
                    CloseQuestPanel();
                }
                else if (dailyPanel.gameObject.activeSelf && !adLoadFailedPanel.gameObject.activeSelf)
                {
                    CloseDailyPanel();
                }
                else if (settingsPanel.gameObject.activeSelf)
                {
                    CloseSettingsPanel();
                }
                else if (statsPanel.gameObject.activeSelf)
                {
                    CloseStatsPanel();
                }
                else if (adLoadFailedPanel.gameObject.activeSelf)
                {
                    CloseAdLoadFailedPanel();
                }
                else if (accountPanel.gameObject.activeSelf && !signupPanel.gameObject.activeSelf && !loginPanel.gameObject.activeSelf && !confirmPanel.gameObject.activeSelf)
                {
                    CloseAccountPanel();
                }
                else if (signupPanel.gameObject.activeSelf)
                {
                    CloseSignupPanel();
                }
                else if (loginPanel.gameObject.activeSelf)
                {
                    CloseLoginPanel();
                }
                else if (confirmPanel.gameObject.activeSelf)
                {
                    CloseConfirmPanel();
                }
                else
                {
                    Quit();
                }
            }
        }
    }

    private string GetTimeRemaining()
    {
        DateTime now = DateTime.Now;
        TimeSpan remaining = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0).AddDays(1) - now;

        // Format as HH:mm:ss
        return remaining.ToString(@"hh\:mm\:ss");
    }

    public void Play()
    {
        buttonClickSfx.Play();
        StartCoroutine(SwitchScene("Game"));
    }

    public void Leaderboard()
    {
        buttonClickSfx.Play();
        StartCoroutine(SwitchScene("Leaderboard"));
    }

    public void Shop()
    {
        buttonClickSfx.Play();
        StartCoroutine(SwitchScene("Shop"));
    }

    public void Quit()
    {
        buttonClickSfx.Play();
        Application.Quit();
    }

    public void OpenStatsPanel()
    {
        statsPanel.gameObject.SetActive(true);
        statsPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", true);

        buttonClickSfx.Play();

        bestScoreTxt.text = playerData.highscore.ToString();
        coinsTxt.text = playerData.coins.ToString();
        totalScoreTxt.text = playerData.totalScore.ToString();
        gamesPlayedTxt.text = playerData.gamesPlayed.ToString();
        mostCoinsCollectedTxt.text = playerData.highestGameCoins.ToString();
        totalTimeTxt.text = Mathf.RoundToInt(playerData.totalTime).ToString();
        totalCoinsTxt.text = playerData.totalCoins.ToString();
        revivesDoneTxt.text = playerData.revivesDone.ToString();
        cubeIconsOwnedTxt.text = playerData.icons[0].Count(x => x != '0').ToString();
        shipIconsOwnedTxt.text = playerData.icons[1].Count(x => x != '0').ToString();
        ballIconsOwnedTxt.text = playerData.icons[2].Count(x => x != '0').ToString();
        ufoIconsOwnedTxt.text = playerData.icons[3].Count(x => x != '0').ToString();
        waveIconsOwnedTxt.text = playerData.icons[4].Count(x => x != '0').ToString();
        colorsOwnedTxt.text = playerData.colors.Count(x => x != '0').ToString();
    }

    public void CloseStatsPanel()
    {
        statsPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", false);
        buttonClickSfx.Play();
        StartCoroutine(DelayedPanelClose(statsPanel));
    }

    public void OpenSettingsPanel()
    {
        settingsPanel.gameObject.SetActive(true);
        settingsPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", true);

        musicVolumeSlider.value = playerData.musicVolume;
        sfxVolumeSlider.value = playerData.sfxVolume;

        buttonClickSfx.Play();
    }

    public void CloseSettingsPanel()
    {
        playerData.SaveData();

        settingsPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", false);
        buttonClickSfx.Play();
        StartCoroutine(DelayedPanelClose(settingsPanel));
    }

    public void OnMusicVolumeChange(float value)
    {
        playerData.musicVolume = value;

        UpdateMusicVolume();
    }

    private void UpdateMusicVolume()
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(playerData.musicVolume) * 20);
    }

    public void OnSfxVolumeChange(float value)
    {
        playerData.sfxVolume = value;

        UpdateSfxVolume();
    }

    private void UpdateSfxVolume()
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(playerData.sfxVolume) * 20);
    }

    public void OpenQuestPanel()
    {
        questPanel.gameObject.SetActive(true);
        questPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", true);

        buttonClickSfx.Play();

        SetQuestTexts();
    }

    public void CloseQuestPanel()
    {
        questPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", false);
        buttonClickSfx.Play();
        StartCoroutine(DelayedPanelClose(questPanel));
    }

    private void SetQuestTexts()
    {
        SetQuestText(playQuest, playerData.playQuestTotal, playerData.playQuestProgress,
            playQuestTitles, "Play", "Games");

        SetQuestText(coinsQuest, playerData.coinsQuestTotal, playerData.coinsQuestProgress,
            coinsQuestTitles, "Collect", "Coins");

        SetQuestText(scoreQuest, playerData.scoreQuestTotal, playerData.scoreQuestProgress,
            scoreQuestTitles, "Score", "Total");
    }

    private void SetQuestText(List<int> questArray, int total, int progress, string[] titles, string descWord1, string descWord2)
    {
        Button[] buttons = { questOneButton, questTwoButton, questThreeButton };
        TMP_Text[] titlesText = { questOneTitle, questTwoTitle, questThreeTitle };
        TMP_Text[] descriptionsText = { questOneDescription, questTwoDescription, questThreeDescription };
        TMP_Text[] noQuestTexts = { noQuestOne, noQuestTwo, noQuestThree };

        int idx = questArray.IndexOf(Mathf.Abs(total));

        if (total < 0)
        {
            noQuestTexts[idx].gameObject.SetActive(true);
            buttons[idx].gameObject.SetActive(false);
            titlesText[idx].gameObject.SetActive(false);
            descriptionsText[idx].gameObject.SetActive(false);
            return;
        }
        else
        {
            noQuestTexts[idx].gameObject.SetActive(false);
            buttons[idx].gameObject.SetActive(true);
            titlesText[idx].gameObject.SetActive(true);
            descriptionsText[idx].gameObject.SetActive(true);
        }

        titlesText[idx].text = $"{titles[idx]} ({Mathf.Min(progress, total)} / {total})";
        descriptionsText[idx].text = $"{descWord1} {total} {descWord2}.";
        buttons[idx].interactable = progress >= total;

        if (buttons[idx].interactable)
        {
            buttons[idx].onClick.RemoveAllListeners();
            buttons[idx].onClick.AddListener(() =>
            {
                int coinsReceived = 100 * (idx + 1);
                int current = playerData.coins;

                playerData.coins += coinsReceived;
                playerData.totalCoins += coinsReceived;

                StartCoroutine(AnimationManager.AnimateCoinText(coinsMainTxt, current, playerData.coins));

                StartCoroutine(AnimateCoins(buttons[idx].GetComponent<RectTransform>().position, coinsReceived));

                // change to negative to indicate quest completion
                switch (descWord2)
                {
                    case "Games":
                        playerData.playQuestProgress = 0;
                        playerData.playQuestTotal = -playerData.playQuestTotal;
                        break;
                    case "Coins":
                        playerData.coinsQuestProgress = 0;
                        playerData.coinsQuestTotal = -playerData.coinsQuestTotal;
                        break;
                    case "Total":
                        playerData.scoreQuestProgress = 0;
                        playerData.scoreQuestTotal = -playerData.scoreQuestTotal;
                        break;
                }

                noQuestTexts[idx].gameObject.SetActive(true);
                buttons[idx].gameObject.SetActive(false);
                titlesText[idx].gameObject.SetActive(false);
                descriptionsText[idx].gameObject.SetActive(false);

                buttonClickSfx.Play();

                playerData.SaveData();
            });
        }
    }

    private bool CheckForNewQuest()
    {
        string savedTime = playerData.lastQuestLoadTime;
        DateTime now = DateTime.Now;

        DateTime today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
        DateTime lastGeneratedTime = string.IsNullOrEmpty(savedTime)
            ? DateTime.MinValue
            : DateTime.Parse(savedTime);

        if (now >= today && lastGeneratedTime < today)
        {
            int[] shuffled = Shuffle(new int[] { 0, 1, 2 });

            playerData.playQuestTotal = playQuest[shuffled[0]];
            playerData.playQuestProgress = 0;
            playerData.coinsQuestTotal = coinsQuest[shuffled[1]];
            playerData.coinsQuestProgress = 0;
            playerData.scoreQuestTotal = scoreQuest[shuffled[2]];
            playerData.scoreQuestProgress = 0;

            playerData.lastQuestLoadTime = now.ToString("o");
            playerData.SaveData();
            return true;
        }

        return false;
    }

    private int[] Shuffle(int[] array)
    {
        System.Random rng = new System.Random();
        int n = array.Length;

        for (int i = n - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);

            (array[j], array[i]) = (array[i], array[j]);
        }

        return array;
    }

    public void OpenDailyPanel()
    {
        dailyPanel.gameObject.SetActive(true);
        dailyPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", true);

        buttonClickSfx.Play();

        CheckForNewDaily();
        SetDailyButtons();
    }

    public void CloseDailyPanel()
    {
        dailyPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", false);
        buttonClickSfx.Play();
        StartCoroutine(DelayedPanelClose(dailyPanel));
    }

    private bool CheckForNewDaily()
    {
        string savedTime = playerData.lastDailyLoadTime;
        DateTime now = DateTime.Now;

        DateTime today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
        DateTime lastGeneratedTime = string.IsNullOrEmpty(savedTime)
            ? DateTime.MinValue
            : DateTime.Parse(savedTime);

        if (now >= today && lastGeneratedTime < today)
        {
            playerData.dailyRewardProgress = 0;
            playerData.lastDailyLoadTime = now.ToString("o");
            playerData.SaveData();
            return true;
        }

        return false;
    }

    private void SetDailyButtons()
    {
        Button[] buttons = { dailyOneButton, dailyTwoButton, dailyThreeButton, dailyFourButton };
        Animator[] chestSprites = { chestOneSprite, chestTwoSprite, chestThreeSprite, chestFourSprite };

        for (int i = 0; i < buttons.Length; i++)
        {
            int idx = i;

            buttons[idx].onClick.RemoveAllListeners();

            if (idx == playerData.dailyRewardProgress)
            {
                chestSprites[idx].SetInteger("state", 0);

                buttons[idx].interactable = true;
                buttons[idx].GetComponentInChildren<TMP_Text>().text = idx == 0 ? "CLAIM" : "WATCH AD";
                buttons[idx].onClick.AddListener(() =>
                {
                    buttons[idx].interactable = false;
                    buttonClickSfx.Play();

                    if (idx == 0)
                    {
                        StartCoroutine(ClaimDailyReward(chestSprites[idx], idx));
                    }
                    else
                    {
                        backgroundMusic.Pause();

                        rewardedAdManager.ShowRewardedAd(() => { }, () =>
                        {
                            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            {
                                StartCoroutine(ClaimDailyReward(chestSprites[idx], idx));

                                backgroundMusic.UnPause();
                            });
                        }, () =>
                        {
                            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            {
                                OpenAdLoadFailedPanel();

                                buttons[idx].interactable = true;

                                backgroundMusic.UnPause();
                            });
                        });
                    }
                });
            }
            else if (idx < playerData.dailyRewardProgress)
            {
                chestSprites[idx].SetInteger("state", 2);

                buttons[idx].interactable = false;
                buttons[idx].GetComponentInChildren<TMP_Text>().text = "CLAIMED";
            }
            else
            {
                chestSprites[idx].SetInteger("state", 0);

                buttons[idx].interactable = false;
                buttons[idx].GetComponentInChildren<TMP_Text>().text = "LOCKED";
            }
        }
    }

    private IEnumerator ClaimDailyReward(Animator chestSprite, int idx)
    {
        chestSprite.SetInteger("state", 1);

        int coinsReceived = (idx + 1) * 30;
        int current = playerData.coins;

        playerData.coins += coinsReceived;
        playerData.totalCoins += coinsReceived;

        playerData.dailyRewardProgress++;
        playerData.SaveData();

        yield return new WaitForSecondsRealtime(0.35f);

        StartCoroutine(AnimationManager.AnimateCoinText(coinsMainTxt, current, playerData.coins));

        StartCoroutine(AnimateCoins(chestSprite.GetComponent<RectTransform>().position, coinsReceived));

        SetDailyButtons();
    }

    public void OpenAdLoadFailedPanel()
    {
        adLoadFailedPanel.gameObject.SetActive(true);
        adLoadFailedPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", true);
    }

    public void CloseAdLoadFailedPanel()
    {
        adLoadFailedPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", false);
        buttonClickSfx.Play();
        StartCoroutine(DelayedPanelClose(adLoadFailedPanel));
    }

    public void Login()
    {
        buttonClickSfx.Play();

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            if (ValidateUsername(loginUsernameTxt.text.Trim(), loginErrorTxt) && ValidatePassword(loginPasswordTxt.text.Trim(), loginErrorTxt))
            {
                LoginRequest request = new LoginRequest(loginUsernameTxt.text.Trim(), loginPasswordTxt.text.Trim());

                spinnerContainer.SetActive(true);

                client.GetAuthorizationRoutes().Login(request, response =>
                {
                    playerData.playerAccessToken = response.accessToken;
                    playerData.playerRefreshToken = response.refreshToken;
                    playerData.playerId = response.playerId;
                    playerData.playerName = loginUsernameTxt.text.Trim();

                    playerData.SaveData();

                    ClearLoginInputFields();

                    CheckLoginState();

                    loginErrorTxt.text = "Successfully logged in";
                    loginErrorTxt.color = Color.green;

                    spinnerContainer.SetActive(false);
                }, error =>
                {
                    loginErrorTxt.text = error.details.Truncate(60);
                    loginErrorTxt.color = Color.red;

                    Debug.Log(error.details);

                    spinnerContainer.SetActive(false);
                });
            }
        }
        else
        {
            loginErrorTxt.text = "No Internet Connection";
            loginErrorTxt.color = Color.red;
        }
    }

    public void Signup()
    {
        buttonClickSfx.Play();

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            bool nameValidate = ValidateUsername(signupUsernameTxt.text.Trim(), signupErrorTxt);
            bool emailValidate = ValidateEmail(signupEmailTxt.text.Trim(), signupErrorTxt);
            bool passwordValidate = ValidatePassword(signupPasswordTxt.text.Trim(), signupErrorTxt);
            bool passwordMatch = PasswordMatch(signupPasswordTxt.text.Trim(), signupConfirmPasswordTxt.text.Trim(), signupErrorTxt);

            if (nameValidate && emailValidate && passwordValidate && passwordMatch)
            {
                SignupRequest request = new SignupRequest(signupUsernameTxt.text.Trim(), signupEmailTxt.text.Trim(), signupPasswordTxt.text.Trim(), signupConfirmPasswordTxt.text.Trim());

                spinnerContainer.SetActive(true);

                client.GetAuthorizationRoutes().Signup(request, response =>
                {
                    playerData.playerAccessToken = response.accessToken;
                    playerData.playerRefreshToken = response.refreshToken;
                    playerData.playerId = response.playerId;
                    playerData.playerName = signupUsernameTxt.text.Trim();

                    playerData.SaveData();

                    ClearSignupInputFields();

                    CheckLoginState();

                    signupErrorTxt.text = "Successfully signed up";
                    signupErrorTxt.color = Color.green;

                    spinnerContainer.SetActive(false);
                }, error =>
                {
                    signupErrorTxt.text = error.details.Truncate(60);
                    signupErrorTxt.color = Color.red;

                    spinnerContainer.SetActive(false);
                });
            }
        }
        else
        {
            signupErrorTxt.text = "No Internet Connection";
            signupErrorTxt.color = Color.red;
        }
    }

    private bool ValidateUsername(string username, TMP_Text text)
    {
        if (username.Length > 20 || username.Length < 8)
        {
            text.text = "Username should be 8 to 20 characters";
            text.color = Color.red;
            return false;
        }
        if (username.Any(u => !char.IsLetterOrDigit(u)))
        {
            text.text = "Username should only contain alphanumeric characters";
            text.color = Color.red;
            return false;
        }

        return true;
    }

    private bool ValidatePassword(string password, TMP_Text text)
    {
        if (password.Length > 20 || password.Length < 8)
        {
            text.text = "Password should be 8 to 20 characters";
            text.color = Color.red;
            return false;
        }
        if (password.Any(u => !char.IsLetterOrDigit(u)))
        {
            text.text = "Password should only contain alphanumeric characters";
            text.color = Color.red;
            return false;
        }

        return true;
    }

    private bool ValidateEmail(string email, TMP_Text text)
    {
        try
        {
            MailAddress address = new MailAddress(email);

            if (email.Length > 100 || email.Length < 15)
            {
                text.text = "Email should be 15 to 100 characters";
                text.color = Color.red;
                return false;
            }
            if (address.Address != email)
            {
                text.text = "Invalid email";
                text.color = Color.red;
                return false;
            }

            return true;
        }
        catch (Exception)
        {
            text.text = "Invalid email";
            text.color = Color.red;
            return false;
        }
    }

    private bool PasswordMatch(string password, string confirmPassword, TMP_Text text)
    {
        if (password != confirmPassword)
        {
            text.text = "Passwords do not match";
            text.color = Color.red;
            return false;
        }

        return true;
    }

    public void OpenAccountPanel()
    {
        CheckLoginState();
        accountErrorTxt.text = "";
        accountPanel.gameObject.SetActive(true);
        accountPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", true);
        buttonClickSfx.Play();
    }

    public void CloseAccountPanel()
    {
        accountPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", false);
        buttonClickSfx.Play();
        StartCoroutine(DelayedPanelClose(accountPanel));
    }

    public void OpenSignupPanel()
    {
        signupPanel.gameObject.SetActive(true);
        signupErrorTxt.text = "";
        signupPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", true);
        buttonClickSfx.Play();

        ClearSignupInputFields();
    }

    public void OpenLoginPanel()
    {
        loginPanel.gameObject.SetActive(true);
        loginErrorTxt.text = "";
        loginPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", true);
        buttonClickSfx.Play();

        ClearLoginInputFields();
    }

    public void CloseSignupPanel()
    {
        signupPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", false);
        buttonClickSfx.Play();
        StartCoroutine(DelayedPanelClose(signupPanel));
    }

    public void CloseLoginPanel()
    {
        loginPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", false);
        buttonClickSfx.Play();
        StartCoroutine(DelayedPanelClose(loginPanel));
    }

    private void Logout()
    {
        CloseConfirmPanel();

        playerData.playerAccessToken = "";
        playerData.playerRefreshToken = "";
        playerData.playerId = 0;
        playerData.playerName = "";

        playerData.SaveData();

        CheckLoginState();
    }

    private void SaveDataToServer()
    {
        CloseConfirmPanel();

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            saveButton.interactable = false;
            loadButton.interactable = false;

            spinnerContainer.SetActive(true);

            if (JwtHelper.IsExpired(playerData.playerAccessToken))
            {
                RefreshToken refreshToken = new RefreshToken(playerData.playerRefreshToken);

                client.GetAuthorizationRoutes().Refresh(refreshToken, response =>
                {
                    playerData.playerAccessToken = response.accessToken;
                    playerData.playerRefreshToken = response.refreshToken;

                    playerData.SaveData();

                    Debug.Log("New access token has been issued");

                    SavePlayerData();
                }, error =>
                {
                    accountErrorTxt.text = error.details.Truncate(60);
                    accountErrorTxt.color = Color.red;

                    saveButton.interactable = true;
                    loadButton.interactable = true;

                    spinnerContainer.SetActive(false);
                });
            }
            else
            {
                SavePlayerData();
            }

            void SavePlayerData()
            {
                client.GetPlayerRoutes().SavePlayerData(playerData.playerAccessToken, response =>
                {
                    accountErrorTxt.text = response.message.Truncate(60);
                    accountErrorTxt.color = Color.green;

                    saveButton.interactable = true;
                    loadButton.interactable = true;

                    spinnerContainer.SetActive(false);
                }, error =>
                {
                    accountErrorTxt.text = error.details.Truncate(60);
                    accountErrorTxt.color = Color.red;

                    saveButton.interactable = true;
                    loadButton.interactable = true;

                    spinnerContainer.SetActive(false);
                }, progress => { });
            }
        }
        else
        {
            accountErrorTxt.text = "No Internet Connection";
            accountErrorTxt.color = Color.red;
        }
    }

    private void LoadDataFromServer()
    {
        CloseConfirmPanel();

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            saveButton.interactable = false;
            loadButton.interactable = false;

            spinnerContainer.SetActive(true);

            if (JwtHelper.IsExpired(playerData.playerAccessToken))
            {
                RefreshToken refreshToken = new RefreshToken(playerData.playerRefreshToken);

                client.GetAuthorizationRoutes().Refresh(refreshToken, response =>
                {
                    playerData.playerAccessToken = response.accessToken;
                    playerData.playerRefreshToken = response.refreshToken;

                    playerData.SaveData();

                    Debug.Log("New access token has been issued");

                    LoadPlayerData();
                }, error =>
                {
                    accountErrorTxt.text = error.details.Truncate(60);
                    accountErrorTxt.color = Color.red;

                    saveButton.interactable = true;
                    loadButton.interactable = true;

                    spinnerContainer.SetActive(false);
                });
            }
            else
            {
                LoadPlayerData();
            }

            void LoadPlayerData()
            {
                client.GetPlayerRoutes().LoadPlayerData(playerData.playerAccessToken, response =>
                {
                    playerData.SetPlayerDataFromServer(PlayerData.LoadData());

                    playerData.SaveData();

                    accountErrorTxt.text = response.message.Truncate(60);
                    accountErrorTxt.color = Color.green;

                    saveButton.interactable = true;
                    loadButton.interactable = true;

                    bestScoreMainTxt.text = $"BEST SCORE: {playerData.highscore}";
                    coinsMainTxt.text = playerData.coins.ToString();

                    UpdateMusicVolume();
                    UpdateSfxVolume();

                    spinnerContainer.SetActive(false);
                }, error =>
                {
                    accountErrorTxt.text = error.details.Truncate(60);
                    accountErrorTxt.color = Color.red;

                    saveButton.interactable = true;
                    loadButton.interactable = true;

                    spinnerContainer.SetActive(false);
                }, progress => { });
            }
        }
        else
        {
            accountErrorTxt.text = "No Internet Connection";
            accountErrorTxt.color = Color.red;
        }
    }

    public void ConfirmLogout()
    {
        confirmPanel.gameObject.SetActive(true);
        confirmPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", true);
        buttonClickSfx.Play();

        confirmPanelText.text = "Are you sure, you want to logout? Your data will not be lost.";

        confirmPanelOkButton.onClick.RemoveAllListeners();

        confirmPanelOkButton.onClick.AddListener(Logout);
    }

    public void ConfirmSaveData()
    {
        confirmPanel.gameObject.SetActive(true);
        confirmPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", true);
        buttonClickSfx.Play();

        confirmPanelText.text = "This will save your data to the server and will overwrite the previous data.";

        confirmPanelOkButton.onClick.RemoveAllListeners();

        confirmPanelOkButton.onClick.AddListener(SaveDataToServer);
    }

    public void ConfirmLoadData()
    {
        confirmPanel.gameObject.SetActive(true);
        confirmPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", true);
        buttonClickSfx.Play();

        confirmPanelText.text = "This will load your data from the server and overwrite your current progress.";

        confirmPanelOkButton.onClick.RemoveAllListeners();

        confirmPanelOkButton.onClick.AddListener(LoadDataFromServer);
    }

    public void CloseConfirmPanel()
    {
        confirmPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", false);
        buttonClickSfx.Play();
        StartCoroutine(DelayedPanelClose(confirmPanel));
    }

    private void CheckLoginState()
    {
        if (playerData.playerName.Length > 0 && playerData.playerId > 0)
        {
            accountLoginText.text = $"LOGGED IN AS {playerData.playerName}";

            signupSubmitButton.interactable = false;
            loginSubmitButton.interactable = false;
            logoutButton.interactable = true;
            signupButton.interactable = false;
            loginButton.interactable = false;
            saveButton.interactable = true;
            loadButton.interactable = true;
        }
        else
        {
            accountLoginText.text = $"NOT LOGGED IN";

            signupSubmitButton.interactable = true;
            loginSubmitButton.interactable = true;
            logoutButton.interactable = false;
            signupButton.interactable = true;
            loginButton.interactable = true;
            saveButton.interactable = false;
            loadButton.interactable = false;
        }
    }

    private void ClearSignupInputFields()
    {
        signupUsernameTxt.text = "";
        signupEmailTxt.text = "";
        signupPasswordTxt.text = "";
        signupConfirmPasswordTxt.text = "";
    }

    private void ClearLoginInputFields()
    {
        loginUsernameTxt.text = "";
        loginPasswordTxt.text = "";
    }

    private IEnumerator AnimateCoins(Vector2 position, int targetCount, float time = 1f)
    {
        targetCount /= 10;

        for (int i = 0; i < targetCount; i++)
        {
            GameObject coin = Instantiate(coinPrefab, position, Quaternion.identity, canvas.transform);
            StartCoroutine(AnimationManager.AnimateCoinIcon(coin.GetComponent<RectTransform>(), coinsIcon.GetComponent<RectTransform>()));

            yield return new WaitForSecondsRealtime(time / targetCount);
        }
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
        SceneManager.LoadScene(name);
    }
}
