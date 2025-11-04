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
        private bool _excludedTagsExpanded;
        private bool _isVisible;
        private int _openTagDropdownKey = -1;
        private CustomSlotSetting? _originalConfig;

        private bool _requiredTagsExpanded;
        private int _selectedSlotIndex = -1;
        private Vector2 _slotScrollPosition;
        private Vector2 _tagDropdownScrollPosition;
        private Vector2 _tagScrollPosition;
        private string _tagSearchText = "";
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

        private void OnDestroy()
        {
            _isVisible = false;
            _tempCustomSlots?.Clear();
            _slotBackups.Clear();
            _positionInputs.Clear();
            _openTagDropdownKey = -1;
            _tagDropdownScrollPosition = Vector2.zero;
            _tagSearchText = "";
            _requiredTagsExpanded = false;
            _excludedTagsExpanded = false;
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
        }

        private static bool IsMouseButton(KeyCode keyCode)
        {
            if (keyCode < KeyCode.Mouse0) return false;
            return keyCode <= KeyCode.Mouse6;
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

            _tempDisplaySetting.InventorySlotDisplayRows =
                DrawQuantityConfig("玩家库存侧槽位列表最大显示行数", _tempDisplaySetting.InventorySlotDisplayRows);

            GUILayout.Space(10);

            _tempDisplaySetting.PetSlotDisplayRows =
                DrawQuantityConfig("宠物侧槽位列表最大显示行数", _tempDisplaySetting.PetSlotDisplayRows);

            GUILayout.Space(10);

            _tempDisplaySetting.PetSlotDisplayColumns =
                DrawQuantityConfig("宠物侧槽位列表显示列数", _tempDisplaySetting.PetSlotDisplayColumns);

            GUILayout.Space(10);

            _tempDisplaySetting.PetInventoryDisplayColumns =
                DrawQuantityConfig("宠物侧背包列表显示列数", _tempDisplaySetting.PetInventoryDisplayColumns);

            GUILayout.Space(10);

            GUILayout.Label("宠物侧槽位列表显示位置");
            _tempDisplaySetting.PetSlotDisplayPosition = (PetSlotDisplayPosition)GUILayout.SelectionGrid(
                (int)_tempDisplaySetting.PetSlotDisplayPosition,
                [
                    "显示在玩家库存上方",
                    "显示在玩家库存下方",
                    "显示在宠物图标下方",
                ],
                3);
            GUILayout.Label(
                "提示：在装有 SuperPet Mod 且未启用新版本 Super Pet 显示样式适配时，此选项无效",
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    normal = { textColor = Color.red },
                    wordWrap = true,
                });

            GUILayout.Space(10);

            _tempDisplaySetting.NewSuperPetDisplayCompact =
                GUILayout.Toggle(_tempDisplaySetting.NewSuperPetDisplayCompact,
                    "是否启用新版本 Super Pet 显示样式适配");

            _tempDisplaySetting.AllowModifyOtherModPetDisplay =
                GUILayout.Toggle(_tempDisplaySetting.AllowModifyOtherModPetDisplay,
                    "是否允许本 Mod 修改其它 Mod 修改的宠物背包显示样式");

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

        private static int DrawQuantityConfig(string label, int currentValue)
        {
            var quantity = currentValue;

            GUILayout.Label(label, GUI.skin.label);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"当前值: {quantity}", GUILayout.Width(200));

                if (GUILayout.Button("-", GUILayout.Width(30))) quantity = Math.Max(1, quantity - 1);
                if (GUILayout.Button("+", GUILayout.Width(30))) quantity++;
            }

            return quantity;
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
            {
                CloseTagDropdown();
                _selectedSlotIndex = isSelected ? -1 : index;
            }

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
                ExcludedTags = [..source.ExcludedTags],
                ShowIn = source.ShowIn,
                ForbidDeathDrop = source.ForbidDeathDrop,
                ForbidWeightCalculation = source.ForbidWeightCalculation,
                ForbidItemsWithSameID = source.ForbidItemsWithSameID,
                DisableModifier = source.DisableModifier,
            };
        }

        private void DrawTagSelector(List<string> tags, int tagIndex, bool isRequired)
        {
            var tagTypeOffset = isRequired ? 0 : 10000;
            var dropdownKey = _selectedSlotIndex * 1000 + tagTypeOffset + tagIndex;

            GUILayout.BeginHorizontal();

            var currentTag = tags[tagIndex];
            var allTags = TagManager.AllTags;

            var isValidTag = !string.IsNullOrEmpty(currentTag) && allTags.Any(t => t.name == currentTag);
            var displayName = string.IsNullOrEmpty(currentTag) ? "选择标签..." : GetTagDisplayName(currentTag);

            var mainButtonStyle = new GUIStyle(GUI.skin.button);
            if (!string.IsNullOrEmpty(currentTag) && !isValidTag)
            {
                mainButtonStyle.normal.textColor = Color.red;
                displayName = $"[无效] {currentTag}";
            }

            if (GUILayout.Button(displayName, mainButtonStyle, GUILayout.ExpandWidth(true)))
            {
                if (_openTagDropdownKey == dropdownKey)
                {
                    CloseTagDropdown();
                }
                else
                {
                    _openTagDropdownKey = dropdownKey;
                    _tagSearchText = "";
                }
            }

            if (GUILayout.Button("删除", GUILayout.Width(60)))
            {
                tags.RemoveAt(tagIndex);
                if (_openTagDropdownKey == dropdownKey) CloseTagDropdown();
                return;
            }

            GUILayout.EndHorizontal();

            if (_openTagDropdownKey != dropdownKey) return;
            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
            GUILayout.Label("搜索:", GUILayout.Width(40));
            _tagSearchText = GUILayout.TextField(_tagSearchText);

            if (GUILayout.Button("清空", GUILayout.Width(40))) _tagSearchText = "";
            GUILayout.EndHorizontal();

            var filteredTags = allTags.Where(filteredTag =>
                string.IsNullOrEmpty(_tagSearchText) ||
                filteredTag.name.IndexOf(_tagSearchText, StringComparison.OrdinalIgnoreCase) >= 0
            ).ToList();

            _tagDropdownScrollPosition = GUILayout.BeginScrollView(
                _tagDropdownScrollPosition,
                GUILayout.Height(150));

            if (filteredTags.Count == 0)
                GUILayout.Label("未找到匹配的标签", GUI.skin.label);
            else
                foreach (var filteredTag in filteredTags)
                {
                    var tagDisplayName = GetTagDisplayName(filteredTag.name);
                    var isCurrentTag = currentTag == filteredTag.name;

                    var dropdownButtonStyle = new GUIStyle(GUI.skin.button);
                    if (isCurrentTag)
                    {
                        dropdownButtonStyle.normal.textColor = Color.green;
                        dropdownButtonStyle.fontStyle = FontStyle.Bold;
                    }

                    if (!GUILayout.Button(tagDisplayName, dropdownButtonStyle, GUILayout.ExpandWidth(true))) continue;
                    tags[tagIndex] = filteredTag.name;
                    CloseTagDropdown();
                }

            GUILayout.EndScrollView();

            if (GUILayout.Button("关闭", GUILayout.Height(25))) CloseTagDropdown();

            GUILayout.EndVertical();
        }

        private static string GetTagDisplayName(string tagName)
        {
            if (string.IsNullOrEmpty(tagName)) return "未选择";

            var tag = TagManager.GetTagByName(tagName);
            return tag != null ? $"{tag.DisplayName} ({tag.name})" : tagName;
        }

        private void CloseTagDropdown()
        {
            _openTagDropdownKey = -1;
            _tagSearchText = "";
        }

        private void DrawTagSection(string title, List<string> tags, ref bool isExpanded, bool isRequired)
        {
            var expandButtonText = isExpanded ? "▼" : "▶";
            var tagCountText = tags.Count > 0 ? $" ({tags.Count})" : "";
            var fullTitle = $"{expandButtonText} {title}{tagCountText}";

            if (GUILayout.Button(fullTitle, GUILayout.Height(25)))
            {
                if (isExpanded)
                {
                    isExpanded = false;
                }
                else
                {
                    _requiredTagsExpanded = false;
                    _excludedTagsExpanded = false;
                    isExpanded = true;
                }

                CloseTagDropdown();
            }

            if (!isExpanded) return;
            GUILayout.BeginVertical("box");

            _tagScrollPosition = GUILayout.BeginScrollView(_tagScrollPosition, GUILayout.Height(280));

            if (tags.Count == 0)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label("当前列表为空", GUI.skin.label);
                GUILayout.Space(5);
                var descriptionStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    normal = { textColor = Color.gray },
                    wordWrap = true,
                };

                var emptyDescription = isRequired
                    ? "名称将显示为 \"?\" 但是允许放置任何物品"
                    : "不会排除任何物品";
                GUILayout.Label(emptyDescription, descriptionStyle);
                GUILayout.EndVertical();
            }
            else
            {
                for (var i = 0; i < tags.Count; i++) DrawTagSelector(tags, i, isRequired);
            }

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("添加标签"))
            {
                tags.Add("");

                var newTagIndex = tags.Count - 1;
                var tagTypeOffset = isRequired ? 0 : 10000;
                var newDropdownKey = _selectedSlotIndex * 1000 + tagTypeOffset + newTagIndex;
                _openTagDropdownKey = newDropdownKey;
                _tagSearchText = "";
            }

            if (GUILayout.Button("清理无效标签"))
            {
                var originalCount = tags.Count;
                var validTags = ValidateAndCleanTags(tags);
                tags.Clear();
                tags.AddRange(validTags);
                var removedCount = originalCount - tags.Count;
                var tagTypeName = isRequired ? "必需" : "排除";
                ModLogger.Log(removedCount > 0
                    ? $"已从{tagTypeName}标签清理 {removedCount} 个无效标签"
                    : $"{tagTypeName}标签没有无效标签需要清理");
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private static List<string> ValidateAndCleanTags(List<string> tags)
        {
            var allTags = TagManager.AllTags;
            var validTags = new List<string>();
            var removedTags = new List<string>();

            foreach (var tagName in tags)
            {
                if (string.IsNullOrEmpty(tagName))
                {
                    removedTags.Add("空标签");
                    continue;
                }

                var tag = allTags.FirstOrDefault(t => t.name == tagName);
                if (tag != null)
                    validTags.Add(tagName);
                else
                    removedTags.Add(tagName);
            }

            if (removedTags.Count > 0) ModLogger.LogWarning($"已清理无效标签: {string.Join(", ", removedTags)}");

            return validTags;
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

            DrawTagSection("必需标签", slot.RequiredTags, ref _requiredTagsExpanded, true);

            GUILayout.Space(5);

            DrawTagSection("排除标签", slot.ExcludedTags, ref _excludedTagsExpanded, false);

            GUILayout.Space(10);

            GUILayout.Label("槽位设置:", GUI.skin.label);

            GUILayout.BeginHorizontal();
            GUILayout.Label("显示位置:", GUILayout.Width(120));
            slot.ShowIn = (ShowIn)GUILayout.SelectionGrid((int)slot.ShowIn, ["角色", "宠物"], 2);
            GUILayout.EndHorizontal();

            slot.ForbidDeathDrop = GUILayout.Toggle(slot.ForbidDeathDrop, "禁止死亡掉落");
            slot.ForbidWeightCalculation = GUILayout.Toggle(slot.ForbidWeightCalculation, "禁止重量计算");
            slot.ForbidItemsWithSameID = GUILayout.Toggle(slot.ForbidItemsWithSameID, "禁止相同ID物品");
            slot.DisableModifier = GUILayout.Toggle(slot.DisableModifier, "禁用物品属性修正词条（比如移动速度、背包容量等）");

            GUILayout.FlexibleSpace();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("取消", GUILayout.Height(30)))
                if (_slotBackups.ContainsKey(index))
                {
                    var backup = _slotBackups[index];
                    slot.Key = backup.Key;
                    slot.RequiredTags = new(backup.RequiredTags);
                    slot.ExcludedTags = new(backup.ExcludedTags);
                    slot.ShowIn = backup.ShowIn;
                    slot.ForbidDeathDrop = backup.ForbidDeathDrop;
                    slot.ForbidWeightCalculation = backup.ForbidWeightCalculation;
                    slot.ForbidItemsWithSameID = backup.ForbidItemsWithSameID;
                    slot.DisableModifier = backup.DisableModifier;
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
            _tempDisplaySetting = SlotDisplaySetting.Instance.Clone() as SlotDisplaySetting;

            _originalConfig = ConfigManager.LoadConfigFromFile<CustomSlotSetting>(CustomSlotManager.ConfigName, false);
            _tempCustomSlots = _originalConfig.CustomSlots.Select(slot => new CustomSlotEditingData
            {
                Key = slot.Key,
                RequiredTags = ValidateAndCleanTags([..slot.RequiredTags]),
                ExcludedTags = ValidateAndCleanTags([..slot.ExcludedTags]),
                ShowIn = slot.Settings.ShowIn,
                ForbidDeathDrop = slot.Settings.ForbidDeathDrop,
                ForbidWeightCalculation = slot.Settings.ForbidWeightCalculation,
                ForbidItemsWithSameID = slot.Settings.ForbidItemsWithSameID,
                DisableModifier = slot.Settings.DisableModifier,
            }).ToList();

            RefreshPositionInputs();
        }

        private void CancelAllSlotConfig()
        {
            if (_originalConfig == null) return;

            _tempCustomSlots = _originalConfig.CustomSlots.Select(slot => new CustomSlotEditingData
            {
                Key = slot.Key,
                RequiredTags = ValidateAndCleanTags([..slot.RequiredTags]),
                ExcludedTags = ValidateAndCleanTags([..slot.ExcludedTags]),
                ShowIn = slot.Settings.ShowIn,
                ForbidDeathDrop = slot.Settings.ForbidDeathDrop,
                ForbidWeightCalculation = slot.Settings.ForbidWeightCalculation,
                ForbidItemsWithSameID = slot.Settings.ForbidItemsWithSameID,
                DisableModifier = slot.Settings.DisableModifier,
            }).ToList();

            _slotBackups.Clear();
            _selectedSlotIndex = -1;
            RefreshPositionInputs();
        }

        private void SaveSlotRowsConfig()
        {
            if (_tempDisplaySetting == null) return;

            var config = SlotDisplaySetting.Instance;
            config.CopyFrom(_tempDisplaySetting);
            config.Validate();
            ConfigManager.SaveConfigToFile(config, "SlotDisplaySetting.json");

            _tempDisplaySetting.CopyFrom(config);
            ModLogger.Log("槽位行数配置已保存");
        }

        private static void SaveKeyCodeConfig()
        {
            var config = UIConfig.Instance;
            config.Validate();
            ConfigManager.SaveConfigToFile(config, "UIConfig.json");
            ModLogger.Log("按键配置已保存");
        }

        private void SaveSlotConfig()
        {
            if (_tempCustomSlots == null || _originalConfig == null) return;

            foreach (var slot in _tempCustomSlots)
            {
                slot.RequiredTags = ValidateAndCleanTags(slot.RequiredTags);
                slot.ExcludedTags = ValidateAndCleanTags(slot.ExcludedTags);
            }

            var customSlots = _tempCustomSlots.Select(slot => new CustomSlot(
                slot.Key,
                slot.RequiredTags.ToArray(),
                slot.ExcludedTags.ToArray(),
                new(
                    slot.ShowIn,
                    slot.ForbidDeathDrop,
                    slot.ForbidWeightCalculation,
                    slot.ForbidItemsWithSameID,
                    slot.DisableModifier
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
                RequiredTags = ValidateAndCleanTags([..slot.RequiredTags]),
                ExcludedTags = ValidateAndCleanTags([..slot.ExcludedTags]),
                ShowIn = slot.Settings.ShowIn,
                ForbidDeathDrop = slot.Settings.ForbidDeathDrop,
                ForbidWeightCalculation = slot.Settings.ForbidWeightCalculation,
                ForbidItemsWithSameID = slot.Settings.ForbidItemsWithSameID,
                DisableModifier = slot.Settings.DisableModifier,
            }).ToList();
            _selectedSlotIndex = -1;
        }

        private void AddNewSlot()
        {
            if (_tempCustomSlots == null) return;
            _tempCustomSlots.Add(new()
            {
                Key = $"NewSlot_{Guid.NewGuid().ToString()[..8]}",
                RequiredTags = [],
                ExcludedTags = [],
                ShowIn = ShowIn.Character,
                ForbidDeathDrop = false,
                ForbidWeightCalculation = false,
                ForbidItemsWithSameID = false,
                DisableModifier = false,
            });
            _selectedSlotIndex = _tempCustomSlots.Count - 1;
            RefreshPositionInputs();
        }

        private class CustomSlotEditingData
        {
            public string Key { get; set; } = string.Empty;
            public List<string> RequiredTags { get; set; } = [];
            public List<string> ExcludedTags { get; set; } = [];
            public ShowIn ShowIn { get; set; } = ShowIn.Character;
            public bool ForbidDeathDrop { get; set; }
            public bool ForbidWeightCalculation { get; set; }
            public bool ForbidItemsWithSameID { get; set; }
            public bool DisableModifier { get; set; }
        }
    }
}