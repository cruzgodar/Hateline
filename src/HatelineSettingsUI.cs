using Monocle;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Celeste;
using System.Linq;

namespace Celeste.Mod.Hateline;

public static class HatelineSettingsUI
{
    public static void CreateMenu(TextMenu menu, bool inGame)
    {
        HatelineModule.RegisterHats();
        HatSelector(menu, inGame);
    }
    
    public static void HatSelector(TextMenu menu, bool inGame)
    {
        int index = 0;
        string SelectedHat = HatelineModule.Settings.SelectedHat;
        TextMenuExt.OptionSubMenu hatSelectionMenu = new(Dialog.Clean("HATELINE_HAT_HATSFROM")) { ItemIndent = 25f };
        bool doSessionHint = Engine.Scene is Level && HatelineModule.Instance.HasForcedHat;
        TextMenuExt.EaseInSubHeaderExt conf = new(Dialog.Clean("HATELINE_HAT_CONFIRM"), false, menu)
        {
            TextColor = Color.Goldenrod,
            HeightExtra = 0f,
            IncludeWidthInMeasurement = false,
        };
        TextMenuExt.EaseInSubHeaderExt conf_req = new(Dialog.Clean("HATELINE_HAT_NEEDCONFIRM"), false, menu)
        {
            TextColor = Color.OrangeRed,
            HeightExtra = 0f,
            IncludeWidthInMeasurement = false,
        };
        TextMenuExt.EaseInSubHeaderExt sessionHint = new(Dialog.Clean("HATELINE_HAT_SESSIONHINT"), doSessionHint, menu)
        {
            TextColor = Color.SteelBlue,
            HeightExtra = 0f,
            IncludeWidthInMeasurement = false,
        };
        TextMenuExt.EaseInSubHeaderExt notEnabledHint = new(Dialog.Clean("HATELINE_HAT_NOTENABLEDHINT"), false, menu)
        {
            TextColor = Color.DarkGray,
            HeightExtra = 0f,
            IncludeWidthInMeasurement = false,
        };

        hatSelectionMenu.OnValueChange += (index2) => {
            // everest will call the OnEnter of first-option of currentmenu before entering there... i hate it.
            foreach (var item in hatSelectionMenu.CurrentMenu)
                if (item is TextMenuExt.EaseInSubHeaderExt item2 && item2.TextColor == Color.Gray)
                    item2.FadeVisible = false;
            conf.FadeVisible = false;
            conf_req.FadeVisible = index != index2;
        };
        hatSelectionMenu.OnPressed += () => {
            index = hatSelectionMenu.MenuIndex;
            var _option = hatSelectionMenu.CurrentMenu[0] as TextMenu.Option<string>;
            string FirstHat = _option.Values.Count > 0 ? _option.Values[_option.Index].Item2 : HatelineModule.HAT_NONE;
            if (SelectedHat != FirstHat)
            {
                HatelineModule.Settings.SelectedHat = SelectedHat = FirstHat;
                HatelineModule.ReloadHat();
                conf.FadeVisible = true;
            }
            _option.OnEnter?.Invoke();
            conf_req.FadeVisible = false;
            if (!HatelineModule.Settings.Enabled)
                notEnabledHint.FadeVisible = true;
        };

        Dictionary<string, List<string>> GroupedHats = new(){
            { "Hateline",  new()}
        };
        foreach ((string hat, var attr) in HatelineModule.Instance.HatAttributes)
        {
            string mod = attr["mod"];
            if (!GroupedHats.ContainsKey(attr["mod"]))
                GroupedHats.Add(mod, new());
            GroupedHats[mod].Add(hat);
        }
        foreach ((string mod, var hats) in GroupedHats)
        {
            List<TextMenu.Item> options = new();
            TextMenu.Option<string> option = new(Dialog.Clean("HATELINE_SETTINGS_CURHAT"));
            options.Add(option);

            foreach (string hat in hats)
            {
                if (hat == SelectedHat)
                {
                    hatSelectionMenu.SetInitialSelection(index);
                }
                string name = Dialog.Clean("hateline_hat_" + hat);
                name = (name == "") ? hat : name;
                option.Add(name, hat, hat == SelectedHat);
            }
            option.UnselectedColor = Color.DarkGray;
            option.Change(Hat => {
                HatelineModule.Settings.SelectedHat = SelectedHat = Hat;
                HatelineModule.ReloadHat();
                conf.FadeVisible = true;
            });
            option.OnLeave += () => {
                conf.FadeVisible = false;
            };

            string modname = mod;
            if (Dialog.Has(mod))
                modname = Dialog.Clean(mod);
            else if (Dialog.Has("modname_" + mod))
                modname = Dialog.Clean("modname_" + mod);
            hatSelectionMenu.Add(modname, options);
            index++;
        }
        menu.Add(hatSelectionMenu);
        index = hatSelectionMenu.MenuIndex;
        hatSelectionMenu.OnLeave += () => {
            conf.FadeVisible = false;
            notEnabledHint.FadeVisible = false;
        };
        int index2 = menu.IndexOf(hatSelectionMenu) + 1;
        menu.Insert(index2, conf);
        menu.Insert(index2, sessionHint);
        menu.Insert(index2, conf_req);
        menu.Insert(index2, notEnabledHint);

        if (inGame)
        {
            Player player = Engine.Scene?.Tracker.GetEntity<Player>();
            if (player != null && player.StateMachine.State == Player.StIntroWakeUp)
            {
                hatSelectionMenu.Disabled = true;
            }
        }
        // For invalid settings
        if (index == 0 && (SelectedHat == null || !GroupedHats["Hateline"].Contains(SelectedHat)))
        {
            index = -1;
            conf_req.FadeVisible = true;
        }
    }
}