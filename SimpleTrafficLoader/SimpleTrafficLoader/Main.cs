using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;

using CCL.GTAIV;

using SimpleTrafficLoader.Classes;

using IVSDKDotNet;
using IVSDKDotNet.Enums;
using static IVSDKDotNet.Native.Natives;

namespace SimpleTrafficLoader
{
    public class Main : Script
    {

        #region Variables
        // Static stuff
        private static Dictionary<string, string> zoneToIslandDict = new Dictionary<string, string>()
        {
            // Alderney
            { "WESDY", "Alderney" },
            { "LEFWO", "Alderney" },
            { "ALDCI", "Alderney" },
            { "BERCH", "Alderney" },
            { "NORMY", "Alderney" },
            { "ACTRR", "Alderney" },
            { "PORTU", "Alderney" },
            { "TUDOR", "Alderney" },
            { "ACTIP", "Alderney" },
            { "ALSCF", "Alderney" },

            // Algonquin
            { "NORWO", "Algonquin" },
            { "EAHOL", "Algonquin" },
            { "NOHOL", "Algonquin" },
            { "VASIH", "Algonquin" },
            { "LANCA", "Algonquin" },
            { "MIDPE", "Algonquin" },
            { "MIDPA", "Algonquin" },
            { "MIDPW", "Algonquin" },
            { "PUGAT", "Algonquin" },
            { "HATGA", "Algonquin" },
            { "LANCE", "Algonquin" },
            { "STARJ", "Algonquin" },
            { "WESMI", "Algonquin" },
            { "TMEQU", "Algonquin" },
            { "THTRI", "Algonquin" },
            { "EASON", "Algonquin" },
            { "THPRES", "Algonquin" },
            { "FISSN", "Algonquin" },
            { "FISSO", "Algonquin" },
            { "LOWEA", "Algonquin" },
            { "LITAL", "Algonquin" },
            { "SUFFO", "Algonquin" },
            { "CASGC", "Algonquin" },
            { "CITH" , "Algonquin" },
            { "CHITO", "Algonquin" },
            { "THXCH", "Algonquin" },
            { "CASGR", "Algonquin" },

            // Bohan
            { "BOULE", "Bohan" },
            { "NRTGA", "Bohan" },
            { "LTBAY", "Bohan" },
            { "FORSI", "Bohan" },
            { "INSTI", "Bohan" },
            { "STHBO", "Bohan" },
            { "CHAPO", "Bohan" },

            // Dukes
            { "STEIN", "Dukes" },
            { "MEADP", "Dukes" },
            { "FRANI", "Dukes" },
            { "WILLI", "Dukes" },
            { "MEADH", "Dukes" },
            { "EISLC", "Dukes" },
            { "BOAB" , "Dukes" },
            { "CERHE", "Dukes" },
            { "BEECW", "Dukes" },

            // Broker
            { "SCHOL", "Broker" },
            { "DOWTW", "Broker" },
            { "ROTTH", "Broker" },
            { "ESHOO", "Broker" },
            { "OUTL", "Broker" },
            { "SUTHS", "Broker" },
            { "HOBEH", "Broker" },
            { "FIREP", "Broker" },
            { "FIISL", "Broker" },
            { "BEGGA", "Broker" },

            // Happiness Island
            { "HAPIN", "Happiness Island" },

            // Charge Island
            { "CHISL", "Charge Island" },

            // Colony Island
            { "COISL", "Colony Island" },

            // Bridges, tunnels etc TODO
            { "BRALG", "Liberty City" },
            { "BRBRO", "Liberty City" },
            { "BREBB", "Liberty City" },
            { "BRDBB", "Liberty City" },
            { "NOWOB", "Liberty City" },
            { "HIBRG", "Liberty City" },
            { "LEAPE", "Liberty City" },
            { "BOTU", "Liberty City" },

            // Liberty City
            { "LIBERTY", "Liberty City" }
        };
        private static string[] policiesArr = new string[]
        {
            "LOAD_AT_MORNING",
            "LOAD_AT_MIDDAY",
            "LOAD_AT_EVENING",
            "LOAD_AT_NIGHT",

            "DO_NOT_LOAD_WHEN_ON_MISSION",
        };

        // ImGui stuff
        public bool MenuOpened;

        private int selectedPolicyComboIndex;

        private int selectedIslandListBoxIndex;
        private int selectedNeighborhoodListBoxIndex;
        private int selectedModelListBoxIndex;
        private int selectedPolicyListBoxIndex;

        private string modelToAdd;

        // Lists
        private List<string> availableSpawnGroupFiles;
        private List<SpawnGroup> loadedSpawnGroups;

        // Zone stuff
        private bool didZoneUpdateOccur;
        private TimeSpan lastZoneUpdate;
        private string previousZone;
        private string currentZone;

        // Group stuff
        private string currentlyLoadedGroupFileName;
        private bool preventGroupLoading;

        // Other
        private int playerPed;
        #endregion

        #region Constructor
        public Main()
        {
            // IV-SDK .NET stuff
            WaitTickInterval = 1000;
            Uninitialize += Main_Uninitialize;
            Initialized += Main_Initialized;
            OnImGuiRendering += Main_OnImGuiRendering;
            WaitTick += Main_WaitTick;
            Tick += Main_Tick;
        }
        #endregion

