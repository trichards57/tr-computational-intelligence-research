/* 
 * File:   Field.cpp
 * Author: Tony
 * 
 * Created on 03 November 2010, 22:04
 */

#include "Field.h"
#include "StringTools.h"
#include "Point.h"
#include <fstream>
#include <vector>
#include <stdlib.h>
#include <math.h>

using namespace std;

enum SensorState
{
    Boundary,
    End,
    None
};

struct SensorReading
{
public:
    PointF Point;
    SensorState State;
};

Field::Field(const int width, const int height, const string filename)
{
	_width = width;
	_height = height;
    int size = width * height;
    squares = new FieldSquare[size];

    vector<string> lines;
    ifstream dataFile(filename.c_str());
    if (dataFile.is_open())
    {
        string line;
        while (dataFile.good())
        {
            getline (dataFile, line);
            lines.push_back(line);
        }
        if (lines.size() == 0)
        {
            throw new InvalidFileException();
        }

        dataFile.close();
    }

    vector<SensorReading> readings;

    double maxX = 0.0;
    double maxY = 0.0;
    PointF startPointF;

    for (unsigned int i = 0; i < lines.size(); i++)
    {
        string line = lines[i];
		if (line == "")
			continue;
        vector<string> parts;
        StringTools::Tokenize(line, parts);
        PointF origin(atof(parts[0].c_str()), atof(parts[1].c_str()));

        if (i == 0)
            startPointF = origin;

        for (unsigned int j = 2; j < parts.size(); j += 3)
        {
            SensorState state;

            if (parts[j+2] == "boundary")
                state = Boundary;
            else if (parts[j+2] == "end")
                state = End;
            else
                state = None;

            if (state == None)
            {
                continue;
            }

            double angle(atof(parts[j].c_str()));
            double range(atof(parts[j+1].c_str()));

            PointF point(origin.X + range * sin(angle), origin.Y + range * cos(angle));
            
            SensorReading reading;
            reading.Point = point;
            reading.State = state;

            if (point.X > maxX)
                maxX = point.X;
            if (point.Y > maxY)
                maxY = point.Y;

            readings.push_back(reading);
        }
    }

    float xRectSize = maxX / width;
    float yRectSize = maxY / height;

    int startPointX = (int)floor(startPointF.X / xRectSize);
    int startPointY = (int)floor(startPointF.Y / yRectSize);

    this->startPoint = Point(startPointX, startPointY);

    for (unsigned int i = 0; i < readings.size(); i++)
    {
        int scalePointX = (int)floor(readings[i].Point.X / xRectSize);
        int scalePointY = (int)floor(readings[i].Point.Y / yRectSize);

        int index = scalePointX + scalePointY * width;

        if (squares[index].GetPosition() == Point(scalePointX,scalePointY))
        {
            squares[index].SetPosition(Point(scalePointX, scalePointY));
            switch (readings[i].State)
            {
                case Boundary:
                    squares[index].SetType(Wall);
                    break;
                case End:
                    squares[index].SetType(Destination);
                    break;
                default:
                    squares[index].SetType(Passable);
                    break;
            }
        }
        else
        {
            switch (readings[i].State)
            {
                case Boundary:
                    if (squares[index].GetType() == Passable)
                        squares[index].SetType(Wall);
                    break;
                case End:
                    squares[index].SetType(Destination);
                    break;
            }
        }
    }
}

void Field::CycleAgents()
{
	for (unsigned int i = 0; i < agents.size(); i++)
		agents[i].Process(this);

	int size = _width * _height;

	for (int i = 0; i < size; i++)
	{
		if (squares[i].GetPheremoneLevel() > 1 && squares[i].IsDestination() == false)
			squares[i].SetPheremoneLevel(squares[i].GetPheremoneLevel() - FieldSquare::PheremoneDecayLevel);
	}
}

Field::~Field()
{
	delete[] squares;
	squares = NULL;
}

