// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the MULTIAGENTNATIVE_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// MULTIAGENTNATIVE_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef MULTIAGENTNATIVE_EXPORTS
#define MULTIAGENTNATIVE_API __declspec(dllexport)
#else
#define MULTIAGENTNATIVE_API __declspec(dllimport)
#endif

// This class is exported from the MultiAgentNative.dll
class MULTIAGENTNATIVE_API CMultiAgentNative {
public:
	CMultiAgentNative(void);
	// TODO: add your methods here.
};

extern MULTIAGENTNATIVE_API int nMultiAgentNative;

MULTIAGENTNATIVE_API int fnMultiAgentNative(void);

extern "C" MULTIAGENTNATIVE_API HRESULT DecreaseValues(int arSize, unsigned int ar[], int value); 