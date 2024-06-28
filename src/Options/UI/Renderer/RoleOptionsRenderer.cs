using System.Linq;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.UI.Tabs;
using VentLib.Options.Extensions;
using VentLib.Options.Interfaces;
using VentLib.Utilities.Extensions;
using Sentry.Unity.NativeUtils;
using VentLib.Options.UI.Options;
using Il2CppSystem;
using VentLib.Utilities.Optionals;
using TMPro;

namespace VentLib.Options.UI.Renderer;

public class RoleOptionsRenderer: IRoleOptionRender
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(SettingsRenderer));
    private static readonly Color[] Colors = { Color.green, Color.red, Color.blue };
    private float Height = 0f;
    private float OptionCount = 0f;
    
    public void SetHeight(float height) => Height = height;
    public float GetHeight() => Height;
    public float GetOptionCount() => OptionCount;
    public void RenderTabs(IGameOptionTab[] tabs, RolesSettingsMenu menu)
    {
        // Tab Button
        float xPos = -1.928f;
        tabs.ForEach(tb => {
            tb.SetPosition(new Vector2(xPos, 2.27f));
            xPos += 0.762f;
        });
        Height = -1.928f;
        OptionCount = 0;
        // All Role Settings
        tabs.ForEach((tb, index) => {
            // Create Title Card
            string name;
            if (tb is GameOptionTab) name = (tb as GameOptionTab).name; else name = nameof(tb) + " Settings";
            Color[] colors;
            if (tb is GameOptionTab && (tb as GameOptionTab).name.ToLower().Contains("impostor"))
                colors = new Color[] {Palette.ImpostorRoleHeaderTextRed, Palette.ImpostorRoleHeaderRed, Palette.ImpostorRoleHeaderVeryDarkRed, Palette.ImpostorRoleHeaderDarkRed};
            else
                colors = new Color[] {Palette.CrewmateRoleHeaderTextBlue, Palette.CrewmateRoleHeaderBlue, Palette.CrewmateRoleHeaderVeryDarkBlue, Palette.CrewmateRoleHeaderDarkBlue};
            CategoryHeaderEditRole categoryHeaderEditRole = UnityEngine.Object.Instantiate<CategoryHeaderEditRole>(menu.categoryHeaderEditRoleOrigin, Vector3.zero, Quaternion.identity, menu.RoleChancesSettings.transform);
		    categoryHeaderEditRole.SetHeader(name, 20, colors);
		    categoryHeaderEditRole.transform.localPosition = new Vector3(4.986f, Height, -2f);
		    Height -= 0.522f;
            OptionCount += 1;
            tb.PreRender(1).Where(opt => opt is RoleOption).ForEach(opt => {
                RoleOption roleOptionInstance = opt as RoleOption;
                RoleOptionSetting roleOptionSetting = UnityEngine.Object.Instantiate<RoleOptionSetting>(menu.roleOptionSettingOrigin, Vector3.zero, Quaternion.identity, menu.RoleChancesSettings.transform);
		        roleOptionSetting.transform.localPosition = new Vector3(-0.15f, Height, -2f);
		        roleOptionSetting.OnValueChanged = new System.Action<OptionBehaviour>(_ => {});
		        roleOptionSetting.SetClickMask(menu.ButtonClickMask);
                roleOptionSetting.labelSprite.color = roleOptionInstance.Color;
                roleOptionSetting.titleText.text = roleOptionInstance.Name(false);
                roleOptionInstance.Behaviour = UnityOptional<RoleOptionSetting>.NonNull(roleOptionSetting);
                SpriteRenderer[] componentsInChildren = roleOptionSetting.GetComponentsInChildren<SpriteRenderer>(true);
		        for (int i = 0; i < componentsInChildren.Length; i++)
		        {
			        componentsInChildren[i].material.SetInt(PlayerMaterial.MaskLayer, 20);
		        }
		        foreach (TextMeshPro textMeshPro in roleOptionSetting.GetComponentsInChildren<TextMeshPro>(true))
		        {
			        textMeshPro.fontMaterial.SetFloat("_StencilComp", 3f);
			        textMeshPro.fontMaterial.SetFloat("_Stencil", (float)20);
		        }
		        menu.roleChances.Add(roleOptionSetting);
                roleOptionInstance.BindPlusMinusButtons();
                Height += -0.43f;
                OptionCount += 1;
            });
            if (index + 1 != tabs.Count()) Height -= 0.22f;
        });
        OptionCount += 1;
        menu.scrollBar.CalculateAndSetYBounds(OptionCount, 1f, 6f, 0.43f);
    }

    public void PreRender(GameOption option, RenderOptions renderOptions, RolesSettingsMenu menu)
    {
        
    }

    
    public void Render(GameOption option, (int level, int index) info, RenderOptions renderOptions, RolesSettingsMenu menu)
    {
        
    }

    public void PostRender(RolesSettingsMenu menu)
    {
        
    }

    public void Close()
    {
    }
}