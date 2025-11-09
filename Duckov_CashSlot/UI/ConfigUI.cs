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

            _windowRect = GUILayout.Window(999, _windowRect, DrawWindow, Localization.Tr("window.title"));
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
            if (GUILayout.Toggle(_currentTab == 0, Localization.Tr("tab.rows"), "Button"))
            {
                _currentTab = 0;
                _selectedSlotIndex = -1;
            }

            if (GUILayout.Toggle(_currentTab == 1, Localization.Tr("tab.slots"), "Button")) _currentTab = 1;
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
                DrawQuantityConfig(Localization.Tr("rows.inventory.max"), _tempDisplaySetting.InventorySlotDisplayRows);

            GUILayout.Space(10);

            _tempDisplaySetting.PetSlotDisplayRows =
                DrawQuantityConfig(Localization.Tr("rows.pet.max"), _tempDisplaySetting.PetSlotDisplayRows);

            GUILayout.Space(10);

            _tempDisplaySetting.PetSlotDisplayColumns =
                DrawQuantityConfig(Localization.Tr("rows.pet.columns"), _tempDisplaySetting.PetSlotDisplayColumns);

            GUILayout.Space(10);

            _tempDisplaySetting.PetInventoryDisplayColumns =
                DrawQuantityConfig(Localization.Tr("rows.pet.inv.columns"),
                    _tempDisplaySetting.PetInventoryDisplayColumns);

            GUILayout.Space(10);

            GUILayout.Label(Localization.Tr("rows.pet.position.title"));
            _tempDisplaySetting.PetSlotDisplayPosition = (PetSlotDisplayPosition)GUILayout.SelectionGrid(
                (int)_tempDisplaySetting.PetSlotDisplayPosition,
                [
                    Localization.Tr("rows.pet.position.inv.above"),
                    Localization.Tr("rows.pet.position.inv.below"),
                    Localization.Tr("rows.pet.position.pet.below"),
                ],
                3);
            GUILayout.Label(
                Localization.Tr("rows.pet.hint.superpet.invalid"),
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    normal = { textColor = Color.red },
                    wordWrap = true,
                });

            GUILayout.Space(10);

            _tempDisplaySetting.NewSuperPetDisplayCompact =
                GUILayout.Toggle(_tempDisplaySetting.NewSuperPetDisplayCompact,
                    Localization.Tr("rows.superpet.compact"));

            _tempDisplaySetting.AllowModifyOtherModPetDisplay =
                GUILayout.Toggle(_tempDisplaySetting.AllowModifyOtherModPetDisplay,
                    Localization.Tr("rows.superpet.allow_modify_others"));

            _tempDisplaySetting.DontNeedMoreSlotReminder =
                GUILayout.Toggle(_tempDisplaySetting.DontNeedMoreSlotReminder,
                    Localization.Tr("rows.dont_need_more_slot_reminder"));
            if (_tempDisplaySetting.DontNeedMoreSlotReminder)
                GUILayout.Label(
                    Localization.Tr("rows.dont_need_more_slot_reminder.warning"),
                    new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 16,
                        normal = { textColor = Color.red },
                        wordWrap = true,
                    });

            GUILayout.Space(20);

            DrawKeyCodeConfig();

            GUILayout.FlexibleSpace();

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Localization.Tr("common.reset_default"), GUILayout.Height(30)))
                _tempDisplaySetting.LoadDefault();
            if (GUILayout.Button(Localization.Tr("common.save"), GUILayout.Height(30))) SaveSlotRowsConfig();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private static int DrawQuantityConfig(string label, int currentValue)
        {
            var quantity = currentValue;

            GUILayout.Label(label, GUI.skin.label);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(Localization.TrFormat("quantity.current_value", quantity), GUILayout.Width(200));

                if (GUILayout.Button("-", GUILayout.Width(30))) quantity = Math.Max(1, quantity - 1);
                if (GUILayout.Button("+", GUILayout.Width(30))) quantity++;
            }

            return quantity;
        }

        private void DrawKeyCodeConfig()
        {
            GUILayout.Label(Localization.Tr("keybinding.title"), GUI.skin.label);
            GUILayout.BeginHorizontal();
            GUILayout.Label(Localization.TrFormat("keybinding.current", UIConfig.Instance.ToggleKey),
                GUILayout.Width(200));

            if (_detectingKey)
            {
                GUILayout.Label(Localization.Tr("keybinding.press_to_set"), GUI.skin.label);
                if (GUILayout.Button(Localization.Tr("common.cancel"), GUILayout.Width(100))) _detectingKey = false;
            }
            else
            {
                if (GUILayout.Button(Localization.Tr("keybinding.detect"), GUILayout.Width(100))) _detectingKey = true;
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
            if (GUILayout.Button(Localization.Tr("slots.add"), GUILayout.Height(30))) AddNewSlot();
            if (GUILayout.Button(Localization.Tr("slots.cancel_all"), GUILayout.Height(30))) CancelAllSlotConfig();
            if (GUILayout.Button(Localization.Tr("common.reset_default"), GUILayout.Height(30))) ResetToDefault();
            if (GUILayout.Button(Localization.Tr("slots.save"), GUILayout.Height(30))) SaveSlotConfig();
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
                GUILayout.Label(Localization.Tr("slots.select_left_to_edit"), GUI.skin.label);
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

            if (GUILayout.Button(Localization.Tr("slots.confirm"), GUILayout.Width(50)))
                if (int.TryParse(_positionInputs[index], out var targetPosition))
                    MoveSlotToPosition(index, targetPosition - 1);

            GUILayout.Space(10);

            var buttonStyle = new GUIStyle(GUI.skin.button);
            if (isSelected) buttonStyle.normal = buttonStyle.active;

            var slotName = slot.Key;
            if (!string.IsNullOrWhiteSpace(slot.Name))
                slotName += $" ({slot.Name})";

            if (GUILayout.Button(slotName, buttonStyle, GUILayout.ExpandWidth(true)))
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
                Name = source.Name,
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
            var displayName = string.IsNullOrEmpty(currentTag)
                ? Localization.Tr("tags.select.placeholder")
                : GetTagDisplayName(currentTag);

            var mainButtonStyle = new GUIStyle(GUI.skin.button);
            if (!string.IsNullOrEmpty(currentTag) && !isValidTag)
            {
                mainButtonStyle.normal.textColor = Color.red;
                displayName = string.Format(Localization.Tr("tags.invalid.prefix"), currentTag);
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

            if (GUILayout.Button(Localization.Tr("tags.delete"), GUILayout.Width(60)))
            {
                tags.RemoveAt(tagIndex);
                if (_openTagDropdownKey == dropdownKey) CloseTagDropdown();
                return;
            }

            GUILayout.EndHorizontal();

            if (_openTagDropdownKey != dropdownKey) return;
            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
            GUILayout.Label(Localization.Tr("tags.search.label"), GUILayout.Width(40));
            _tagSearchText = GUILayout.TextField(_tagSearchText);

            if (GUILayout.Button(Localization.Tr("tags.search.clear"), GUILayout.Width(40))) _tagSearchText = "";
            GUILayout.EndHorizontal();

            var filteredTags = allTags.Where(filteredTag =>
                string.IsNullOrEmpty(_tagSearchText) ||
                filteredTag.name.IndexOf(_tagSearchText, StringComparison.OrdinalIgnoreCase) >= 0
            ).ToList();

            _tagDropdownScrollPosition = GUILayout.BeginScrollView(
                _tagDropdownScrollPosition,
                GUILayout.Height(150));

            if (filteredTags.Count == 0)
                GUILayout.Label(Localization.Tr("tags.search.no_result"), GUI.skin.label);
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

            if (GUILayout.Button(Localization.Tr("tags.close"), GUILayout.Height(25))) CloseTagDropdown();

            GUILayout.EndVertical();
        }

        private static string GetTagDisplayName(string tagName)
        {
            if (string.IsNullOrEmpty(tagName)) return Localization.Tr("tags.none");

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

            _tagScrollPosition = GUILayout.BeginScrollView(_tagScrollPosition, GUILayout.Height(260));

            if (tags.Count == 0)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label(Localization.Tr("list.empty"), GUI.skin.label);
                GUILayout.Space(5);
                var descriptionStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    normal = { textColor = Color.gray },
                    wordWrap = true,
                };

                var emptyDescription = isRequired
                    ? Localization.Tr("tags.empty.required.desc")
                    : Localization.Tr("tags.empty.excluded.desc");
                GUILayout.Label(emptyDescription, descriptionStyle);
                GUILayout.EndVertical();
            }
            else
            {
                for (var i = 0; i < tags.Count; i++) DrawTagSelector(tags, i, isRequired);
            }

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Localization.Tr("tags.add")))
            {
                tags.Add("");

                var newTagIndex = tags.Count - 1;
                var tagTypeOffset = isRequired ? 0 : 10000;
                var newDropdownKey = _selectedSlotIndex * 1000 + tagTypeOffset + newTagIndex;
                _openTagDropdownKey = newDropdownKey;
                _tagSearchText = "";
            }

            if (GUILayout.Button(Localization.Tr("tags.clean_invalid")))
            {
                var originalCount = tags.Count;
                var validTags = ValidateAndCleanTags(tags);
                tags.Clear();
                tags.AddRange(validTags);
                var removedCount = originalCount - tags.Count;
                var tagTypeName = Localization.Tr(isRequired ? "tags.type.required" : "tags.type.excluded");
                ModLogger.Log(removedCount > 0
                    ? Localization.TrFormat("tags.clean.log.cleaned", tagTypeName, removedCount)
                    : Localization.TrFormat("tags.clean.log.none", tagTypeName));
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
                    removedTags.Add(Localization.Tr("tags.empty_tag"));
                    continue;
                }

                var tag = allTags.FirstOrDefault(t => t.name == tagName);
                if (tag != null)
                    validTags.Add(tagName);
                else
                    removedTags.Add(tagName);
            }

            if (removedTags.Count > 0)
                ModLogger.LogWarning(Localization.TrFormat("tags.clean.invalid_list", string.Join(", ", removedTags)));

            return validTags;
        }

        private void DrawSlotConfigDetails(CustomSlotEditingData slot, int index)
        {
            if (_tempCustomSlots == null) return;

            GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();
            GUILayout.Label(Localization.Tr("slot.key"), GUILayout.Width(120));
            slot.Key = GUILayout.TextField(slot.Key);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Localization.Tr("slot.name"), GUILayout.Width(120));
            slot.Name = GUILayout.TextField(slot.Name);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            DrawTagSection(Localization.Tr("tags.required.title"), slot.RequiredTags, ref _requiredTagsExpanded, true);

            GUILayout.Space(5);

            DrawTagSection(Localization.Tr("tags.excluded.title"), slot.ExcludedTags, ref _excludedTagsExpanded, false);

            GUILayout.Space(10);

            GUILayout.Label(Localization.Tr("slot.settings.title"), GUI.skin.label);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Localization.Tr("slot.display_position"), GUILayout.Width(120));
            slot.ShowIn = (ShowIn)GUILayout.SelectionGrid((int)slot.ShowIn,
                [Localization.Tr("slot.showin.character"), Localization.Tr("slot.showin.pet")], 2);
            GUILayout.EndHorizontal();

            slot.ForbidDeathDrop =
                GUILayout.Toggle(slot.ForbidDeathDrop, Localization.Tr("slot.toggle.forbid_death_drop"));
            slot.ForbidWeightCalculation = GUILayout.Toggle(slot.ForbidWeightCalculation,
                Localization.Tr("slot.toggle.forbid_weight_calc"));
            slot.ForbidItemsWithSameID =
                GUILayout.Toggle(slot.ForbidItemsWithSameID, Localization.Tr("slot.toggle.forbid_same_id"));
            slot.DisableModifier =
                GUILayout.Toggle(slot.DisableModifier, Localization.Tr("slot.toggle.disable_modifier"));

            GUILayout.FlexibleSpace();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Localization.Tr("common.cancel"), GUILayout.Height(30)))
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

            if (GUILayout.Button(Localization.Tr("slots.remove"), GUILayout.Height(30)))
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
                Name = slot.Settings.Name ?? string.Empty,
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
                Name = slot.Settings.Name ?? string.Empty,
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
            ModLogger.Log(Localization.Tr("config.rows.saved"));
        }

        private static void SaveKeyCodeConfig()
        {
            var config = UIConfig.Instance;
            config.Validate();
            ConfigManager.SaveConfigToFile(config, "UIConfig.json");
            ModLogger.Log(Localization.Tr("config.key.saved"));
        }

        private void SaveSlotConfig()
        {
            if (_tempCustomSlots == null || _originalConfig == null) return;

            foreach (var slot in _tempCustomSlots)
            {
                slot.RequiredTags = ValidateAndCleanTags(slot.RequiredTags);
                slot.ExcludedTags = ValidateAndCleanTags(slot.ExcludedTags);
            }

            var customSlots = _tempCustomSlots.Select(slot =>
            {
                var slotName = string.IsNullOrWhiteSpace(slot.Name) ? null : slot.Name;
                return new CustomSlot(
                    slot.Key,
                    slot.RequiredTags.ToArray(),
                    slot.ExcludedTags.ToArray(),
                    new(
                        slotName,
                        slot.ShowIn,
                        slot.ForbidDeathDrop,
                        slot.ForbidWeightCalculation,
                        slot.ForbidItemsWithSameID,
                        slot.DisableModifier
                    )
                );
            }).ToArray();

            _originalConfig.CustomSlots = customSlots;
            _originalConfig.Validate();
            ConfigManager.SaveConfigToFile(_originalConfig, CustomSlotManager.ConfigName);
            CustomSlotManager.Reload();

            _originalConfig = ConfigManager.LoadConfigFromFile<CustomSlotSetting>(CustomSlotManager.ConfigName, false);

            ModLogger.Log(Localization.Tr("config.slots.saved"));
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
                Name = slot.Settings.Name ?? string.Empty,
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
                Name = string.Empty,
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
            public string Name { get; set; } = string.Empty;
            public ShowIn ShowIn { get; set; } = ShowIn.Character;
            public bool ForbidDeathDrop { get; set; }
            public bool ForbidWeightCalculation { get; set; }
            public bool ForbidItemsWithSameID { get; set; }
            public bool DisableModifier { get; set; }
        }
    }
}