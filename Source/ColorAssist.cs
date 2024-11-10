using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using NineSolsAPI;
using NineSolsAPI.Utils;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements.UIR;

namespace ColorAssist {
    [BepInDependency(NineSolsAPICore.PluginGUID)]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class ColorAssist : BaseUnityPlugin {
    #if DEBUG
        private ConfigEntry<bool> enableSomethingConfig;
        private ConfigEntry<KeyboardShortcut> somethingKeyboardShortcut;
    #endif
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
        public ConfigEntry<bool> isAttackEffect;
        public ConfigEntry<int> R;
        public ConfigEntry<int> G;
        public ConfigEntry<int> B;
        public ConfigEntry<int> A;
        public ConfigEntry<int> R2;
        public ConfigEntry<int> G2;
        public ConfigEntry<int> B2;
        public ConfigEntry<int> A2;

        public Color healthBarColor;
        public Color attackEffectColor;
        public Color defaultColor;

        private const string Protanopia = "Protanopia";
        private const string Protanomaly = "Protanomaly";
        private const string Deuteranopia = "Deuteranopia";
        private const string Deuteranomaly = "Deuteranomaly";
        private const string Tritanopia = "Tritanopia";
        private const string Tritanomaly = "Tritanomaly";
        private const string Achromatopsia = "Achromatopsia";
        private const string Achromatomaly = "Achromatomaly";

        public Material[] colorBlindMaterials;
        public Material kickHint;
        public Material defaultMaterial;
        private AssetBundle shader;
        private ColorblindFilter colorblindFilter;
        private Harmony harmony;

        public static ColorAssist Instance { get; private set; }

        private void Awake() {
            Log.Init(Logger);
            RCGLifeCycle.DontDestroyForever(gameObject);
            harmony = Harmony.CreateAndPatchAll(typeof(ColorAssist).Assembly);
            Instance = this;

        #if DEBUG
            enableSomethingConfig = Config.Bind("General", "Enable", true, "Enable the feature");
            somethingKeyboardShortcut = Config.Bind("General", "Shortcut", new KeyboardShortcut(KeyCode.H, KeyCode.LeftControl), "Shortcut to execute");
        #endif

            curFilter = Config.Bind<string>("", "Current Filter", "",
                    new ConfigDescription("", null,
                    new ConfigurationManagerAttributes { Order = 20 }));

            isEnableFilter = Config.Bind<bool>("", "Enable Filter", false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 19 }));

            isProtanopia = Config.Bind<bool>("", Protanopia, false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 18 }));

            isProtanomaly = Config.Bind<bool>("", Protanomaly, false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 17 }));

            isDeuteranopia = Config.Bind<bool>("", Deuteranopia, false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 16 }));

            isDeuteranomaly = Config.Bind<bool>("", Deuteranomaly, false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 15 }));

            isTritanopia = Config.Bind<bool>("", Tritanopia, false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 14 }));

            isTritanomaly = Config.Bind<bool>("", Tritanomaly, false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 13 }));

            isAchromatopsia = Config.Bind<bool>("", Achromatopsia, false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 12 }));

            isAchromatomaly = Config.Bind<bool>("", Achromatomaly, false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 11 }));

            isHeealthBar = Config.Bind<bool>("", "Enable HeealthBar Color", false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 10 }));

            R = Config.Bind<int>("", "Red", 255,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 9 }));

            G = Config.Bind<int>("", "Green", 255,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 8 }));

            B = Config.Bind<int>("", "Blue", 0,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 7 }));

            A = Config.Bind<int>("", "Alpha", 255,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 6 }));

            isAttackEffect = Config.Bind<bool>("", "Enable AttackEffect Color", false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 5 }));

            R2 = Config.Bind<int>("", "Red2", 255,
                new ConfigDescription("", null,
                new ConfigurationManagerAttributes { Order = 4 }));

            G2 = Config.Bind<int>("", "Green2", 255,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 3 }));

            B2 = Config.Bind<int>("", "Blue2", 0,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 2 }));

            A2 = Config.Bind<int>("", "Alpha2", 255,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 1 }));

            colorBlindMaterials = LoadColorBlindMaterials();

