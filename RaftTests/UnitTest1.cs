using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System.Threading;

namespace RaftTests
{
    public class Tests
    {
        public string filename1;
        public string filename2;
        public string filename3;
        public string filename4;
        public string filename5;
        [SetUp]
        public void Setup()
        {
            filename1 = "../../../nodeLogs/one.txt";
            filename2 = "../../../nodeLogs/two.txt";
            filename3 = "../../../nodeLogs/three.txt";
            filename4 = "../../../nodeLogs/four.txt";
            filename5 = "../../../nodeLogs/five.txt";
            File.WriteAllText(filename1, "");
            File.WriteAllText(filename2, "");
            File.WriteAllText(filename3, "");
            File.WriteAllText(filename4, "");
            File.WriteAllText(filename5, "");
        }

        [Test]

        public void ALeaderGetSelectedIfTwoOfThreeNodesAreHealthy()
        {
            
            List<SubjectNode> list = [new SubjectNode(filename1), new SubjectNode(filename2), new SubjectNode(filename3, false)];

            foreach (var item in list)
            {
                item.List = list;
            }
            Thread.Sleep(3000);
            bool IsLeader = false;
            foreach (var item in list)
            {
                if (item.GetRole() == Role.LEADER)
                {
                    IsLeader = true;
                }
            }
            Assert.That(IsLeader, Is.EqualTo(true));
        }


        [Test]
        public void ALeaderGetsElectedIfThreeOfFiveNodesAreHealthy()
        {
          
            List<SubjectNode> list = [new SubjectNode(filename1), new SubjectNode(filename2), new SubjectNode(filename3), new SubjectNode(filename4, false), new SubjectNode(filename5, false)];

            foreach (var item in list)
            {
                item.List = list;
            }
            Thread.Sleep(3000);
            bool IsLeader = false;
            foreach (var item in list)
            {
                if (item.GetRole() == Role.LEADER)
                {
                    IsLeader = true;
                }
            }
            Assert.That(IsLeader, Is.EqualTo(true));
        }
        [Test]
        public Task ALeaderDoesNotGetElectedIfThreeOfFiveNodesAreUnhealthy()
        {
           
            List<SubjectNode> list = [new SubjectNode(filename1, true, true), new SubjectNode(filename2, true, true), new SubjectNode(filename3, false), new SubjectNode(filename4, false), new SubjectNode(filename5, false)];
            foreach (var item in list)
            {
                item.List = list;
            }

            int test = list[0].StartElection();
            int test2 = list[1].StartElection();
            int test3 = list[2].StartElection();


            bool IsLeader = false;
            foreach (var item in list)
            {
                if (item.GetRole() == Role.LEADER)
                {
                    IsLeader = true;
                }
            }

            Assert.Multiple(() =>
            {
                Assert.That(test, Is.EqualTo(2));
                Assert.That(test2, Is.EqualTo(2));
                Assert.That(test3, Is.EqualTo(0));
                Assert.That(IsLeader, Is.EqualTo(false));
            });
            return Task.CompletedTask;
        }

        [Test]
        public void ANodeContinuesAsLeaderIfAllNodesRemainHealthy()
        {
           
            List<SubjectNode> list = [new SubjectNode(filename1), new SubjectNode(filename2), new SubjectNode(filename3), new SubjectNode(filename4, false), new SubjectNode(filename5, false)];

            foreach (var item in list)
            {
                item.List = list;
            }
            Thread.Sleep(3000);
            SubjectNode leader = new("");
            foreach (var item in list)
            {
                if (item.GetRole() == Role.LEADER)
                {
                    leader = item;
                }
            }
            SubjectNode leaderAfterTime = new("");
            Thread.Sleep(5000);
            foreach (var item in list)
            {
                if (item.GetRole() == Role.LEADER)
                {
                    leaderAfterTime = item;
                }
            }
            Assert.That(leaderAfterTime, Is.EqualTo(leader));
        }


        [Test]
        public void ANodeWillCallForAnElectionIfMessagesFromTheLeaderToThatNodeTakeTooLong()
        {
            
            List<SubjectNode> list = [new SubjectNode(filename1), new SubjectNode(filename2), new SubjectNode(filename3), new SubjectNode(filename4), new SubjectNode(filename5)];

            foreach (var item in list)
            {
                item.List = list;
            }
            Thread.Sleep(1000);
            string leader = "";
            foreach (var item in list)
            {
                if (item.GetRole() == Role.LEADER)
                {
                    leader = item.LogFileName;
                    item.AlterHealth(false);
                }
            }
            string leaderAfterTime = "";
            Thread.Sleep(5000);
            foreach (var item in list)
            {
                if (item.GetRole() == Role.LEADER)
                {
                    leaderAfterTime = item.LogFileName;
                }
            }
            Assert.That(leader, Is.Not.EqualTo(leaderAfterTime));
        }



        [Test]
        public void ANodeContinuesAsLeaderEvenIfTwoOfFiveNodesBecomeUnhealthy()
        {
            
            List<SubjectNode> list = [new SubjectNode(filename1), new SubjectNode(filename2), new SubjectNode(filename3), new SubjectNode(filename4), new SubjectNode(filename5)];

            foreach (var item in list)
            {
                item.List = list;
            }
            Thread.Sleep(1000);
            string leader = "";
            int numberUnhealthy = 0;
            foreach (var item in list)
            {
                if (item.GetRole() == Role.LEADER)
                {
                    leader = item.LogFileName;

                }
                else
                {
                    if (numberUnhealthy < 2)
                    {
                        item.AlterHealth(false);
                        numberUnhealthy++;
                    }
                }
            }
            string leaderAfterTime = "";
            Thread.Sleep(5000);
            foreach (var item in list)
            {
                if (item.GetRole() == Role.LEADER)
                {
                    leaderAfterTime = item.LogFileName;
                }
            }
            Assert.That(leader, Is.EqualTo(leaderAfterTime));
        }

