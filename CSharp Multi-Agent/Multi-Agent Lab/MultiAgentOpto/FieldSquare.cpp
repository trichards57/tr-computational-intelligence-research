#include "StdAfx.h"
#include "FieldSquare.h"


FieldSquare::FieldSquare(Point position, FieldSquareType type)
{
	SetPosition(position);
	SetType(type);
}

FieldSquare::FieldSquare()
{
	Point p;
	p.X = 0;
	p.Y = 0;

	SetPosition(p);
	SetType(FieldSquareType::Passable);
}

FieldSquare::~FieldSquare(void)
{
}

void FieldSquare::SetPheremoneLevel(float value)
{
	if (value > MAXIMUM_PHEREMONE)
		_pheremoneLevel = MAXIMUM_PHEREMONE;
	else if (value < 0)
		_pheremoneLevel = 0;
	else
		_pheremoneLevel = value;
}

void FieldSquare::SetType(FieldSquareType value)
{
	_type = value;

	if (_type == FieldSquareType::Destination)
		_pheremoneLevel = MAXIMUM_PHEREMONE;
	else if (_type == FieldSquareType::Wall)
		_pheremoneLevel = 0;
}