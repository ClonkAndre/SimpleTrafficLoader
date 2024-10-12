using System;
using System.Collections.Generic;
using System.Linq;

using CCL.GTAIV;

using IVSDKDotNet;
using IVSDKDotNet.Attributes;
using static IVSDKDotNet.Native.Natives;

namespace SimpleTrafficLoader.Classes
{
    internal class SpawnGroup
    {

        #region Variables
        // Details
        [ExcludeFromJsonSerialization()] public Guid ID;
        public string GroupName;
        [ExcludeFromJsonSerialization()] public string GroupNameEdit;

        // Control
        public bool Disabled;
        [ExcludeFromJsonSerialization()] public bool IsGroupActive;
        [ExcludeFromJsonSerialization()] public bool WasUnloadRequested;

        // Lists
        public List<string> TargetIslands;
        public List<string> TargetNeighborhoods;
        public List<string> ModelsToLoad;
        public List<string> LoadPolicies;
        [ExcludeFromJsonSerialization()] private List<NativeModel> theNativeModels;
        #endregion

        #region Constructor
        public SpawnGroup()
        {
            theNativeModels = new List<NativeModel>();
        }
        #endregion

        #region Methods
        public void Prepare()
        {
            // Generate random ID for this group
            ID = Guid.NewGuid();

            GroupNameEdit = GroupName;

            // Check and add models that should be loaded in a seperate list
            for (int i = 0; i < ModelsToLoad.Count; i++)
            {
                string model = ModelsToLoad[i];

                int modelHash = 0;

                // Check if model name was given or the model hash
                if (!int.TryParse(model, out modelHash))
                {
                    modelHash = (int)RAGE.AtStringHash(model);
                }

                // Check stuff
                if (modelHash == 0)
                    continue;
                if (!IS_MODEL_IN_CDIMAGE(modelHash))
                    continue;

                // Add model to list
                theNativeModels.Add(new NativeModel(modelHash));
            }
        }

        public void LoadModels()
        {
            if (!CanDoStuff())
                return;

            // If group is unloading then prevent any models of this group from loading
            if (WasUnloadRequested)
                return;

            // If models are already loaded then return
            if (AreAllModelsLoaded())
                return;

            Logging.LogDebug("- - - About to LOAD models of group '{0}'! - - -", GroupName);

            // Load models
            for (int i = 0; i < theNativeModels.Count; i++)
            {
                NativeModel model = theNativeModels[i];

                // If model is already loaded then skip
                if (model.IsInMemory)
                    continue;

                // Get model hash
                int modelHash = (int)model.Hash;

                // Request/Load this model
                if (!ModSettings.ForceLoadModels)
                {
                    // Load models one-by-one
                    IVStreaming.ScriptRequestModel(modelHash);
                    IVStreaming.LoadAllRequestedModels(false);
                    return;
                }
                else
                {
                    // Request model to be loaded instantly
                    IVStreaming.ScriptRequestModel(modelHash);
                }
            }

            // Load all requested models instantly
            IVStreaming.LoadAllRequestedModels(false);
        }
        public void UnloadModels()
        {
            if (!CanDoStuff())
                return;

            // If models are already unloaded then return
            if (!AreAllModelsLoaded())
                return;

            // If unload was already requested then return
            if (WasUnloadRequested)
                return;

            WasUnloadRequested = true;

            Logging.LogDebug("- - - About to UNLOAD group '{0}'! - - -", GroupName);

            // Mark loaded models as no longer needed
            // This might take a few seconds for them to actually get unloaded
            for (int i = 0; i < theNativeModels.Count; i++)
            {
                NativeModel model = theNativeModels[i];

                // If model is already unloaded then skip
                if (!model.IsInMemory)
                    continue;

                model.MarkAsNoLongerNeeded();
            }

            IsGroupActive = false;
            WasUnloadRequested = false;
        }
        #endregion

        #region Functions
        public bool AreAllModelsLoaded()
        {
            if (theNativeModels.Count == 0)
                return false;

            return theNativeModels.All(x => x.IsInMemory);
        }
        public bool CanDoStuff()
        {
            return IsGroupActive && theNativeModels.Count != 0;
        }
        #endregion

    }
}
