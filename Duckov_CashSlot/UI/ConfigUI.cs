using System;
using System.Collections.Generic;
using System.Linq;
using Duckov_CashSlot.Configs;
using Duckov_CashSlot.Data;
using UnityEngine;

namespace Duckov_CashSlot.UI
{
    public class ConfigUI : MonoBehaviour
    {
        private const float WindowWidth = 1200f;
        private const float WindowHeight = 700f;
        private const string WindowTitle = "Duckov CashSlot 配置";
        private readonly Dictionary<int, string> _positionInputs = [];
        private readonly Dictionary<int, CustomSlotEditingData> _slotBackups = [];
        private int _currentTab;
        private bool _detectingKey;
        private bool _isVisible;
        private CustomSlotSetting? _originalConfig;
        private int _selectedSlotIndex = -1;
        private Vector2 _slotScrollPosition;
        private Vector2 _tagScrollPosition;
        private List<CustomSlotEditingData>? _tempCustomSlots;

        private SlotDisplaySetting? _tempDisplaySetting;
        private Rect _windowRect;

        private void Start()
        {
            _isVisible = false;
            _currentTab = 0;
            LoadTempData();
        }

        private void Update()
        {
            if (!enabled) return;

            if (_detectingKey)
            {
                foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (keyCode == KeyCode.None) continue;
                    if (IsMouseButton(keyCode)) continue;

                    if (Input.GetKeyDown(keyCode))
                    {
                        UIConfig.Instance.ToggleKey = keyCode;
                        _detectingKey = false;
                        SaveKeyCodeConfig();
                        break;
                    }
                }
                return;
            }

            if (!Input.GetKeyDown(UIConfig.Instance.ToggleKey)) return;
            _isVisible = !_isVisible;
            if (_isVisible) LoadTempData();
        }

        private static bool IsMouseButton(KeyCode keyCode)
        {
            return keyCode >= KeyCode.Mouse0 && keyCode <= KeyCode.Mouse6;
        }

        private void OnDestroy()
        {
            _isVisible = false;
            _tempCustomSlots?.Clear();
            _slotBackups.Clear();
            _positionInputs.Clear();
            _tempDisplaySetting = null;
            _originalConfig = null;
            _detectingKey = false;
            _selectedSlotIndex = -1;
        }

        private void OnGUI()
        {
            if (!enabled || !_isVisible) return;

            if (_windowRect.width == 0 || _windowRect.height == 0)
            {
                var screenWidth = Screen.width;
                var screenHeight = Screen.height;
                _windowRect = new(
                    (screenWidth - WindowWidth) / 2,
                    (screenHeight - WindowHeight) / 2,
                    WindowWidth,
                    WindowHeight);
            }

            _windowRect = GUILayout.Window(999, _windowRect, DrawWindow, WindowTitle);

            var currentEvent = Event.current;
            if (currentEvent.type != EventType.Layout && currentEvent.type != EventType.Repaint)
                if (_windowRect.Contains(currentEvent.mousePosition))
                    currentEvent.Use();
        }

        private void DrawWindow(int windowId)
        {
            var dragArea = new Rect(0, 0, WindowWidth, 30);
            GUI.DragWindow(dragArea);

            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(_currentTab == 0, "槽位行数配置", "Button"))
            {
                _currentTab = 0;
                _selectedSlotIndex = -1;
            }

            if (GUILayout.Toggle(_currentTab == 1, "槽位配置", "Button")) _currentTab = 1;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            switch (_currentTab)
            {
                case 0:
                    DrawSlotRowsConfig();
                    break;
                case 1:
                    DrawSlotConfig();
                    break;
            }

            GUILayout.EndVertical();

            GUILayout.EndVertical();

