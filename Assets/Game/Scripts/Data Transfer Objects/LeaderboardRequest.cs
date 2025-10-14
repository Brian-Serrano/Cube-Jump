using Newtonsoft.Json;
using UnityEngine;

public class LeaderboardRequest
{
    [JsonProperty("highscore")]
    public int highscore;

    public LeaderboardRequest(int highscore)
    {
        this.highscore = highscore;
    }
}
