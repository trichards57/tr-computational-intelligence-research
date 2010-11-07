/* 
 * File:   FieldSquare.h
 * Author: Tony
 *
 * Created on 03 November 2010, 22:05
 */

#ifndef FIELDSQUARE_H
#define	FIELDSQUARE_H

#include <limits>
#include "Point.h"

enum FieldSquareType
{
    Passable = 0,
    Wall,
    Destination
};

class FieldSquare {
public:
    static const unsigned int SuccessPheremoneLevel = 1000;
    static const unsigned int PheremoneDecayLevel = SuccessPheremoneLevel / 1000;
    const unsigned int MaxPheremoneLevel;
    FieldSquare();
    FieldSquare(Point);
    virtual ~FieldSquare();

    Point GetPosition(void) const { return position; };
    unsigned int GetPheremoneLevel(void) const { return pheremoneLevel; };
    bool IsDestination(void) const { return (type == Destination); };
    bool IsPassable(void) const { return !(type == Wall); };
    FieldSquareType GetType(void) const { return type; };

    void SetPosition(Point value) {
        position = value;
    };
    void SetPheremoneLevel(unsigned int value);
    void SetType(FieldSquareType value);

private:
    Point position;
    unsigned int pheremoneLevel;
    FieldSquareType type;
};

#endif	/* FIELDSQUARE_H */

