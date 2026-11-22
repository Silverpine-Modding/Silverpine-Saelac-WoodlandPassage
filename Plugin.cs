using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Silverpine.ModdingTools;

namespace WoodlandPassage;

[BepInPlugin(Id, Title, "1.0.0")]
[BepInDependency("Saelac.Silverpine.ModdingTools", "1.10.0")]
public sealed class Plugin : BaseUnityPlugin
{
    internal const string Id = "silverpine.woodlandpassage";
    internal const string Title = "Woodland Passage";

    internal static ConfigEntry<bool> SmallTrees, GroundWood, SmallRocks, MediumRocks;

    private void Awake()
    {
        SmallTrees = Config.Bind("Passage", "SmallTrees", true,
            "Walk through oak, pine, swamp, willow and glowing mushroom trees.");
        GroundWood = Config.Bind("Passage", "SticksAndFelledTrees", true,
            "Walk through or fly over ground sticks and felled trees.");
        SmallRocks = Config.Bind("Passage", "SmallRocks", true,
            "Walk through or fly over small rocks.");
        MediumRocks = Config.Bind("Passage", "MediumRocks", false,
            "Walk through or fly over medium rocks.");

        SmallTrees.SettingChanged += TreeFooting.OnSettingChanged;
        new Harmony(Id).PatchAll(typeof(Plugin).Assembly);
        InventoryModTools.RegisterSession(Id + ".settings", Title, PassageSettings.Open);
    }
}
