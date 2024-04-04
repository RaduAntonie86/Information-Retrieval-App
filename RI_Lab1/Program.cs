using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using WordFrequency;

List<String> globalArray = new();
SortedDictionary<int, ushort> rarVector;
Dictionary<String, SortedDictionary<int, ushort>> documents = new();
List<String> stopwords = new (File.ReadAllLines("C:\\Users\\Radu\\source\\repos\\RI_Lab1\\RI_Lab1\\stopwords.txt"));
String folderPath = "C:\\Users\\Radu\\source\\repos\\RI_Lab1\\RI_Lab1\\ReutersDataSet\\Reuters\\Reuters_34\\Testing";

// Get all XML files in the folder
string[] xmlFiles = Directory.GetFiles(folderPath, "*.xml");
StringBuilder sb = new StringBuilder();

foreach (string xmlFile in xmlFiles)
{
    sb = new();
    rarVector = new();
    XmlDocument xmlDocument = new();
    xmlDocument.Load(xmlFile);
    foreach (XmlNode node in xmlDocument.ChildNodes)
    {
        WriteXmlNode(sb, node);
    }
    processDocumentByWord(sb.ToString());
    documents.Add("D" + (documents.Count + 1), rarVector);
    Console.WriteLine("D" + documents.Count + ":");
    foreach (var rarElement in rarVector)
        Console.WriteLine(rarElement.Key + " " + rarElement.Value);
}

using (StreamWriter writer = new StreamWriter("myfile.txt"))
{
    foreach (var outerKey in documents.Keys)
    {
        // Write outer key
        writer.Write(outerKey + " \n");

        // Write inner dictionary contents
        foreach (var innerKeyValue in documents[outerKey])
        {
            writer.Write(innerKeyValue.Key + ":" + innerKeyValue.Value + " \n");
        }

        // Write newline
        writer.WriteLine();
    }
}

/*
using (StreamWriter writer = new StreamWriter("wordlist.csv"))
{
    foreach (var word in globalArray)
    {
        // Write outer key
        writer.Write(word + " \n");
    }
}
*/

void addWordToArray(String word)
{
    int index = -1;
    PorterStemmer porterStemmer = new();
    porterStemmer.StemWord(word);
    word = extraCleaningSteps(word);
    if (isStopWord(word))
        return;
    if (!globalArray.Contains(word))
    {
        globalArray.Add(word);
    }
    if (!string.IsNullOrEmpty(word))
        index = globalArray.FindIndex(a => a.Contains(word));
    if (index != -1)
    {
        if (rarVector.ContainsKey(index))
            rarVector[index]++;
        else
            rarVector.Add(index, 1);
    }
}

bool isStopWord(String word)
{
    foreach(String stopword in stopwords)
        if(word.Equals(stopword))
            return true;
    return false;
}

static String extraCleaningSteps(String word)
{
    StringBuilder sb = new StringBuilder(word);
    for (int i = 0; i < sb.Length; i++)
    {
        if (sb[i].Equals('\''))
        {
            sb.Remove(i, sb.Length - i);
            break;
        }
        else if (!char.IsLetter(sb[i]) && !char.IsNumber(sb[i]))
        {
            sb.Remove(i, 1);
            i--;
        }
    }
    return sb.ToString();
}

void processDocumentByWord(String text)
{
    String str = "";
    foreach(var character in text)
    {
        if (isSeparator(character))
        {
            if(str != "" && !isSeparator(str[0]))
                addWordToArray(str);
            str = "";
        }
        else
            str += character;
    }
}

bool isSeparator(char c)
{
    switch(c)
    {
        case (' '):
        case ('.'):
        case (','):
        case ('-'):
        case (':'):
            return true;
        default:
            return false;
    }
}

static void WriteXmlNode(StringBuilder sb, XmlNode node)
{
    if (node.NodeType == XmlNodeType.Text)
    {
        sb.Append(node.InnerText);
    }
    else if (node.NodeType == XmlNodeType.Element)
    {
        foreach (XmlNode childNode in node.ChildNodes)
        {
            WriteXmlNode(sb, childNode);
        }
        sb.Append(" ");
    }
}