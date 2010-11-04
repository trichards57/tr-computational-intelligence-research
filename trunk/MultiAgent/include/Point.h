/* 
 * File:   Point.h
 * Author: Tony
 *
 * Created on 03 November 2010, 22:07
 */

#ifndef POINT_H
#define	POINT_H

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
};

#endif	/* POINT_H */

