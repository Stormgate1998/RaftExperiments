using System.Timers;

public class SubjectNode
{
    public Role CurrentRole { get; set; }
    public Guid Identifier { get; }

    public int WaitTime { get; set; }
    public string LogFileName { get; set; }
    public List<SubjectNode> List { get; set; }
    public System.Timers.Timer ElectionCountdownTimer { get; set; }

    public System.Timers.Timer HeartbeatTimer { get; set; }
    public SubjectNode(string fileName)
    {
        LogFileName = fileName;
        Guid current;

        (_, _, current) = ReadNumbersFromFile(LogFileName);
        if (current == Guid.Empty)
        {
            Identifier = System.Guid.NewGuid();
        }
        else
        {
            Identifier = current;
        }

        CurrentRole = Role.FOLLOWER;
        Random random = new Random();
        WaitTime = random.Next(100, 601);

        ElectionCountdownTimer = new System.Timers.Timer(WaitTime);
        HeartbeatTimer = new System.Timers.Timer(50);
        HeartbeatTimer.AutoReset = true;
        ElectionCountdownTimer.AutoReset = true;
        List = [];
        ElectionCountdownTimer.Elapsed += BecomeLeader;
        HeartbeatTimer.Elapsed += SendHeartbeats;
        HeartbeatTimer.Enabled = true;
        ElectionCountdownTimer.Enabled = true;
        ElectionCountdownTimer.Start();
        HeartbeatTimer.Stop();
    }


    private static readonly object fileLock = new(); // Lock object to synchronize file access

    public void WriteNumbersToFile(int number1, Guid number2, string filePath)
    {
        lock (fileLock)
        {
            // Check if the file is being read
            while (IsFileInUse(filePath))
            {
                Console.WriteLine("File is currently being read. Please try again later.");
                System.Threading.Thread.Sleep(250); // Wait for 1 second before retrying
            }

            // Write numbers to file
            string data = $"{number1},{number2},{this.Identifier}";
            File.WriteAllText(filePath, data);
        }
    }

    public static (int, Guid, Guid) ReadNumbersFromFile(string filePath)
    {
        lock (fileLock)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            {
                // Return empty information if the file is blank or doesn't exist
                return (0, Guid.Empty, Guid.Empty);
            }

            // Read numbers from file
            string[] numbers = File.ReadAllText(filePath).Split(',');
            int number1 = Convert.ToInt32(numbers[0]);
            Guid guid = Guid.Parse(numbers[1]);
            Guid id = Guid.Parse(numbers[2]);
            return (number1, guid, id);
        }
    }
    private static bool IsFileInUse(string filePath)
    {
        try
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                fs.ReadByte();
            }
            return false;
        }
        catch (IOException)
        {
            return true;
        }
    }

    public void BecomeLeader(Object source, ElapsedEventArgs e)
    {
        while (List.Count != 3)
        {
            Thread.Sleep(100);
        }
        var (term, _, _) = ReadNumbersFromFile(LogFileName);
        term += 1;
        WriteNumbersToFile(term, Identifier, LogFileName);
        int voteCount = 1;
        foreach (var node in List)
        {
            if (node.Identifier != Identifier)
            {
                bool result = node.ProcessVoteRequest(Identifier, term);
                if (result)
                {
                    voteCount++;
                }
            }
        }

        if (voteCount >= 2)
        {
            Console.WriteLine($"Node {Identifier} is leader for term {term}");
            CurrentRole = Role.LEADER;
            HeartbeatTimer.Start();
            ElectionCountdownTimer.Stop();
        }
    }

    public bool ProcessVoteRequest(Guid voterGuid, int term)
    {
        var (currentTerm, votedFor, _) = ReadNumbersFromFile(LogFileName);
        if (term > currentTerm)
        {
            if (CurrentRole != Role.FOLLOWER)
            {
                HeartbeatTimer.Stop();
                CurrentRole = Role.FOLLOWER;
            }
            WriteNumbersToFile(term, voterGuid, LogFileName);
            return true;
        }
        else if (term == currentTerm)
        {
            return voterGuid == votedFor;
        }
        return false;
    }

    public bool ProcessHeartbeat(Guid leaderGuid)
    {
        if (CurrentRole == Role.CANDIDATE)
        {
            CurrentRole = Role.FOLLOWER;
        }
        ElectionCountdownTimer.Stop();
        ElectionCountdownTimer.Start();
        return true;
    }

    public void SendHeartbeats(Object source, ElapsedEventArgs e)
    {
        Console.WriteLine("Sending Heartbeats");
        foreach (var item in List)
        {
            if (item != this)
            {
                _ = item.ProcessHeartbeat(Identifier);
            }
        }
    }
}

public enum Role
{
    LEADER,
    FOLLOWER,
    CANDIDATE
}
/*
create 3 nodes
each has a unique, stored GUID and a randomly chosen wait time
for each node:

Run down the time determined in wait time
when completed, increment round number by one and check for leader in file
there is no leader for the current round, nominate yourself
Request votes from the other 2 nodes, with the round number included.
If get back 1 vote, declare self leader and tell other nodes
Store leader in file.
Reset timer

If recieve vote request:
If round being voted on not current round, increment to current round
If not declared self nominee or voted this round, return vote yes
otherwise, vote no

if recieve leader comfirmation
Reset timer


Needs to lock file when voting



Timer going
Check state
If leader, don't restart


Get design parameters from reading


if heartbeat not recieved in time:
begin election


begin election:
increment current term
vote for self
send off vote requests

if get majority:
declare leader
Begin sending heartbeats
heartbeat has leaderId, term number
if heartbeat recieved, becomes follower

if candidate times out, begin new election






*/