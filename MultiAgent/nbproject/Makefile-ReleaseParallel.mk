#
# Generated Makefile - do not edit!
#
# Edit the Makefile in the project folder instead (../Makefile). Each target
# has a -pre and a -post target defined where you can add customized code.
#
# This makefile implements configuration specific macros and targets.


# Environment
MKDIR=mkdir
CP=cp
GREP=grep
NM=nm
CCADMIN=CCadmin
RANLIB=ranlib
CC=gcc.exe
CCC=g++.exe
CXX=g++.exe
FC=
AS=as.exe

# Macros
CND_PLATFORM=MinGW-Windows
CND_CONF=ReleaseParallel
CND_DISTDIR=dist

# Include project Makefile
include Makefile

# Object Directory
OBJECTDIR=build/${CND_CONF}/${CND_PLATFORM}

# Object Files
OBJECTFILES= \
	${OBJECTDIR}/Classes/StringTools.o \
	${OBJECTDIR}/main.o \
	${OBJECTDIR}/Classes/Agent.o \
	${OBJECTDIR}/Classes/Field.o \
	${OBJECTDIR}/Classes/FieldSquare.o


# C Compiler Flags
CFLAGS=

# CC Compiler Flags
CCFLAGS=-fopenmp
CXXFLAGS=-fopenmp

# Fortran Compiler Flags
FFLAGS=

# Assembler Flags
ASFLAGS=

# Link Libraries and Options
LDLIBSOPTIONS=

# Build Targets
.build-conf: ${BUILD_SUBPROJECTS}
	"${MAKE}"  -f nbproject/Makefile-ReleaseParallel.mk dist/ReleaseParallel/MinGW-Windows/multiagent.exe

dist/ReleaseParallel/MinGW-Windows/multiagent.exe: ${OBJECTFILES}
	${MKDIR} -p dist/ReleaseParallel/MinGW-Windows
	${LINK.cc} -o ${CND_DISTDIR}/${CND_CONF}/${CND_PLATFORM}/multiagent ${OBJECTFILES} ${LDLIBSOPTIONS} 

${OBJECTDIR}/Classes/StringTools.o: Classes/StringTools.cpp 
	${MKDIR} -p ${OBJECTDIR}/Classes
	${RM} $@.d
	$(COMPILE.cc) -g -Iinclude -MMD -MP -MF $@.d -o ${OBJECTDIR}/Classes/StringTools.o Classes/StringTools.cpp

${OBJECTDIR}/main.o: main.cpp 
	${MKDIR} -p ${OBJECTDIR}
	${RM} $@.d
	$(COMPILE.cc) -g -Iinclude -MMD -MP -MF $@.d -o ${OBJECTDIR}/main.o main.cpp

${OBJECTDIR}/Classes/Agent.o: Classes/Agent.cpp 
	${MKDIR} -p ${OBJECTDIR}/Classes
	${RM} $@.d
	$(COMPILE.cc) -g -Iinclude -MMD -MP -MF $@.d -o ${OBJECTDIR}/Classes/Agent.o Classes/Agent.cpp

${OBJECTDIR}/Classes/Field.o: Classes/Field.cpp 
	${MKDIR} -p ${OBJECTDIR}/Classes
	${RM} $@.d
	$(COMPILE.cc) -g -Iinclude -MMD -MP -MF $@.d -o ${OBJECTDIR}/Classes/Field.o Classes/Field.cpp

${OBJECTDIR}/Classes/FieldSquare.o: Classes/FieldSquare.cpp 
	${MKDIR} -p ${OBJECTDIR}/Classes
	${RM} $@.d
	$(COMPILE.cc) -g -Iinclude -MMD -MP -MF $@.d -o ${OBJECTDIR}/Classes/FieldSquare.o Classes/FieldSquare.cpp

# Subprojects
.build-subprojects:

# Clean Targets
.clean-conf: ${CLEAN_SUBPROJECTS}
	${RM} -r build/ReleaseParallel
	${RM} dist/ReleaseParallel/MinGW-Windows/multiagent.exe

# Subprojects
.clean-subprojects:

# Enable dependency checking
.dep.inc: .depcheck-impl

include .dep.inc
