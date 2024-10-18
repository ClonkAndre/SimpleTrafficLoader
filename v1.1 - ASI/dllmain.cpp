// Simple Traffic Loader ASI Version
// Version 1.1 made by ItsClonkAndre

#include "ZHookBase.cpp"
#include "INI.h"

#pragma region Compiler stuff
bool DoesListContainString(std::vector<std::string>, std::string);
bool CanStringBeAddedToList(std::string);
#pragma endregion

#pragma region Enums and Variables
// Enums
enum class Zones {
    None = 0,
    Alderney = 1,
    HappinessIsland = 2,
    Algonquin = 3,
    ChargeIsland = 4,
    ColonyIsland = 5,
    Bohan = 6,
    Dukes = 7,
    Broker = 8,
    LibertyCity = 9
};

// Variables
Player player;
Ped playerPed;
float pX, pY, pZ;
int tickTemp;
Zones currentZone = Zones::None;

std::vector<std::string> modelsThatCanBeUnloaded;
std::vector<std::string> modelsEverywhere;
std::vector<std::string> modelsBohan;
std::vector<std::string> modelsDukes;
std::vector<std::string> modelsBroker;
std::vector<std::string> modelsAlgonquin;
std::vector<std::string> modelsAlderney;
std::vector<std::string> modelsChargeIsland;
std::vector<std::string> modelsColonyIsland;

std::map<std::string, Zones> GTAZoneNamesLocations{
    {"WESDY", Zones::Alderney},
    {"LEFWO", Zones::Alderney},
    {"ALDCI", Zones::Alderney},
    {"BERCH", Zones::Alderney},
    {"NORMY", Zones::Alderney},
    {"ACTRR", Zones::Alderney},
    {"PORTU", Zones::Alderney},
    {"TUDOR", Zones::Alderney},
    {"ACTIP", Zones::Alderney},
    {"ALSCF", Zones::Alderney},
    {"HAPIN", Zones::HappinessIsland},
    {"CASGR", Zones::Algonquin},
    {"THXCH", Zones::Algonquin},
    {"FISSO", Zones::Algonquin},
    {"CHITO", Zones::Algonquin},
    {"CITH", Zones::Algonquin},
    {"CASGC", Zones::Algonquin},
    {"SUFFO", Zones::Algonquin},
    {"LITAL", Zones::Algonquin},
    {"LOWEA", Zones::Algonquin},
    {"FISSN", Zones::Algonquin},
    {"THPRES", Zones::Algonquin},
    {"EASON", Zones::Algonquin},
    {"THTRI", Zones::Algonquin},
    {"TMEQU", Zones::Algonquin},
    {"WESMI", Zones::Algonquin},
    {"STARJ", Zones::Algonquin},
    {"LANCE", Zones::Algonquin},
    {"HATGA", Zones::Algonquin},
    {"PUGAT", Zones::Algonquin},
    {"MIDPW", Zones::Algonquin},
    {"MIDPA", Zones::Algonquin},
    {"MIDPE", Zones::Algonquin},
    {"LANCA", Zones::Algonquin},
    {"VASIH", Zones::Algonquin},
    {"NOHOL", Zones::Algonquin},
    {"EAHOL", Zones::Algonquin},
    {"NORWO", Zones::Algonquin},
    {"CHISL", Zones::ChargeIsland},
    {"COISL", Zones::ColonyIsland},
    {"STHBO", Zones::Bohan},
    {"CHAPO", Zones::Bohan},
    {"FORSI", Zones::Bohan},
    {"BOULE", Zones::Bohan},
    {"NRTGA", Zones::Bohan},
    {"INSTI", Zones::Bohan},
    {"LTBAY", Zones::Bohan},
    {"STEIN", Zones::Dukes},
    {"MEADP", Zones::Dukes},
    {"FRANI", Zones::Dukes},
    {"WILLI", Zones::Dukes},
    {"MEADH", Zones::Dukes},
    {"EISLC", Zones::Dukes},
    {"BOAB", Zones::Dukes},
    {"CERHE", Zones::Dukes},
    {"BEECW", Zones::Dukes},
    {"SCHOL", Zones::Broker},
    {"DOWTW", Zones::Broker},
    {"ROTTH", Zones::Broker},
    {"ESHOO", Zones::Broker},
    {"OUTL", Zones::Broker},
    {"SUTHS", Zones::Broker},
    {"HOBEH", Zones::Broker},
    {"FIREP", Zones::Broker},
    {"FIISL", Zones::Broker},
    {"BEGGA", Zones::Broker},
    {"BRALG", Zones::LibertyCity},
    {"BRBRO", Zones::LibertyCity},
    {"BREBB", Zones::LibertyCity},
    {"BRDBB", Zones::LibertyCity},
    {"NOWOB", Zones::LibertyCity},
    {"HIBRG", Zones::LibertyCity},
    {"LEAPE", Zones::LibertyCity},
    {"BOTU", Zones::LibertyCity},
    {"LIBERTY", Zones::LibertyCity}
};
#pragma endregion

