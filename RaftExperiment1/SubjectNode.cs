using System.Timers;

public class SubjectNode
{
    public Role CurrentRole { get; set; }

    public Guid currentLeader { get; set; }
    public Guid Identifier { get; }

    private Dictionary<string, (string, int)> keyValueLog = [];

    public bool IsHealthy { get; set; }

    public bool IsTest { get; set; }

    public int WaitTime { get; set; }
    public string LogFileName { get; set; }
    public List<SubjectNode> List { get; set; }
    public System.Timers.Timer ElectionCountdownTimer { get; set; }

    public System.Timers.Timer HeartbeatTimer { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public SubjectNode(string fileName, bool isAlive = true, bool IsTest = false)
    {
        IsHealthy = isAlive;
        this.IsTest = IsTest;
        LogFileName = "";
        List = [];
        if (IsHealthy)
        {


            LogFileName = fileName;
            Guid current;
            Guid leader;
            int term;
            (term, leader, current) = ReadNumbersFromFile(LogFileName);
            if (current == Guid.Empty)
            {
                Identifier = System.Guid.NewGuid();
            }
            else
            {
                Identifier = current;
            }


            Random random = new Random();
            WaitTime = random.Next(200, 900);

            ElectionCountdownTimer = new System.Timers.Timer(WaitTime);
            HeartbeatTimer = new System.Timers.Timer(50)
            {
                AutoReset = true
            };
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            HeartbeatTimer.Elapsed += SendHeartbeats;
            ElectionCountdownTimer.AutoReset = true;
            List = [];
            ElectionCountdownTimer.Elapsed += BecomeLeader;
            HeartbeatTimer.Enabled = true;
            ElectionCountdownTimer.Enabled = true;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).

            if (leader == Identifier)
            {
                ElectionCountdownTimer.Stop();
                HeartbeatTimer.Start();
                CurrentRole = Role.LEADER;
                currentLeader = Identifier;
                Console.WriteLine($"Node {Identifier} is leader for term {term}");
            }
            else
            {
                ElectionCountdownTimer.Start();
                HeartbeatTimer.Stop();
                CurrentRole = Role.FOLLOWER;
                currentLeader = leader;
            }
        }
        else
        {
            CurrentRole = Role.FOLLOWER;
        }
    }


    public Guid FindLeader()
    {
        return currentLeader;
    }


    private static readonly object fileLock = new(); // Lock object to synchronize file access

