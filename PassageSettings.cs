using BepInEx.Configuration;
using Silverpine.ModdingTools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WoodlandPassage;

internal sealed class PassageSettings : ModToolBehaviour
{
    internal static void Open(InventoryUI inventory, ModToolSession session)
    {
        var template = ModUi.GetInventoryButtonTemplate(inventory);
        var overlay = ModUi.CreateOverlay(inventory, Plugin.Title, new Vector2(720f, 470f));
        var window = overlay.Root.AddComponent<PassageSettings>();
        window.AttachSession(session);

        var layout = overlay.Panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(32, 32, 22, 22);
        layout.spacing = 8f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        var panel = overlay.Panel.transform;
        ModUi.CloneTitle(template, panel, Plugin.Title);
        var note = ModUi.CloneTitle(template, panel, "Walk through trees. Walk or fly across wood and rocks.", 42f);
        note.fontSize = 22f;
        AddToggle(template, panel, "Small standing trees", Plugin.SmallTrees);
        AddToggle(template, panel, "Ground sticks and felled trees", Plugin.GroundWood);
        AddToggle(template, panel, "Small rocks", Plugin.SmallRocks);
        AddToggle(template, panel, "Medium rocks", Plugin.MediumRocks);
        ModUi.CloneButton(template, panel, "Close", window.Close);
    }

    private static void AddToggle(Button template, Transform panel, string label, ConfigEntry<bool> setting)
    {
        string Caption() => label + (setting.Value ? "  [On]" : "  [Off]");
        var button = ModUi.CloneButton(template, panel, Caption(), () => setting.Value = !setting.Value);
        var text = button.GetComponentInChildren<TextMeshProUGUI>(true);
        button.onClick.AddListener(() => text.text = Caption());
    }

    private void Close()
    {
        ReleaseSession();
        Destroy(gameObject);
    }
}
