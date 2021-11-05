using System.Collections.Generic;

public class SteamApp
{
    public string appid { get; set; }
    public string name { get; set; }
}

public class Applist
{
    public List<SteamApp> apps { get; set; } = new List<SteamApp>();
}

public class GamesResponse
{
    public Applist applist { get; set; } = new Applist();
}