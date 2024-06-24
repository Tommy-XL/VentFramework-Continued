using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Extensions;
using VentLib.Options.Interfaces;
using VentLib.Options.UI.Controllers;
using VentLib.Options.UI.Options;
using VentLib.Utilities.Extensions;

namespace VentLib.Options.UI.Renderer;

public class RoleOptionsRenderer: IRoleOptionRender
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(SettingsRenderer));
    private static readonly Color[] Colors = { Color.green, Color.red, Color.blue };
    private float Height = 0f;
    
    public void SetHeight(float height) => Height = height;
    public float GetHeight() => Height;
    public void RenderTabs(IGameOptionTab[] tabs, RolesSettingsMenu menu)
    {
        float xPos = -1.928f;
        tabs.ForEach(tb => {
            tb.SetPosition(new UnityEngine.Vector2(xPos, 2.27f));
            xPos += 0.762f;
        });
    }

    public void PreRender(GameOption option, RenderOptions renderOptions, RolesSettingsMenu menu)
    {
        
    }

    
    public void Render(GameOption option, (int level, int index) info, RenderOptions renderOptions, RolesSettingsMenu menu)
    {
        
    }

    public void PostRender(RolesSettingsMenu menu)
    {
        if (!SettingsOptionController.ModSettingsOpened) return;
		menu.scrollBar.SetYBoundsMax(-Height - 1.65f);
    }

    public void Close()
    {
    }
}