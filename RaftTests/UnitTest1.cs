using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System.Threading;

namespace RaftTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]

        public void ALeaderGetSelectedIfTwoOfThreeNodesAreHealthy()
        {
            string filename1 = "../../../nodeLogs/one.txt";
            string filename2 = "../../../nodeLogs/two.txt";
            string filename3 = "../../../nodeLogs/three.txt";
            // string filename4 = "./nodeLogs/four.txt";
            // string filename5 = "./nodeLogs/five.txt";
            File.WriteAllText(filename1, "");
            File.WriteAllText(filename2, "");
            File.WriteAllText(filename3, "");
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
            string filename1 = "../../../nodeLogs/one.txt";
            string filename2 = "../../../nodeLogs/two.txt";
            string filename3 = "../../../nodeLogs/three.txt";
            string filename4 = "../../../nodeLogs/four.txt";
            string filename5 = "../../../nodeLogs/five.txt";
            File.WriteAllText(filename1, "");
            File.WriteAllText(filename2, "");
            File.WriteAllText(filename3, "");
            File.WriteAllText(filename4, "");
            File.WriteAllText(filename5, "");
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
            string filename1 = "../../../nodeLogs/one.txt";
            string filename2 = "../../../nodeLogs/two.txt";
            string filename3 = "../../../nodeLogs/three.txt";
            string filename4 = "../../../nodeLogs/four.txt";
            string filename5 = "../../../nodeLogs/five.txt";
            File.WriteAllText(filename1, "");
            File.WriteAllText(filename2, "");
            File.WriteAllText(filename3, "");
            File.WriteAllText(filename4, "");
            File.WriteAllText(filename5, "");
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
            string filename1 = "../../../nodeLogs/one.txt";
            string filename2 = "../../../nodeLogs/two.txt";
            string filename3 = "../../../nodeLogs/three.txt";
            string filename4 = "../../../nodeLogs/four.txt";
            string filename5 = "../../../nodeLogs/five.txt";
            File.WriteAllText(filename1, "");
            File.WriteAllText(filename2, "");
            File.WriteAllText(filename3, "");
            File.WriteAllText(filename4, "");
            File.WriteAllText(filename5, "");
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
            string filename1 = "../../../nodeLogs/one.txt";
            string filename2 = "../../../nodeLogs/two.txt";
            string filename3 = "../../../nodeLogs/three.txt";
            string filename4 = "../../../nodeLogs/four.txt";
            string filename5 = "../../../nodeLogs/five.txt";
            File.WriteAllText(filename1, "");
            File.WriteAllText(filename2, "");
            File.WriteAllText(filename3, "");
            File.WriteAllText(filename4, "");
            File.WriteAllText(filename5, "");
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
            string filename1 = "../../../nodeLogs/one.txt";
            string filename2 = "../../../nodeLogs/two.txt";
            string filename3 = "../../../nodeLogs/three.txt";
            string filename4 = "../../../nodeLogs/four.txt";
            string filename5 = "../../../nodeLogs/five.txt";
            File.WriteAllText(filename1, "");
            File.WriteAllText(filename2, "");
            File.WriteAllText(filename3, "");
            File.WriteAllText(filename4, "");
            File.WriteAllText(filename5, "");
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
                    if(numberUnhealthy < 2)
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
            string filename1 = "../../../nodeLogs/one.txt";
            string filename2 = "../../../nodeLogs/two.txt";
            string filename3 = "../../../nodeLogs/three.txt";
            string filename4 = "../../../nodeLogs/four.txt";
            string filename5 = "../../../nodeLogs/five.txt";
            File.WriteAllText(filename1, "");
            File.WriteAllText(filename2, "");
            File.WriteAllText(filename3, "");
            File.WriteAllText(filename4, "");
            File.WriteAllText(filename5, "");
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

    }
}