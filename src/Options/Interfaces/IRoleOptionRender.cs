using UnityEngine;
using VentLib.Options.UI;
using VentLib.Options.UI.Options;

namespace VentLib.Options.Interfaces;

public interface IRoleOptionRender
{
    void RenderTabs(IGameOptionTab[] tabs, RolesSettingsMenu menu);

    void PreRender(GameOption option, RenderOptions renderOptions, RolesSettingsMenu menu);

    void Render(GameOption option, (int level, int index) info, RenderOptions renderOptions, RolesSettingsMenu menu);

    void PostRender(RolesSettingsMenu menu);

    void SetHeight(float height);

    float GetHeight();

    float GetChancesHeight();

    void Close();

    Color[] GetColorsFromName(string name);

    bool IsColorBright(Color color);
}