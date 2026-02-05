using System;

namespace frontend.Services
{
    public static class EventBus
    {
        public static event Action<TimeEntryDto>? NewEntrySaved;

        public static void PublishNewEntry(TimeEntryDto dto)
        {
            try
            {
                NewEntrySaved?.Invoke(dto);
            }
            catch { }
        }
    }
}
