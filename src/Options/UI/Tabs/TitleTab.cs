using System;
using UnityEngine;
using System.Collections.Generic;
using VentLib.Logging;
using VentLib.Options.Interfaces;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Optionals;
using VentLib.Options.Extensions;

namespace VentLib.Options.UI.Tabs;

public class TitleTab: IGameOptionTab
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(VanillaTab));
    protected UnityOptional<CategoryHeaderEditRole> TabButton = UnityOptional<CategoryHeaderEditRole>.Null();
    protected UnityOptional<RolesSettingsMenu> RelatedMenu = UnityOptional<RolesSettingsMenu>.Null();

    private OrderedSet<GameOption> options = new();
    private readonly List<Action<IGameOptionTab>> callbacks = new();

    internal string titleText;
    internal Color[] colors;

    public TitleTab(string titleText, int colorId = 0, Color[]? colors = null)
    {
        this.titleText = titleText;

        switch (colorId)
        {
            case 0:
                this.colors = new Color[] {Palette.CrewmateRoleHeaderTextBlue, Palette.CrewmateRoleHeaderBlue, Palette.CrewmateRoleHeaderVeryDarkBlue, Palette.CrewmateRoleHeaderDarkBlue};
                break;
            case 1:
                this.colors = new Color[] {Palette.ImpostorRoleHeaderTextRed, Palette.ImpostorRoleHeaderRed, Palette.ImpostorRoleHeaderVeryDarkRed, Palette.ImpostorRoleHeaderDarkRed};
                break;
            default:
                if (colors == null) throw new ArgumentNullException(nameof(colors) + " cannot be null when " + nameof(colorId) + " is 2. Use 0 (Crew) or 1 (Impostor).");
                if (colors.Length < 4) throw new ArgumentOutOfRangeException(nameof(colors) + " must have 4 or 5 colors.");
                this.colors = colors;
                break;
        }
    }

    public void Activate()
    {

    }
    
    public void Deactivate()
    {
    
    }

    public void AddEventListener(Action<IGameOptionTab> callback) => callbacks.Add(callback);

    public void AddOption(GameOption option)
    {
        if (options.Contains(option)) return;
        options.Add(option);
    }

    public void RemoveOption(GameOption option) => options.Remove(option);

    // Ignores this when looking for options
    public virtual bool Ignore() => true;

    public void HandleClick()
    {
        callbacks.ForEach(cb => cb(this));
    }

    public virtual StringOption InitializeOption(StringOption sourceBehavior)
    {
        throw new NotImplementedException("TitleTab.InitializeOption is not meant to be called!");
    }

    public void Setup(RolesSettingsMenu menu)
    {
        CategoryHeaderEditRole headerEditRole = UnityEngine.Object.Instantiate<CategoryHeaderEditRole>(menu.categoryHeaderEditRoleOrigin, Vector3.zero, Quaternion.identity, menu.RoleChancesSettings.transform);
		headerEditRole.SetHeader(titleText, 20, colors);
        TabButton = UnityOptional<CategoryHeaderEditRole>.NonNull(headerEditRole);
    }

    public void SetPosition(Vector2 position)
    {
        TabButton.IfPresent(btn => btn.transform.localPosition = position);
    }
    
    public void Show()
    {
        TabButton.IfPresent(button => button.gameObject.SetActive(true));
    }

    public void Hide()
    {
        TabButton.IfPresent(button => button.gameObject.SetActive(false));
    }

    public virtual List<GameOption> PreRender() => GetOptions();

    public Optional<Vector3> GetPosition() => TabButton.Map(btn => btn.transform.localPosition);

    public List<GameOption> GetOptions() => options.AsList();
}