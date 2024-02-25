Console.WriteLine("Hello, World!");
string filename1 = "./nodeLogs/one.txt";
string filename2 = "./nodeLogs/two.txt";
string filename3 = "./nodeLogs/three.txt";

List<SubjectNode> list = [new SubjectNode(filename1), new SubjectNode(filename2), new SubjectNode(filename3)];

foreach (var item in list)
{
    item.List = list;
}

while (true)
{

}