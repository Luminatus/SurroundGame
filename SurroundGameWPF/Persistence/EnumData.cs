namespace SurroundGameWPF.Persistence
{
    public enum Direction
    {
        None = 0,
        Up = 2,
        Right = 1,
        Down = -2,
        Left = -1,
        DeadEnd = 5
    }

    public enum TileState
    {
        Unoccupied = 0,
        RedWall = 1,
        RedField = -1,
        BlueWall = 2,
        BlueField = -2,
        GreenWall = 3,
        GreenField = -3,
        YellowWall = 4,
        YellowField = -4,
        PurpleWall = 5,
        PurpleField = -5,
        OrangeWall = 6,
        OrangeField = -6
    }

    public enum GameMode
    {
        Game = 0,
        Playback = 1,
        GameAndPlayback = 2
    }

    public enum Players
    {
        None = 0,
        Red = 1,
        Blue = 2,
        Green = 3,
        Yellow = 4,
        Purple = 5,
        Orange = 6
    }

    public enum ActionType
    {
        Rotate = 0,
        Step = 1,
        EndGame = 2,
    }
}
