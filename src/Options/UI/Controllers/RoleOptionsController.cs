using TMPro;
using System;
using System.Linq;
using UnityEngine;
using VentLib.Logging;
using VentLib.Utilities;
using AmongUs.GameOptions;
using VentLib.Options.Enum;
using VentLib.Options.UI.Tabs;
using VentLib.Options.Interfaces;
using VentLib.Options.Extensions;
using VentLib.Options.UI.Options;
using VentLib.Options.UI.Renderer;
using VentLib.Utilities.Optionals;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Collections;
using VentLib.Options.UI;
using VentLib.Options.Events;
using VentLib.Options.UI.Tabs.Vanilla;
using System.Collections.Generic;

namespace VentLib.Options.UI.Controllers;

[LoadStatic]
public static class RoleOptionController
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(RoleOptionController));
    private static readonly List<IGameOptionTab> BuiltinGameTabs = new List<IGameOptionTab>() { new EngineerTab(), new GuardianAngelTab(), new ScientistTab(),
                                                                                                new TrackerTab(), new NoisemakerTab(), new ShapeshifterTab(), new PhantomTab() };
    private static UnityOptional<RolesSettingsMenu> _lastInitialized = new();
    private static IRoleOptionRender _renderer = new RoleOptionsRenderer();
    private static OrderedSet<Action<IControllerEvent>> _tabEvents = new();
    public static RenderOptions RenderOptions { get; set; } = new();
    private static readonly OrderedSet<GameOptionTab> Tabs = new();
    private static IGameOptionTab? _currentTab;
    internal static bool Enabled = true;
    static RoleOptionController()
    {
        BuiltinGameTabs.ForEach(tb => tb.AddEventListener(tb2 => CurrentTab = tb2));
        _currentTab = null;
    }

    public static IGameOptionTab? CurrentTab
    {
        get => _currentTab;
        set
        {
            SwitchTab(value);
            _currentTab = value;
        }
    }

    private static IGameOptionTab[] AllTabs(bool allowIgnoredFiles = false) => BuiltinGameTabs.Concat(Tabs).Where(t => allowIgnoredFiles | !t.Ignore()).ToArray();

    public static void Enable() => Enabled = true;
    
    public static void AddTab(GameOptionTab tab)
    {
        Tabs.Add(tab);
        tab.AddEventListener(tb => CurrentTab = tb);
        Refresh();
        tab.Show();
    }

    public static void RemoveTab(GameOptionTab tab)
    {
        Tabs.Remove(tab);
        Refresh();
        tab.Hide();
    }

    public static void ClearTabs()
    {
        Tabs.ToArray().ForEach(RemoveTab);
    }

    public static void RemoveBuiltInTabs(Type? type = null)
    {
        if (type != null) {
            BuiltinGameTabs.RemoveAll(tb => {
                if (tb.GetType() == type) {
                    tb.Hide(); tb.Deactivate();
                    return true;
                } else return false;
            });
            return;
        }
        BuiltinGameTabs.ForEach(tb => {tb.Hide(); tb.Deactivate();});
        BuiltinGameTabs.Clear();
    }

    public static void RegisterEventHandler(Action<IControllerEvent> eventHandler)
    {
        _tabEvents.Add(eventHandler);
    }

    internal static void HandleOpen(RolesSettingsMenu menu)
    {
        log.Debug("HandleOpen ran.");
        _currentTab = null; // They start on the ALL tab.
        _lastInitialized = UnityOptional<RolesSettingsMenu>.NonNull(menu);
        menu.QuotaTabSelectables = new Il2CppSystem.Collections.Generic.List<UiElement>();
		menu.roleChances = new Il2CppSystem.Collections.Generic.List<RoleOptionSetting>();
        menu.roleTabs = new Il2CppSystem.Collections.Generic.List<PassiveButton>();
        menu.roleTabs.Add(menu.AllButton);

        AllTabs(true).ForEach(tab => tab.Setup(_lastInitialized.Get()));

        _renderer.RenderTabs(AllTabs(true), menu);
        
        OptionOpenEvent openEvent = new();
        _tabEvents.ForEach(handler => handler(openEvent));
        // menu.AllButton.OnClick.AddListener((Action)(() => CurrentTab = null));
    }

    internal static void OpenChancesTab() => CurrentTab = null;

    internal static void Refresh()
    {
        if (!_lastInitialized.Exists())
        {
            log.Warn("Unable to Refresh Option Controller", "OptionController");
            return;
        }
        AllTabs().ForEach(tab => tab.Setup(_lastInitialized.Get()));
        _renderer.RenderTabs(AllTabs(), _lastInitialized.Get());
    }

    internal static void DoRender(RolesSettingsMenu menu)
    {
        if (!Enabled) return;
        if (_currentTab == null) {
            // All tab detection.
            return;
        }
        CurrentTab.GetOptions().ForEach(child => {
            switch (child.OptionType) {
                case OptionType.String:
                    (child as TextOption).HideMembers();
                    break;
                case OptionType.Bool:
                    (child as BoolOption).HideMembers();
                    break;
                case OptionType.Int:
                case OptionType.Float:
                    (child as FloatOption).HideMembers();
                    break;
                default:
                    (child as UndefinedOption).HideMembers();
                    break;
            }
        });
        CurrentTab.PreRender().ForEach((option, index) => RenderCheck(option, index, menu));
        _renderer.PostRender(menu);
    }

    internal static void ValidateOptionBehaviour(GameOption option, RolesSettingsMenu menu, bool preRender = true)
    {
        
    }

    private static void RenderCheck(GameOption option, int index, RolesSettingsMenu menu)
    {
        ValidateOptionBehaviour(option, menu);
        _renderer.Render(option, (option.Level, index), RenderOptions, menu);
    }

    private static void SwitchTab(IGameOptionTab? newTab)
    {
        _currentTab?.Deactivate();
        newTab?.Activate();
        TabChangeEvent changeEvent = new(_currentTab, newTab);
        _tabEvents.ForEach(handler => handler(changeEvent));
    }
}