#include "StdAfx.h"
#include "Field.h"

using namespace std;

Field::Field(void)
{
}

Field::Field(string fileName)
{
	ifstream file(fileName);

	cout << "Loading sensor data file : " << fileName << endl;

	if (file.is_open())
	{
		while (file.good())
		{
			string line;
			getline(file, line);

			string::size_type lastPos = line.find_first_not_of(",", 0);
			string::size_type pos = line.find_first_of(",", lastPos);
			vector<string> parts;

			while (string::npos != pos || string.npos != lastPos)
			{
				parts.push_back(line.substr(lastPos, pos - lastPos));
				lastPos = line.find_first_not_of(",", pos);
				pos = line.find_first_of(",", lastPos);
			}
		}
	}
	else
	{
		cout << "Unable to open sensor data file." << endl;
	}
}

Field::~Field(void)
{
}