#pragma region Methods
inline void LoadModel(std::string modelName) {
    Hash model = GET_HASH_KEY(modelName.c_str());
	if (!HAS_MODEL_LOADED(model)) {
		REQUEST_MODEL(model);
		LOAD_ALL_OBJECTS_NOW();
	}
}
inline void UnloadModel(std::string modelName) {
    Hash model = GET_HASH_KEY(modelName.c_str());
    if (HAS_MODEL_LOADED(model)) {
        MARK_MODEL_AS_NO_LONGER_NEEDED(model);
    }
}

inline void AddModelsToUnloadedList(std::vector<std::string> originalList) {
    for (int i = 0; i < originalList.size(); i++) {
        std::string str = originalList[i];
        if (!DoesListContainString(modelsThatCanBeUnloaded, str)) {
            if (CanStringBeAddedToList(str)) modelsThatCanBeUnloaded.push_back(str);
        }
    }
}
inline void UnloadEveryModelFromList(Zones except) {
    if (except != Zones::Alderney) AddModelsToUnloadedList(modelsAlderney);
    if (except != Zones::Algonquin) AddModelsToUnloadedList(modelsAlgonquin);
    if (except != Zones::ChargeIsland) AddModelsToUnloadedList(modelsChargeIsland);
    if (except != Zones::ColonyIsland) AddModelsToUnloadedList(modelsColonyIsland);
    if (except != Zones::Bohan) AddModelsToUnloadedList(modelsBohan);
    if (except != Zones::Dukes) AddModelsToUnloadedList(modelsDukes);
    if (except != Zones::Broker) AddModelsToUnloadedList(modelsBroker);
}
#pragma endregion

#pragma region Functions
bool DoesListContainString(std::vector<std::string> list, std::string str) {
    for (int i = 0; i < list.size(); i++) {
        if (list[i] == str) return true;
    }
    return false;
}

bool CanStringBeAddedToList(std::string str) {
    bool bohan = DoesListContainString(modelsBohan, str);
    bool dukes = DoesListContainString(modelsDukes, str);
    bool broker = DoesListContainString(modelsBroker, str);
    bool algonquin = DoesListContainString(modelsAlgonquin, str);
    bool alderney = DoesListContainString(modelsAlderney, str);
    bool chargeIsland = DoesListContainString(modelsChargeIsland, str);
    bool colonyIsland = DoesListContainString(modelsColonyIsland, str);

    bool result = true;

    if (bohan) {
        if (currentZone == Zones::Bohan) {
            result = false;
        }
    }
    if (dukes) {
        if (currentZone == Zones::Dukes) {
            result = false;
        }
    }
    if (broker) {
        if (currentZone == Zones::Broker) {
            result = false;
        }
    }
    if (algonquin) {
        if (currentZone == Zones::Algonquin) {
            result = false;
        }
    }
    if (alderney) {
        if (currentZone == Zones::Alderney) {
            result = false;
        }
    }
    if (chargeIsland) {
        if (currentZone == Zones::ChargeIsland) {
            result = false;
        }
    }
    if (colonyIsland) {
        if (currentZone == Zones::ColonyIsland) {
            result = false;
        }
    }

    return result;
}
#pragma endregion


void scriptLoad() {
    INI<> ini("SimpleTrafficLoader.ini", true);

    if (ini.select("Everywhere")) {
        int modelsCount = std::stoi(ini.get("ModelsCount", "0"));
        if (modelsCount != 0) {
            for (int i = 0; i < modelsCount; i++) {
                std::string model = ini.get(std::to_string(i));
                if (model != "")
                    modelsEverywhere.push_back(model);
            }
        }
    }
    if (ini.select("Bohan")) {
        int modelsCount = std::stoi(ini.get("ModelsCount", "0"));
        if (modelsCount != 0) {
            for (int i = 0; i < modelsCount; i++) {
                std::string model = ini.get(std::to_string(i));
                if (model != "")
                    modelsBohan.push_back(model);
            }
        }
    }
    if (ini.select("Dukes")) {
        int modelsCount = std::stoi(ini.get("ModelsCount", "0"));
        if (modelsCount != 0) {
            for (int i = 0; i < modelsCount; i++) {
                std::string model = ini.get(std::to_string(i));
                if (model != "")
                    modelsDukes.push_back(model);
            }
        }
    }
    if (ini.select("Broker")) {
        int modelsCount = std::stoi(ini.get("ModelsCount", "0"));
        if (modelsCount != 0) {
            for (int i = 0; i < modelsCount; i++) {
                std::string model = ini.get(std::to_string(i));
                if (model != "")
                    modelsBroker.push_back(model);
            }
        }
    }
    if (ini.select("Algonquin")) {
        int modelsCount = std::stoi(ini.get("ModelsCount", "0"));
        if (modelsCount != 0) {
            for (int i = 0; i < modelsCount; i++) {
                std::string model = ini.get(std::to_string(i));
                if (model != "")
                    modelsAlgonquin.push_back(model);
            }
        }
    }
    if (ini.select("Alderney")) {
        int modelsCount = std::stoi(ini.get("ModelsCount", "0"));
        if (modelsCount != 0) {
            for (int i = 0; i < modelsCount; i++) {
                std::string model = ini.get(std::to_string(i));
                if (model != "")
                    modelsAlderney.push_back(model);
            }
        }
    }
    if (ini.select("ChargeIsland")) {
        int modelsCount = std::stoi(ini.get("ModelsCount", "0"));
        if (modelsCount != 0) {
            for (int i = 0; i < modelsCount; i++) {
                std::string model = ini.get(std::to_string(i));
                if (model != "")
                    modelsChargeIsland.push_back(model);
            }
        }
    }
    if (ini.select("ColonyIsland")) {
        int modelsCount = std::stoi(ini.get("ModelsCount", "0"));
        if (modelsCount != 0) {
            for (int i = 0; i < modelsCount; i++) {
                std::string model = ini.get(std::to_string(i));
                if (model != "")
                    modelsColonyIsland.push_back(model);
            }
        }
    }
}

