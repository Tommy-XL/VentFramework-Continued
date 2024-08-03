using TMPro;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Extensions;
using VentLib.Options.Interfaces;
using VentLib.Options.UI.Controllers;
using VentLib.Options.UI.Options;
using VentLib.Utilities.Extensions;

namespace VentLib.Options.UI.Renderer;

public class SettingsRenderer: IGameOptionRenderer
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(SettingsRenderer));
    private static readonly Color[] Colors = { Color.green, Color.red, Color.blue };
    private float Height = 0f;
    
    public void SetHeight(float height) => Height = height;
    public float GetHeight() => Height;

    public virtual void PreRender(GameOption option, RenderOptions renderOptions, GameOptionsMenu menu)
    {
        bool isTitle = option.IsTitle;
        
        if (isTitle)
        {
            return;
        }
        
        OptionBehaviour Behaviour = option.GetBehaviour();
        // Behaviour.transform.FindChild("Title Text").GetComponent<RectTransform>().sizeDelta = new Vector2(3.5f, 0.37f);
        
        if (option.OptionType != Enum.OptionType.Bool) {
            Behaviour.transform.FindChild("MinusButton (1)").localPosition += new Vector3(0.3f, 0f, 0f);
            Behaviour.transform.FindChild("PlusButton (1)").localPosition += new Vector3(0.3f, 0f, 0f);
            Behaviour.transform.FindChild("Value_TMP (1)").localPosition += new Vector3(0.3f, 0f, 0f);
            Behaviour.transform.FindChild("ValueBox").gameObject.SetActive(false);
        } else {
            Behaviour.transform.FindChild("Toggle").localPosition += new Vector3(1.3f, 0f, 0f);
        }
        Behaviour.transform.FindChild("Title Text").transform.localPosition -= new Vector3(0.15f, 0f, 0f);
        Behaviour.transform.Find("LabelBackground").localPosition += new Vector3(1.75f + (0.4f * (option.Level - 1)), option.Level == 1 ? 0.005f : 0, 0f);
    }

    
    public virtual void Render(GameOption option, (int level, int index) info, RenderOptions renderOptions, GameOptionsMenu menu)
    {
        if (option.OptionType == Enum.OptionType.Title)
        {
            CategoryHeaderMasked categoryHeader = (option as UndefinedOption)!.Header.Get();
            categoryHeader.transform.localPosition = new Vector3(-0.903f, Height, -2f);
            categoryHeader.transform.parent = menu.settingsContainer;
            categoryHeader.gameObject.SetActive(SettingsOptionController.ModSettingsOpened);
            Height -= 0.64f;
            return;
        }
        int lvl = info.level - 1;
        OptionBehaviour Behaviour = option.GetBehaviour();
        
        Transform transform = Behaviour.transform;
        SpriteRenderer render = Behaviour.transform.Find("LabelBackground").GetComponent<SpriteRenderer>();
        if (lvl > 0)
        {
            render.color = Colors[Mathf.Clamp((lvl - 1) % 3, 0, 2)];
            Behaviour.transform.Find("Title Text").transform.localPosition = new Vector3(-0.885f + 0.23f * Mathf.Clamp(lvl - 1, 0, int.MaxValue), 0f);
            //transform.FindChild("Title_TMP").GetComponent<RectTransform>().sizeDelta = new Vector2(3.4f, 0.37f);
            // render.transform.localPosition = new Vector3(0.1f + 0.11f * (lvl - 1), 0f);
        }
        float lvlCalculation = (0.2f * lvl);
        render.size = new Vector2(7f - lvlCalculation, 0.55f);
        // Behaviour.transform.FindChild("Title Text").transform.localPosition = new Vector3(-1.08f + lvlCalculation, 0f);
        // Behaviour.transform.FindChild("Title Text").GetComponent<RectTransform>().sizeDelta = new Vector2(5.1f - lvlCalculation, 0.28f);

        transform.localPosition = new Vector3(0.952f, Height, -2f);   
        transform.parent = menu.settingsContainer;
        Behaviour.gameObject.SetActive(SettingsOptionController.ModSettingsOpened);
        Height -= 0.45f;
    }

    public virtual void PostRender(GameOptionsMenu menu)
    {
        if (!SettingsOptionController.ModSettingsOpened) return;
        menu.scrollBar.ContentYBounds.max = -Height - 1.65f;
    }

    public virtual void Close()
    {
    }
}