        [Test]
        public void AvoidingTwoDoubleVoting()
        {
            
            List<SubjectNode> list = [new SubjectNode(filename1, true, true), new SubjectNode(filename2, true, true), new SubjectNode(filename3, true, true), new SubjectNode(filename4, true, true), new SubjectNode(filename5, true, true)];

            foreach (var item in list)
            {
                item.List = list;
            }
            list[0].StartElection();
            list.Remove(list[2]);
            list.Remove(list[2]);
            list.Remove(list[2]);
            list.Add(new(filename3));
            list.Add(new(filename4));
            list.Add(new(filename5));
            int result = list[1].StartElection(1);

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void AllowFormerUnhealthyNodeToBecomeLeader()
        {
            
            List<SubjectNode> list = [new SubjectNode(filename1, true, true), new SubjectNode(filename2, true, true), new SubjectNode(filename3, true, true), new SubjectNode(filename4, true, true), new SubjectNode(filename5, true, true)];

            foreach (var item in list)
            {
                item.List = list;
            }
            list[4].AlterHealth(false);
            list[0].StartElection();
            Thread.Sleep(1000);
            list[4].AlterHealth(true);
            Thread.Sleep(1000);
            list[0].AlterHealth(false);
            list[4].StartElection();
            Role testRole = list[4].GetRole();

            Assert.That(testRole, Is.EqualTo(Role.LEADER));
        }

        [Test]
        public void EventualGet_ReturnsValue()
        {
           
            File.WriteAllText(filename3, "");
            List<SubjectNode> list = [new SubjectNode(filename1), new SubjectNode(filename2, true, true), new SubjectNode(filename3, true, true)];
            Gateway gateway = new Gateway();
            Thread.Sleep(5000);
            string key = "testKey";
            string value = "testValue";
            List<SubjectNode> ActualList = list;
            SubjectNode leader = gateway.FindLeader(ActualList);
            leader.AddToLog(key, value, 1);
            Thread.Sleep(1000);
            ActualList = list;
            string? result = gateway.EventualGet(key, ActualList);

            // Assert
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        public void StrongGet_ReturnsValue()
        {
           
            List<SubjectNode> list = [new SubjectNode(filename1), new SubjectNode(filename2), new SubjectNode(filename3)];
            Gateway gateway = new Gateway();
            Thread.Sleep(3000);
            string key = "testKey";
            string value = "testValue";
            List<SubjectNode> ActualList = list;
            SubjectNode leader = gateway.FindLeader(ActualList);
            leader.AddToLog(key, value, 1);

            ActualList = list;
            string? result = gateway.StrongGet(key, ActualList);

            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        public void CompareVersionAndSwap_Successful()
        {
            
            List<SubjectNode> list = [new SubjectNode(filename1), new SubjectNode(filename2, true, true), new SubjectNode(filename3, true, true)];
            Thread.Sleep(10000);
            List<SubjectNode> AcutalList;
            Gateway gateway = new Gateway();
            Thread.Sleep(5000);
            string key = "testKey";
            string newValue = "newValue";
            AcutalList = list;
            SubjectNode leader = gateway.FindLeader(AcutalList);
            leader.AddToLog(key, "oldValue", 1);
            Thread.Sleep(5000);
            int expectedVersion = 1;

            // Act
            bool success = gateway.CompareVersionAndSwap(key, newValue, expectedVersion, AcutalList);
            string? result = gateway.EventualGet(key, AcutalList);

            // Assert
            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo(newValue));
        }

        [Test]
        public void CompareVersionAndSwap_Failure_KeyNotFound()
        {
           
            List<SubjectNode> list = [new SubjectNode(filename1), new SubjectNode(filename2), new SubjectNode(filename3)];
            Gateway gateway = new Gateway();
            Thread.Sleep(3000);
            string key = "nonExistingKey";
            string newValue = "newValue";
            int expectedVersion = 1;

            List<SubjectNode> ActualList = list;
            bool success = gateway.CompareVersionAndSwap(key, newValue, expectedVersion, ActualList);


            Assert.That(success, Is.False);
        }

        [Test]
        public void ConfirmIsLeaderFailsIfEnoughNodesUnhealthy()
        {
            
            List<SubjectNode> list = [new SubjectNode(filename1), new SubjectNode(filename2), new SubjectNode(filename3)];
            Gateway gateway = new Gateway();
            Thread.Sleep(3000);
            var ActualList = list;
            gateway.FindLeader(ActualList);

            foreach (var item in list)
            {
                if (item.GetRole() != Role.LEADER)
                {
                    item.AlterHealth(false);
                }
            }
            ActualList = list;
            bool success = gateway.IsLeaderValid(ActualList);
            Assert.IsFalse(success);

        }
        //test Confirm Leader works
        //test confirm leader returns false when not leader
        //test confirm leader returns false when most nodes are unhealthy
        //test EventualGet works
        //test StrongGet works and fails when not leader
        //test compareVerisionAndSwap works

    }
}