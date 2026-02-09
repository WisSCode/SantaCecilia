namespace frontend.Services;

public static class EventBus
{
    public static event Action<WorkedTimeDto>? NewEntrySaved;

    public static void PublishNewEntry(WorkedTimeDto dto)
    {
        try
        {
            NewEntrySaved?.Invoke(dto);
        }
        catch { }
    }
}
