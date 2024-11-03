namespace OSProj.Generator
{
    public enum TaskType
    {
        Base = 0,
        Extended = 1,
    }

    public enum OSTaskStatus
    {
        Created = 0,
        Running = 1,
        Ready = 2,
        Suspended = 3,
        Waiting = 4,
        Succeded = 5
    }
}