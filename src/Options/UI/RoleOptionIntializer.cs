using System;
using TMPro;
using UnityEngine;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace VentLib.Options.UI;

class RoleOptionIntializer
{
    public static UnityOptional<GameObject> RoleTemplate = UnityOptional<GameObject>.Null();
    public static RoleOptionIntialized Intitialize()
    {
        if (!RoleTemplate.Exists()) throw new NullReferenceException("RoleOptionIntializer.RoleTemplate is null. It must not be null to create a new RoleOptionIntialized.");
        GameObject templateClone = UnityEngine.Object.Instantiate(RoleTemplate.Get());
        templateClone.SetActive(false);
        return new RoleOptionIntialized()
        {
            Background = templateClone.FindChild<SpriteRenderer>("Background"),
            RoleHeader = templateClone.transform.Find("RoleHeader").gameObject,
            RoleImage = templateClone.FindChild<SpriteRenderer>("RoleImage"),
            RoleDesc = templateClone.FindChild<TextMeshPro>("RoleDesc"),
            MainObject = templateClone
        };
    }
    internal class RoleOptionIntialized
    {
        public SpriteRenderer Background { get; internal init; } = null!;
        public SpriteRenderer RoleImage { get; internal init; } = null!;
        public GameObject MainObject { get; internal init; } = null!;
        public GameObject RoleHeader { get; internal init; } = null!;
        public TextMeshPro RoleDesc { get; internal init; } = null!;

        internal RoleOptionIntialized() {}
    }
}