#if DEBUG
            KeybindManager.Add(this, TestMethod, () => somethingKeyboardShortcut.Value);
#endif
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

            isAttackEffect.SettingChanged += (s, e) => UpdateAttackEffectColor();
            R2.SettingChanged += (s, e) => UpdateAttackEffectColor();
            G2.SettingChanged += (s, e) => UpdateAttackEffectColor();
            B2.SettingChanged += (s, e) => UpdateAttackEffectColor();
            A2.SettingChanged += (s, e) => UpdateAttackEffectColor();

            defaultMaterial = new Material(Shader.Find("Sprites/Default"));
            defaultColor = new Color(0.259f, 1.000f, 0.521f, 1.000f);
            UpdateHealthColor();
            UpdateAttackEffectColor();

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

#if DEBUG

        private void TestMethod() {
            if (!enableSomethingConfig.Value) return;

            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (var obj in allObjects) {
                if (obj.name == "MultiSpriteEffect_Prefab 識破提示Variant") {
                    kickHint = obj.GetComponentInChildren<SpriteRenderer>().material;
                }

                //if (obj.name.Contains("Fire_FX_damage_Long jiechuan")) {
                //    ToastManager.Toast("111");
                //    ToastManager.Toast(obj.transform.Find("Animator").Find("sprite"));
                //    obj.transform.Find("Animator").Find("sprite").GetComponent<SpriteRenderer>().material = kickHint;
                //    obj.transform.Find("Animator").Find("sprite").GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", new Color(0, 0, 1, 1));

                //    obj.transform.Find("Animator").Find("ParticleSystem").GetComponent<ParticleSystemRenderer>().material = kickHint;
                //    obj.transform.Find("Animator").Find("ParticleSystem").GetComponent<ParticleSystemRenderer>().material.SetColor("_OutlineColor", new Color(0, 0, 1, 1));
                //}
            }
            MakeOutline("Animator/View/YiGung/Weapon/Foo/FooSprite", attackEffectColor);
            MakeOutline("Animator/View/YiGung/Weapon/Sword/Effect", attackEffectColor);
            MakeOutline("Animator/View/YiGung/Attack Effect/紅白白紅/紅/AttackEffect", attackEffectColor);
            MakeOutline("Animator/View/YiGung/Attack Effect/紅白白紅/究極紅/AttackEffect", attackEffectColor);
            return;

            ToastManager.Toast("TestMethod Triggered");
            ToastManager.Toast(GameObject.Find("Attack Effect/Upper4/Sprite"));
            ToastManager.Toast(GameObject.Find("Attack Effect/紅白白紅/紅/AttackEffect"));
            ToastManager.Toast(GameObject.Find("Attack Effect/紅白白紅/究極紅/AttackEffect"));

            GameObject.Find("Animator/View/YiGung/Weapon/Foo/FooSprite").GetComponent<SpriteRenderer>().material = kickHint;
            GameObject.Find("Animator/View/YiGung/Weapon/Foo/FooSprite").GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", new Color(0, 0, 1, 1));

            //GameObject.Find("Animator/View/YiGung/Weapon/Foo/P_spark Up").GetComponent<ParticleSystemRenderer>().material = kickHint;
            //GameObject.Find("Animator/View/YiGung/Weapon/Foo/P_spark Up").GetComponent<ParticleSystemRenderer>().material.SetColor("_OutlineColor", new Color(0, 0, 1, 1));

            //GameObject.Find("Animator/View/YiGung/Weapon/Foo/P_spark Circle").GetComponent<ParticleSystemRenderer>().material = kickHint;
            //GameObject.Find("Animator/View/YiGung/Weapon/Foo/P_spark Circle").GetComponent<ParticleSystemRenderer>().material.SetColor("_OutlineColor", new Color(0, 0, 1, 1));

            GameObject.Find("Animator/View/YiGung/Weapon/Sword/Effect").GetComponent<SpriteRenderer>().material = kickHint;
            GameObject.Find("Animator/View/YiGung/Weapon/Sword/Effect").GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", new Color(0, 0, 1, 1));

            //GameObject.Find("Animator/View/YiGung/Attack Effect/Upper4/Sprite").GetComponent<SpriteRenderer>().material = kickHint;
            //GameObject.Find("Animator/View/YiGung/Attack Effect/Upper4/Sprite").GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", new Color(0, 0, 1, 1));

            GameObject.Find("Animator/View/YiGung/Attack Effect/紅白白紅/紅/AttackEffect").GetComponent<SpriteRenderer>().material = kickHint;
            GameObject.Find("Animator/View/YiGung/Attack Effect/紅白白紅/紅/AttackEffect").GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", new Color(0, 0, 1, 1));

            GameObject.Find("Animator/View/YiGung/Attack Effect/紅白白紅/究極紅/AttackEffect").GetComponent<SpriteRenderer>().material = kickHint;
            GameObject.Find("Animator/View/YiGung/Attack Effect/紅白白紅/究極紅/AttackEffect").GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", new Color(0, 0, 1, 1));
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.Keypad1)) {
                foreach (MonsterBase monsterBase in UnityEngine.Object.FindObjectsOfType<MonsterBase>()) {
                    monsterBase.ChangeStateIfValid(MonsterBase.States.Attack4);
                }
            }

            if (Input.GetKeyDown(KeyCode.Keypad2)) {
                foreach (MonsterBase monsterBase in UnityEngine.Object.FindObjectsOfType<MonsterBase>()) {
                    monsterBase.ChangeStateIfValid(MonsterBase.States.Attack8);
                }
            }

            if (Input.GetKeyDown(KeyCode.Keypad3)) {
                foreach (MonsterBase monsterBase in UnityEngine.Object.FindObjectsOfType<MonsterBase>()) {
                    monsterBase.ChangeStateIfValid(MonsterBase.States.Attack11);
                }
            }

            if (Input.GetKeyDown(KeyCode.Keypad4)) {
                foreach (MonsterBase monsterBase in UnityEngine.Object.FindObjectsOfType<MonsterBase>()) {
                    monsterBase.ChangeStateIfValid(MonsterBase.States.Attack15);
                }
            }

            if (Input.GetKeyDown(KeyCode.Keypad5)) {
                foreach (MonsterBase monsterBase in UnityEngine.Object.FindObjectsOfType<MonsterBase>()) {
                    monsterBase.ChangeStateIfValid(MonsterBase.States.Attack16);
                }
            }

            if (Input.GetKeyDown(KeyCode.Keypad6)) {
                foreach (MonsterBase monsterBase in UnityEngine.Object.FindObjectsOfType<MonsterBase>()) {
                    monsterBase.ChangeStateIfValid(MonsterBase.States.Attack19);
                }
            }
        }

