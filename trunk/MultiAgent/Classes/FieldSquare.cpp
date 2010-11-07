/* 
 * File:   FieldSquare.cpp
 * Author: Tony
 * 
 * Created on 03 November 2010, 22:05
 */

#include "FieldSquare.h"

FieldSquare::FieldSquare()
    : MaxPheremoneLevel(std::numeric_limits<unsigned int>::max()), pheremoneLevel(1)
{
}

FieldSquare::FieldSquare(Point location)
    : MaxPheremoneLevel(std::numeric_limits<unsigned int>::max()),
    position(location), pheremoneLevel(1)
{
}

void FieldSquare::SetPheremoneLevel(unsigned int value)
{
    if (value > MaxPheremoneLevel)
        pheremoneLevel = MaxPheremoneLevel;
    else
        pheremoneLevel = value;
}

void FieldSquare::SetType(FieldSquareType value) {
    type = value;
    switch (type)
    {
        case Passable:
            SetPheremoneLevel(1);
            break;
        case Wall:
            SetPheremoneLevel(0);
            break;
        case Destination:
            SetPheremoneLevel(MaxPheremoneLevel);
            break;
    }
}

FieldSquare::~FieldSquare() {
}

