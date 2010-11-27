/* 
 * File:   Agent.h
 * Author: Tony
 *
 * Created on 03 November 2010, 22:02
 */

#ifndef AGENT_H
#define	AGENT_H

class Field;

class Agent {
public:
    Agent();
    Agent(const Agent& orig);
    virtual ~Agent();
private:

public:
	void Process(const Field* field);
};

#endif	/* AGENT_H */