#endif

        private void ChangeFilter(string filterName) {
            Material material = Array.Find(colorBlindMaterials, m => m.name == filterName);
            if (material != null) SetFilterMaterial(material);
            isEnableFilter.Value = true;
        }

        private void UpdateAttackEffectColor() {
            attackEffectColor = new Color(
                        R2.Value / 255f, // Convert R from 0-255 to 0-1
                        G2.Value / 255f, // Convert G from 0-255 to 0-1
                        B2.Value / 255f, // Convert B from 0-255 to 0-1
                        A2.Value / 255f  // Convert A from 0-255 to 0-1
                    );
            if(SceneManager.GetActiveScene().name == "A11_S0_Boss_YiGung_回蓬萊" || SceneManager.GetActiveScene().name == "A11_S0_Boss_YiGung") {
                MakeOutline("Animator/View/YiGung/Weapon/Foo/FooSprite", attackEffectColor);
                MakeOutline("Animator/View/YiGung/Weapon/Sword/Effect", attackEffectColor);
                MakeOutline("Animator/View/YiGung/Attack Effect/紅白白紅/紅/AttackEffect", attackEffectColor);
                MakeOutline("Animator/View/YiGung/Attack Effect/紅白白紅/究極紅/AttackEffect", attackEffectColor);
            }
        }

        private void UpdateHealthColor() {
            healthBarColor = new Color(
                        R.Value / 255f, // Convert R from 0-255 to 0-1
                        G.Value / 255f, // Convert G from 0-255 to 0-1
                        B.Value / 255f, // Convert B from 0-255 to 0-1
                        A.Value / 255f  // Convert A from 0-255 to 0-1
                    );
            GameObject RecoverableHealth = GameObject.Find("GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera/HideUIAbilityCheck/[Activate] PlayerUI Folder/PlayerInGameUI renderer/LeftTop/HealthBarBase/HealthBar/BG renderer/RecoverableHealth");
            if (RecoverableHealth) {
                if (isHeealthBar.Value) {
                    RecoverableHealth.GetComponent<SpriteRenderer>().color = healthBarColor;
                } else
                    RecoverableHealth.GetComponent<SpriteRenderer>().color = new Color(0.566f, 0.000f, 0.161f, 0.706f);
            }

            GameObject InternalInjury = GameObject.Find("GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera/MonsterHPRoot/BossHPRoot/UIBossHP(Clone)/Offset(DontKeyAnimationOnThisNode)/AnimationOffset/HealthBar/BG/MaxHealth/Internal Injury");  
            if (InternalInjury) {
                if (isHeealthBar.Value) {
                    InternalInjury.GetComponent<SpriteRenderer>().color = healthBarColor;
                } else
                    InternalInjury.GetComponent<SpriteRenderer>().color = new Color(0.481f, 0.000f, 0.242f, 1.000f);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {

            if (scene.name == "TitleScreenMenu") {
                AttachColorblindFilter();
                UpdateFilterUsage();
            }

            if(kickHint == null) {
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

                foreach (var obj in allObjects) {
                    if (obj.name == "MultiSpriteEffect_Prefab 識破提示Variant") {
                        Logger.LogInfo("Object found: " + obj.name);
                        Logger.LogInfo(obj.GetComponentInChildren<SpriteRenderer>().material);
                        kickHint = obj.GetComponentInChildren<SpriteRenderer>().material;
                    }
                }
            }

            if (scene.name == "A11_S0_Boss_YiGung_回蓬萊" || scene.name == "A11_S0_Boss_YiGung") {
                MakeOutline("Animator/View/YiGung/Weapon/Foo/FooSprite", attackEffectColor);
                MakeOutline("Animator/View/YiGung/Weapon/Sword/Effect", attackEffectColor);
                MakeOutline("Animator/View/YiGung/Attack Effect/紅白白紅/紅/AttackEffect", attackEffectColor);
                MakeOutline("Animator/View/YiGung/Attack Effect/紅白白紅/究極紅/AttackEffect", attackEffectColor);
            }

            UpdateHealthColor();
        }

        private void MakeOutline(string path, Color color) {
            var spriteRenderer = GameObject.Find(path)?.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) {
                Logger.LogInfo("SpriteRenderer not found at the given path.");
                return;
            }

            // Assign the outline material to the SpriteRenderer
            spriteRenderer.material = isAttackEffect.Value ? kickHint : defaultMaterial; // Set the material to the outline material

            // Check if the material has the "_OutlineColor" property before setting it
            if (spriteRenderer.material.HasProperty("_OutlineColor")) {
                spriteRenderer.material.SetColor("_OutlineColor", isAttackEffect.Value ? color : defaultColor);
                Logger.LogInfo("Outline color set successfully.");
            } else {
                Logger.LogInfo("Material does not support _OutlineColor property.");
            }
        }

        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            shader?.Unload(false);
            harmony.UnpatchSelf();
        }
    }
}
