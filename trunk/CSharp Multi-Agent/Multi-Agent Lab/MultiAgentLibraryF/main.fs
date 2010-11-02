open System.Drawing
open System.IO

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

type ReadingType =
    | None
    | Boundary
    | End

type SensorReading = 
    {
        Range : float;
        Angle : float;
        Type : ReadingType;
        Origin : PointF
    }

type SensorPointF =
    {
        PointF : PointF;
        Type : ReadingType;
    }

type SensorPoint =
    {
        Point : Point;
        Type : ReadingType;
    }

let CreateField width height (lines : string list) = 
    let lineSplitter (line : string) = 
        List.ofArray (line.Split([|','|],System.StringSplitOptions.RemoveEmptyEntries))

    let lineMapper line =
        async {
            let partMapper origin part1 part2 part3 =
                let readType t = 
                    match t with
                    | "boundary" -> Boundary
                    | "end" -> End
                    | _ -> None
                { Origin = origin; Angle = (System.Double.Parse(part1)); Range = (System.Double.Parse(part2)); Type = (readType part3)}
            let (origin, list) = match line with
                                 | originX :: originY :: tail -> (new PointF(System.Single.Parse(originX), System.Single.Parse(originY)), tail)
                                 | _ -> failwith "Line did not match a suitable pattern."
            let mapFunc = partMapper origin
            let group l = 
                let rec looper acc = function
                    | [] | [_] | [_;_] -> List.rev acc
                    | h1 :: h2 :: h3 :: tl -> looper ((mapFunc h1 h2 h3) :: acc) tl
                looper [] l
            return (group list)
        }

    let rec readingReducer seed = function
        | [] -> seed
        | hd :: tl -> hd :: (readingReducer seed tl)

    let rawPointMapper (reading : SensorReading) =
        async {
            let x = reading.Origin.X + (float32 (reading.Range * sin(reading.Angle)))
            let y = reading.Origin.Y + (float32 (reading.Range * cos(reading.Angle)))
            return { PointF = new PointF(x, y); Type = reading.Type }
        }

    let pointScaler xScale yScale (point : PointF) =
        new Point((int (round(point.X / xScale))), (int (round(point.Y / yScale))))

    let sensorPointScaler pointScaler point =
        { Point = (pointScaler point.PointF); Type = point.Type }

    let firstLineParts = (lines.Head.Split([|","|],System.StringSplitOptions.RemoveEmptyEntries))

    let startPoint = new PointF(System.Single.Parse(firstLineParts.[0]), System.Single.Parse(firstLineParts.[1]))

    let points = lines |> List.map lineSplitter          // Split the data up by commas
                       |> List.map lineMapper            // Convert each line in to a set of readings
                       |> Async.Parallel                 // Prepare to run in parallel
                       |> Async.RunSynchronously         // Run in parallel
                       |> Array.reduce readingReducer    // Merge all the lines in to a single list
                                                         // Cannot be run in parallel.
                       |> List.map rawPointMapper        // Convert each reading to a point in space
                       |> Async.Parallel                 // Prepare to run in parallel
                       |> Async.RunSynchronously         // Run in parallel

    // Calculate the dimensions of the map
    let maxX = (Array.maxBy (fun item -> item.PointF.X) points).PointF.X
    let maxY = (Array.maxBy (fun item -> item.PointF.Y) points).PointF.Y

    // Get the size of each new map square
    let xRectSize = maxX / (float32 width)
    let yRectSize = maxY / (float32 height)

    let scaler = pointScaler xRectSize yRectSize

    // Scale the starting point
    let scaledStartPoint = pointScaler xRectSize yRectSize startPoint

    let pointComparer (point1 : Point) = function
        | (point2 : Point) when point1.X > point2.X -> 1
        | (point2 : Point) when point1.X < point2.X -> -1
        | (point2 : Point) when point1.X = point2.X && point1.Y > point2.Y -> 1
        | (point2 : Point) when point1.X = point2.X && point1.Y < point2.Y -> -1
        | _ -> 0

    let sensorPointComparer (point1 : SensorPoint) (point2 : SensorPoint) =
        pointComparer point1.Point point2.Point

    let typeMerger t1 = function
        | t2 when t1 = End || t2 = End -> End
        | t2 when (t1 = Boundary && t2 = None) || (t2 = Boundary && t1 = None) -> Boundary
        | t2 -> t2

    let rec pointMerger acc itemList = 
        match (itemList, acc) with
            | ([], list) -> list
            | (itemHd :: [], []) -> [itemHd]
            | ([], []) -> []
            | (itemHd :: itemTl, hd :: tl) when itemHd.Point <> hd.Point -> pointMerger (itemHd :: hd :: tl) itemTl
            | (itemHd :: itemTl, hd :: tl) when itemHd.Point = hd.Point -> pointMerger ({ Point = itemHd.Point; Type = (typeMerger itemHd.Type hd.Type) } :: tl) itemTl
            | (itemHd :: itemTl, []) -> pointMerger (itemHd :: []) itemTl
            
    let sensorReadings = points |> Array.map (sensorPointScaler scaler)
                                |> Array.sortWith sensorPointComparer 
                                |> List.ofArray
                                |> pointMerger []
    
    let readingTypeToSquareType = function
        | End -> Destination
        | Boundary -> Wall
        | _ -> Passable

    let readingsToMapMapper readings item = 
        async {
            let innerMapper readings = function
                | mapItem when List.exists (fun t -> t.Point = mapItem) readings ->
                    let rding = List.find (fun t -> t.Point = mapItem) readings
                    CreateSquare mapItem (readingTypeToSquareType rding.Type)
                | mapItem -> CreateSquare mapItem Passable
            return innerMapper readings item
        }

    let data = [ for x in 0 .. (width-1) do
                    for y in 0 .. (height-1) do
                        yield new Point(x,y) ] |> Seq.map (readingsToMapMapper sensorReadings)
                                               |> Async.Parallel
                                               |> Async.RunSynchronously
    data


let CreateSquareField width lines =
    CreateField width width lines


let ReadDataFile filename =
    if File.Exists filename then
        List.ofArray (File.ReadAllLines filename)
    else
        []

let data = ReadDataFile "C:\Users\Tony\Documents\Computational Intelligance Program\sensorData.csv" 
           |> CreateSquareField 100

for x in 0..99 do
    for y in 0..99 do
        let sq = Array.find (fun s -> s.Position = new Point(x,y)) data
        match sq with
            | s when s.Type = Wall -> printf "w"
            | s when s.Type = Destination -> printf "d"
            | _ -> printf " "
    printf "\n"

printf "Hello"