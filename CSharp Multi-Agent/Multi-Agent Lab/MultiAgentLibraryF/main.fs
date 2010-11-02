module main

open System.Windows

let MaxPheromoneLevel  = float (System.Int32.MaxValue / 2)

type Agent = 
    { 
        StartPosition : Point;
        Position : Point;
        PastRoute : Point list;
    }

let CalculateRoute currentRoute newPoint =
    let rec loopTrimmer route newPoint newRoute = 
        if (route = []) then newRoute
        elif (List.head(route) = newPoint) then newRoute
        else loopTrimmer (List.tail(route)) newPoint (newRoute @ [List.head(route)])
        
    (loopTrimmer currentRoute newPoint []) @ [newPoint]

type FieldSquareType =
    | Passable
    | Wall
    | Destination

type FieldSquare =
    {
        Position : Point;
        PheromoneLevel : float;
        Type : FieldSquareType
    }

let CreateSquare position squareType =
    let calcPheromone = function
        | Passable -> 1.0
        | Wall -> 0.0
        | Destination -> MaxPheromoneLevel

    { Position = position; PheromoneLevel = (calcPheromone squareType); Type = squareType}

let CreateField width height =
    "Fix Me"