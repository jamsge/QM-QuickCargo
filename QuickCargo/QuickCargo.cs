using HarmonyLib;
using MGSC;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;


namespace QuickCargo
{

    public class Plugin
    {
        public static BasePickupItem currentSelectedItem;
        public static List<ItemTab> tabs = new List<ItemTab> {null, null, null, null, null, null, null };
        public static CommonButton sortButton;
        public static ScreenWithShipCargo screen;
        public static int numDown = -1;

        [Hook(ModHookType.AfterBootstrap)]
        public static void Bootstrap(IModContext context)
        {
            // Plugin startup logic
            Debug.Log("QuickCargo is loaded! :)");
            var harmony = new Harmony("QuickCargo");
            harmony.PatchAll();
            sortButton = null;
        }

        [Hook(ModHookType.SpaceUpdateBeforeGameLoop)]
        public static void Update(IModContext context)
        {
            if (InputHelper.GetKey(KeyCode.Alpha1)) { numDown = 0; }
            else if (InputHelper.GetKey(KeyCode.Alpha2)) { numDown = 1; }
            else if (InputHelper.GetKey(KeyCode.Alpha3)) { numDown = 2; }
            else if (InputHelper.GetKey(KeyCode.Alpha4)) { numDown = 3; }
            else if (InputHelper.GetKey(KeyCode.Alpha5)) { numDown = 4; }
            else if (InputHelper.GetKey(KeyCode.Alpha6)) { numDown = 5; }
            else if (InputHelper.GetKey(KeyCode.Alpha7)) { numDown = 6; }
            else { numDown = -1; }
            MoveItem();
            if (InputHelper.GetKeyDown(KeyCode.S) && sortButton != null)
            {
                Debug.Log(screen);
                Debug.Log(sortButton);

                MethodInfo method = typeof(ScreenWithShipCargo).GetMethod("SortArsenalButtonOnOnClick", BindingFlags.NonPublic | BindingFlags.Instance);
                var parameters = new object[] { sortButton };
                method.Invoke(screen, parameters);

            }
        }

        public static void MoveItem()
        {

            if (numDown >= 0 && currentSelectedItem != null && tabs[numDown] != null && !tabs[numDown].IsSelected)
            {
                Debug.Log(currentSelectedItem.ToString());
                tabs[numDown].DropItemInTab(currentSelectedItem);
                currentSelectedItem = null;
            }
        }
    }

    [HarmonyPatch(typeof(ItemTab), "OnPointerClick")]
    public static class Patch_OnItemTabClick
    {
        public static void Postfix(ItemTab __instance)
        {
            Plugin.screen.RefreshView();
        }

    }

    [HarmonyPatch(typeof(ItemSlot), "OnPointerEnter")]
    public static class Patch_OnItemSlotEnter
    {
        public static void Prefix(ItemSlot __instance)
        {
            Plugin.currentSelectedItem = __instance.Item;
        }
    }

    [HarmonyPatch(typeof(ItemSlot), "OnPointerExit")]
    public static class Patch_OnItemSlotExit
    {
        public static void Prefix(ItemSlot __instance)
        {
            Plugin.currentSelectedItem = null;
        }
    }

    [HarmonyPatch(typeof(ScreenWithShipCargo), nameof(ScreenWithShipCargo.Show))]
    public static class Patch_OnItemTabInit
    {
        public static void Postfix(ScreenWithShipCargo __instance)
        {
            Plugin.screen = __instance;
            Debug.Log(Plugin.screen);
            ItemTab[] tabs = __instance.GetComponentsInChildren<ItemTab>();
            CommonButton[] buttons = __instance.GetComponentsInChildren<CommonButton>();
            for (int i = 0; i < tabs.Length; ++i)
            {
                Plugin.tabs[i] = tabs[i];
            }
            foreach(CommonButton b in buttons)
            {
                if (b.name == "SortArsenalButton")
                {
                    Plugin.sortButton = b;
                    break;
                }                
            }
        }
    }
}
