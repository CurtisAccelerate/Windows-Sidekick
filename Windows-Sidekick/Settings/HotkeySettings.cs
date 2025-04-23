// path: WindowsSidekick/Settings/HotkeySettings.cs
namespace WindowsSidekick.Settings
{
    public class HotkeySettings
    {
        /// <summary>
        /// Modifier key: Ctrl
        /// </summary>
        public bool Ctrl { get; set; } = true;
        /// <summary>
        /// Modifier key: Alt
        /// </summary>
        public bool Alt { get; set; } = false;
        /// <summary>
        /// Modifier key: Shift
        /// </summary>
        public bool Shift { get; set; } = false;
        /// <summary>
        /// Modifier key: Windows key
        /// </summary>
        public bool Win { get; set; } = false;
        /// <summary>
        /// The primary key for the hotkey, specified as a Keys enum name (e.g., "Oem3" for backtick)
        /// </summary>
        public string Key { get; set; } = "Oem3";
    }
}