void scriptUnload() {
    modelsEverywhere.clear();
    modelsBohan.clear();
    modelsDukes.clear();
    modelsBroker.clear();
    modelsAlgonquin.clear();
    modelsAlderney.clear();
    modelsChargeIsland.clear();
    modelsColonyIsland.clear();
}

void scriptTick() {
    player = Scripting::CONVERT_INT_TO_PLAYERINDEX(Scripting::GET_PLAYER_ID());
    GET_PLAYER_CHAR(player, &playerPed);
    GET_CHAR_COORDINATES(playerPed, &pX, &pY, &pZ);

    const char* rawZoneName = GET_NAME_OF_ZONE(pX, pY, pZ);
    currentZone = GTAZoneNamesLocations.find(rawZoneName)->second;

    tickTemp++;
    if (tickTemp % 18 == 0) {
        uint rnd;
        std::string modelName;

        // Unload models if there are any
        if (modelsThatCanBeUnloaded.size() != 0) {
            for (int i = 0; i < modelsThatCanBeUnloaded.size(); i++) {
                UnloadModel(modelsThatCanBeUnloaded[i]);
            }
            modelsThatCanBeUnloaded.clear();
        }

        // Load models that should only be loaded in a specific zone
        switch (currentZone) {
            case Zones::Alderney:
                if (modelsAlderney.size() != 0) {
                    GENERATE_RANDOM_INT_IN_RANGE(0, modelsAlderney.size(), &rnd);
                    modelName = modelsAlderney[rnd];
                    LoadModel(modelName);
                }
                break;
            case Zones::Algonquin:
                if (modelsAlgonquin.size() != 0) {
                    GENERATE_RANDOM_INT_IN_RANGE(0, modelsAlgonquin.size(), &rnd);
                    modelName = modelsAlgonquin[rnd];
                    LoadModel(modelName);
                }
                break;
            case Zones::ChargeIsland:
                if (modelsChargeIsland.size() != 0) {
                    GENERATE_RANDOM_INT_IN_RANGE(0, modelsChargeIsland.size(), &rnd);
                    modelName = modelsChargeIsland[rnd];
                    LoadModel(modelName);
                }
                break;
            case Zones::ColonyIsland:
                if (modelsColonyIsland.size() != 0) {
                    GENERATE_RANDOM_INT_IN_RANGE(0, modelsColonyIsland.size(), &rnd);
                    modelName = modelsColonyIsland[rnd];
                    LoadModel(modelName);
                }
                break;
            case Zones::Bohan:
                if (modelsBohan.size() != 0) {
                    GENERATE_RANDOM_INT_IN_RANGE(0, modelsBohan.size(), &rnd);
                    modelName = modelsBohan[rnd];
                    LoadModel(modelName);
                }
                break;
            case Zones::Dukes:
                if (modelsDukes.size() != 0) {
                    GENERATE_RANDOM_INT_IN_RANGE(0, modelsDukes.size(), &rnd);
                    modelName = modelsDukes[rnd];
                    LoadModel(modelName);
                }
                break;
            case Zones::Broker:
                if (modelsBroker.size() != 0) {
                    GENERATE_RANDOM_INT_IN_RANGE(0, modelsBroker.size(), &rnd);
                    modelName = modelsBroker[rnd];
                    LoadModel(modelName);
                }
                break;
        }

        // Load models that should be loaded everywhere
        if (currentZone != Zones::LibertyCity || currentZone != Zones::HappinessIsland) {
            if (modelsEverywhere.size() != 0) {
                GENERATE_RANDOM_INT_IN_RANGE(0, modelsEverywhere.size(), &rnd);
                modelName = modelsEverywhere[rnd];
                LoadModel(modelName);
            }
        }

        // Set lists to be unloaded except list from current zone
        UnloadEveryModelFromList(currentZone);

        tickTemp = 0;
    }
}