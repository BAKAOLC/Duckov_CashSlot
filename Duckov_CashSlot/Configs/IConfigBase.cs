﻿namespace Duckov_CashSlot.Configs
{
    public interface IConfigBase
    {
        void LoadDefault();
        void LoadFromFile(string filePath);
        void SaveToFile(string filePath);
    }
}