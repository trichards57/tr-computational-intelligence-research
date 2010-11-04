/* 
 * File:   Field.cpp
 * Author: Tony
 * 
 * Created on 03 November 2010, 22:04
 */

#include "Field.h"

Field::Field(int width, int height)
{
    int size = width * height;
    agents = new Agent[size];
    for (int i = 0; i < size; i++)
        Agent[i] = NULL;
}

Field::Field(int width, int height, std::string filename)
: Field(width, height)
{
    
}

Field::~Field()
{
    delete[] agents;
    agents = NULL;
}

