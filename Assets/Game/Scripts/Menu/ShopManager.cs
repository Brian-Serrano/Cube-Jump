using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("Shop UI")]
    public ConfigHandler configHandler;
    public GameObject shopItemPrefab;
    public GameObject shopColorItemPrefab;
    public Transform confirmPanel;
    public TMP_Text confirmText;
    public Button confirmOkButton;
    public Button confirmCancelButton;
    public TMP_Text shopCoinsText;
    public Animator crossfade;

    [Header("UI Groups")]
    public List<Transform> tabsScroll;
    public List<Button> tabButtons;

    [Header("Shop Item Button Sprites")]
    public Sprite unownedButton;
    public Sprite ownedButton;

    [Header("Background")]
    public SpriteRenderer background;
    public Camera mainCamera;

    [Header("Audio Source")]
    public AudioSource buttonClickSfx;

    [Header("AudioMixer")]
    public AudioMixer audioMixer;

    private PlayerData playerData;
    private ShopTabs selectedTab;
    private IAPV5Manager iAPV5Manager;

    private Color gray = new Color(0.45f, 0.45f, 0.45f, 1f);

    private void Awake()
    {
        playerData = PlayerData.LoadData();
        selectedTab = ShopTabs.CUBE;
        iAPV5Manager = IAPV5Manager.GetInstance();

        UpdateTab();

        shopCoinsText.text = playerData.coins.ToString();

        float worldHeight = mainCamera.orthographicSize * 2f;
        float worldWidth = worldHeight * mainCamera.aspect;

        Vector2 backgroundSpriteSize = background.sprite.bounds.size;

        float backgroundScaleX = worldWidth / backgroundSpriteSize.x;
        float backgroundScaleY = worldHeight / backgroundSpriteSize.y;

        float backgroundScale = Mathf.Max(backgroundScaleX, backgroundScaleY);

        background.transform.localScale = new Vector3(backgroundScale, backgroundScale, 1f);

        for (int i = 0; i < tabButtons.Count; i++)
        {
            int index = i;
            tabButtons[index].onClick.AddListener(() =>
            {
                buttonClickSfx.Play();

                selectedTab = (ShopTabs)index;

                UpdateTab();
            });
        }

        for (int i = 0; i < 5; i++)
        {
            List<ShopItem> shopItems = GetShopItems((ShopTabs)i);

            foreach (ShopItem shopItem in shopItems)
            {
                GameObject shopItemObj = Instantiate(shopItemPrefab, tabsScroll[i].GetChild(0).GetChild(0));

                Image shopItemImg = shopItemObj.transform.GetChild(0).GetComponent<Image>();

                shopItemImg.sprite = shopItem.itemSprite;

                SetMaterialColor(shopItemImg);

                UpdateShopItem(shopItemObj.transform, shopItem, (ShopTabs)i);
            }
        }

        foreach (ShopColorItem shopColorItem in configHandler.colorShopItems)
        {
            GameObject shopColorItemObj = Instantiate(shopColorItemPrefab, tabsScroll[5].GetChild(0).GetChild(0));

            shopColorItemObj.transform.GetChild(0).GetComponent<Image>().color = shopColorItem.color;

            UpdateShopColorItem(shopColorItemObj.transform, shopColorItem);
        }

        Input.multiTouchEnabled = false;
    }

    private void Start()
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(playerData.musicVolume) * 20);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(playerData.sfxVolume) * 20);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Back();
        }
    }

    private List<ShopItem> GetShopItems(ShopTabs tab)
    {
        return tab switch
        {
            ShopTabs.CUBE => configHandler.cubeShopItems,
            ShopTabs.SHIP => configHandler.shipShopItems,
            ShopTabs.BALL => configHandler.ballShopItems,
            ShopTabs.UFO => configHandler.ufoShopItems,
            ShopTabs.WAVE => configHandler.waveShopItems,
            _ => configHandler.cubeShopItems
        };
    }

    private void UpdateShopItems(ShopTabs tab)
    {
        List<ShopItem> shopItems = GetShopItems(tab);

        foreach (ShopItem shopItem in shopItems)
        {
            Transform shopItemObj = tabsScroll[(int)tab].GetChild(0).GetChild(0).GetChild(shopItem.index);

            UpdateShopItem(shopItemObj, shopItem, tab);
        }
    }

    private void UpdateShopColorItems()
    {
        foreach (ShopColorItem shopColorItem in configHandler.colorShopItems)
        {
            Transform shopColorItemObj = tabsScroll[5].GetChild(0).GetChild(0).GetChild(shopColorItem.index);

            UpdateShopColorItem(shopColorItemObj, shopColorItem);
        }
    }

    private void UpdateShopItem(Transform shopItemObj, ShopItem shopItem, ShopTabs tab)
    {
        shopItemObj.GetChild(2).gameObject.SetActive(playerData.icons[(int)tab][shopItem.index] == '2');

        Button shopItemButton = shopItemObj.GetChild(1).GetComponent<Button>();

        switch (playerData.icons[(int)tab][shopItem.index])
        {
            case '0':
                shopItemButton.GetComponentInChildren<TMP_Text>().text = shopItem.currency == Currency.COIN ? $"{shopItem.cost} <sprite index=0>" : $"$ {shopItem.cost}";
                shopItemButton.GetComponent<Image>().sprite = unownedButton;

                if (shopItem.currency == Currency.COIN)
                {
                    if (shopItem.cost <= playerData.coins)
                    {
                        shopItemButton.interactable = true;

                        shopItemButton.onClick.RemoveAllListeners();
                        shopItemButton.onClick.AddListener(() =>
                        {
                            if (shopItem.cost <= playerData.coins)
                            {
                                OpenConfirmPanel();

                                confirmText.text = $"Are you sure you want to buy {shopItem.itemName} for {shopItem.cost} coins?";

                                confirmOkButton.onClick.RemoveAllListeners();
                                confirmOkButton.onClick.AddListener(() =>
                                {
                                    CloseConfirmPanel();

                                    int current = playerData.coins;

                                    playerData.coins -= shopItem.cost;
                                    playerData.icons[(int)tab] = playerData.icons[(int)tab].Remove(shopItem.index, 1).Insert(shopItem.index, "1");

                                    StartCoroutine(AnimationManager.AnimateCoinText(shopCoinsText, current, playerData.coins));

                                    playerData.SaveData();

                                    for (int i = 0; i < 5; i++)
                                    {
                                        UpdateShopItems((ShopTabs)i);
                                    }

                                    UpdateShopColorItems();
                                });
                            }
                        });
                    }
                    else
                    {
                        shopItemButton.interactable = false;
                    }
                }
                else
                {
                    shopItemButton.interactable = true;

                    shopItemButton.onClick.RemoveAllListeners();
                    shopItemButton.onClick.AddListener(() =>
                    {
                        OpenConfirmPanel();

                        confirmText.text = $"Are you sure you want to buy {shopItem.itemName} for {shopItem.cost} coins?";

                        confirmOkButton.onClick.RemoveAllListeners();
                        confirmOkButton.onClick.AddListener(() =>
                        {
                            buttonClickSfx.Play();

                            confirmOkButton.interactable = false;
                            confirmCancelButton.interactable = false;

                            iAPV5Manager.Buy(shopItem.id, (id) =>
                            {
                                playerData.icons[(int)tab] = playerData.icons[(int)tab].Remove(shopItem.index, 1).Insert(shopItem.index, "1");

                                playerData.SaveData();

                                confirmOkButton.interactable = true;
                                confirmCancelButton.interactable = true;

                                CloseConfirmPanel();

                                UpdateShopItem(shopItemObj, shopItem, tab);
                            }, () =>
                            {
                                confirmOkButton.interactable = true;
                                confirmCancelButton.interactable = true;
                            });
                        });
                    });
                }

                break;
            case '1':
                shopItemButton.GetComponentInChildren<TMP_Text>().text = "EQUIP";
                shopItemButton.GetComponent<Image>().sprite = ownedButton;
                shopItemButton.interactable = true;

                shopItemButton.onClick.RemoveAllListeners();
                shopItemButton.onClick.AddListener(() =>
                {
                    int previousItemIndex = playerData.icons[(int)tab].IndexOf('2');
                    if (previousItemIndex != -1)
                    {
                        playerData.icons[(int)tab] = playerData.icons[(int)tab].Remove(previousItemIndex, 1).Insert(previousItemIndex, "1");
                        UpdateShopItem(tabsScroll[(int)tab].GetChild(0).GetChild(0).GetChild(previousItemIndex), GetShopItems(tab)[previousItemIndex], tab);
                    }

                    playerData.icons[(int)tab] = playerData.icons[(int)tab].Remove(shopItem.index, 1).Insert(shopItem.index, "2");
                    buttonClickSfx.Play();
                    playerData.SaveData();

                    UpdateShopItem(shopItemObj, shopItem, tab);
                });
                break;
            case '2':
                shopItemButton.GetComponentInChildren<TMP_Text>().text = "EQUIPPED";
                shopItemButton.GetComponent<Image>().sprite = ownedButton;
                shopItemButton.interactable = false;
                break;
        }
    }

    private void UpdateShopColorItem(Transform shopColorItemObj, ShopColorItem shopColorItem)
    {
        bool isEquipped = playerData.colors[shopColorItem.index] == '2' || playerData.colors[shopColorItem.index] == '3';

        shopColorItemObj.GetChild(1).gameObject.SetActive(isEquipped);

        switch (playerData.colors[shopColorItem.index])
        {
            case '0':
                shopColorItemObj.GetChild(2).gameObject.SetActive(true);
                shopColorItemObj.GetChild(3).gameObject.SetActive(false);

                Button shopItemButton = shopColorItemObj.GetChild(2).GetChild(0).GetComponent<Button>();

                shopItemButton.GetComponentInChildren<TMP_Text>().text = shopColorItem.currency == Currency.COIN ? $"{shopColorItem.cost} <sprite index=0>" : $"$ {shopColorItem.cost}";

                if (shopColorItem.currency == Currency.COIN)
                {
                    if (shopColorItem.cost <= playerData.coins)
                    {
                        shopItemButton.interactable = true;

                        shopItemButton.onClick.RemoveAllListeners();
                        shopItemButton.onClick.AddListener(() =>
                        {
                            if (shopColorItem.cost <= playerData.coins)
                            {
                                OpenConfirmPanel();

                                confirmText.text = $"Are you sure you want to buy {shopColorItem.itemName} for {shopColorItem.cost} coins?";

                                confirmOkButton.onClick.RemoveAllListeners();
                                confirmOkButton.onClick.AddListener(() =>
                                {
                                    CloseConfirmPanel();

                                    int current = playerData.coins;

                                    playerData.coins -= shopColorItem.cost;
                                    playerData.colors = playerData.colors.Remove(shopColorItem.index, 1).Insert(shopColorItem.index, "1");

                                    StartCoroutine(AnimationManager.AnimateCoinText(shopCoinsText, current, playerData.coins));

                                    playerData.SaveData();

                                    for (int i = 0; i < 5; i++)
                                    {
                                        UpdateShopItems((ShopTabs)i);
                                    }

                                    UpdateShopColorItems();
                                });
                            }
                        });
                    }
                    else
                    {
                        shopItemButton.interactable = false;
                    }
                }
                else
                {
                    shopItemButton.interactable = true;

                    shopItemButton.onClick.RemoveAllListeners();
                    shopItemButton.onClick.AddListener(() =>
                    {
                        OpenConfirmPanel();

                        confirmText.text = $"Are you sure you want to buy {shopColorItem.itemName} for {shopColorItem.cost} coins?";

                        confirmOkButton.onClick.RemoveAllListeners();
                        confirmOkButton.onClick.AddListener(() =>
                        {
                            buttonClickSfx.Play();

                            confirmOkButton.interactable = false;
                            confirmCancelButton.interactable = false;

                            iAPV5Manager.Buy(shopColorItem.id, (id) =>
                            {
                                playerData.colors = playerData.colors.Remove(shopColorItem.index, 1).Insert(shopColorItem.index, "1");

                                playerData.SaveData();

                                confirmOkButton.interactable = true;
                                confirmCancelButton.interactable = true;

                                CloseConfirmPanel();

                                UpdateShopColorItem(shopColorItemObj, shopColorItem);
                            }, () =>
                            {
                                confirmOkButton.interactable = true;
                                confirmCancelButton.interactable = true;
                            });
                        });
                    });
                }

                break;
            case '1':
                shopColorItemObj.GetChild(2).gameObject.SetActive(false);
                shopColorItemObj.GetChild(3).gameObject.SetActive(true);

                Button buttonOne = shopColorItemObj.GetChild(3).GetChild(0).GetComponent<Button>();
                Button buttonTwo = shopColorItemObj.GetChild(3).GetChild(1).GetComponent<Button>();

                buttonOne.interactable = true;
                buttonTwo.interactable = true;

                buttonOne.onClick.RemoveAllListeners();
                buttonOne.onClick.AddListener(() =>
                {
                    EquipShopColorItem(shopColorItemObj, shopColorItem, '3', "3");
                });

                buttonTwo.onClick.RemoveAllListeners();
                buttonTwo.onClick.AddListener(() =>
                {
                    EquipShopColorItem(shopColorItemObj, shopColorItem, '2', "2");
                });
                break;
            case '2':
            case '3':
                Transform activeButtons = shopColorItemObj.GetChild(3);

                shopColorItemObj.GetChild(2).gameObject.SetActive(false);

                activeButtons.gameObject.SetActive(true);

                activeButtons.GetChild(0).GetComponent<Button>().interactable = false;
                activeButtons.GetChild(1).GetComponent<Button>().interactable = false;
                break;
        }
    }

    private void EquipShopColorItem(Transform shopColorItemObj, ShopColorItem shopColorItem, char num1, string num2)
    {
        int previousItemIndex = playerData.colors.IndexOf(num1);
        if (previousItemIndex != -1)
        {
            playerData.colors = playerData.colors.Remove(previousItemIndex, 1).Insert(previousItemIndex, "1");
            UpdateShopColorItem(tabsScroll[5].GetChild(0).GetChild(0).GetChild(previousItemIndex), configHandler.colorShopItems[previousItemIndex]);
        }

        playerData.colors = playerData.colors.Remove(shopColorItem.index, 1).Insert(shopColorItem.index, num2);
        buttonClickSfx.Play();
        playerData.SaveData();

        UpdateShopColorItem(shopColorItemObj, shopColorItem);

        SetMaterialColors();
    }

    private void SetMaterialColors()
    {
        for (int i = 0; i < 5; i++)
        {
            Transform content = tabsScroll[i].GetChild(0).GetChild(0);

            for (int j = 0; j < content.childCount; j++)
            {
                Image image = content.GetChild(j).GetChild(0).GetComponent<Image>();

                SetMaterialColor(image);
            }
        }
    }

    private void SetMaterialColor(Image image)
    {
        image.material.SetColor("_PrimaryColor", configHandler.colorShopItems[playerData.colors.IndexOf('3')].color);
        image.material.SetColor("_SecondaryColor", configHandler.colorShopItems[playerData.colors.IndexOf('2')].color);
    }

    private void UpdateTab()
    {
        for (int i = 0; i < tabButtons.Count; i++)
        {
            tabsScroll[i].gameObject.SetActive(i == (int)selectedTab);
            tabButtons[i].GetComponent<Image>().color = i == (int)selectedTab ? Color.white : gray;
        }
    }

    private void OpenConfirmPanel()
    {
        confirmPanel.gameObject.SetActive(true);
        buttonClickSfx.Play();
        confirmPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", true);
    }

    public void CloseConfirmPanel()
    {
        confirmPanel.GetChild(1).GetComponent<Animator>().SetBool("isOpen", false);
        buttonClickSfx.Play();
        StartCoroutine(DelayedPanelClose(confirmPanel));
    }

    public void Back()
    {
        buttonClickSfx.Play();
        StartCoroutine(SwitchScene("Menu"));
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
