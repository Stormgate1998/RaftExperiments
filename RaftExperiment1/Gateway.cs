using System;

public class Gateway
{
    private Guid Leader;

    public Gateway()
    {
        Leader = new("");
    }

    private int ChooseRandomIndex(List<SubjectNode> Nodes)
    {
        Random rand = new Random();
        return rand.Next(0, Nodes.Count);
    }


    public SubjectNode FindLeader(List<SubjectNode> Nodes)
    {
        SubjectNode currentLeader = new("");

        Guid? guid = null;
        while (guid == null)
        {
            int index = ChooseRandomIndex(Nodes);
            guid = Nodes[index].FindLeader();
        }
        foreach (var item in Nodes)
        {
            if (item.Identifier == guid)
            {
                currentLeader = item;
            }
        }
        return currentLeader;
    }

    public bool IsLeaderValid(List<SubjectNode> Nodes)
    {
        SubjectNode newLeader = FindLeader(Nodes);
        return newLeader.Identifier == Leader;
    }


    public string? EventualGet(string key, List<SubjectNode> Nodes)
    {
        // Proxy EventualGet request to any node
        var randomNode = ChooseRandomIndex(Nodes);
        return Nodes[randomNode].EventualGet(key);
    }

    public string? StrongGet(string key, List<SubjectNode> Nodes)
    {
        // Proxy StrongGet request to the leader
        if (IsLeaderValid(Nodes))
        {
            foreach(var item in Nodes)
            {
                if(item.Identifier == Leader) {
                    return item.StrongGet(key);
                }
            }
            return null;
        }
        else
        {
            // Re-elect leader and retry StrongGet
            Leader = FindLeader(Nodes).Identifier;
            if (IsLeaderValid(Nodes))
            {
                foreach (var item in Nodes)
                {
                    if (item.Identifier == Leader)
                    {
                        return item.StrongGet(key);
                    }
                }
               
            }
            else
            {
                return null; // Leader not valid
            }
        }
        return null;
    }

    public bool CompareVersionAndSwap(string key, string newValue, int expectedVersion, List<SubjectNode> Nodes)
    {
        if (IsLeaderValid(Nodes))
        {
            foreach (var item in Nodes)
            {
                if (item.Identifier == Leader)
                {
                    return item.CompareVersionAndSwap(key, newValue, expectedVersion);
                }
            }
        }
        else
        {
            Leader = FindLeader(Nodes).Identifier;
            if (IsLeaderValid(Nodes))
            {
                foreach (var item in Nodes)
                {
                    if (item.Identifier == Leader)
                    {
                        return item.CompareVersionAndSwap(key, newValue, expectedVersion);
                    }
                }
            }
            else
            {
                return false; // Leader not valid
            }
        }
        return false;
    }


}