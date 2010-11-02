#pragma once
#include "Point.h"

#define MAXIMUM_PHEREMONE 0x7FFFFFFF

enum FieldSquareType
{
	Passable,
	Wall,
	Destination
};

class FieldSquare
{
private:
	Point _position;
	float _pheremoneLevel;
	FieldSquareType _type;
public:
	FieldSquare(int x, int y, FieldSquareType type) { Point p; p.X = x; p.Y = y; FieldSquare(p, type); };
	FieldSquare(Point position, FieldSquareType type);
	FieldSquare(void);
	~FieldSquare(void);
	void SetPosition(Point value) { _position = value; };
	Point GetPosition(void) { return _position; };
	void SetPheremoneLevel(float value);
	float GetPheremoneLevel(void) { return _pheremoneLevel; };
	void SetType(FieldSquareType value);
	FieldSquareType GetType() { return _type; };
};

