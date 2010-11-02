#pragma once

#include "Point.h"
#include "FieldSquare.h"

#define FIELD_WIDTH 100
#define FIELD_HEIGHT 100

#include "stdafx.h"

using namespace std;

class Field
{
private:
	Point _startPoint;
	FieldSquare _columns[FIELD_WIDTH][FIELD_HEIGHT];
public:
	Field(void);
	Field(string fileName);
	~Field(void);

	Point GetStartPoint() { return _startPoint; };

	FieldSquare GetFieldSquare(int x, int y) { return _columns[x][y]; };
	void SetFieldSquare(int x, int y, FieldSquare value) { _columns[x][y] = value; };
};

