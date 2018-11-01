using MaterialUI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections;

/**<summary> Main controller for UI </summary>*/
public class UIController : MonoBehaviour
{
    [SerializeField] private CanvasGroup defaultCanvas;
    [SerializeField] private CanvasGroup minigameCanvas;

    [SerializeField] private GameObject debugUI;
    [SerializeField] private GameObject locationIndicator;
    //[SerializeField] private VectorImageData locationFound, locationNotFound;
    [SerializeField] private Text experienceText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text creditsText;
    [SerializeField] private Text playerNameText;

    [SerializeField] private RectTransform equippedItemsLayout;
    [SerializeField] private GameObject equippedItemPrefab;

    [SerializeField] private Image levelIndicator;
    [SerializeField] private Image energyIndicator;

    private LocationController locationController;
    private WorldController worldController;
    private Player player;
    //private Button locationIcon;
    private List<EquippedItem> equippedItems = new List<EquippedItem>();
    private Sequence uiAnimationSeq;

    private void Start()
    {
        locationController = FindObjectOfType<LocationController>();
        worldController = FindObjectOfType<WorldController>();
        player = FindObjectOfType<Player>();
        //locationIcon = locationIndicator.GetComponent<Button>();

        ServerAPI.OnLocationResponse += OnLocationResponse;
        worldController.OnFinish += OnFinish;
        player.OnExpUpdated += OnExpUpdated;
        player.OnEnergyUpdated += OnEnergyUpdated;
        player.OnCreditsUpdated += OnCreditsUpdated;
        player.OnActiveItemChanged += OnActiveItemChanged;
        player.OnEquippedItemsChanged += OnEquippedItemsChanged;
        StateManager.OnSceneStateChanged += ChangeUI;

        debugUI.SetActive(App.config.debug);

        ChangeUI(StateManager.sceneState);
    }

    public void EnableEclipseRealmUI()
    {
        defaultCanvas.transform.Find("Eclipse Realm UI").gameObject.SetActive(true);
    }

    public void DisableEclipseRealmUI()
    {
        defaultCanvas.transform.Find("Eclipse Realm UI").gameObject.SetActive(false);
    }

    /**<summary> Change UI canvas between default and minigame </summary>*/
    public void ChangeUI(SceneState state) // TODO enabling and disabling Eclipse Realm UI should happen here instead
    {
        uiAnimationSeq.Kill(false);
        uiAnimationSeq = DOTween.Sequence();
        switch (state)
        {
            case SceneState.Default:
                defaultCanvas.gameObject.SetActive(true);
                uiAnimationSeq.Append(DOTween.To(() => minigameCanvas.alpha, x => minigameCanvas.alpha = x, 0, 0.25f).OnComplete(() => minigameCanvas.gameObject.SetActive(false)));
                uiAnimationSeq.Append(DOTween.To(() => defaultCanvas.alpha, x => defaultCanvas.alpha = x, 1, 0.25f));
                break;
            case SceneState.Minigame:
                minigameCanvas.gameObject.SetActive(true);
                uiAnimationSeq.Append(DOTween.To(() => defaultCanvas.alpha, x => defaultCanvas.alpha = x, 0, 0.25f).OnComplete(() => defaultCanvas.gameObject.SetActive(false)));
                uiAnimationSeq.Append(DOTween.To(() => minigameCanvas.alpha, x => minigameCanvas.alpha = x, 1, 0.25f));
                break;
        }
    }

    /**<summary> Input handler for the dropdown in main view of the app </summary>*/
    public async void MainViewDropdown (int itemID)
    {
        switch (itemID)
        {
            case 0: // Restart
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
            case 1: // Select Building
                locationController.SelectLocationDialog();
                break;
            case 2: // Dummy Location
                await ServerAPI.LocationFineDummy(locationController.buildingID, new byte[0]);
                break;
            case 3: // Test Image
                FindObjectOfType<ARControllerTest>().RandomImage();
                break;
            case 4: // Get Items
                await ServerAPI.GetItems(worldController.gameID);
                // -> WorldController.OnGetItemsResponse
                break;
            case 5: // Save Items
                worldController.SaveItems();
                // -> WorldController.OnSaveItemsResponse
                break;
            case 6: // Delete Items
                await ServerAPI.DeleteItemsDummy(locationController.buildingID, locationController.areaID);
                // -> WorldController.OnDeleteItemsResponse
                break;
            default:
                break;
                
        }
    }

