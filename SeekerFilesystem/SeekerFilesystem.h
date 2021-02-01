#pragma once

#include <oaidl.h>
#include <vector>
#include <string>

enum CriteriaType
{
    EXCLUDE = 1 << 0,
    FLIPFLOP = 1 << 1,
    SINGLE = 1 << 2
};

typedef struct CriteriaInfo
{
    CriteriaType type;
    char* keyphrase;
};

typedef struct SearchParameters
{
    int criteriaCount;
    char* rootDirectory;
    bool isRecursive;
};

typedef struct SearchResults
{
    int count;
    SAFEARRAY* entries;
};

extern "C"
{
    __declspec(dllexport) void __stdcall FindFiles(const CriteriaInfo* criteria, const SearchParameters searchParameters, SearchResults& outResults);
    __declspec(dllexport) void __stdcall GetSubdirectories(const char* directory, SearchResults& outReults);
}

SAFEARRAY* GetNativeToManagedStringArray(std::vector<std::string>* stringVector);
void FindFilesRR(const CriteriaInfo* criteria, const SearchParameters searchParameters, const int requiredCriteriaThreshold, std::vector<std::string>& outResults, int& outResultCount);
