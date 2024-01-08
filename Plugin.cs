using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Linq;
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
                        var totalScrap = 0;
                        var totalValue = 0;
                        var grabbablesWithIndex = FindObjectsOfType<GrabbableObject>()
                                  .Select((grabbable, index) => new { grabbable, index });
                        foreach (var item in grabbablesWithIndex)
                        {
                            var grabbable = item.grabbable;
                            var index = item.index;
                            var itemProps = grabbable.itemProperties;
                            var isEligibleItem = grabbable is { isInShipRoom: false, isInElevator: false }
                                                     && itemProps is { minValue: > 0, maxValue: > 0, isScrap: true };

                            if (!isEligibleItem) continue;

                            var minValue = Mathf.Min(itemProps.minValue, itemProps.maxValue);
                            var maxValue = Mathf.Max(itemProps.minValue, itemProps.maxValue);

                            var randomValue = random.Next(minValue, maxValue);
                            var clampedValue = Mathf.Clamp(randomValue, grabbable.scrapValue - 6 * index, grabbable.scrapValue + 9 * index);
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