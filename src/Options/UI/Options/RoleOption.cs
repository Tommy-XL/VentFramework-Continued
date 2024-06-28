using System;
using UnityEngine;
using UnityEngine.UI;
using VentLib.Options.Enum;
using VentLib.Options.Events;
using VentLib.Utilities.Optionals;

namespace VentLib.Options.UI.Options;
public class RoleOption: GameOption
{
    internal UnityOptional<RoleOptionSetting> Behaviour = new();

    internal FloatOption? MaximumOption;

    internal void HideMembers()
    {
        Behaviour.IfPresent(behaviour => behaviour.gameObject.SetActive(false));
        Children.ForEach(child => {
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
                case OptionType.Player:
                    (child as UndefinedOption).HideMembers();
                    break;
                default:
                    (child as UndefinedOption).HideMembers();
                    break;
            }
        });
    }

    public void IncrementChance()
    {
        Optional<object> oldValue = Value.Map(v => v.Value);

        Behaviour.IfPresent(b => {
            SetValue(EnforceIndexConstraint(Index.OrElse(DefaultIndex) + 1, true), false);
            b.chanceText.text = GetValueText();
        });
        Behaviour.IfNotPresent(() => {
            SetValue(EnforceIndexConstraint(Index.OrElse(DefaultIndex) + 1, true), false);
        });

        object newValue = GetValue();

        OptionValueIncrementEvent incrementEvent = new(this, oldValue, newValue);
        EventHandlers.ForEach(eh => eh(incrementEvent));
    }

    public void DecrementChance()
    {
        Optional<object> oldValue = Value.Map(v => v.Value);
        
        Behaviour.IfPresent(b => {
            SetValue(EnforceIndexConstraint(Index.OrElse(DefaultIndex) - 1, true), false);
            b.chanceText.text = GetValueText();
        });
        Behaviour.IfNotPresent(() => {
            SetValue(EnforceIndexConstraint(Index.OrElse(DefaultIndex) - 1, true), false);
        });
        
        object newValue = GetValue();
        
        OptionValueDecrementEvent decrementEvent = new(this, oldValue, newValue);
        EventHandlers.ForEach(eh => eh(decrementEvent));
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
                MaximumOption = Children.Find(child => child.Name() == "Maximum") as FloatOption;
                PassiveButton plusButtonNumber = b.transform.FindChild("Role #/PlusButton (1)").GetComponent<PassiveButton>();
                PassiveButton minusButtonNumber = b.transform.FindChild("Role #/MinusButton (1)").GetComponent<PassiveButton>();

                plusButtonNumber.OnClick = new Button.ButtonClickedEvent();
                plusButtonNumber.OnMouseOut = new Button.ButtonClickedEvent();
                plusButtonNumber.OnMouseOver = new Button.ButtonClickedEvent();
                plusButtonNumber.OnClick.AddListener((Action)IncrementRoleCount);
                SpriteRenderer plusActiveSpriteNumber = plusButtonNumber.gameObject.transform.FindChild("InactiveSprite").gameObject.GetComponent<SpriteRenderer>();
                plusButtonNumber.OnMouseOut.AddListener((Action)(() => plusActiveSpriteNumber.color = Color.white));
                plusButtonNumber.OnMouseOver.AddListener((Action)(() => plusActiveSpriteNumber.color = Color.cyan));
                plusButtonNumber.gameObject.SetActive(true);

                minusButtonNumber.OnClick = new Button.ButtonClickedEvent();
                minusButtonNumber.OnMouseOut = new Button.ButtonClickedEvent();
                minusButtonNumber.OnMouseOver = new Button.ButtonClickedEvent();
                minusButtonNumber.OnClick.AddListener((Action)DecrementRoleCount);
                SpriteRenderer minusActiveSpriteNumber = minusButtonNumber.gameObject.transform.FindChild("InactiveSprite").gameObject.GetComponent<SpriteRenderer>();
                minusButtonNumber.OnMouseOut.AddListener((Action)(() => minusActiveSpriteNumber.color = Color.white));
                minusButtonNumber.OnMouseOver.AddListener((Action)(() => minusActiveSpriteNumber.color = Color.cyan));
                minusButtonNumber.gameObject.SetActive(true);
                b.countText.text = MaximumOption.GetValueText();
            } else {
                b.transform.FindChild("Role #").gameObject.SetActive(false);
            }
            PassiveButton plusButton = b.transform.FindChild("Chance %/PlusButton (1)").GetComponent<PassiveButton>();
            PassiveButton minusButton = b.transform.FindChild("Chance %/MinusButton (1)").GetComponent<PassiveButton>();

            plusButton.OnClick = new Button.ButtonClickedEvent();
            plusButton.OnMouseOut = new Button.ButtonClickedEvent();
            plusButton.OnMouseOver = new Button.ButtonClickedEvent();
            plusButton.OnClick.AddListener((Action)IncrementChance);
            SpriteRenderer plusActiveSprite = plusButton.gameObject.transform.FindChild("InactiveSprite").gameObject.GetComponent<SpriteRenderer>();
            plusButton.OnMouseOut.AddListener((Action)(() => plusActiveSprite.color = Color.white));
            plusButton.OnMouseOver.AddListener((Action)(() => plusActiveSprite.color = Color.cyan));
            plusButton.gameObject.SetActive(true);

            minusButton.OnClick = new Button.ButtonClickedEvent();
            minusButton.OnMouseOut = new Button.ButtonClickedEvent();
            minusButton.OnMouseOver = new Button.ButtonClickedEvent();
            minusButton.OnClick.AddListener((Action)DecrementChance);
            SpriteRenderer minusActiveSprite = minusButton.gameObject.transform.FindChild("InactiveSprite").gameObject.GetComponent<SpriteRenderer>();
            minusButton.OnMouseOut.AddListener((Action)(() => minusActiveSprite.color = Color.white));
            minusButton.OnMouseOver.AddListener((Action)(() => minusActiveSprite.color = Color.cyan));
            minusButton.gameObject.SetActive(true);
            Value.Map(v => v.Value).IfPresent(v => b.chanceText.text = GetValueText());
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
            OptionType = OptionType.Bool,
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