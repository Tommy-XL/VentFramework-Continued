using System;
using UnityEngine;
using UnityEngine.UI;
using VentLib.Logging;
using VentLib.Options.Enum;
using VentLib.Options.Events;
using VentLib.Utilities.Optionals;

namespace VentLib.Options.UI.Options;
public class FloatOption: GameOption
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(FloatOption));
    // a float option uses the string class yes. fight me
    internal UnityOptional<StringOption> Behaviour = new();

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
                    break;
                default:
                    (child as UndefinedOption).HideMembers();
                    break;
            }
        });
    }

    public void Increment()
    {
        Optional<object> oldValue = Value.Map(v => v.Value);
        Behaviour.IfPresent(b => {
            b.Value = b.oldValue = SetValue(EnforceIndexConstraint(Index.OrElse(DefaultIndex) + 1, true), false);
            b.ValueText.text = GetValueText();
        });
        Behaviour.IfNotPresent(() => {
            SetValue(EnforceIndexConstraint(Index.OrElse(DefaultIndex) + 1, true), false);
        });

        object newValue = GetValue();

        OptionValueIncrementEvent incrementEvent = new(this, oldValue, newValue);
        EventHandlers.ForEach(eh => eh(incrementEvent));
    }

    public void Decrement()
    {
        Optional<object> oldValue = Value.Map(v => v.Value);
        
        Behaviour.IfPresent(b => {
            b.Value = b.oldValue = SetValue(EnforceIndexConstraint(Index.OrElse(DefaultIndex) - 1, true), false);
            b.ValueText.text = GetValueText();
        });
        Behaviour.IfNotPresent(() => {
            SetValue(EnforceIndexConstraint(Index.OrElse(DefaultIndex) + 1, true), false);
        });
        
        object newValue = GetValue();
        
        OptionValueDecrementEvent decrementEvent = new(this, oldValue, newValue);
        EventHandlers.ForEach(eh => eh(decrementEvent));
    }

    internal void BindPlusMinusButtons()
    {
        Behaviour.IfPresent(b => {
            PassiveButton plusButton = b.transform.FindChild("PlusButton (1)").GetComponent<PassiveButton>();
            PassiveButton minusButton = b.transform.FindChild("MinusButton (1)").GetComponent<PassiveButton>();

            plusButton.OnClick = new Button.ButtonClickedEvent();
            plusButton.OnMouseOut = new Button.ButtonClickedEvent();
            plusButton.OnMouseOver = new Button.ButtonClickedEvent();
            plusButton.OnClick.AddListener((Action)Increment);
            SpriteRenderer plusActiveSprite = plusButton.gameObject.transform.FindChild("InactiveSprite").gameObject.GetComponent<SpriteRenderer>();
            plusButton.OnMouseOut.AddListener((Action)(() => plusActiveSprite.color = Color.white));
            plusButton.OnMouseOver.AddListener((Action)(() => plusActiveSprite.color = Color.cyan));

            minusButton.OnClick = new Button.ButtonClickedEvent();
            minusButton.OnMouseOut = new Button.ButtonClickedEvent();
            minusButton.OnMouseOver = new Button.ButtonClickedEvent();
            minusButton.OnClick.AddListener((Action)Decrement);
            SpriteRenderer minusActiveSprite = minusButton.gameObject.transform.FindChild("InactiveSprite").gameObject.GetComponent<SpriteRenderer>();
            minusButton.OnMouseOut.AddListener((Action)(() => minusActiveSprite.color = Color.white));
            minusButton.OnMouseOver.AddListener((Action)(() => minusActiveSprite.color = Color.cyan));
        });
    }

    public static FloatOption From(GameOption option)
    {
        FloatOption floatOption = new FloatOption() {
            name = option.name,
            Key = option.Key,
            Description = option.Description,
            IOSettings = option.IOSettings,
            Values = option.Values,
            DefaultIndex = option.DefaultIndex,
            ValueType = option.ValueType,
            Attributes = option.Attributes,
        };
        option.EventHandlers.ForEach(floatOption.RegisterEventHandler);
        option.Children.ForEach(floatOption.Children.Add);
        return floatOption;
    }
}