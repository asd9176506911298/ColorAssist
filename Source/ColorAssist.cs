using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using NineSolsAPI;
using NineSolsAPI.Utils;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorAssist {
    [BepInDependency(NineSolsAPICore.PluginGUID)]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class ColorAssist : BaseUnityPlugin {
        private ConfigEntry<bool> enableSomethingConfig;
        private ConfigEntry<KeyboardShortcut> somethingKeyboardShortcut;

        public ConfigEntry<string> curFilter;
        private ConfigEntry<bool> isEnableFilter;
        private ConfigEntry<bool> isProtanopia;
        private ConfigEntry<bool> isProtanomaly;
        private ConfigEntry<bool> isDeuteranopia;
        private ConfigEntry<bool> isDeuteranomaly;
        private ConfigEntry<bool> isTritanopia;
        private ConfigEntry<bool> isTritanomaly;
        private ConfigEntry<bool> isAchromatopsia;
        private ConfigEntry<bool> isAchromatomaly;
        public ConfigEntry<bool> isHeealthBar;
        public ConfigEntry<int> R;
        public ConfigEntry<int> G;
        public ConfigEntry<int> B;
        public ConfigEntry<int> A;

        private const string Protanopia = "Protanopia";
        private const string Protanomaly = "Protanomaly";
        private const string Deuteranopia = "Deuteranopia";
        private const string Deuteranomaly = "Deuteranomaly";
        private const string Tritanopia = "Tritanopia";
        private const string Tritanomaly = "Tritanomaly";
        private const string Achromatopsia = "Achromatopsia";
        private const string Achromatomaly = "Achromatomaly";

        public Material[] colorBlindMaterials;
        private AssetBundle shader;
        private ColorblindFilter colorblindFilter;
        private Harmony harmony;

        public static ColorAssist Instance { get; private set; }

        private void Awake() {
            Log.Init(Logger);
            RCGLifeCycle.DontDestroyForever(gameObject);
            harmony = Harmony.CreateAndPatchAll(typeof(ColorAssist).Assembly);
            Instance = this;

            //enableSomethingConfig = Config.Bind("General", "Enable", true, "Enable the feature");
            //somethingKeyboardShortcut = Config.Bind("General", "Shortcut", new KeyboardShortcut(KeyCode.H, KeyCode.LeftControl), "Shortcut to execute");

            curFilter = Config.Bind<string>("", "Current Filter", "",
                    new ConfigDescription("", null,
                    new ConfigurationManagerAttributes { Order = 14 }));

            isEnableFilter = Config.Bind<bool>("", "Enable Filter", false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 13 }));

            isProtanopia = Config.Bind<bool>("", Protanopia, false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 12 }));

            isProtanomaly = Config.Bind<bool>("", Protanomaly, false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 11 }));

            isDeuteranopia = Config.Bind<bool>("", Deuteranopia, false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 10 }));

            isDeuteranomaly = Config.Bind<bool>("", Deuteranomaly, false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 9 }));

            isTritanopia = Config.Bind<bool>("", Tritanopia, false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 8 }));

            isTritanomaly = Config.Bind<bool>("", Tritanomaly, false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 7 }));

            isAchromatopsia = Config.Bind<bool>("", Achromatopsia, false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 6 }));

            isAchromatomaly = Config.Bind<bool>("", Achromatomaly, false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 5 }));

            isHeealthBar = Config.Bind<bool>("", "Enable HeealthBar Color", false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 5 }));

            R = Config.Bind<int>("", "Red", 255,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 4 }));

            G = Config.Bind<int>("", "Green", 255,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 3 }));

            B = Config.Bind<int>("", "Blue", 0,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 2 }));

            A = Config.Bind<int>("", "Alpha", 255,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 1 }));

            colorBlindMaterials = LoadColorBlindMaterials();

            //KeybindManager.Add(this, TestMethod, () => somethingKeyboardShortcut.Value);

            isEnableFilter.SettingChanged += (_, _) => UpdateFilterUsage();

            // Registering filter setting changes
            isProtanopia.SettingChanged += (s, e) => ChangeFilter(Protanopia);
            isProtanomaly.SettingChanged += (s, e) => ChangeFilter(Protanomaly);
            isDeuteranopia.SettingChanged += (s, e) => ChangeFilter(Deuteranopia);
            isDeuteranomaly.SettingChanged += (s, e) => ChangeFilter(Deuteranomaly);
            isTritanopia.SettingChanged += (s, e) => ChangeFilter(Tritanopia);
            isTritanomaly.SettingChanged += (s, e) => ChangeFilter(Tritanomaly);
            isAchromatopsia.SettingChanged += (s, e) => ChangeFilter(Achromatopsia);
            isAchromatomaly.SettingChanged += (s, e) => ChangeFilter(Achromatomaly);

            isHeealthBar.SettingChanged += (s, e) => UpdateHealthColor();
            R.SettingChanged += (s, e) => UpdateHealthColor();
            G.SettingChanged += (s, e) => UpdateHealthColor();
            B.SettingChanged += (s, e) => UpdateHealthColor();
            A.SettingChanged += (s, e) => UpdateHealthColor();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void Start() {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private Material[] LoadColorBlindMaterials() {
            shader = AssemblyUtils.GetEmbeddedAssetBundle("ColorAssist.Resources.shader");
            return new Material[] {
                shader.LoadAsset<Material>(Protanopia),
                shader.LoadAsset<Material>(Protanomaly),
                shader.LoadAsset<Material>(Deuteranopia),
                shader.LoadAsset<Material>(Deuteranomaly),
                shader.LoadAsset<Material>(Tritanopia),
                shader.LoadAsset<Material>(Tritanomaly),
                shader.LoadAsset<Material>(Achromatopsia),
                shader.LoadAsset<Material>(Achromatomaly)
            };
        }

        private void UpdateFilterUsage() {
            if (colorblindFilter == null) AttachColorblindFilter();
            colorblindFilter?.SetUseFilter(isEnableFilter.Value);
        }

        private void AttachColorblindFilter() {
            foreach (Camera camera in Camera.allCameras) {
                if (camera.gameObject.name == "ApplicationUICam") {
                    colorblindFilter = camera.gameObject.GetComponent<ColorblindFilter>()
                                       ?? camera.gameObject.AddComponent<ColorblindFilter>();
                    return;
                }
            }
        }

        private void SetFilterMaterial(Material filterMaterial) {
            AttachColorblindFilter();
            if (colorblindFilter != null) {
                colorblindFilter.setMaterial(filterMaterial);
                curFilter.Value = filterMaterial.name;
            }
        }

        private void TestMethod() {
            if (!enableSomethingConfig.Value) return;
            ToastManager.Toast("TestMethod Triggered");
            AttachColorblindFilter();
        }

        private void ChangeFilter(string filterName) {
            Material material = Array.Find(colorBlindMaterials, m => m.name == filterName);
            if (material != null) SetFilterMaterial(material);
        }

        private void UpdateHealthColor() {
            GameObject RecoverableHealth = GameObject.Find("GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera/HideUIAbilityCheck/[Activate] PlayerUI Folder/PlayerInGameUI renderer/LeftTop/HealthBarBase/HealthBar/BG renderer/RecoverableHealth");
            if (RecoverableHealth) {    
                if (isHeealthBar.Value) {
                    RecoverableHealth.GetComponent<SpriteRenderer>().color = new Color(
                        R.Value / 255f, // Convert R from 0-255 to 0-1
                        G.Value / 255f, // Convert G from 0-255 to 0-1
                        B.Value / 255f, // Convert B from 0-255 to 0-1
                        A.Value / 255f  // Convert A from 0-255 to 0-1
                    );
                } else
                    RecoverableHealth.GetComponent<SpriteRenderer>().color = new Color(0.566f, 0.000f, 0.161f, 0.706f);


            }

            GameObject InternalInjury = GameObject.Find("GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera/MonsterHPRoot/BossHPRoot/UIBossHP(Clone)/Offset(DontKeyAnimationOnThisNode)/AnimationOffset/HealthBar/BG/MaxHealth/Internal Injury");  
            if (InternalInjury) {
                if (isHeealthBar.Value) {
                    InternalInjury.GetComponent<SpriteRenderer>().color = new Color(
                        R.Value / 255f, // Convert R from 0-255 to 0-1
                        G.Value / 255f, // Convert G from 0-255 to 0-1
                        B.Value / 255f, // Convert B from 0-255 to 0-1
                        A.Value / 255f  // Convert A from 0-255 to 0-1
                    );
                } else
                    InternalInjury.GetComponent<SpriteRenderer>().color = new Color(0.481f, 0.000f, 0.242f, 1.000f);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {

            if (scene.name == "TitleScreenMenu") {
                AttachColorblindFilter();
                UpdateFilterUsage();
            }

            UpdateHealthColor();
        }

        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            shader?.Unload(false);
            harmony.UnpatchSelf();
        }
    }
}
