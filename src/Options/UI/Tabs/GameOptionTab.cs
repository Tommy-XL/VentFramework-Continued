using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Interfaces;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using Object = UnityEngine.Object;

namespace VentLib.Options.UI.Tabs;

public class GameOptionTab : IGameOptionTab
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(GameOptionTab));
    private List<GameOption> Options { get; } = new();
    public string name;
    private Func<Sprite> spriteSupplier;

    private UnityOptional<Sprite> sprite = UnityOptional<Sprite>.Null();
    protected UnityOptional<RoleSettingsTabButton> TabButton = UnityOptional<RoleSettingsTabButton>.Null();
    protected UnityOptional<RolesSettingsMenu> RelatedMenu = UnityOptional<RolesSettingsMenu>.Null();

    private OrderedSet<Action<IGameOptionTab>> callbacks = new();

    public GameOptionTab(string name, Func<Sprite> spriteSupplier)
    {
        this.name = name;
        this.spriteSupplier = spriteSupplier;
    }

    public void AddEventListener(Action<IGameOptionTab> callback) => callbacks.Add(callback);

    public virtual void AddOption(GameOption option)
    {
        if (Options.Contains(option)) return;
        Options.Add(option);
    }

    public virtual void RemoveOption(GameOption option) => Options.Remove(option);

    public void HandleClick()
    {
        callbacks.ForEach(cb => cb(this));
    }

    public virtual StringOption InitializeOption(StringOption sourceBehavior)
    {
        if (!RelatedMenu.Exists()) throw new ArgumentException("Cannot Initialize Behaviour because menu does not exist");
        return Object.Instantiate(sourceBehavior, RelatedMenu.Get().AdvancedRolesSettings.transform);
    }

    public virtual void Setup(RolesSettingsMenu menu)
    {
        TabButton.OrElseSet(() => {
            var button = UnityEngine.Object.Instantiate<RoleSettingsTabButton>(menu.roleSettingsTabButtonOrigin, Vector3.zero, Quaternion.identity, menu.tabParent);
            button.icon.sprite = spriteSupplier();
            menu.roleTabs.Add(button.Button);
            return button;
        });
    }

    public virtual bool Ignore() => false;

    public void SetPosition(Vector2 position)
    {
        TabButton.IfPresent(btn => btn.transform.localPosition = new Vector3(position.x, position.y, -2f));
    }

    public List<GameOption> PreRender(int? targetLevel = null) => targetLevel != null ? Options.SelectMany(opt => opt.GetDisplayedMembers()).Where(opt => opt.Level == targetLevel).ToList() : Options.SelectMany(opt => opt.GetDisplayedMembers()).ToList();

    public Optional<Vector3> GetPosition() => TabButton.Map(btn => btn.transform.localPosition);
    
    public List<GameOption> GetOptions() => Options;

    public void Activate()
    {
        log.Info($"Activated Tab \"{name}\"", "TabSwitch");
        // GetTabHighlight().IfPresent(highlight => highlight.enabled = true);
        RelatedMenu.IfPresent(menu => {
            
        });
    }

    public void Deactivate()
    {
        log.Debug($"Deactivated Tab \"{name}\"", "TabSwitch");
        // GetTabHighlight().IfPresent(highlight => highlight.enabled = false);
        RelatedMenu.IfPresent(menu => {

        });
    }

    public void Show()
    {
        TabButton.IfPresent(button => button.gameObject.SetActive(true));
    }

    public void Hide()
    {
        TabButton.IfPresent(button => button.gameObject.SetActive(false));
    }


    private UnityOptional<SpriteRenderer> GetTabBackground() => TabButton.UnityMap(button => button.background);
    
    private UnityOptional<PassiveButton> GetPassiveButton() => TabButton.UnityMap(button => button.Button);
}