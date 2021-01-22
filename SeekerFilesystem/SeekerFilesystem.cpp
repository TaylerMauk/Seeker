#include "pch.h"
#include "SeekerFilesystem.h"

#include <filesystem>
#include <iostream>

namespace fs = std::filesystem;

__declspec(dllexport) void __stdcall FindFiles(const CriteriaInfo* criteria, const SearchParameters searchParameters, SearchResults& outResults)
{
    std::vector<std::string> resultFiles;

    CriteriaInfo* pSearchCriteriaL = new CriteriaInfo[searchParameters.criteriaCount];

    int flipflopCriteriaCount = 0;
    int singleCriteriaCount = 0;
    size_t tmpLength;
    for (int i = 0; i < searchParameters.criteriaCount; ++i)
    {
        // Enumerate number of FLIPFLOP and SINGLE criteria
        if (criteria[i].type & CriteriaType::FLIPFLOP)
            ++flipflopCriteriaCount;
        else if (criteria[i].type & CriteriaType::SINGLE)
            ++singleCriteriaCount;

        // Allow for case-insensitivity by normalizing to upper
        std::string tmp(criteria[i].keyphrase);
        for (auto& c : tmp)
            c = std::toupper(c);

        tmpLength = tmp.length();
        pSearchCriteriaL[i].keyphrase = new char[tmpLength + 1];
        strncpy_s(pSearchCriteriaL[i].keyphrase, tmpLength + 1, tmp.c_str(), tmpLength);
        pSearchCriteriaL[i].type = criteria[i].type;
    }

    // Save candidate threshold of required criteria counts
    int requiredCriteriaThreshold = (flipflopCriteriaCount / 2) + singleCriteriaCount;

    try
    {
        FindFilesRR(pSearchCriteriaL, searchParameters, requiredCriteriaThreshold, resultFiles, outResults.count);
        outResults.entries = GetNativeToManagedStringArray(&resultFiles);
    }
    catch (const std::exception& e)
    {
        std::cout << "FindFiles: " << e.what() << std::endl;
    }

    delete[] pSearchCriteriaL;
}

__declspec(dllexport) void __stdcall GetSubdirectories(const char* directory, SearchResults& outResults)
{
    fs::directory_iterator dirIterator;
    try
    {
        dirIterator = fs::directory_iterator(directory);
    }
    catch (const std::exception& e)
    {
        std::cout << "GetSubdirectories (DIR enum): " << e.what() << std::endl;
        throw;
    }

    std::vector<std::string> subdirectories;
    for (auto& entry : dirIterator)
    {
        try
        {
            if (entry.is_directory())
            {
                subdirectories.push_back(entry.path().string());
                ++outResults.count;
            }
        }
        catch (const std::exception& e) {}
    }

    outResults.entries = GetNativeToManagedStringArray(&subdirectories);
}

SAFEARRAY* GetNativeToManagedStringArray(std::vector<std::string>* stringVector)
{
    size_t numberOfElements = stringVector->size();

    SAFEARRAYBOUND bound;
    bound.lLbound = 0;
    bound.cElements = numberOfElements;
    SAFEARRAY* returnArray = SafeArrayCreate(VT_BSTR, 1, &bound);

    BSTR* pData;
    HRESULT hr = SafeArrayAccessData(returnArray, (void**)&pData);
    if (SUCCEEDED(hr))
    {
        wchar_t* pWideString = nullptr;
        const char* pCurrentString;
        size_t stringLength;
        size_t outStringLength;

        for (size_t i = 0; i < numberOfElements; ++i)
        {
            pCurrentString = stringVector->at(i).c_str();
            stringLength = strlen(pCurrentString);
            pWideString = new wchar_t[stringLength + 1];

            mbstowcs_s(&outStringLength, pWideString, stringLength + 1, pCurrentString, stringLength);
            *pData++ = SysAllocString(pWideString);
        }

        SafeArrayUnaccessData(returnArray);
        delete[] pWideString;
    }
    else
    {
        std::cout << "GetNativeToManagedStringArray: Failed to access outResults" << std::endl;
        throw;
    }

    return returnArray;
}

void FindFilesRR(const CriteriaInfo* criteria, const SearchParameters searchParameters, const int requiredCriteriaThreshold, std::vector<std::string>& outResults, int& outResultCount)
{
    fs::directory_iterator dirIterator;
    try
    {
        dirIterator = fs::directory_iterator(searchParameters.rootDirectory);
    }
    catch (const std::exception& e)
    {
        return;
    }

    fs::path entryPath;
    std::string entryFileName;
    int criteriaMatched;
    bool isKeyphraseFound;
    SearchParameters recurseSearchParams{};
    for (auto& entry : dirIterator)
    {
        try
        {
            entryPath = entry.path();
            if (entry.is_directory() && searchParameters.isRecursive)
            {
                recurseSearchParams.criteriaCount = searchParameters.criteriaCount;
                recurseSearchParams.rootDirectory = entryPath.string().c_str();
                recurseSearchParams.isRecursive = searchParameters.isRecursive;
                FindFilesRR(criteria, recurseSearchParams, requiredCriteriaThreshold, outResults, outResultCount);
            }
            else
            {
                entryFileName = entryPath.filename().string();
                for (auto& c : entryFileName)
                    c = std::toupper(c);

                criteriaMatched = 0;

                // Process search criteria
                for (int i = 0; i < searchParameters.criteriaCount; ++i)
                {
                    isKeyphraseFound = entryFileName.find(criteria[i].keyphrase) != std::string::npos;

                    switch (criteria[i].type)
                    {
                    case CriteriaType::EXCLUDE:
                        if (isKeyphraseFound)
                            goto ExcludeEntry;
                        break;
                    case CriteriaType::EXCLUDE | CriteriaType::FLIPFLOP:
                        if (!isKeyphraseFound)
                        {
                            ++criteriaMatched;

                            // Skip next criterion if flipflop, it is already satisfied
                            if (criteria[i + 1].type & CriteriaType::FLIPFLOP)
                                ++i;
                        }

                        break;
                    case CriteriaType::FLIPFLOP:
                        if (isKeyphraseFound)
                        {
                            ++criteriaMatched;

                            // Skip next criterion if flipflop, it is already satisfied
                            if (criteria[i + 1].type & CriteriaType::FLIPFLOP)
                                ++i;
                        }

                        break;
                    case CriteriaType::SINGLE:
                        if (isKeyphraseFound)
                            ++criteriaMatched;
                        break;
                    }
                }

                if (criteriaMatched >= requiredCriteriaThreshold)
                {
                    outResults.push_back(entryPath.string());
                    ++outResultCount;
                }

            ExcludeEntry:;
            }
        }
        catch (const std::exception& e) { std::cout << "FindFilesRR: exception" << std::endl; }
    }

    return;
}
