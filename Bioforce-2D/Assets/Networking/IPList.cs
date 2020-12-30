
class IPList
{
    private int LowestIP { get; set; }
    private int HighestIP { get; set; }
    private int CurrentIP { get; set; }

    public IPList(int lowestIP, int highestIP)
    {
        LowestIP = lowestIP;
        HighestIP = highestIP;
        CurrentIP = LowestIP;
    }
    public int GetDifBetweenLowestAndHighestIP()
    {
        return (HighestIP - LowestIP) + 1;
    }
    public int GetNextIP()
    {
        if (MorePorts())
            return CurrentIP++;

        return -1;
    }

    private bool MorePorts()
    {
        return (HighestIP - CurrentIP) >= 0;
    }
}
