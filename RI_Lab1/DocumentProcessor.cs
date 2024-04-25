using System.Text;
using System.Xml;
using WordFrequency;

internal class DocumentProcessor
{
    static SortedDictionary<int, ushort> rarVector = new();
    static List<String> stopwords = new();
    private static int currentDoc = 0;
    public static void convertDocumentToRar(String folderPath, ref Dictionary<String, SortedDictionary<int, ushort>> documents, ref List<String> globalArray, ref List<List<String>> topics, String stopWordsPath = "")
    {
        if (stopWordsPath != "")
            stopwords = new(File.ReadLines(stopWordsPath));
        else
            stopwords = new();
        string[] xmlFiles = Directory.GetFiles(folderPath, "*.xml");

        foreach (string xmlFile in xmlFiles)
        {
            StringBuilder sb = new();
            rarVector = new();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlFile);
            writeTopics(xmlDocument, ref topics);

            foreach (XmlNode node in xmlDocument.ChildNodes)
            {
                WriteXmlNode(sb, node);
            }

            processDocumentByWord(sb.ToString(), globalArray);

            String documentName = generateDocumentName(xmlFile, folderPath);
            documents.Add(documentName + ": ", rarVector);
        }

        writeDocuments(documents, topics);

        writeWords(globalArray);
    }

    static private void addWordToArray(String word, List<String> globalArray)
    {
        int index = -1;
        PorterStemmer porterStemmer = new();
        word = word.ToLower();

        word = porterStemmer.StemWord(word);

        word = extraCleaningSteps(word);
        if (isStopWord(word) || word == "" || int.TryParse(word, out _))
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

    static private bool isStopWord(String word)
    {
        foreach (String stopword in stopwords)
            if (word.Equals(stopword))
                return true;
        return false;
    }

    static private String extraCleaningSteps(String word)
    {
        StringBuilder sb = new(word);
        for (int i = 0; i < sb.Length; i++)
        {
            if (sb[i].Equals('\''))
            {
                sb.Remove(i, sb.Length - i);
                break;
            }
            else if (!char.IsLetter(sb[i]) && !char.IsDigit(sb[i]))
            {
                sb.Remove(i, 1);
                i--;
            }
        }
        if (sb.Length == 1 && (char.IsLetter(sb[0]) || char.IsDigit(sb[0])))
            return "";
        sb.Replace(" ", "");
        return sb.ToString();
    }

    static private void processDocumentByWord(String text, List<String> globalArray)
    {
        String str = "";
        foreach (var character in text)
        {
            if (isSeparator(character))
            {
                if (str != "" && str != " " && !isSeparator(str[0]))
                    addWordToArray(str, globalArray);
                str = "";
            }
            else
                str += character;
        }
    }

    static private bool isSeparator(char c)
    {
        switch (c)
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

    static private void WriteXmlNode(StringBuilder sb, XmlNode node)
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

    static private void writeTopics(XmlDocument xmlDocument, ref List<List<String>> topics)
    {
        XmlNodeList codesNodes = xmlDocument.GetElementsByTagName("codes");
        foreach (XmlNode codesNode in codesNodes)
        {
            if (codesNode.Attributes["class"] != null && codesNode.Attributes["class"].Value == "bip:topics:1.0")
            {
                topics.Add(new List<string>());
                foreach (XmlNode codeNode in codesNode.ChildNodes)
                {
                    if (codeNode.Name == "code" && codeNode.Attributes["code"] != null)
                    {
                        string code = codeNode.Attributes["code"].Value;
                        topics[currentDoc].Add(code);
                    }
                }
                currentDoc++;
            }
        }
    }

    static private String generateDocumentName(string xmlFile, string folderPath)
    {
        String documentName = xmlFile;
        String folderPathTemp = folderPath.Replace("\\\\", "\\");
        folderPathTemp += "\\";
        return documentName.Replace(folderPathTemp, "");
    }

    static private void writeScreenFile(StreamWriter writer, String str)
    {
        writer.Write(str);
        Console.Write(str);
    }
    
    static private void writeDocuments(Dictionary<String, SortedDictionary<int, ushort>> documents, List<List<String>> topics)
    {
        using (StreamWriter writer = new StreamWriter("rarVector.txt"))
        {
            int currentDoc = 0;
            foreach (var outerKey in documents.Keys)
            {
                writeScreenFile(writer, outerKey + " \n");

                writeScreenFile(writer, "Topics: ");

                foreach (var topic in topics[currentDoc])
                {
                    writeScreenFile(writer, topic + " ");
                }
                writeScreenFile(writer, "\n");

                foreach (var innerKeyValue in documents[outerKey])
                {
                    writeScreenFile(writer, (innerKeyValue.Key + 1) + ":" + innerKeyValue.Value + " \n");
                }
                currentDoc++;
                writeScreenFile(writer, "\n");
            }
        }
    }

    static void writeWords(List<String> globalArray)
    {
        using (StreamWriter writer = new StreamWriter("wordlist.txt"))
        {
            for (int i = 0; i < globalArray.Count; i++)
                writer.Write(i + ": " + globalArray[i] + " \n");
        }
    }
}