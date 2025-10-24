using System.Text;

namespace Duckov_CashSlot.Data
{
    public readonly struct SlotSettings(
        ShowIn showIn = ShowIn.Character,
        bool forbidDeathDrop = false,
        bool forbidWeightCalculation = false)
    {
        public ShowIn ShowIn { get; } = showIn;
        public bool ForbidDeathDrop { get; } = forbidDeathDrop;
        public bool ForbidWeightCalculation { get; } = forbidWeightCalculation;

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Show In: {ShowIn}");
            stringBuilder.AppendLine($"Forbid Death Drop: {ForbidDeathDrop}");
            stringBuilder.Append($"Forbid Weight Calculation: {ForbidWeightCalculation}");
            return stringBuilder.ToString();
        }
    }
}