using UnityEngine;

namespace NotificationSamples.Demo
{
    /// <summary>
    /// Inventory item data.
    /// </summary>
    [CreateAssetMenu(fileName = "InventoryItemData", menuName = "NotificationsSamples/Inventory Item", order = 1)]
    public class InventoryItemData : ScriptableObject
    {
        [Tooltip("Item's title.")]
        public string Title;

        [Tooltip("Item's description.")]
        public string Description;

        [Tooltip("The item's icon to use in the UI.")]
        public Sprite Icon;

        [Tooltip("Item's initial cost.")]
        public int InitialCost;

        [Tooltip("Cost increase every time it is bought.")]
        public int CostIncrease;

        [Tooltip("How long it takes to create the item after buying it (minutes).")]
        public float InitialCreationTime;

        [Tooltip("Increase the creation time every time it is bought (minutes).")]
        public float CreationTimeIncrease;

        [Tooltip("Currency bonus the item provides per second.")]
        public float CurrencyBonus;

        [Tooltip("Notification icon ID.")]
        public string IconId;
    }
}
