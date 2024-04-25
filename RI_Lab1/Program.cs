using System.Text;
using System.Xml;
using WordFrequency;

List<string> globalArray = [];
Dictionary<string, SortedDictionary<int, ushort>> documents = [];
List<List<String>> topics = [];
String stopWordsPath = "C:\\Users\\Radu\\source\\repos\\RI_Lab1\\RI_Lab1\\stopwords.txt";
String folderPath = "C:\\Users\\Radu\\source\\repos\\RI_Lab1\\RI_Lab1\\ReutersDataSet\\Reuters\\Reuters_7083";

DocumentProcessor.convertDocumentToRar(folderPath, ref documents, ref globalArray, ref topics, stopWordsPath);