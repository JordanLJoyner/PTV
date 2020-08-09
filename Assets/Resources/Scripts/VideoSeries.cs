using System.Collections.Generic;

[System.Serializable]
public class VideoSeries {
    public string Name;
    public string FilePath;
    public List<string> Tags = new List<string>();
}

[System.Serializable]
public enum ScheduleType{
    START,
    HARD_RANDOM, //Load all videos, pick at random
    DISTRIBUTED_RANDOM, //Loads all shows, picks a show, picks a random episode (smoother distribution)
    RANDOM_FROM_SHOWS, //Does hard random from the list of supplied shows
    SCHEDULED_BUT_RANDOM_EPISODE, //Load the schedule, go through it randomly
    SEQUENTIAL, //Load the schedule go through it in order
    MAX
}

[System.Serializable]
public enum ScheduleItemType {
    RANDOM, //Randomly pick from videos for the given series
    SEQUENTIAL //Go through the given series in order (pull from save data)
}

[System.Serializable]
public class ScheduleItem {
    public ScheduleItemType scheduleType;
    public string showName;
}

[System.Serializable]
public class Schedule {
    public ScheduleType scheduleType;
    public List<ScheduleItem> items;
}

[System.Serializable]
public class SaveDataItem { 
    public string showName;
    public string lastWatchedEpisode;
    public SaveDataItem(string show, string episode) {
        showName = show;
        lastWatchedEpisode = episode;
    }
}