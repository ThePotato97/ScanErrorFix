using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Random = System.Random;

namespace ScanFix
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is patched!");
        }
        
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.TextPostProcess))]
        internal class TextProcessPatch
        {
            private static bool Prefix(ref string modifiedDisplayText, TerminalNode node)
            {
                if (modifiedDisplayText.Contains("[scanForItems]"))
                {
                    try
                    {
                        Random random = new Random(StartOfRound.Instance.randomMapSeed + 91);
                        int totalScrap = 0;
                        int totalValue = 0;
                        foreach (var grabbable in FindObjectsOfType<GrabbableObject>())
                        {
                            var itemProps = grabbable.itemProperties;
                            var isEligibleItem = !itemProps.isScrap && grabbable is { isInShipRoom: true, isInElevator: true }
                                                     && itemProps is { minValue: 0, maxValue: 0 };
                            if (!isEligibleItem) continue;

                            int minValue = Mathf.Min(itemProps.minValue, itemProps.maxValue);
                            int maxValue = Mathf.Max(itemProps.minValue, itemProps.maxValue);

                            int randomValue = random.Next(minValue, maxValue);
                            int clampedValue = Mathf.Clamp(randomValue, grabbable.scrapValue - 6 * totalScrap, grabbable.scrapValue + 9 * totalScrap);
                            totalValue += clampedValue;
                            totalScrap++;
                        }
                        modifiedDisplayText = modifiedDisplayText.Replace("[scanForItems]", $"There are {totalScrap} objects outside the ship, totalling at an approximate value of ${totalValue}.");
                    } catch
                    {
                        modifiedDisplayText = modifiedDisplayText.Replace("[scanForItems]", "FAILED TO SCAN FOR ITEMS");
                    }
                }
                return true;
            }
        }
    }
}