// MultiAgentOpto.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "Field.h"

using namespace std;

int main(int argc, char* argv[])
{
	if (argc != 2)
	{
		cout << "Multi-Agent Route Finder" << endl;
		cout << "Please specify data file name." << endl;

		int dummy;
		cin >> dummy;

		return 1;
	}
	string dataFileName(argv[1]);
	Field mainFile(dataFileName);

	int dummy;

	cin >> dummy;

	return 0;
}

