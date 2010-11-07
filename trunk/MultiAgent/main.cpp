/* 
 * File:   main.cpp
 * Author: Tony
 *
 * Created on 03 November 2010, 21:55
 */

#include <cstdlib>
#include <iostream>
#include <fstream>

#include "StringTools.h"
#include "Field.h"

using namespace std;

void writeInstructions(void) {
    cout << "Command Line Usage : " << endl
         << "MultiAgentConsole.exe DataFile.csv [/w:width] [/h:height] [/ma:maxAgentCount]" << endl
         << "                                   [/sa:startingAgentCount] [/c:CycleCount]" << endl
         << "    /w  : An integer specifying the desired width of the map. Default : 100" << endl
         << "    /h  : An integer specifying the desired height of the map." << endl
         << "          Default : 100" << endl
         << "    /ma : An integer specifying the maximum number of agents for the" << endl
         << "          simulation. Default : 250" << endl
         << "    /sa : An integer specifying the starting number of agents for the" << endl
         << "          simulation. Default : 1" << endl
         << "    /c  : An integer specifying the number of cycles the sequence will" << endl
         << "          run for. Default : 10000" << endl << endl
         << "Return Values : " << endl
         << "0 : Success" << endl
         << "1 : Argument Error" << endl
         << "2 : Data File Not Found" << endl;
}

/*
 * 
 */
int main(int argc, char** argv) {
    cout << "Multi-Agent Lab Console : 0.1" << endl
         << "#############################################################################" << endl;

    if (argc < 2)
    {
        cout << "No arguments specified." << endl;
        writeInstructions();

        return 1;
    }

    ifstream file(argv[1]);
    if (!file)
    {
        cout << "Data file not valid." << endl;
        writeInstructions();

        return 2;
    }
    if (file.is_open())
        file.close();

    string fileName(argv[1]);
    int mapWidth = 100;
    int mapHeight = 100;
    int maxAgents = 250;
    int startAgents = 1;
    int cycleCount = 10000;

    for (int i = 2; i < argc; i++)
    {
        string arg(argv[i]);
        vector<string> parts;
        StringTools::Tokenize(arg, parts, ":");

        if (parts.size() == 2)
        {
            if (parts[0] == "w")
                mapWidth = atoi(parts[1].c_str());
            else if (parts[0] == "h")
                mapHeight = atoi(parts[1].c_str());
            else if (parts[0] == "ma")
                maxAgents = atoi(parts[1].c_str());
            else if (parts[0] == "sa")
                startAgents = atoi(parts[1].c_str());
            else if (parts[0] == "c")
                cycleCount = atoi(parts[1].c_str());
            else
                cout << "Unknown argument : /" << parts[0] << endl;
        }
        else
            cout << "Invalid argument : /" << parts[0] << endl;
    }

    cout << "Data File       : " << fileName << endl
         << "Map Width       : " << mapWidth << endl
         << "Map Height      : " << mapHeight << endl
         << "Max Agents      : " << maxAgents << endl
         << "Starting Agents : " << startAgents << endl
         << "Cycle Count     : " << cycleCount << endl << endl;

    cout << "Loading data file..." << endl;
    Field field(mapWidth, mapHeight, fileName);
    cout << "Data file loaded." << endl;

    return 0;

}

