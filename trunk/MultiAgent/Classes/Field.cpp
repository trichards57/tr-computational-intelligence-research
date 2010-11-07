/* 
 * File:   Field.cpp
 * Author: Tony
 * 
 * Created on 03 November 2010, 22:04
 */

#include <omp.h>

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
            InvalidFileException exp;
            throw exp;
        }

        dataFile.close();
    }

    vector<SensorReading> readings;

    float maxX = 0.0;
    float maxY = 0.0;
    PointF startPointF;

    for (int i = 0; i < lines.size(); i++)
    {
        string line = lines[i];
        vector<string> parts;
        StringTools::Tokenize(line, parts);
        PointF origin(atof(parts[0].c_str()), atof(parts[1].c_str()));

        if (i == 0)
            startPointF = origin;

        for (int j = 2; j < parts.size(); j += 3)
        {
            SensorState state;

            if (parts[i+2] == "boundary")
                state = Boundary;
            else if (parts[i+2] == "end")
                state = End;
            else
                state = None;

            if (state == None)
            {
                // A useless reading that can be ignored.
                continue;
            }

            double angle(atof(parts[i].c_str()));
            double range(atof(parts[i+1].c_str()));

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

    int startPointX = (int)round(startPointF.X / xRectSize);
    int startPointY = (int)round(startPointF.Y / yRectSize);

    this->startPoint = Point(startPointX, startPointY);

    for (int i = 0; i < readings.size(); i++)
    {
        int scalePointX = (int)round(readings[i].Point.X / xRectSize);
        int scalePointY = (int)round(readings[i].Point.Y / yRectSize);

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

Field::~Field()
{
    delete[] agents;
    agents = NULL;
}

