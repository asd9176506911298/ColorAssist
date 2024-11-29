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
        public ConfigEntry<bool> isNormalHeealthBar;
        public ConfigEntry<bool> isInternalHeealthBar;
        public ConfigEntry<bool> isBossHeealthBar;
        public ConfigEntry<bool> isAttackEffect;
        public ConfigEntry<Color> _NormalHpColor;
        public ConfigEntry<Color> _InternalHpColor;
        public ConfigEntry<Color> _BossHpColor;
        public ConfigEntry<Color> _AttackEffectColor;

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

            isNormalHeealthBar = Config.Bind<bool>("", "Enable NormalHeealthBar Color", false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 10 }));

            _NormalHpColor = Config.Bind<Color>("", "Normal Health Color", new Vector4(255f, 255f, 0f, 255f),
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 9 }));

            isInternalHeealthBar = Config.Bind<bool>("", "Enable InternalHeealthBar Color", false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 8 }));

            _InternalHpColor = Config.Bind<Color>("", "Internal Health Color", new Vector4(255f,255f,0f,255f),
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 7 }));

            isBossHeealthBar = Config.Bind<bool>("", "Enable BossHeealthBar Color", false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 6 }));

            _BossHpColor = Config.Bind<Color>("", "Boss Health Color", new Vector4(255f, 255f, 0f, 255f),
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 5 }));

            isAttackEffect = Config.Bind<bool>("", "Enable AttackEffect Color", false,
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 4 }));

            _AttackEffectColor = Config.Bind<Color>("", "Attack Effect Color", new Vector4(255f, 255f, 0f, 255f),
                            new ConfigDescription("", null,
                            new ConfigurationManagerAttributes { Order = 3 }));

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

            isNormalHeealthBar.SettingChanged += (s, e) => UpdateHealthColor();
            _NormalHpColor.SettingChanged += (s, e) => UpdateHealthColor();

            isInternalHeealthBar.SettingChanged += (s, e) => UpdateHealthColor();
            _InternalHpColor.SettingChanged += (s, e) => UpdateHealthColor();

            isBossHeealthBar.SettingChanged += (s, e) => UpdateHealthColor();
            _BossHpColor.SettingChanged += (s, e) => UpdateHealthColor();

            isAttackEffect.SettingChanged += (s, e) => UpdateAttackEffectColor();
            _AttackEffectColor.SettingChanged += (s, e) => UpdateAttackEffectColor();

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

            GameObject bar = GameObject.Find("GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera/HideUIAbilityCheck/[Activate] PlayerUI Folder/PlayerInGameUI renderer/LeftTop/HealthBarBase/HealthBar");
            bar.GetComponent<Animator>().enabled = false;

            GameObject NormalHealth = GameObject.Find("GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera/HideUIAbilityCheck/[Activate] PlayerUI Folder/PlayerInGameUI renderer/LeftTop/HealthBarBase/HealthBar/BG renderer/Health");
            ToastManager.Toast(NormalHealth);
            if (NormalHealth) {
                ToastManager.Toast(NormalHealth.GetComponent<SpriteRenderer>().color);
                NormalHealth.GetComponent<SpriteRenderer>().color = _NormalHpColor.Value;
            }
            ToastManager.Toast("test");
            return;
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
            MakeOutline("Animator/View/YiGung/Weapon/Foo/FooSprite", _AttackEffectColor.Value);
            MakeOutline("Animator/View/YiGung/Weapon/Sword/Effect", _AttackEffectColor.Value);
            MakeOutline("Animator/View/YiGung/Attack Effect/紅白白紅/紅/AttackEffect", _AttackEffectColor.Value);
            MakeOutline("Animator/View/YiGung/Attack Effect/紅白白紅/究極紅/AttackEffect", _AttackEffectColor.Value);
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
            if(SceneManager.GetActiveScene().name == "A11_S0_Boss_YiGung_回蓬萊" || SceneManager.GetActiveScene().name == "A11_S0_Boss_YiGung") {
                MakeOutline("Animator/View/YiGung/Weapon/Foo/FooSprite", _AttackEffectColor.Value);
                MakeOutline("Animator/View/YiGung/Weapon/Sword/Effect", _AttackEffectColor.Value);
                MakeOutline("Animator/View/YiGung/Attack Effect/紅白白紅/紅/AttackEffect", _AttackEffectColor.Value);
                MakeOutline("Animator/View/YiGung/Attack Effect/紅白白紅/究極紅/AttackEffect", _AttackEffectColor.Value);
            }
        }

        private void UpdateHealthColor() {
            
            // Update health bar color for specific GameObjects
            UpdateBarColor(
                "GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera/HideUIAbilityCheck/[Activate] PlayerUI Folder/PlayerInGameUI renderer/LeftTop/HealthBarBase/HealthBar/BG renderer/RecoverableHealth",
                isInternalHeealthBar.Value,
                ConvertToColor(_InternalHpColor.Value),
                new Color(0.566f, 0.000f, 0.161f, 0.706f)
            );

            UpdateBarColor(
                "GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera/MonsterHPRoot/BossHPRoot/UIBossHP(Clone)/Offset(DontKeyAnimationOnThisNode)/AnimationOffset/HealthBar/BG/MaxHealth/Internal Injury",
                isInternalHeealthBar.Value,
                ConvertToColor(_InternalHpColor.Value),
                new Color(0.481f, 0.000f, 0.242f, 1.000f)
            );

      
            DisableAnimator("GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera/HideUIAbilityCheck/[Activate] PlayerUI Folder/PlayerInGameUI renderer/LeftTop/HealthBarBase/HealthBar");
            UpdateBarColor(
                "GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera/HideUIAbilityCheck/[Activate] PlayerUI Folder/PlayerInGameUI renderer/LeftTop/HealthBarBase/HealthBar/BG renderer/Health",
                isNormalHeealthBar.Value,
                ConvertToColor(_NormalHpColor.Value),
                new Color(0.321f, 1f, 0.678f, 1f)
            );



            DisableAnimator("GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera/MonsterHPRoot/BossHPRoot/UIBossHP(Clone)");
            UpdateBarColor(
                "GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera/MonsterHPRoot/BossHPRoot/UIBossHP(Clone)/Offset(DontKeyAnimationOnThisNode)/AnimationOffset/HealthBar/BG/MaxHealth/Health",
                true,
                ConvertToColor(_BossHpColor.Value),
                new Color(1f, 0.239f, 0.324f, 1f)
            );
  
        }

        // Helper Method to Update Bar Color
        private void UpdateBarColor(string objectPath, bool condition, Color trueColor, Color falseColor, string animatorPath = "") {
            GameObject barObject = GameObject.Find(objectPath);
            if (barObject) {
                SpriteRenderer renderer = barObject.GetComponent<SpriteRenderer>();
                if (renderer) {
                    renderer.color = condition ? trueColor : falseColor;
                }
            }

            if(animatorPath != "") {
                if (condition)
                    DisableAnimator(animatorPath);
                else
                    EnableAnimator(animatorPath);
            }
        }

        // Helper Method to Disable Animator
        public void DisableAnimator(string objectPath) {
            GameObject animObject = GameObject.Find(objectPath);
            if (animObject) {
                Animator animator = animObject.GetComponent<Animator>();
                if (animator) {
                    animator.enabled = false;
                }
            }
        }

        // Helper Method to Enable Animator
        private void EnableAnimator(string objectPath) {
            GameObject animObject = GameObject.Find(objectPath);
            if (animObject) {
                Animator animator = animObject.GetComponent<Animator>();
                if (animator) {
                    animator.enabled = true;
                }
            }
        }

        // Helper Method to Convert ConfigEntry<Color> to UnityEngine.Color
        private Color ConvertToColor(Color colorConfig) {
            return new Color(colorConfig.r, colorConfig.g, colorConfig.b, colorConfig.a);
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
                MakeOutline("Animator/View/YiGung/Weapon/Foo/FooSprite", _AttackEffectColor.Value);
                MakeOutline("Animator/View/YiGung/Weapon/Sword/Effect", _AttackEffectColor.Value);
                MakeOutline("Animator/View/YiGung/Attack Effect/紅白白紅/紅/AttackEffect", _AttackEffectColor.Value);
                MakeOutline("Animator/View/YiGung/Attack Effect/紅白白紅/究極紅/AttackEffect", _AttackEffectColor.Value);
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