        #region Methods
        private void LoadGroupFromFile(string fileName)
        {
            try
            {
                if (loadedSpawnGroups != null)
                    loadedSpawnGroups.Clear();

                string path = string.Format("{0}\\Groups\\{1}", ScriptResourceFolder, fileName);

                if (!File.Exists(path))
                {
                    if (fileName != "Default.json")
                    {
                        Logging.LogWarning("Could not find the '{0}' file which contains the vehicle groups that should load. Simple Traffic Loader might not work as expected. Please choose another group to load, or restore this group. Trying to load default group instead.", fileName);
                        LoadGroupFromFile("Default.json");
                    }
                    else
                    {
                        Logging.LogWarning("Could not find the default group file (Default.json). Simple Traffic Loader might not work as expected as this group is required. Make sure to restore this group or redownload if necessary.", fileName);
                    }

                    return;
                }

                loadedSpawnGroups = Helper.ConvertJsonStringToObject<List<SpawnGroup>>(File.ReadAllText(path));

                if (loadedSpawnGroups.Count == 0)
                {
                    Logging.LogWarning("No groups were loaded.");
                }
                else
                {
                    // Prepare loaded groups
                    loadedSpawnGroups.ForEach(x => x.Prepare());

                    Logging.Log("Loaded {0} groups!", loadedSpawnGroups.Count);
                }

                currentlyLoadedGroupFileName = fileName;
            }
            catch (Exception ex)
            {
                Logging.LogError("Failed to load group '{0}'. Details: {1}", fileName, ex);
            }
        }
        private void SaveCurrentlyLoadedGroupToFile()
        {
            try
            {
                if (loadedSpawnGroups == null)
                    return;
                if (string.IsNullOrWhiteSpace(currentlyLoadedGroupFileName))
                    return;

                string path = string.Format("{0}\\Groups\\{1}", ScriptResourceFolder, currentlyLoadedGroupFileName);

                try
                {
                    File.WriteAllText(path, Helper.ConvertObjectToJsonString(loadedSpawnGroups, true));
                    Logging.Log("Saved {0} groups to file '{1}'!", loadedSpawnGroups.Count, currentlyLoadedGroupFileName);
                }
                catch (Exception ex)
                {
                    Logging.LogError("Failed to save groups to file '{0}'! Details: {1}", currentlyLoadedGroupFileName, ex);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError("Failed to save group '{0}'. Details: {1}", currentlyLoadedGroupFileName, ex);
            }
        }

        private void SetVehicleBudget(uint value)
        {
            if (value == 0)
                return;

            IVStreaming.VehicleModelBudget = value;
        }
        private void AutomaticallyDetermineVehicleBudget()
        {
            try
            {
                string filePath = string.Format("{0}\\pc\\models\\cdimages\\vehicles.img", IVGame.GameStartupPath);

                if (!File.Exists(filePath))
                {
                    Logging.LogWarning("Could not determine size of vehicles.img file! The file was not found.{0}Path: {1}", Environment.NewLine, filePath);
                    return;
                }

                using (FileStream s = File.OpenRead(filePath))
                {
                    long size = s.Length + 1000000;

                    if (size > uint.MaxValue)
                        size = uint.MaxValue;

                    SetVehicleBudget((uint)size);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError("An error occured while trying to automatically set the vehicle budget value! Details: {0}", ex);
            }
        }

        private void LoadGroup(SpawnGroup group)
        {
            if (group == null)
                return;

            // Check all group load policies
            if (group.LoadPolicies != null)
            {
                // Check day time policies
                DayState dayState = NativeWorld.GetDayState();

                bool containsLoadAtMorningPolicy =  group.LoadPolicies.Contains("LOAD_AT_MORNING");
                bool containsLoadAtMiddayPolicy =   group.LoadPolicies.Contains("LOAD_AT_MIDDAY");
                bool containsLoadAtEveningPolicy =  group.LoadPolicies.Contains("LOAD_AT_EVENING");
                bool containsLoadAtNightPolicy =    group.LoadPolicies.Contains("LOAD_AT_NIGHT");

                if (containsLoadAtMorningPolicy
                    || containsLoadAtMiddayPolicy
                    || containsLoadAtEveningPolicy
                    || containsLoadAtNightPolicy)
                {
                    int matches = 0;

                    // Morning
                    if (containsLoadAtMorningPolicy)
                    {
                        if (dayState == DayState.Morning)
                        {
                            matches++;
                        }
                        else
                        {
                            if (Logging.EnableDebugLogging)
                                Logging.LogWarning("Policy 'LOAD_AT_MORNING' of group '{0}' failed to match.", group.GroupName);
                        }
                    }

                    // Midday
                    if (containsLoadAtMiddayPolicy)
                    {
                        if (dayState == DayState.Day)
                        {
                            matches++;
                        }
                        else
                        {
                            if (Logging.EnableDebugLogging)
                                Logging.LogWarning("Policy 'LOAD_AT_MIDDAY' of group '{0}' failed to match.", group.GroupName);
                        }
                    }

                    // Evening
                    if (containsLoadAtEveningPolicy)
                    {
                        if (dayState == DayState.Evening)
                        {
                            matches++;
                        }
                        else
                        {
                            if (Logging.EnableDebugLogging)
                                Logging.LogWarning("Policy 'LOAD_AT_EVENING' of group '{0}' failed to match.", group.GroupName);
                        }
                    }

                    // Night
                    if (containsLoadAtNightPolicy)
                    {
                        if (dayState == DayState.Night)
                        {
                            matches++;
                        }
                        else
                        {
                            if (Logging.EnableDebugLogging)
                                Logging.LogWarning("Policy 'LOAD_AT_NIGHT' of group '{0}' failed to match.", group.GroupName);
                        }
                    }

                    // Check if no policy matched
                    if (matches == 0)
                    {
                        return;
                    }
                }

                // Check other policies
                bool doNotLoadWhenOnMission = group.LoadPolicies.Contains("DO_NOT_LOAD_WHEN_ON_MISSION");

                if (doNotLoadWhenOnMission && IVTheScripts.IsPlayerOnAMission())
                {
                    if (Logging.EnableDebugLogging)
                        Logging.LogWarning("Not loading group {0} ({1}) because the player is on a mission and the 'DO_NOT_LOAD_WHEN_ON_MISSION' policy is set.", group.GroupName, group.ID);
                    return;
                }
            }

            // Set group to be active so it can start loading its models
            group.IsGroupActive = true;
        }
        private void UnloadGroup(SpawnGroup group)
        {
            if (group == null)
                return;

            // Create unload lambda action
            Action groupUnloadAction = () =>
            {
                // Unload models of this group
                group.UnloadModels();
            };

            // Unload group instantly
            groupUnloadAction.Invoke();
        }

        private void LoadGroups(string forZone)
        {
            // Check if we can load any more groups
            if (!(ModSettings.MaxLoadedGroups <= 0))
            {
                if (FindAllActiveGroups(true).Length >= ModSettings.MaxLoadedGroups)
                {
                    if (Logging.EnableDebugLogging)
                        Logging.LogWarning("Cannot load groups for zone '{0}' because the set limit ({1}) of loaded groups was reached!", forZone, ModSettings.MaxLoadedGroups);

                    return;
                }
            }

            if (Logging.EnableDebugLogging)
                Logging.LogWarning("Looking for groups to load for zone '{0}'..", forZone);

            // This searches for all groups which zone are equal to the one given to this function, and which are NOT loaded.
            SpawnGroup[] foundGroups = FindGroupsByArea(forZone, false, false, true);

            if (foundGroups.Length != 0)
                Logging.LogDebug("Found {0} group(s) to load for zone '{1}'.", foundGroups.Length, forZone);
            else
                Logging.LogDebug("No groups to load found for zone '{0}'.", forZone);

            for (int i = 0; i < foundGroups.Length; i++)
                LoadGroup(foundGroups[i]);
        }
        private void UnloadGroups(string forZone)
        {
            if (Logging.EnableDebugLogging)
                Logging.LogWarning("Looking for groups to unload for zone '{0}'..", forZone);

            // This searches for all groups which zone are NOT equal to the one given to this function, and which ARE loaded.
            SpawnGroup[] foundGroups = FindGroupsByArea(forZone, true, true);

            if (foundGroups.Length != 0)
                Logging.LogDebug("Found {0} group(s) to unload for zone '{1}'.", foundGroups.Length, forZone);
            else
                Logging.LogDebug("No groups to unload found for zone '{0}'.", forZone);

            for (int i = 0; i < foundGroups.Length; i++)
                UnloadGroup(foundGroups[i]);
        }

        private void VerifyActiveGroupPolicies()
        {
            SpawnGroup[] activeGroups = FindAllActiveGroups(false);

            for (int i = 0; i < activeGroups.Length; i++)
            {
                SpawnGroup group = activeGroups[i];

                // Get the current "day state"
                DayState dayState = NativeWorld.GetDayState();

                // Check group policies
                for (int p = 0; p < group.LoadPolicies.Count; p++)
                {
                    string policy = group.LoadPolicies[p];

                    switch (policy)
                    {
                        case "LOAD_AT_MORNING":
                            {
                                // If it's not morning, continue
                                if (dayState == DayState.Morning)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (Logging.EnableDebugLogging)
                                        Logging.LogWarning("Group '{0}' is loaded but it no longer matches its policy 'LOAD_AT_MORNING'. Unloading.", group.GroupName);
                                }
                            }
                            break;
                        case "LOAD_AT_MIDDAY":
                            {
                                // If it's not midday, return
                                if (dayState == DayState.Day)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (Logging.EnableDebugLogging)
                                        Logging.LogWarning("Group '{0}' is loaded but it no longer matches its policy 'LOAD_AT_MIDDAY'. Unloading.", group.GroupName);
                                }
                            }
                            break;
                        case "LOAD_AT_EVENING":
                            {
                                // If it's not evening, return
                                if (dayState == DayState.Evening)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (Logging.EnableDebugLogging)
                                        Logging.LogWarning("Group '{0}' is loaded but it no longer matches its policy 'LOAD_AT_EVENING'. Unloading.", group.GroupName);
                                }
                            }
                            break;
                        case "LOAD_AT_NIGHT":
                            {
                                // If it's not night, return
                                if (dayState == DayState.Night)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (Logging.EnableDebugLogging)
                                        Logging.LogWarning("Group '{0}' is loaded but it no longer matches its policy 'LOAD_AT_NIGHT'. Unloading.", group.GroupName);
                                }
                            }
                            break;
                    }

                    // A policy didnt match anymore! Unload group
                    UnloadGroup(group);

                    break;
                }
            }
        }
        private void TellAllGroupsToUnload()
        {
            loadedSpawnGroups.ForEach(x => x.UnloadModels());
        }

        private void AddIslandToGroup(SpawnGroup group, string fromZone)
        {
            if (zoneToIslandDict.TryGetValue(fromZone, out string island))
            {
                if (group.TargetIslands == null)
                {
                    group.TargetIslands = new List<string>();
                }
                else
                {
                    // Check if island already exists in list
                    if (group.TargetIslands.Contains(island))
                    {
                        ShowSubtitleMessageFIXED(4000, "{0} already exists in the list!", island);
                        return;
                    }
                }

                // Add island to list
                group.TargetIslands.Add(island);
            }
            else
            {
                ShowSubtitleMessageFIXED(4000, "Could not find island for zone {0} ({1})!", fromZone, GET_STRING_FROM_TEXT_FILE(currentZone));
            }
        }
        private void AddNeighborhoodToGroup(SpawnGroup group, string fromZone)
        {
            string neighborhood = GET_STRING_FROM_TEXT_FILE(fromZone);

            if (neighborhood == "NULL")
            {
                ShowSubtitleMessageFIXED(4000, "Could not find neighborhood for zone {0}!", fromZone);
                return;
            }

            if (group.TargetNeighborhoods == null)
            {
                group.TargetNeighborhoods = new List<string>();
            }
            else
            {
                // Check if neighborhood already exists in list
                if (group.TargetNeighborhoods.Contains(neighborhood))
                {
                    ShowSubtitleMessageFIXED(4000, "{0} already exists in the list!", neighborhood);
                    return;
                }
            }

            // Add neighborhood to list
            group.TargetNeighborhoods.Add(neighborhood);
        }
        private void AddVehicleModelToGroup(SpawnGroup group, string model)
        {
            if (string.IsNullOrWhiteSpace(model))
            {
                // Try get model from current vehicle of the player
                GET_CAR_CHAR_IS_USING(playerPed, out int vehicle);

                // Check if player is not sitting in any vehicle
                if (vehicle == 0)
                {
                    ShowSubtitleMessageFIXED(4000, "Cannot add empty model to list!");
                    return;
                }

                // Get model of vehicle
                GET_CAR_MODEL(vehicle, out uint modelRaw);

                // Get name of model from vehicle
                model = GET_DISPLAY_NAME_FROM_VEHICLE_MODEL(modelRaw);

                // Check for invalid string
                if (string.IsNullOrWhiteSpace(model))
                {
                    ShowSubtitleMessageFIXED(4000, "Cannot add empty model to list!");
                    return;
                }
            }

            if (group.ModelsToLoad == null)
            {
                group.ModelsToLoad = new List<string>();
            }
            else
            {
                // Check if model already exists in list
                if (group.ModelsToLoad.Contains(model))
                {
                    ShowSubtitleMessageFIXED(4000, "{0} already exists in the list!", model);
                    return;
                }
            }

            // Add model to list
            group.ModelsToLoad.Add(model);

            // Refresh group
            group.Prepare();
        }
        private void AddPolicyToGroup(SpawnGroup group, string policy)
        {
            if (string.IsNullOrWhiteSpace(policy))
            {
                ShowSubtitleMessageFIXED(4000, "Cannot add empty policy to list!");
                return;
            }

            if (group.LoadPolicies == null)
            {
                group.LoadPolicies = new List<string>();
            }
            else
            {
                // Check if policy already exists in list
                if (group.LoadPolicies.Contains(policy))
                {
                    ShowSubtitleMessageFIXED(4000, "{0} already exists in the list!", policy);
                    return;
                }
            }

            // Add policy to list
            group.LoadPolicies.Add(policy);
        }

        private void ShowSubtitleMessageFIXED(uint time, string str, params string[] args)
        {
            // This should've been done like that from the beginning but of course... i've done it differently
            ShowSubtitleMessage(string.Format(str, args), time);
        }
        #endregion

        #region Functions
        /// <summary>
        /// Will check the "Groups" folder for available groups and adds them to a list of available groups.
        /// </summary>
        /// <returns><see langword="true"/> if there are any groups. Otherwise, <see langword="false"/>.</returns>
        private bool CheckAvailableSpawnGroups()
        {
            try
            {
                // Clear list from previous available groups
                if (availableSpawnGroupFiles != null)
                    availableSpawnGroupFiles.Clear();

                // Get all available groups
                string[] files = Directory.GetFiles(ScriptResourceFolder + "\\Groups", "*.json");

                if (files.Length == 0)
                {
                    Logging.LogWarning("There are no spawn groups available. The mod will abort now. Redownload if neccessary.");
                    return false;
                }

                // Create new list and initialize it with the size of the available groups count
                availableSpawnGroupFiles = null;
                availableSpawnGroupFiles = new List<string>(files.Length);

                // Add each available group file to list
                for (int i = 0; i < files.Length; i++)
                {
                    string fileName = Path.GetFileName(files[i]);
                    availableSpawnGroupFiles.Add(fileName);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logging.LogError("Failed to check for available spawn group files. Details: {0}", ex);
            }

            return false;
        }

        /// <summary>
        /// Checks if any group is allowed to load.
        /// </summary>
        /// <returns><see langword="true"/> if groups can load. Otherwise, <see langword="false"/>.</returns>
        private bool CanGroupsLoad()
        {
            if (IVNetwork.IsNetworkSession())
                return false;
            if (IVCutsceneMgr.IsRunning())
                return false;
            if (IVStreaming.DisableStreaming)
                return false;

            return true;
        }

        /// <summary>
        /// Checks if there are any active groups.
        /// </summary>
        /// <returns><see langword="true"/> if there are. Otherwise, <see langword="false"/>.</returns>
        private bool AreAnyGroupsActive()
        {
            return loadedSpawnGroups.Any(x => x.IsGroupActive);
        }

        /// <summary>
        /// Finds all currently active <see cref="SpawnGroup"/> instances.
        /// </summary>
        /// <param name="includeGroupsWithNoPolicies">Setting this to <see langword="true"/> will also return all <see cref="SpawnGroup"/> instances which have no policies.</param>
        /// <returns>All the <see cref="SpawnGroup"/> instances found.</returns>
        private SpawnGroup[] FindAllActiveGroups(bool includeGroupsWithNoPolicies)
        {
            return loadedSpawnGroups.Where(x =>
            {

                if (x.Disabled)
                    return false;

                if (!x.IsGroupActive)
                    return false;

                if (x.LoadPolicies == null && !includeGroupsWithNoPolicies)
                    return false;

                //if (!x.AreAllModelsLoaded() || x.WasUnloadRequested)
                //    return false;

                return true;

            }).ToArray();
        }

        /// <summary>
        /// Tries to find a <see cref="SpawnGroup"/> instance by its <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="SpawnGroup"/> to find.</param>
        /// <returns>If found, the <see cref="SpawnGroup"/> is returned. Otherwise, <see langword="null"/>.</returns>
        private SpawnGroup FindGroupByName(string name)
        {
            return loadedSpawnGroups.Where(x => x.GroupName == name).FirstOrDefault();
        }

        /// <summary>
        /// Tries to find multiple <see cref="SpawnGroup"/> instances by their target <paramref name="area"/>.
        /// </summary>
        /// <param name="zone">The target zone to look for e.g. STEIN for Steinway.</param>
        /// <param name="nonEqualCheck">If set to <see langword="true"/>, this returns all <see cref="SpawnGroup"/> instances which are NOT equal to the given <paramref name="area"/>.</param>
        /// <param name="hasToBeActive">Only return <see cref="SpawnGroup"/> instances which are currently flagged as active.</param>
        /// <param name="includeGlobalGroups">Also returns <see cref="SpawnGroup"/> instances which target islands and neighborhoods is set to <see langword="null"/> (Which means this group is active everywhere).</param>
        /// <returns>All the <see cref="SpawnGroup"/> instances found.</returns>
        private SpawnGroup[] FindGroupsByArea(string zone, bool nonEqualCheck, bool hasToBeActive, bool includeGlobalGroups = false)
        {
            return loadedSpawnGroups.Where(x =>
            {

                if (x.Disabled)
                    return false;

                if (hasToBeActive && !x.IsGroupActive)
                    return false;

                if (x.TargetIslands == null && x.TargetNeighborhoods == null)
                    return includeGlobalGroups;

                // Check if specified islands match
                if (x.TargetIslands != null)
                {
                    int matches = 0;

                    // Go through all specified areas
                    for (int i = 0; i < x.TargetIslands.Count; i++)
                    {
                        string island = x.TargetIslands[i];

                        // Try get the island this zone belongs to
                        if (zoneToIslandDict.TryGetValue(zone, out string foundIsland))
                        {
                            // Check if islands match or not based on the flags
                            if (nonEqualCheck)
                            {
                                if (island != foundIsland)
                                {
                                    Logging.LogDebug("Target Island '{0}' matches found Island '{1}' for zone '{2}' of group '{3}'!", island, foundIsland, zone, x.GroupName);

                                    // Match!
                                    matches++;
                                }
                                else
                                {
                                    Logging.LogDebug("Found Island '{0}' DOES NOT match target Island '{1}' for zone '{2}' of group '{3}'!", foundIsland, island, zone, x.GroupName);
                                }
                            }
                            else
                            {
                                if (island == foundIsland)
                                {
                                    Logging.LogDebug("Target Island '{0}' matches found Island '{1}' for zone '{2}' of group '{3}'!", island, foundIsland, zone, x.GroupName);

                                    // Match!
                                    matches++;
                                }
                                else
                                {
                                    Logging.LogDebug("Found Island '{0}' DOES NOT match target Island '{1}' for zone '{2}' of group '{3}'!", foundIsland, island, zone, x.GroupName);
                                }
                            }

                        }
                        else
                        {
                            Logging.LogDebug("Target Island '{0}' NOT FOUND for zone '{1}' of group '{2}'!", island, zone, x.GroupName);
                        }
                    }

                    // If no matching island for area found then return otherwise continue with area check
                    if (matches == 0)
                        return false;
                }

                // Check if specified neighborhoods match
                if (x.TargetNeighborhoods != null)
                {
                    string actualZoneName = GET_STRING_FROM_TEXT_FILE(zone);

                    if (nonEqualCheck)
                        return !x.TargetNeighborhoods.Contains(actualZoneName);
                    else
                        return x.TargetNeighborhoods.Contains(actualZoneName);
                }
                else
                {
                    return true;
                }

            }).ToArray();
        }
        #endregion

        private void Main_Uninitialize(object sender, EventArgs e)
        {
            if (loadedSpawnGroups == null)
                return;

            // Unload all groups only if IV-SDK .NET is not about to shut down
            if (!CLR.CLRBridge.IsShuttingDown)
            {
                if (ModSettings.UnloadAllGroupsWhenModUnloads)
                    loadedSpawnGroups.ForEach(x => x.UnloadModels());
            }

            loadedSpawnGroups.Clear();
            loadedSpawnGroups = null;
        }
        private void Main_Initialized(object sender, EventArgs e)
        {
            // Check and add available spawn groups
            if (!CheckAvailableSpawnGroups())
            {
                Abort();
                return;
            }

            // Load settings
            ModSettings.Load(Settings);

            // Set the vehicle budget
            SetVehicleBudget(ModSettings.VehicleBudgetOverride);

            // If allowed, try to automatically determine vehicle budget value
            if (ModSettings.AutomaticallyDetermineVehicleBudget)
                AutomaticallyDetermineVehicleBudget();
        }

        private void Main_OnImGuiRendering(IntPtr devicePtr, ImGuiIV_DrawingContext ctx)
        {
            if (!MenuOpened)
                return;

            if (ImGuiIV.Begin("Simple Traffic Loader", ref MenuOpened))
            {
                if (ImGuiIV.BeginTabBar("SimpleTrafficLoaderTabBar"))
                {
                    DebugTabItem();
                    GroupsTabItem();
                }
                ImGuiIV.EndTabBar();
            }
            ImGuiIV.End();
        }
        private void DebugTabItem()
        {
#if DEBUG
            if (ImGuiIV.BeginTabItem("DEBUG##SimpleTrafficLoaderTI"))
            {
                ImGuiIV.SeparatorText("Debugging");
                ImGuiIV.CheckBox("Enable Debug Logging", ref Logging.EnableDebugLogging);
                ImGuiIV.TextUnformatted("Current Day State: {0}", NativeWorld.GetDayState());

                ImGuiIV.Spacing();
                ImGuiIV.SeparatorText("Lists");
                ImGuiIV.TextUnformatted("Added Groups: {0}", loadedSpawnGroups.Count);

                ImGuiIV.Spacing();
                ImGuiIV.SeparatorText("Zones");
                ImGuiIV.TextUnformatted("Did Zone Update Occur: {0}", didZoneUpdateOccur);
                ImGuiIV.TextUnformatted("Last Zone Update: {0}", lastZoneUpdate);
                ImGuiIV.TextUnformatted("Current Zone: {0} ({1})", currentZone, GET_STRING_FROM_TEXT_FILE(currentZone));
                ImGuiIV.TextUnformatted("Previous Zone: {0} ({1})", previousZone, GET_STRING_FROM_TEXT_FILE(previousZone));

                ImGuiIV.Spacing();
                ImGuiIV.SeparatorText("Budget");
                ImGuiIV.TextUnformatted("Current Vehicle Budget: {0} bytes {1}", IVStreaming.VehicleModelBudget, ModSettings.AutomaticallyDetermineVehicleBudget ? "(Automatically determined)" : "");

                ImGuiIV.EndTabItem();
            }
#endif
        }
        private void GroupsTabItem()
        {
            if (ImGuiIV.BeginTabItem("Groups##SimpleTrafficLoaderTI"))
            {
                ImGuiIV.SeparatorText("Details");
                ImGuiIV.TextUnformatted("Can Groups Load: {0}", CanGroupsLoad() ? "Yes" : "No");
                ImGuiIV.TextUnformatted("Loaded groups: {0}", loadedSpawnGroups.Count);

                ImGuiIV.Spacing();
                ImGuiIV.SeparatorText("Zones");
                string currentZoneActualName = GET_STRING_FROM_TEXT_FILE(currentZone);
                string previousZoneActualName = GET_STRING_FROM_TEXT_FILE(previousZone);

                ImGuiIV.TextUnformatted("Current Zone: {0}", currentZoneActualName);
                ImGuiIV.TextUnformatted("Previous Zone: {0}", previousZoneActualName == "NULL" ? "N/A" : previousZoneActualName);

                ImGuiIV.Spacing();
                ImGuiIV.SeparatorText("Control");
                ImGuiIV.CheckBox("Prevent Group Loading", ref preventGroupLoading);

                ImGuiIV.Spacing();
                ImGuiIV.SeparatorText("The Groups");

                ImGuiIV.TextUnformatted("Available groups to load");

                ImGuiIV.HelpMarker(string.Format("Lists all group files which are located within the 'Groups' directory.{0}" +
                    "Clicking on an item in this list will load the group from file.", Environment.NewLine));
                ImGuiIV.SameLine();
                if (ImGuiIV.BeginCombo("##SimpleTrafficLoaderAvailGroups", currentlyLoadedGroupFileName))
                {
                    for (int i = 0; i < availableSpawnGroupFiles.Count; i++)
                    {
                        string groupFileName = availableSpawnGroupFiles[i];

                        if (ImGuiIV.Selectable(groupFileName))
                            LoadGroupFromFile(groupFileName);
                    }

                    ImGuiIV.EndCombo();
                }

                ImGuiIV.SameLine();

                if (ImGuiIV.Button("Refresh"))
                    CheckAvailableSpawnGroups();

                if (ImGuiIV.Button("Save currently loaded group to file"))
                    SaveCurrentlyLoadedGroupToFile();

                ImGuiIV.Spacing(2);

                if (ImGuiIV.Button("Create new group"))
                    loadedSpawnGroups.Add(new SpawnGroup());

                ImGuiIV.Spacing(2);

                for (int i = 0; i < loadedSpawnGroups.Count; i++)
                {
                    SpawnGroup group = loadedSpawnGroups[i];

                    if (ImGuiIV.CollapsingHeader(string.Format("{0} (ID: {1})##SimpleTrafficLoaderCH", group.GroupName, group.ID)))
                    {

                        ImGuiIV.SeparatorText("Control");

                        ImGuiIV.PushStyleColor(eImGuiCol.Button, Color.FromArgb(255, 199, 53, 42));
                        ImGuiIV.PushStyleColor(eImGuiCol.ButtonActive, Color.FromArgb(255, 168, 45, 35));
                        ImGuiIV.PushStyleColor(eImGuiCol.ButtonHovered, Color.FromArgb(255, 255, 70, 56));
                        if (ImGuiIV.Button("Delete this group"))
                        {
                            loadedSpawnGroups.RemoveAt(i);
                            continue;
                        }
                        ImGuiIV.PopStyleColor(3);

                        ImGuiIV.HelpMarker("Makes the group unable to be loaded.");
                        ImGuiIV.SameLine();
                        ImGuiIV.CheckBox(string.Format("Disable##SimpleTrafficLoader{0}", group.GroupName), ref group.Disabled);

                        ImGuiIV.Spacing();
                        ImGuiIV.SeparatorText("Details");

                        ImGuiIV.InputText(string.Format("##SimpleTrafficLoader_{0}_Name", group.GroupName), ref group.GroupNameEdit);
                        ImGuiIV.SameLine();
                        if (ImGuiIV.Button("Set name"))
                        {
                            group.GroupName = group.GroupNameEdit;
                            group.GroupNameEdit = group.GroupName;
                        }

                        ImGuiIV.TextUnformatted("ID: {0}", group.ID);
                        ImGuiIV.TextUnformatted("IsGroupActive: {0}", group.IsGroupActive);
                        ImGuiIV.TextUnformatted("WasUnloadRequested: {0}", group.WasUnloadRequested);

                        ImGuiIV.HelpMarker(string.Format("This shows if all models of this group are currently loaded.{0}" +
                            "If another group happens to have the same models as this group, and they are loaded, this group would also say that its models are loaded.", Environment.NewLine));
                        ImGuiIV.SameLine();
                        ImGuiIV.TextUnformatted("AreAllModelsLoaded: {0}", group.AreAllModelsLoaded());
                        

                        ImGuiIV.Spacing();
                        ImGuiIV.SeparatorText("Lists");

                        // Target Islands
                        if (ImGuiIV.CollapsingHeader(string.Format("Target Islands##SimpleTrafficLoader_{0}_TICH", group.GroupName)))
                        {
                            if (ImGuiIV.Button("Add current island to list"))
                                AddIslandToGroup(group, currentZone);

                            if (group.TargetIslands != null)
                            {
                                if (ImGuiIV.Button("Remove selected island from list"))
                                {
                                    if (group.TargetIslands.Count != 0)
                                        group.TargetIslands.RemoveAt(selectedIslandListBoxIndex);

                                    selectedIslandListBoxIndex = 0;
                                }

                                ImGuiIV.Spacing(2);

                                if (ImGuiIV.BeginListBox(string.Format("Target Islands##SimpleTrafficLoaderTargetIslandsLB{0}", group.GroupName), new Vector2(ImGuiIV.FloatMin, 100f)))
                                {
                                    for (int m = 0; m < group.TargetIslands.Count; m++)
                                    {
                                        if (ImGuiIV.Selectable(group.TargetIslands[m], m == selectedIslandListBoxIndex))
                                            selectedIslandListBoxIndex = m;
                                    }
                                    ImGuiIV.EndListBox();
                                }
                            }
                            else
                            {
                                ImGuiIV.Spacing();
                                ImGuiIV.TextDisabled("This group has no specific islands specified.");
                            }

                            ImGuiIV.Spacing(2);
                        }

                        // Target Neighborhoods
                        if (ImGuiIV.CollapsingHeader(string.Format("Target Neighborhoods##SimpleTrafficLoader_{0}_TNCH", group.GroupName)))
                        {
                            if (ImGuiIV.Button("Add current neighborhood to list"))
                                AddNeighborhoodToGroup(group, currentZone);

                            if (group.TargetNeighborhoods != null)
                            {
                                if (ImGuiIV.Button("Remove selected neighborhood from list"))
                                {
                                    if (group.TargetNeighborhoods.Count != 0)
                                        group.TargetNeighborhoods.RemoveAt(selectedNeighborhoodListBoxIndex);

                                    selectedNeighborhoodListBoxIndex = 0;
                                }

                                ImGuiIV.Spacing(2);

                                if (ImGuiIV.BeginListBox(string.Format("Target Neighborhoods##SimpleTrafficLoaderTargetNeighborhoodsLB{0}", group.GroupName), new Vector2(ImGuiIV.FloatMin, 100f)))
                                {
                                    for (int m = 0; m < group.TargetNeighborhoods.Count; m++)
                                    {
                                        if (ImGuiIV.Selectable(group.TargetNeighborhoods[m], m == selectedNeighborhoodListBoxIndex))
                                            selectedNeighborhoodListBoxIndex = m;
                                    }
                                    ImGuiIV.EndListBox();
                                }
                            }
                            else
                            {
                                ImGuiIV.Spacing();
                                ImGuiIV.TextDisabled("This group has no specific neighborhoods specified.");
                            }

                            ImGuiIV.Spacing(2);
                        }

                        // Models to load
                        if (ImGuiIV.CollapsingHeader(string.Format("Models to load##SimpleTrafficLoader_{0}_MDLSCH", group.GroupName)))
                        {
                            ImGuiIV.HelpMarker(string.Format("Enter the model of the vehicle and press the button to add the model to the list.{0}" +
                                "Leave the textbox empty to add the model of the current vehicle the player is in to the list.", Environment.NewLine));
                            ImGuiIV.SameLine();

                            ImGuiIV.InputText("Model name##SimpleTrafficLoader", ref modelToAdd);

                            if (ImGuiIV.Button("Add model to list"))
                            {
                                AddVehicleModelToGroup(group, modelToAdd);
                                modelToAdd = null;
                            }

                            if (group.ModelsToLoad != null)
                            {
                                if (ImGuiIV.Button("Remove selected model from list"))
                                {
                                    if (group.ModelsToLoad.Count != 0)
                                        group.ModelsToLoad.RemoveAt(selectedModelListBoxIndex);

                                    selectedModelListBoxIndex = 0;
                                }

                                ImGuiIV.Spacing(2);

                                //ImGuiIV.HelpMarker("Items which got a little circle in front of their name, means that this model is currently loaded.");
                                if (ImGuiIV.BeginListBox(string.Format("Models to load##SimpleTrafficLoaderModelsLB{0}", group.GroupName), new Vector2(ImGuiIV.FloatMin, 100f)))
                                {
                                    for (int m = 0; m < group.ModelsToLoad.Count; m++)
                                    {
                                        string model = group.ModelsToLoad[m];

                                        //if (HAS_MODEL_LOADED((int)RAGE.AtStringHash(model)))
                                        //{
                                        //    ImGuiIV.Bullet();
                                        //    ImGuiIV.SameLine();
                                        //}

                                        if (ImGuiIV.Selectable(model, m == selectedModelListBoxIndex))
                                            selectedModelListBoxIndex = m;
                                    }
                                    ImGuiIV.EndListBox();
                                }
                            }
                            else
                            {
                                ImGuiIV.Spacing();
                                ImGuiIV.TextDisabled("This group has no models to load specified.");
                            }

                            ImGuiIV.Spacing(2);
                        }

                        // Policies
                        if (ImGuiIV.CollapsingHeader(string.Format("Policies##SimpleTrafficLoader_{0}_PLCSSCH", group.GroupName)))
                        {
                            ImGuiIV.Combo("Policies", ref selectedPolicyComboIndex, policiesArr);

                            if (ImGuiIV.Button("Add policy to list"))
                                AddPolicyToGroup(group, policiesArr[selectedPolicyComboIndex]);

                            if (group.LoadPolicies != null)
                            {
                                if (ImGuiIV.Button("Remove selected policy from list"))
                                {
                                    if (group.LoadPolicies.Count != 0)
                                        group.LoadPolicies.RemoveAt(selectedPolicyListBoxIndex);

                                    selectedPolicyListBoxIndex = 0;
                                }

                                ImGuiIV.Spacing(2);

                                if (ImGuiIV.BeginListBox(string.Format("Load Policies##SimpleTrafficLoaderPoliciesLB{0}", group.GroupName), new Vector2(ImGuiIV.FloatMin, 100f)))
                                {
                                    for (int p = 0; p < group.LoadPolicies.Count; p++)
                                    {
                                        if (ImGuiIV.Selectable(group.LoadPolicies[p], p == selectedPolicyListBoxIndex))
                                            selectedPolicyListBoxIndex = p;
                                    }
                                    ImGuiIV.EndListBox();
                                }
                            }
                            else
                            {
                                ImGuiIV.Spacing();
                                ImGuiIV.TextDisabled("This group has no policies specified.");
                            }
                        }

                        ImGuiIV.Spacing(3);
                    }
                }

                ImGuiIV.EndTabItem();
            }
            
        }

        private void Main_WaitTick(object sender, EventArgs e)
        {
            if (CanGroupsLoad())
            {
                // Checks if all active groups still meet their policies
                VerifyActiveGroupPolicies();

                // Load models of active groups one-by-one
                SpawnGroup[] groups = FindAllActiveGroups(true);

                for (int i = 0; i < groups.Length; i++)
                {
                    SpawnGroup group = groups[i];
                    group.LoadModels();
                }
            }
            else
            {
                if (AreAnyGroupsActive())
                    TellAllGroupsToUnload();
            }
        }
        private void Main_Tick(object sender, EventArgs e)
        {
            if (!CanGroupsLoad())
                return;

            // Load groups if not loaded yet
            if (loadedSpawnGroups == null)
                LoadGroupFromFile(ModSettings.LoadGroupByDefault);

            // Get player stuff
            int playerIndex = CONVERT_INT_TO_PLAYERINDEX(GET_PLAYER_ID());
            GET_PLAYER_CHAR(playerIndex, out playerPed);
            GET_CHAR_COORDINATES(playerPed, out Vector3 playerPos);

            // Get current zone player is in
            string rawZone = NativeWorld.GetZoneName(playerPos);

            // Check if zone changed
            if (currentZone != rawZone)
            {
                if (string.IsNullOrEmpty(previousZone))
                    previousZone = rawZone;

                if (currentZone != previousZone)
                {
                    // Set previous zone
                    previousZone = currentZone;

                    didZoneUpdateOccur = false;
                }

                // Set current zone
                currentZone = rawZone;

                if (!didZoneUpdateOccur)
                {

                    // Unload groups which are not set to the current zone
                    UnloadGroups(currentZone);

                    // Load groups for current zone
                    if (!preventGroupLoading)
                        LoadGroups(currentZone);

                    didZoneUpdateOccur = true;
                    lastZoneUpdate = DateTime.Now.TimeOfDay;
                }
            }
        }

    }
}
