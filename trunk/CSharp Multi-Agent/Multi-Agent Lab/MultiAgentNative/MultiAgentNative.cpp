// MultiAgentNative.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "MultiAgentNative.h"
#include <mmintrin.h>

// This is an example of an exported variable
MULTIAGENTNATIVE_API int nMultiAgentNative=0;

// This is an example of an exported function.
MULTIAGENTNATIVE_API int fnMultiAgentNative(void)
{
	return 42;
}

// This is the constructor of a class that has been exported.
// see MultiAgentNative.h for the class definition
CMultiAgentNative::CMultiAgentNative()
{
	return;
}

extern "C" MULTIAGENTNATIVE_API HRESULT DecreaseValues(int arSize, unsigned int ar[], int value)
{
	
	__m64 sub = _mm_set_pi32(value,value);
	__m64* var = (__m64*)ar;
	__m64 res = _mm_sub_pi32(*var, sub);
	*var = res;
	_mm_empty();

	return 0;
}