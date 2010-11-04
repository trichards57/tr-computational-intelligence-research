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
#include <string>

class Field {
public:
    Field(int width, int height);
    Field(int width) : Field(width, width) {};
    Field(int width, int height, std::string filename);
    virtual ~Field();
    void CycleAgents();
private:
    Point startPoint;
    Agent *agents;
    
};

#endif	/* FIELD_H */

