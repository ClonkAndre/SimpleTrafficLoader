using IVSDKDotNet;
using IVSDKDotNet.Attributes;

namespace SimpleTrafficLoader
{
    [ShowStaticFieldsInInspector()]
    internal class ModSettings
    {

        #region Variables
        // General
        public static bool UnloadAllGroupsWhenModUnloads;
        public static int MaxLoadedGroups;
        public static bool ForceLoadModels;

        // Budget
        public static bool AutomaticallyDetermineVehicleBudget;
        public static uint VehicleBudgetOverride;
        #endregion

        public static void Load(SettingsFile settings)
        {
            // General
            UnloadAllGroupsWhenModUnloads = settings.GetBoolean("General", "UnloadAllGroupsWhenModUnloads", true);
            MaxLoadedGroups =               settings.GetInteger("General", "MaxLoadedGroups", 5);
            ForceLoadModels =               settings.GetBoolean("General", "ForceLoadModels", false);

            // Budget
            AutomaticallyDetermineVehicleBudget = settings.GetBoolean("Budget", "AutomaticallyDetermineVehicleBudget", true);
            uint.TryParse(settings.GetValue("Budget", "VehicleBudgetOverride", "0"), out VehicleBudgetOverride);
        }

    }
}
