/* 
 * File:   StringTools.h
 * Author: Tony
 *
 * Created on 05 November 2010, 22:20
 */

#ifndef STRINGTOOLS_H
#define	STRINGTOOLS_H

#include <string>
#include <vector>

using namespace std;

class StringTools {
public:
    static void Tokenize(const string& str, vector<string>& tokens, const string& delimiters = ",");
    virtual ~StringTools() {};
private:
    StringTools() {};
    StringTools(const StringTools& orig) {};
};

#endif	/* STRINGTOOLS_H */

