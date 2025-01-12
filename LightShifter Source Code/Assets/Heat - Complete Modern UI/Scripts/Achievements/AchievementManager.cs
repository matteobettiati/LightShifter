using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.Heat
{
    [DisallowMultipleComponent]
    public class AchievementManager : MonoBehaviour
    {
        // Resources
        public UIManager UIManagerAsset;
        [SerializeField] private Transform allParent;
        [SerializeField] private Transform commonParent;
        [SerializeField] private Transform rareParent;
        [SerializeField] private Transform legendaryParent;
        [SerializeField] private GameObject achievementPreset;
        [SerializeField] private TextMeshProUGUI totalUnlockedObj;
        [SerializeField] private TextMeshProUGUI totalValueObj;
        [SerializeField] private TextMeshProUGUI commonUnlockedObj;
        [SerializeField] private TextMeshProUGUI commonlTotalObj;
        [SerializeField] private TextMeshProUGUI rareUnlockedObj;
        [SerializeField] private TextMeshProUGUI rareTotalObj;
        [SerializeField] private TextMeshProUGUI legendaryUnlockedObj;
        [SerializeField] private TextMeshProUGUI legendaryTotalObj;

        // Settings
        public bool useLocalization = true;
        [SerializeField] private bool useAlphabeticalOrder = true;
        private int commonCount;
        private int commonUnlockedCount;
        private int legendaryCount;
        private int legendaryUnlockedCount;

        // Helpers
        private LocalizedObject localizedObject;
        private int rareCount;
        private int rareUnlockedCount;

        // Values
        private int totalCount;
        private int totalUnlockedCount;

        private void Awake()
        {
            InitializeItems();
        }

        private static int SortByName(AchievementLibrary.AchievementItem o1, AchievementLibrary.AchievementItem o2)
        {
            // Compare the names and sort by A to Z
            return o1.title.CompareTo(o2.title);
        }

        public void InitializeItems()
        {
            // Check for core resources
            if (UIManagerAsset == null || UIManagerAsset.achievementLibrary == null || achievementPreset == null)
                return;

            // Sort achievements by alphabetical order if enabled
            if (useAlphabeticalOrder)
                UIManagerAsset.achievementLibrary.achievements.Sort(SortByName);

            // Check for localization
            if (useLocalization)
            {
                localizedObject = gameObject.GetComponent<LocalizedObject>();
                if (localizedObject == null || localizedObject.CheckLocalizationStatus() == false)
                    useLocalization = false;
            }

            // Clear parent transforms and start the loop
            foreach (Transform child in allParent) Destroy(child.gameObject);
            foreach (Transform child in commonParent) Destroy(child.gameObject);
            foreach (Transform child in rareParent) Destroy(child.gameObject);
            foreach (Transform child in legendaryParent) Destroy(child.gameObject);
            for (var i = 0; i < UIManagerAsset.achievementLibrary.achievements.Count; i++)
            {
                // Temp variables
                Transform parent = null;
                AchievementLibrary.AchievementType type = default;
                totalCount++;

                // Set achievement type and parent
                if (UIManagerAsset.achievementLibrary.achievements[i].type == AchievementLibrary.AchievementType.Common)
                {
                    parent = commonParent;
                    type = AchievementLibrary.AchievementType.Common;
                    commonCount++;
                }
                else if (UIManagerAsset.achievementLibrary.achievements[i].type ==
                         AchievementLibrary.AchievementType.Rare)
                {
                    parent = rareParent;
                    type = AchievementLibrary.AchievementType.Rare;
                    rareCount++;
                }
                else if (UIManagerAsset.achievementLibrary.achievements[i].type ==
                         AchievementLibrary.AchievementType.Legendary)
                {
                    parent = legendaryParent;
                    type = AchievementLibrary.AchievementType.Legendary;
                    legendaryCount++;
                }

                // Create the base object
                var go = Instantiate(achievementPreset, new Vector3(0, 0, 0), Quaternion.identity);
                go.transform.SetParent(parent, false);

                // Get the ach component
                var tempAI = go.GetComponent<AchievementItem>();
                tempAI.lockedIndicator.SetActive(true);
                tempAI.unlockedIndicator.SetActive(false);

                // Check for the state
                if ((UIManagerAsset.achievementLibrary.achievements[i].isHidden
                     && PlayerPrefs.GetString("ACH_" + UIManagerAsset.achievementLibrary.achievements[i].title) ==
                     "true")
                    || UIManagerAsset.achievementLibrary.achievements[i].isHidden == false
                    || UIManagerAsset.achievementLibrary.achievements[i].dataBehaviour ==
                    AchievementLibrary.DataBehaviour.Unlocked)
                {
                    tempAI.iconObj.sprite = UIManagerAsset.achievementLibrary.achievements[i].icon;

                    // Check for localization
                    if (useLocalization == false)
                    {
                        tempAI.titleObj.text = UIManagerAsset.achievementLibrary.achievements[i].title;
                        tempAI.descriptionObj.text = UIManagerAsset.achievementLibrary.achievements[i].description;
                    }

                    else
                    {
                        var titleLoc = tempAI.titleObj.GetComponent<LocalizedObject>();
                        if (titleLoc != null)
                        {
                            titleLoc.tableIndex = localizedObject.tableIndex;
                            titleLoc.localizationKey = UIManagerAsset.achievementLibrary.achievements[i].titleKey;
                        }

                        var descLoc = tempAI.descriptionObj.GetComponent<LocalizedObject>();
                        if (descLoc != null)
                        {
                            descLoc.tableIndex = localizedObject.tableIndex;
                            descLoc.localizationKey = UIManagerAsset.achievementLibrary.achievements[i].decriptionKey;
                        }
                    }
                }

                // Set hidden if it's not unlocked and marked as hidden
                else
                {
                    tempAI.iconObj.sprite = UIManagerAsset.achievementLibrary.achievements[i].hiddenIcon;

                    // Check for localization
                    if (useLocalization == false)
                    {
                        tempAI.titleObj.text = UIManagerAsset.achievementLibrary.achievements[i].hiddenTitle;
                        tempAI.descriptionObj.text =
                            UIManagerAsset.achievementLibrary.achievements[i].hiddenDescription;
                    }

                    else
                    {
                        var titleLoc = tempAI.titleObj.GetComponent<LocalizedObject>();
                        if (titleLoc != null)
                        {
                            titleLoc.tableIndex = localizedObject.tableIndex;
                            titleLoc.localizationKey = UIManagerAsset.achievementLibrary.achievements[i].hiddenTitleKey;
                        }

                        var descLoc = tempAI.descriptionObj.GetComponent<LocalizedObject>();
                        if (descLoc != null)
                        {
                            descLoc.tableIndex = localizedObject.tableIndex;
                            descLoc.localizationKey = UIManagerAsset.achievementLibrary.achievements[i].hiddenDescKey;
                        }
                    }
                }

                // Change variable colors depending on the ach type
                for (var x = 0; x < tempAI.images.Count; x++)
                    if (type == AchievementLibrary.AchievementType.Common)
                        tempAI.images[x].color = new Color(UIManagerAsset.commonColor.r, UIManagerAsset.commonColor.g,
                            UIManagerAsset.commonColor.b, tempAI.images[x].color.a);
                    else if (type == AchievementLibrary.AchievementType.Rare)
                        tempAI.images[x].color = new Color(UIManagerAsset.rareColor.r, UIManagerAsset.rareColor.g,
                            UIManagerAsset.rareColor.b, tempAI.images[x].color.a);
                    else if (type == AchievementLibrary.AchievementType.Legendary)
                        tempAI.images[x].color = new Color(UIManagerAsset.legendaryColor.r,
                            UIManagerAsset.legendaryColor.g, UIManagerAsset.legendaryColor.b, tempAI.images[x].color.a);

                if ((!PlayerPrefs.HasKey("ACH_" + UIManagerAsset.achievementLibrary.achievements[i].title)
                     && UIManagerAsset.achievementLibrary.achievements[i].dataBehaviour ==
                     AchievementLibrary.DataBehaviour.Unlocked)
                    || PlayerPrefs.GetString("ACH_" + UIManagerAsset.achievementLibrary.achievements[i].title) ==
                    "true")
                {
                    if (tempAI.lockedIndicator != null) tempAI.lockedIndicator.SetActive(false);
                    if (tempAI.unlockedIndicator != null) tempAI.unlockedIndicator.SetActive(true);

                    if (type == AchievementLibrary.AchievementType.Common)
                        commonUnlockedCount++;
                    else if (type == AchievementLibrary.AchievementType.Rare)
                        rareUnlockedCount++;
                    else if (type == AchievementLibrary.AchievementType.Legendary) legendaryUnlockedCount++;

                    totalUnlockedCount++;
                }

                // Duplicate the object to the all parent
                var allGo = Instantiate(go, new Vector3(0, 0, 0), Quaternion.identity);
                allGo.transform.SetParent(allParent, false);
            }

            // Parse text data
            ParseTotalText();
        }

        private void ParseTotalText()
        {
            // Parsing data to text and changing colors
            if (totalValueObj != null) totalValueObj.text = totalCount.ToString();
            if (totalUnlockedObj != null) totalUnlockedObj.text = totalUnlockedCount.ToString();
            if (commonUnlockedObj != null) commonUnlockedObj.text = commonUnlockedCount.ToString();
            if (rareUnlockedObj != null) rareUnlockedObj.text = rareUnlockedCount.ToString();
            if (legendaryUnlockedObj != null) legendaryUnlockedObj.text = legendaryUnlockedCount.ToString();

            if (commonlTotalObj != null)
            {
                commonlTotalObj.text = commonCount.ToString();
                foreach (Transform obj in commonlTotalObj.transform.parent)
                {
                    var tempTMP = obj.GetComponent<TextMeshProUGUI>();
                    if (tempTMP != null)
                    {
                        tempTMP.color = new Color(UIManagerAsset.commonColor.r, UIManagerAsset.commonColor.g,
                            UIManagerAsset.commonColor.b, tempTMP.color.a);
                        var glowIMG = tempTMP.GetComponentInChildren<Image>();
                        if (glowIMG != null)
                            glowIMG.color = new Color(UIManagerAsset.commonColor.r, UIManagerAsset.commonColor.g,
                                UIManagerAsset.commonColor.b, glowIMG.color.a);
                        continue;
                    }

                    var tempIMG = obj.GetComponent<Image>();
                    if (tempIMG != null)
                        tempIMG.color = new Color(UIManagerAsset.commonColor.r, UIManagerAsset.commonColor.g,
                            UIManagerAsset.commonColor.b, tempIMG.color.a);
                }
            }

            if (rareTotalObj != null)
            {
                rareTotalObj.text = rareCount.ToString();
                foreach (Transform obj in rareTotalObj.transform.parent)
                {
                    var tempTMP = obj.GetComponent<TextMeshProUGUI>();
                    if (tempTMP != null)
                    {
                        tempTMP.color = new Color(UIManagerAsset.rareColor.r, UIManagerAsset.rareColor.g,
                            UIManagerAsset.rareColor.b, tempTMP.color.a);
                        var glowIMG = tempTMP.GetComponentInChildren<Image>();
                        if (glowIMG != null)
                            glowIMG.color = new Color(UIManagerAsset.rareColor.r, UIManagerAsset.rareColor.g,
                                UIManagerAsset.rareColor.b, glowIMG.color.a);
                        continue;
                    }

                    var tempIMG = obj.GetComponent<Image>();
                    if (tempIMG != null)
                        tempIMG.color = new Color(UIManagerAsset.rareColor.r, UIManagerAsset.rareColor.g,
                            UIManagerAsset.rareColor.b, tempIMG.color.a);
                }
            }

            if (legendaryTotalObj != null)
            {
                legendaryTotalObj.text = legendaryCount.ToString();
                foreach (Transform obj in legendaryTotalObj.transform.parent)
                {
                    var tempTMP = obj.GetComponent<TextMeshProUGUI>();
                    if (tempTMP != null)
                    {
                        tempTMP.color = new Color(UIManagerAsset.legendaryColor.r, UIManagerAsset.legendaryColor.g,
                            UIManagerAsset.legendaryColor.b, tempTMP.color.a);
                        var glowIMG = tempTMP.GetComponentInChildren<Image>();
                        if (glowIMG != null)
                            glowIMG.color = new Color(UIManagerAsset.legendaryColor.r, UIManagerAsset.legendaryColor.g,
                                UIManagerAsset.legendaryColor.b, glowIMG.color.a);
                        continue;
                    }

                    var tempIMG = obj.GetComponent<Image>();
                    if (tempIMG != null)
                        tempIMG.color = new Color(UIManagerAsset.legendaryColor.r, UIManagerAsset.legendaryColor.g,
                            UIManagerAsset.legendaryColor.b, tempIMG.color.a);
                }
            }
        }

        public static void SetAchievement(string title, bool value)
        {
            if (value)
                PlayerPrefs.SetString("ACH_" + title, "true");
            else
                PlayerPrefs.SetString("ACH_" + title, "false");
        }
    }
}