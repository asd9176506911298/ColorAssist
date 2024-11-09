using HarmonyLib;
using NineSolsAPI;
using System;
using UnityEngine;

namespace ColorAssist;

[HarmonyPatch]
public class Patches {

    // Patches are powerful. They can hook into other methods, prevent them from runnning,
    // change parameters and inject custom code.
    // Make sure to use them only when necessary and keep compatibility with other mods in mind.
    // Documentation on how to patch can be found in the harmony docs: https://harmony.pardeike.net/articles/patching.html
    [HarmonyPatch(typeof(SpriteRendererRCGExt), "Awake")]
    [HarmonyPrefix]
    private static bool hookSpriteRendererRCGExt(ref SpriteRendererRCGExt __instance) {
        //ToastManager.Toast($"{__instance.gameObject.name}:{__instance.gameObject.GetComponent<SpriteRenderer>().color}");

        if (!(__instance.gameObject.name == "Internal Injury"))
            return true;

        //ToastManager.Toast(__instance.gameObject.GetComponent<SpriteRenderer>().color);

        if (!ColorAssist.Instance.isHeealthBar.Value) return true;

        __instance.gameObject.GetComponent<SpriteRenderer>().color = ColorAssist.Instance.healthBarColor;
        return true; // the original method should be executed
    }

    [HarmonyPatch(typeof(PoolManager), "Borrow",
    new Type[] { typeof(PoolObject), typeof(Vector3), typeof(Quaternion), typeof(Transform), typeof(Action<PoolObject>) })]
    [HarmonyPostfix]
    public static void Postfix(ref PoolObject __result, PoolObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, Action<PoolObject> handler = null) {
        //ToastManager.Toast(prefab.name);

        Color color = ColorAssist.Instance.isAttackEffect.Value ? ColorAssist.Instance.attackEffectColor : ColorAssist.Instance.defaultColor;

        if (ColorAssist.Instance.kickHint == null) {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var obj in allObjects) {
                if (obj.name == "MultiSpriteEffect_Prefab 識破提示Variant") {
                    ColorAssist.Instance.kickHint = obj.GetComponentInChildren<SpriteRenderer>().material;
                }
            }
        }

        Material material = ColorAssist.Instance.isAttackEffect.Value ? ColorAssist.Instance.kickHint : ColorAssist.Instance.defaultMaterial;

        if (prefab.name == "Effect_TaiDanger") {
            SpriteRenderer Sprite = __result.transform.Find("Sprite").GetComponent<SpriteRenderer>();

            if (ColorAssist.Instance.kickHint != null) {
                Sprite.material = material;
                if (Sprite.material.HasProperty("_OutlineColor")) {
                    Sprite.material.SetColor("_OutlineColor", color);
                } else {
                    Debug.LogWarning("Material does not have '_OutlineColor' property.");
                }
            }
        }

        if (prefab.name == "MultiSpriteEffect_Prefab 識破提示Variant") {
            SpriteRenderer Sprite = __result.transform.Find("View").Find("Sprite").GetComponent<SpriteRenderer>();

            if (Sprite.material.HasProperty("_OutlineColor")) {
                Sprite.material.SetColor("_OutlineColor", color);
            } else {
                Debug.LogWarning("Material does not have '_OutlineColor' property.");
            }
        }

        if (prefab.name == "紅閃 FullScreen Slash FX Damage Danger") {
            Transform animatorTransform = __result.transform.Find("Animator");

            // Process each required child Sprite
            string[] spriteNames = { "Line", "SHape", "SHape 2" };
            foreach (string spriteName in spriteNames) {
                SpriteRenderer Sprite = animatorTransform.Find(spriteName).GetComponent<SpriteRenderer>();
                Sprite.material = material;

                if (Sprite.material.HasProperty("_OutlineColor")) {
                    Sprite.material.SetColor("_OutlineColor", color);
                } else {
                    Debug.LogWarning($"Material in {spriteName} does not have '_OutlineColor' property.");
                }
            }
        }
    }


}