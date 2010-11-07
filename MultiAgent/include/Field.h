/* 
 * File:   Field.h
 * Author: Tony
 *
 * Created on 03 November 2010, 22:04
 */

#ifndef FIELD_H
#define	FIELD_H

#include "Point.h"
#include "Agent.h"
#include "FieldSquare.h"
#include <string>
#include <exception>

class InvalidFileException : public std::exception
{
    virtual const char* what() const throw()
    {
        return "Invalid Data File Specified";
    }
};

class Field {
public:
    Field(int width, int height, std::string filename);
    virtual ~Field();
    void CycleAgents();
private:
    Point startPoint;
    Agent *agents;
    FieldSquare *squares;
};

#endif	/* FIELD_H */

