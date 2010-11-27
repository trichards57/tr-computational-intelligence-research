/* 
 * File:   Point.h
 * Author: Tony
 *
 * Created on 03 November 2010, 22:07
 */

#ifndef POINT_H
#define	POINT_H

struct PointF
{
public:
    double X;
    double Y;

    PointF()
    {
        X = 0.0;
        Y = 0.0;
    }

    PointF(double x, double y)
    {
		if (x < 0)
			x = 0;
		if (y < 0)
			y = 0;
        X = x;
        Y = y;
    }
};

struct Point
{
public:
    int X;
    int Y;
    Point()
    {
        X = 0;
        Y = 0;
    }

    Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    bool operator==(const Point other) const { return (X == other.X && Y == other.Y); };
    bool operator!=(const Point other) const { return !(*this == other); };
};

#endif	/* POINT_H */