    public void Help()
    {
        StartCoroutine(PauseAfterTime(0.3f)); // Let's all agree that this is a bad way of doing this
        string helptext =
            "Welcome to Eclipse Realm!\n" +
            "Try to find and collect as many coins as possible before the skeletons get you.\n" +
            "Pick up coins and items by tapping on them when you are close enough.\n" +
            "The red bar on the left is your health – when that runs out, it's game over.\n" +
            "The blue bar is your attack energy. It refills on its own.\n" +
            "Attack enemies by tapping on them.\n\n" +
            "Good luck!";


        DialogManager.ShowAlert("Help", helptext, true,
            new DialogManager.DialogButton("OK", () => {
                GameObject.FindObjectOfType<EclipseRealm>().Unpause();}));
    }

    IEnumerator PauseAfterTime(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        GameObject.FindObjectOfType<EclipseRealm>().Pause();
    }

    /**<summary> Called when response is got from image based Location API server </summary>*/
    private void OnLocationResponse(bool locationFound, LocationData locationData)
    {
        //TODO: Fix this
        //locationIcon.iconVectorImageData = locationFound ? this.locationFound : locationNotFound;
    }

    /**<summary> Called when player experience is updated </summary>*/
    private void OnExpUpdated(int experience, int nextLevel)
    {
        int levelExperience = experience - player.level * nextLevel;

        DOTween.To(() => levelIndicator.fillAmount, x => levelIndicator.fillAmount = x, levelExperience / (float)nextLevel, 0.8f).SetEase(Ease.OutSine);
    }

    /**<summary> Called when player energy is updated </summary>*/
    private void OnEnergyUpdated(int energy, int maxEnergy)
    {
        DOTween.To(() => energyIndicator.fillAmount, x => energyIndicator.fillAmount = x, energy / (float)maxEnergy, 0.8f).SetEase(Ease.OutSine);
    }

    /**<summary> Called when player credits is updated </summary>*/
    private void OnCreditsUpdated(int credits)
    {

    }

    /**<summary> Called when player Active Item is changed </summary>*/
    private void OnActiveItemChanged(ItemData activeItem, int index)
    {
        if (index >= 0 && equippedItemsLayout.childCount > 0)
            equippedItemsLayout.GetChild(index).GetComponent<Toggle>().isOn = true;
    }

    /**<summary> Called when player Equipped Items are changed </summary>*/
    private void OnEquippedItemsChanged(ItemData[] equippedItems)
    {
        if (equippedItems.Length == 0)
        {
            this.equippedItems.RemoveAt(0);
            Destroy(equippedItemsLayout.GetChild(0).gameObject);
        }
        for (int i = 0; i < equippedItems.Length; i++)
        {
            if (this.equippedItems.Count < equippedItems.Length)
                this.equippedItems.Add(Instantiate(equippedItemPrefab, equippedItemsLayout).GetComponent<EquippedItem>());
            while (this.equippedItems.Count > equippedItems.Length)
            {
                this.equippedItems.RemoveAt(this.equippedItems.Count - 1);
                Destroy(equippedItemsLayout.GetChild(equippedItemsLayout.childCount - 1).gameObject);
            }
            this.equippedItems[i].name = equippedItems[i].name;

            Toggle toggle = equippedItemsLayout.GetChild(i).GetComponentInChildren<Toggle>();
            toggle.group = equippedItemsLayout.GetComponent<ToggleGroup>();
            int index = i;
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(
                delegate
                {
                    if (toggle.isOn)
                    {
                        player.SelectItemAsActive(index);
                    }
                    else
                    {
                        player.SelectItemAsActive(-1);
                    }
                }
            );
        }
        RebuildEquippedLayout();
    }

    /**<summary> Rebuilt Layout to fix frame delay </summary>*/
    private void RebuildEquippedLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(equippedItemsLayout);
    }

    /**<summary> Called when game is finished </summary>*/
    private void OnFinish(bool win)
    {
        /*if (win)
        {
            DialogManager.ShowAlert("All power cores collected succesfully.\nPlay again?",
                () => { worldController.InitArea(); }, "YES",
                "Game Finished!", MaterialIconHelper.GetIcon(MaterialIconEnum.CHANGE_HISTORY),
                () => { }, "NO");
        }
        else
        {
            DialogManager.ShowAlert("Failed to collect all power cores.\nTry again?",
                () => { worldController.InitArea(); }, "YES",
                "Game Over!", MaterialIconHelper.GetIcon(MaterialIconEnum.CHANGE_HISTORY),
                () => { }, "NO");
        }*/
    }

    public void SetCurrentNumberOfEclipseCoins(int n)
    {
        defaultCanvas.transform.Find("Eclipse Realm UI/Player Stats/Current coins text").GetComponent<Text>().text = n.ToString();
    }

    public void SetEclipseCoinText(string s)
    {
        defaultCanvas.transform.Find("Eclipse Realm UI/Player Stats/Current coins text").GetComponent<Text>().text = s;
    }

}