            var currentEvent = Event.current;
            if (currentEvent.type != EventType.Layout && currentEvent.type != EventType.Repaint)
                if (new Rect(0, 0, WindowWidth, WindowHeight).Contains(currentEvent.mousePosition))
                    currentEvent.Use();
        }

        private void DrawSlotRowsConfig()
        {
            if (_tempDisplaySetting == null) return;

            GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            GUILayout.Label("库存槽位显示行数", GUI.skin.label);
            var inventoryRows = _tempDisplaySetting.InventorySlotDisplayRows;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"当前值: {inventoryRows}", GUILayout.Width(200));
            if (GUILayout.Button("-", GUILayout.Width(30)))
                if (inventoryRows > 1)
                    _tempDisplaySetting.InventorySlotDisplayRows--;
            if (GUILayout.Button("+", GUILayout.Width(30))) _tempDisplaySetting.InventorySlotDisplayRows++;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.Label("宠物槽位显示行数", GUI.skin.label);
            var petRows = _tempDisplaySetting.PetSlotDisplayRows;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"当前值: {petRows}", GUILayout.Width(200));
            if (GUILayout.Button("-", GUILayout.Width(30)))
                if (petRows > 1)
                    _tempDisplaySetting.PetSlotDisplayRows--;
            if (GUILayout.Button("+", GUILayout.Width(30))) _tempDisplaySetting.PetSlotDisplayRows++;
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            DrawKeyCodeConfig();

            GUILayout.FlexibleSpace();

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("重置为默认值", GUILayout.Height(30))) _tempDisplaySetting.LoadDefault();
            if (GUILayout.Button("保存配置", GUILayout.Height(30))) SaveSlotRowsConfig();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawKeyCodeConfig()
        {
            GUILayout.Label("界面开关按键", GUI.skin.label);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"当前按键: {UIConfig.Instance.ToggleKey}", GUILayout.Width(200));

            if (_detectingKey)
            {
                GUILayout.Label("按下要设置的按键...", GUI.skin.label);
                if (GUILayout.Button("取消", GUILayout.Width(100))) _detectingKey = false;
            }
            else
            {
                if (GUILayout.Button("检测按键", GUILayout.Width(100))) _detectingKey = true;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawSlotConfig()
        {
            if (_tempCustomSlots == null) return;

            GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));

            GUILayout.BeginVertical(GUILayout.Width(WindowWidth / 2 - 20), GUILayout.ExpandHeight(true));

            _slotScrollPosition = GUILayout.BeginScrollView(_slotScrollPosition, GUILayout.ExpandHeight(true));

            for (var i = 0; i < _tempCustomSlots.Count; i++) DrawSlotItem(i);

            GUILayout.EndScrollView();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("添加新槽位", GUILayout.Height(30))) AddNewSlot();
            if (GUILayout.Button("取消所有配置", GUILayout.Height(30))) CancelAllSlotConfig();
            if (GUILayout.Button("重置为默认值", GUILayout.Height(30))) ResetToDefault();
            if (GUILayout.Button("保存配置", GUILayout.Height(30))) SaveSlotConfig();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(WindowWidth / 2 - 20), GUILayout.ExpandHeight(true));

            if (_selectedSlotIndex >= 0 && _selectedSlotIndex < _tempCustomSlots.Count)
            {
                var selectedSlot = _tempCustomSlots[_selectedSlotIndex];
                if (!_slotBackups.ContainsKey(_selectedSlotIndex))
                    _slotBackups[_selectedSlotIndex] = CloneSlotEditingData(selectedSlot);
                DrawSlotConfigDetails(selectedSlot, _selectedSlotIndex);
            }
            else
            {
                GUILayout.Label("请选择左侧槽位以查看和编辑配置", GUI.skin.label);
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawSlotItem(int index)
        {
            if (_tempCustomSlots == null) return;

            var slot = _tempCustomSlots[index];
            var isSelected = _selectedSlotIndex == index;

            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();

            if (!_positionInputs.ContainsKey(index))
                _positionInputs[index] = (index + 1).ToString();

            var positionInput = GUILayout.TextField(_positionInputs[index], GUILayout.Width(50));
            _positionInputs[index] = positionInput;

            if (GUILayout.Button("确认", GUILayout.Width(50)))
                if (int.TryParse(_positionInputs[index], out var targetPosition))
                    MoveSlotToPosition(index, targetPosition - 1);

            GUILayout.Space(10);

            var buttonStyle = new GUIStyle(GUI.skin.button);
            if (isSelected) buttonStyle.normal = buttonStyle.active;
            if (GUILayout.Button(slot.Key, buttonStyle, GUILayout.ExpandWidth(true)))
                _selectedSlotIndex = isSelected ? -1 : index;

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void MoveSlotToPosition(int currentIndex, int targetIndex)
        {
            if (_tempCustomSlots == null) return;

            if (targetIndex < 0) targetIndex = 0;
            if (targetIndex >= _tempCustomSlots.Count) targetIndex = _tempCustomSlots.Count - 1;

            if (currentIndex == targetIndex)
            {
                RefreshPositionInputs();
                return;
            }

            var item = _tempCustomSlots[currentIndex];
            _tempCustomSlots.RemoveAt(currentIndex);

            var insertIndex = targetIndex;
            _tempCustomSlots.Insert(insertIndex, item);

            if (_selectedSlotIndex == currentIndex)
            {
                _selectedSlotIndex = insertIndex;
            }
            else if (currentIndex < insertIndex)
            {
                if (_selectedSlotIndex > currentIndex && _selectedSlotIndex <= insertIndex)
                    _selectedSlotIndex--;
            }
            else
            {
                if (_selectedSlotIndex >= insertIndex && _selectedSlotIndex < currentIndex)
                    _selectedSlotIndex++;
            }

            RefreshPositionInputs();
        }

        private void RefreshPositionInputs()
        {
            if (_tempCustomSlots == null) return;
            _positionInputs.Clear();
            for (var i = 0; i < _tempCustomSlots.Count; i++) _positionInputs[i] = (i + 1).ToString();
        }

        private static CustomSlotEditingData CloneSlotEditingData(CustomSlotEditingData source)
        {
            return new()
            {
                Key = source.Key,
                RequiredTags = [..source.RequiredTags],
                ShowIn = source.ShowIn,
                ForbidDeathDrop = source.ForbidDeathDrop,
                ForbidWeightCalculation = source.ForbidWeightCalculation,
                ForbidItemsWithSameID = source.ForbidItemsWithSameID,
            };
        }

        private void DrawSlotConfigDetails(CustomSlotEditingData slot, int index)
        {
            if (_tempCustomSlots == null) return;

            GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();
            GUILayout.Label("槽位Key:", GUILayout.Width(120));
            slot.Key = GUILayout.TextField(slot.Key);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.Label("标签列表:", GUI.skin.label);
            _tagScrollPosition = GUILayout.BeginScrollView(_tagScrollPosition, GUILayout.Height(150));

            for (var i = 0; i < slot.RequiredTags.Count; i++)
            {
                GUILayout.BeginHorizontal();
                slot.RequiredTags[i] = GUILayout.TextField(slot.RequiredTags[i]);
                if (GUILayout.Button("删除", GUILayout.Width(60)))
                {
                    slot.RequiredTags.RemoveAt(i);
                    i--;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            if (GUILayout.Button("添加标签")) slot.RequiredTags.Add("");

            GUILayout.Space(10);

            GUILayout.Label("槽位设置:", GUI.skin.label);

            GUILayout.BeginHorizontal();
            GUILayout.Label("显示位置:", GUILayout.Width(120));
            slot.ShowIn = (ShowIn)GUILayout.SelectionGrid((int)slot.ShowIn, ["角色", "宠物"], 2);
            GUILayout.EndHorizontal();

            slot.ForbidDeathDrop = GUILayout.Toggle(slot.ForbidDeathDrop, "禁止死亡掉落");
            slot.ForbidWeightCalculation = GUILayout.Toggle(slot.ForbidWeightCalculation, "禁止重量计算");
            slot.ForbidItemsWithSameID = GUILayout.Toggle(slot.ForbidItemsWithSameID, "禁止相同ID物品");

            GUILayout.FlexibleSpace();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("取消", GUILayout.Height(30)))
                if (_slotBackups.ContainsKey(index))
                {
                    var backup = _slotBackups[index];
                    slot.Key = backup.Key;
                    slot.RequiredTags = new(backup.RequiredTags);
                    slot.ShowIn = backup.ShowIn;
                    slot.ForbidDeathDrop = backup.ForbidDeathDrop;
                    slot.ForbidWeightCalculation = backup.ForbidWeightCalculation;
                    slot.ForbidItemsWithSameID = backup.ForbidItemsWithSameID;
                    _slotBackups.Remove(index);
                }

            if (GUILayout.Button("删除此槽位", GUILayout.Height(30)))
                if (index < _tempCustomSlots.Count)
                {
                    _tempCustomSlots.RemoveAt(index);
                    _selectedSlotIndex = -1;
                    RefreshPositionInputs();
                    _slotBackups.Remove(index);
                }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void LoadTempData()
        {
            _tempDisplaySetting = new()
            {
                InventorySlotDisplayRows = SlotDisplaySetting.Instance.InventorySlotDisplayRows,
                PetSlotDisplayRows = SlotDisplaySetting.Instance.PetSlotDisplayRows,
            };

            _originalConfig = ConfigManager.LoadConfigFromFile<CustomSlotSetting>(CustomSlotManager.ConfigName, false);
            _tempCustomSlots = _originalConfig.CustomSlots.Select(slot => new CustomSlotEditingData
            {
                Key = slot.Key,
                RequiredTags = [..slot.RequiredTags],
                ShowIn = slot.Settings.ShowIn,
                ForbidDeathDrop = slot.Settings.ForbidDeathDrop,
                ForbidWeightCalculation = slot.Settings.ForbidWeightCalculation,
                ForbidItemsWithSameID = slot.Settings.ForbidItemsWithSameID,
            }).ToList();

            RefreshPositionInputs();
        }

        private void CancelAllSlotConfig()
        {
            if (_originalConfig == null) return;

            _tempCustomSlots = _originalConfig.CustomSlots.Select(slot => new CustomSlotEditingData
            {
                Key = slot.Key,
                RequiredTags = [..slot.RequiredTags],
                ShowIn = slot.Settings.ShowIn,
                ForbidDeathDrop = slot.Settings.ForbidDeathDrop,
                ForbidWeightCalculation = slot.Settings.ForbidWeightCalculation,
                ForbidItemsWithSameID = slot.Settings.ForbidItemsWithSameID,
            }).ToList();

            _slotBackups.Clear();
            _selectedSlotIndex = -1;
            RefreshPositionInputs();
        }

        private void SaveSlotRowsConfig()
        {
            if (_tempDisplaySetting == null) return;

            var config = SlotDisplaySetting.Instance;
            config.InventorySlotDisplayRows = _tempDisplaySetting.InventorySlotDisplayRows;
            config.PetSlotDisplayRows = _tempDisplaySetting.PetSlotDisplayRows;
            config.Validate();
            ConfigManager.SaveConfigToFile(config, "SlotDisplaySetting.json");

            _tempDisplaySetting.InventorySlotDisplayRows = config.InventorySlotDisplayRows;
            _tempDisplaySetting.PetSlotDisplayRows = config.PetSlotDisplayRows;

            ModLogger.Log("槽位行数配置已保存");
        }

        private void SaveKeyCodeConfig()
        {
            var config = UIConfig.Instance;
            config.Validate();
            ConfigManager.SaveConfigToFile(config, "UIConfig.json");
            ModLogger.Log("按键配置已保存");
        }

        private void SaveSlotConfig()
        {
            if (_tempCustomSlots == null || _originalConfig == null) return;

            var customSlots = _tempCustomSlots.Select(slot => new CustomSlot(
                slot.Key,
                slot.RequiredTags.ToArray(),
                new(
                    slot.ShowIn,
                    slot.ForbidDeathDrop,
                    slot.ForbidWeightCalculation,
                    slot.ForbidItemsWithSameID
                )
            )).ToArray();

            _originalConfig.CustomSlots = customSlots;
            _originalConfig.Validate();
            ConfigManager.SaveConfigToFile(_originalConfig, CustomSlotManager.ConfigName);
            CustomSlotManager.Reload();

            _originalConfig = ConfigManager.LoadConfigFromFile<CustomSlotSetting>(CustomSlotManager.ConfigName, false);

            ModLogger.Log("槽位配置已保存");
        }

        private void ResetToDefault()
        {
            var defaultConfig = new CustomSlotSetting();
            defaultConfig.LoadDefault();
            _tempCustomSlots = defaultConfig.CustomSlots.Select(slot => new CustomSlotEditingData
            {
                Key = slot.Key,
                RequiredTags = [..slot.RequiredTags],
                ShowIn = slot.Settings.ShowIn,
                ForbidDeathDrop = slot.Settings.ForbidDeathDrop,
                ForbidWeightCalculation = slot.Settings.ForbidWeightCalculation,
                ForbidItemsWithSameID = slot.Settings.ForbidItemsWithSameID,
            }).ToList();
            _selectedSlotIndex = -1;
        }

        private void AddNewSlot()
        {
            if (_tempCustomSlots == null) return;
            _tempCustomSlots.Add(new()
            {
                Key = $"NewSlot_{Guid.NewGuid().ToString()[..8]}",
                RequiredTags = [""],
                ShowIn = ShowIn.Character,
                ForbidDeathDrop = false,
                ForbidWeightCalculation = false,
                ForbidItemsWithSameID = false,
            });
            _selectedSlotIndex = _tempCustomSlots.Count - 1;
            RefreshPositionInputs();
        }

        private class CustomSlotEditingData
        {
            public string Key { get; set; } = string.Empty;
            public List<string> RequiredTags { get; set; } = [];
            public ShowIn ShowIn { get; set; }
            public bool ForbidDeathDrop { get; set; }
            public bool ForbidWeightCalculation { get; set; }
            public bool ForbidItemsWithSameID { get; set; }
        }
    }
}