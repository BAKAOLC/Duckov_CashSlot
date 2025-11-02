using System.Text;

namespace Duckov_CashSlot.Data
{
    public readonly struct SlotSettings(
        ShowIn showIn = ShowIn.Character,
        bool forbidDeathDrop = false,
        bool forbidWeightCalculation = false,
        bool forbidItemsWithSameID = false,
        bool disableModifier = false)
    {
        public ShowIn ShowIn { get; } = showIn;
        public bool ForbidDeathDrop { get; } = forbidDeathDrop;
        public bool ForbidItemsWithSameID { get; } = forbidItemsWithSameID;
        public bool ForbidWeightCalculation { get; } = forbidWeightCalculation;

        public bool DisableModifier { get; } = disableModifier;

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Show In: {ShowIn}");
            stringBuilder.AppendLine($"Forbid Death Drop: {ForbidDeathDrop}");
            stringBuilder.AppendLine($"Forbid Items With Same ID: {ForbidItemsWithSameID}");
            stringBuilder.AppendLine($"Forbid Weight Calculation: {ForbidWeightCalculation}");
            stringBuilder.AppendLine($"Disable Modifier: {DisableModifier}");
            return stringBuilder.ToString().TrimEnd();
        }
    }
}