module SystemState

type ThrusterState = {
    Up : float
    Left : float
    Right : float
    }

let LimitThrusterState state = 
    let limitValue lower upper = function
        | n when n <= lower -> lower
        | n when n >= upper -> upper
        | n -> n

    {
        Up = limitValue 0.0 1.0 state.Up
        Left = limitValue 0.0 1.0 state.Left
        Right = limitValue 0.0 1.0 state.Right
    }

type SystemState = {
        X : float
        Y : float
        DxDt : float
        DyDt : float
        Angle : float
        DAngleDt : float
    }