    private void WriteNumbersToFile(int number1, Guid number2)
    {
        string filePath = LogFileName;
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

    private static (int, Guid, Guid) ReadNumbersFromFile(string filePath)
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

    private void BecomeLeader(Object source, ElapsedEventArgs e)
    {
        StartElection();
    }

    private bool? ProcessVoteRequest(Guid voterGuid, int term)
    { //ELEPHANT Edit for log thing
        if (IsHealthy)
        {

            var (currentTerm, votedFor, _) = ReadNumbersFromFile(LogFileName);
            if (term > currentTerm)
            {
                if (CurrentRole != Role.FOLLOWER)
                {
                    HeartbeatTimer.Stop();
                    CurrentRole = Role.FOLLOWER;
                }
                WriteNumbersToFile(term, voterGuid);
                return true;
            }
            else if (term == currentTerm)
            {
                return voterGuid == votedFor;
            }
            return false;
        }
        return false;

    }

    private bool? ProcessHeartbeat(Guid leaderGuid, int term)
    {
        currentLeader = leaderGuid;
        var (currentTerm, votedFor, _) = ReadNumbersFromFile(LogFileName);
        if (IsHealthy)
        {
            if (votedFor != leaderGuid || currentTerm != term)
            {
                WriteNumbersToFile(term, votedFor);
            }
            if (CurrentRole == Role.CANDIDATE)
            {
                CurrentRole = Role.FOLLOWER;

            }
            if (!IsTest)
            {
                ElectionCountdownTimer.Stop();
                ElectionCountdownTimer.Start();
            }
            return true;
        }
        return null;
    }

    private void SendHeartbeats(Object source, ElapsedEventArgs e)
    {
        SendingHeartbeat();
    }

    private void SendingHeartbeat()
    {
        var (currentTerm, votedFor, _) = ReadNumbersFromFile(LogFileName);
        if (IsHealthy)
        {
            Console.WriteLine("Sending Heartbeats");
            foreach (var item in List)
            {
                if (item != this)
                {
                    _ = item.ProcessHeartbeat(Identifier, currentTerm);
                }
            }
        }
        else
        {
            CurrentRole = Role.FOLLOWER;
        }
    }

    public void AlterHealth(bool IsHealthy)
    {
        this.IsHealthy = IsHealthy;
    }

    public Role GetRole()
    {
        Console.WriteLine($"Current role is: {CurrentRole}");
        return this.CurrentRole;
    }


    public int StartElection(int UsedTerm = -1)
    {
        if (IsHealthy)
        {
            Console.WriteLine($"Beginning election with {Identifier} as candidate");

            ElectionCountdownTimer.Stop();
            while (List.Count == 0)
            {
                Thread.Sleep(100);
            }
            var (term, _, _) = ReadNumbersFromFile(LogFileName);
            term += 1;
            if (UsedTerm != -1)
            {
                term = UsedTerm;
            }
            WriteNumbersToFile(term, Identifier);

            int voteCount = 1;
            foreach (var node in List)
            {
                if (node.Identifier != Identifier)
                {
                    bool? result = node.ProcessVoteRequest(Identifier, term);
                    if (result != null && result != false)
                    {
                        voteCount++;
                        Console.WriteLine(voteCount);
                    }
                }
            }

            if (voteCount * 2 >= List.Count)
            {
                Console.WriteLine($"Node {Identifier} is leader for term {term}");
                CurrentRole = Role.LEADER;
                HeartbeatTimer.Start();
                ElectionCountdownTimer.Stop();
                return voteCount;
            }
            else
            {
                Thread.Sleep(WaitTime * 2);
                if (CurrentRole != Role.FOLLOWER)
                {
                    StartElection(term++);
                }
                else
                {
                    if (IsHealthy)
                    {
                        if (!IsTest)
                        {
                            ElectionCountdownTimer.Start();
                        }
                        return voteCount;
                    }
                }

            }
        }
        return 0;
    }


    public void AddToLog(string key, string value, int logIndex, bool isVerifiedByLeader = false)
    {
        if (isVerifiedByLeader)
        {
            keyValueLog[key] = (value, logIndex);
            Console.WriteLine($"Node {Identifier} as {CurrentRole} stored the value");
        }
        else
        {
            SendingHeartbeat();
            keyValueLog[key] = (value, logIndex);
            Console.WriteLine($"Node {Identifier} as {CurrentRole} stored the value");
            foreach (var item in List)
            {
                SendingHeartbeat();
                if (item.CurrentRole != Role.LEADER)
                {
                    item.AddToLog(key, value, logIndex, true);
                }
            }
            SendingHeartbeat();
        }
    }

    public string? EventualGet(string key)
    {
        // Return the latest value from the log
        if (keyValueLog.TryGetValue(key, out (string, int) value))
        {
            return value.Item1;
        }
        else
        {
            return null; // Key not found
        }
    }

    public bool CompareVersionAndSwap(string key, string newValue, int expectedVersion)
    {
        if (keyValueLog.TryGetValue(key, out (string, int) value))
        {
            var (_, version) = value;
            if (version == expectedVersion)
            {
                Guid leader = FindLeader();
                foreach (var item in List)
                {
                    if (item.Identifier == leader)
                    {
                        item.AddToLog(key, newValue, version + 1);
                    }
                }

                return true; // Successful swap
            }
            else
            {
                return false; // Version mismatch
            }
        }
        else
        {
            return false; // Key not found
        }
    }

    // Method to perform a strong get operation
    public string? StrongGet(string key)
    {
        // Check if this node is the current leader
        if (CurrentRole == Role.LEADER)
        {
            if (ActuallyLeader())
            {
                return EventualGet(key);
            }
        }
        return null;
    }

    public bool ActuallyLeader()
    {
        int count = 0;

        foreach (var node in List)
        {
            if (node.CheckIsLeader(Identifier))
            {
                count++;
            }
        }
        if (count * 2 > List.Count)
        {
            return true;
        }
        return false;
    }

    private bool CheckIsLeader(Guid identifier)
    {
        var subjectNode = FindLeader();
        if (subjectNode == identifier)
        {
            return true;
        }
        return false;
    }
}

public enum Role
{
    FOLLOWER,
    CANDIDATE,
    LEADER,
}
