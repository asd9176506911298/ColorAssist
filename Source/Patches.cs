using HarmonyLib;
using NineSolsAPI;
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

        __instance.gameObject.GetComponent<SpriteRenderer>().color = new Color(
                        ColorAssist.Instance.R.Value / 255f, // Convert R from 0-255 to 0-1
                        ColorAssist.Instance.G.Value / 255f, // Convert G from 0-255 to 0-1
                        ColorAssist.Instance.B.Value / 255f, // Convert B from 0-255 to 0-1
                        ColorAssist.Instance.A.Value / 255f  // Convert A from 0-255 to 0-1
                    );
        return true; // the original method should be executed
    }
}