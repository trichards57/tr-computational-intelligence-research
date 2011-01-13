module Controller

open SystemState

type Controller = { 
    BigYSpeed : float
    MidYSpeed : float
    SmlYSpeed : float
    BigXSpeed : float
    MidXSpeed : float
    SmlXSpeed : float
    BigXError : float
    MidXError : float
    BigYError : float
    MidYError : float
    UpForce : float
    DownForce : float
    PropelAngle : float
    AngleProportionalGain : float
    AngleDifferentialGain : float
    AngleIntegralGain : float
    }

let ControllerFromGenome (genome : float list) = 
    {
        BigYSpeed = genome.Item 0 + 50.0
        MidYSpeed = genome.Item 1 + 50.0
        SmlYSpeed = genome.Item 2 + 50.0
        BigXSpeed = genome.Item 3 + 50.0
        MidXSpeed = genome.Item 4 + 50.0
        SmlXSpeed = genome.Item 5 + 50.0
        BigXError = genome.Item 6 + 50.0
        MidXError = genome.Item 7 + 50.0
        BigYError = genome.Item 8 + 50.0
        MidYError = genome.Item 9 + 50.0
        UpForce = genome.Item 10
        DownForce = genome.Item 11
        PropelAngle = genome.Item 12
        AngleProportionalGain = genome.Item 13
        AngleDifferentialGain = genome.Item 14
        AngleIntegralGain = genome.Item 15 
    }

let GenomeFromController controller = 
    controller.BigYSpeed :: controller.MidYSpeed :: controller.SmlYSpeed ::
        controller.BigXSpeed :: controller.MidXSpeed :: controller.SmlXSpeed ::
        controller.BigXError :: controller.MidXError ::
        controller.BigYError :: controller.MidYError ::
        controller.UpForce :: controller.DownForce :: controller.PropelAngle ::
        controller.AngleProportionalGain :: controller.AngleDifferentialGain :: controller.AngleIntegralGain :: []

let Process controller (state : SystemState.SystemState) targetX targetY targetAngle =
    let xError = targetX - state.X
    let yError = targetY - state.Y

    true