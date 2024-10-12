// Simple Traffic Loader ASI Version
// Version 1.0 made by ItsClonkAndre

#include "ZHookBase.cpp"

#pragma region Variables
std::vector<std::string> models;
std::vector<std::string> requestedModels;
int tickTemp;
#pragma endregion

#pragma region Helper
inline void LoadModel(Hash model) {
	if (!HAS_MODEL_LOADED(model)) {
		REQUEST_MODEL(model);
		LOAD_ALL_OBJECTS_NOW();
	}
}
inline bool DoesFileExists(const std::string& name) {
	struct stat buffer;
	return (stat(name.c_str(), &buffer) == 0);
}
#pragma endregion

void scriptLoad() {
	std::string filename = "SimpleTrafficLoader.txt";
	if (DoesFileExists(filename)) {
		std::ifstream vehicleModelsFile(filename);
		if (vehicleModelsFile.is_open()) {
			std::string str;
			for (std::string line; std::getline(vehicleModelsFile, line); ) {
				models.push_back(line);
			}
		}
	}
}

void scriptUnload() {
	models.clear();
	requestedModels.clear();
}

void scriptTick() {
	if (models.size() != 0) {
		tickTemp++;
		if (tickTemp % 18 == 0) {
			uint rnd;
			GENERATE_RANDOM_INT_IN_RANGE(0, models.size(), &rnd);

			std::string modelName = models[rnd];
			Hash model = GET_HASH_KEY(modelName.c_str());
			LoadModel(model);

			tickTemp = 0;
		}
	}
}