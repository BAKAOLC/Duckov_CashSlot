using System.Text;

namespace Duckov_CashSlot.Data
{
    public readonly struct SlotSettings(
        string? name = null,
        ShowIn showIn = ShowIn.Character,
        bool forbidDeathDrop = false,
        bool forbidWeightCalculation = false,
        bool forbidItemsWithSameID = false,
        bool disableModifier = false,
        bool forbidAutoPlug = false)
    {
        public string? Name { get; } = name;
        public ShowIn ShowIn { get; } = showIn;
        public bool ForbidDeathDrop { get; } = forbidDeathDrop;
        public bool ForbidItemsWithSameID { get; } = forbidItemsWithSameID;
        public bool ForbidWeightCalculation { get; } = forbidWeightCalculation;

        public bool DisableModifier { get; } = disableModifier;
        public bool ForbidAutoPlug { get; } = forbidAutoPlug;

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(Name)) stringBuilder.AppendLine($"Name: {Name}");
            stringBuilder.AppendLine($"Show In: {ShowIn}");
            stringBuilder.AppendLine($"Forbid Death Drop: {ForbidDeathDrop}");
            stringBuilder.AppendLine($"Forbid Items With Same ID: {ForbidItemsWithSameID}");
            stringBuilder.AppendLine($"Forbid Weight Calculation: {ForbidWeightCalculation}");
            stringBuilder.AppendLine($"Disable Modifier: {DisableModifier}");
            stringBuilder.AppendLine($"Forbid Auto Plug: {ForbidAutoPlug}");
            return stringBuilder.ToString().TrimEnd();
        }
    }
}