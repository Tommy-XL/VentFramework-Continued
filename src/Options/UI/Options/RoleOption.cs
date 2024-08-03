using System;
using UnityEngine;
using UnityEngine.UI;
using VentLib.Options.Enum;
using VentLib.Options.Events;
using VentLib.src.Options.UI;
using VentLib.Utilities.Optionals;

namespace VentLib.Options.UI.Options;
public class RoleOption: GameOption
{
    internal Optional<RoleOptionIntializer.RoleOptionIntialized> SettingsHolder = new();
    internal UnityOptional<RoleOptionSetting> Behaviour = new();

    internal Func<Sprite>? roleImageSupplier;
    internal FloatOption PercentageOption = null!;
    internal FloatOption MaximumOption = null!;

    internal void HideMembers()
    {
        // Behaviour.IfPresent(behaviour => behaviour.gameObject.SetActive(false));
        SettingsHolder.IfPresent(holder => {
            if (UnityEngine.Object.IsNativeObjectAlive(holder.MainObject)) holder.MainObject.SetActive(false); else {
                SettingsHolder = Optional<RoleOptionIntializer.RoleOptionIntialized>.Null();
            }
        });
        Children.ForEach(child => {
            switch (child.OptionType) {
                case OptionType.String:
                    (child as TextOption)!.HideMembers();
                    break;
                case OptionType.Bool:
                    (child as BoolOption)!.HideMembers();
                    break;
                case OptionType.Int:
                case OptionType.Float:
                    (child as FloatOption)!.HideMembers();
                    break;
                case OptionType.Player:
                    (child as UndefinedOption)!.HideMembers();
                    break;
                default:
                    (child as UndefinedOption)!.HideMembers();
                    break;
            }
        });
    }

    public void IncrementChance()
    {
        PercentageOption.Increment();
        Behaviour.IfPresent(b => {
            b.chanceText.text = PercentageOption.GetValueText();
        });
    }

    public void DecrementChance()
    {
        PercentageOption.Decrement();
        Behaviour.IfPresent(b => {
            b.chanceText.text = PercentageOption.GetValueText();
        });
    }

    public void IncrementRoleCount()
    {
        MaximumOption.Increment();
        Behaviour.IfPresent(b => {
            b.countText.text = MaximumOption.GetValueText();
        });
    }

    public void DecrementRoleCount()
    {
        MaximumOption.Decrement();
        Behaviour.IfPresent(b => {
            b.countText.text = MaximumOption.GetValueText();
        });
    }

    internal void BindPlusMinusButtons()
    {
        Behaviour.IfPresent(b => {
            if (Children.Find(child => child.Name() == "Maximum") != null) {
                MaximumOption = (Children.Find(child => child.Name() == "Maximum") as FloatOption)!;
                PassiveButton plusButtonNumber = b.transform.FindChild("Role #/PlusButton (1)").GetComponent<PassiveButton>();
                PassiveButton minusButtonNumber = b.transform.FindChild("Role #/MinusButton (1)").GetComponent<PassiveButton>();

                plusButtonNumber.OnClick = new Button.ButtonClickedEvent();
                plusButtonNumber.OnMouseOut = new UnityEngine.Events.UnityEvent();
                plusButtonNumber.OnMouseOver = new UnityEngine.Events.UnityEvent();
                plusButtonNumber.OnClick.AddListener((Action)IncrementRoleCount);
                SpriteRenderer plusActiveSpriteNumber = plusButtonNumber.gameObject.transform.FindChild("InactiveSprite").gameObject.GetComponent<SpriteRenderer>();
                plusButtonNumber.OnMouseOut.AddListener((Action)(() => plusActiveSpriteNumber.color = Color.white));
                plusButtonNumber.OnMouseOver.AddListener((Action)(() => plusActiveSpriteNumber.color = Color.cyan));

                minusButtonNumber.OnClick = new Button.ButtonClickedEvent();
                minusButtonNumber.OnMouseOut = new UnityEngine.Events.UnityEvent();
                minusButtonNumber.OnMouseOver = new UnityEngine.Events.UnityEvent();
                minusButtonNumber.OnClick.AddListener((Action)DecrementRoleCount);
                SpriteRenderer minusActiveSpriteNumber = minusButtonNumber.gameObject.transform.FindChild("InactiveSprite").gameObject.GetComponent<SpriteRenderer>();
                minusButtonNumber.OnMouseOut.AddListener((Action)(() => minusActiveSpriteNumber.color = Color.white));
                minusButtonNumber.OnMouseOver.AddListener((Action)(() => minusActiveSpriteNumber.color = Color.cyan));
                b.countText.text = MaximumOption.GetValueText();
            } else {
                b.transform.FindChild("Role #").gameObject.SetActive(false);
            }
            PercentageOption = (Children.Find(child => child.Name() == "Percentage") as FloatOption)!;
            PassiveButton plusButton = b.transform.FindChild("Chance %/PlusButton (1)").GetComponent<PassiveButton>();
            PassiveButton minusButton = b.transform.FindChild("Chance %/MinusButton (1)").GetComponent<PassiveButton>();

            plusButton.OnClick = new Button.ButtonClickedEvent();
            plusButton.OnMouseOut = new UnityEngine.Events.UnityEvent();
            plusButton.OnMouseOver = new UnityEngine.Events.UnityEvent();
            plusButton.OnClick.AddListener((Action)IncrementChance);
            SpriteRenderer plusActiveSprite = plusButton.gameObject.transform.FindChild("InactiveSprite").gameObject.GetComponent<SpriteRenderer>();
            plusButton.OnMouseOut.AddListener((Action)(() => plusActiveSprite.color = Color.white));
            plusButton.OnMouseOver.AddListener((Action)(() => plusActiveSprite.color = Color.cyan));

            minusButton.OnClick = new Button.ButtonClickedEvent();
            minusButton.OnMouseOut = new UnityEngine.Events.UnityEvent();
            minusButton.OnMouseOver = new UnityEngine.Events.UnityEvent();
            minusButton.OnClick.AddListener((Action)DecrementChance);
            SpriteRenderer minusActiveSprite = minusButton.gameObject.transform.FindChild("InactiveSprite").gameObject.GetComponent<SpriteRenderer>();
            minusButton.OnMouseOut.AddListener((Action)(() => minusActiveSprite.color = Color.white));
            minusButton.OnMouseOver.AddListener((Action)(() => minusActiveSprite.color = Color.cyan));
            // b.chanceText.text = PercentageOption.GetValueText();
            b.chanceText.text = $"{PercentageOption.GetValue()}%";
            // Value.Map(v => v.Value).IfNotPresent(() => b.chanceText.text = "0%");
        });
    }

    public static RoleOption From(GameOption option)
    {
        RoleOption roleOption = new RoleOption() {
            name = option.name,
            Key = option.Key,
            Description = option.Description,
            IOSettings = option.IOSettings,
            OptionType = OptionType.Role,
            Values = option.Values,
            DefaultIndex = option.DefaultIndex,
            ValueType = option.ValueType,
            Attributes = option.Attributes,
        };
        option.EventHandlers.ForEach(roleOption.RegisterEventHandler);
        option.Children.ForEach(roleOption.Children.Add);
        return roleOption;
    }
}