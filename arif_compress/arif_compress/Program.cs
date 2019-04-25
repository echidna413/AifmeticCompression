using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace arif_compress
{
    class Program
    {
        static void Main(string[] args)
        {
            string rawText;

            using (FileStream inputFile = new FileStream("C:\\Users\\diana\\OneDrive\\Рабочий стол\\text.txt", FileMode.Open, FileAccess.Read)) //открывает файл только на чтение
            using (StreamReader reader = new StreamReader(inputFile, Encoding.UTF8)) // создаем «потоковый читатель» и связываем его с файловым потоком 
            {
                rawText = reader.ReadToEnd();
                Console.WriteLine(rawText); //считываем все данные с потока и выводим на экран
            }

            Console.ReadLine();

            var workingText = rawText.ToLower();
            Dictionary<char, int> freqDict = new Dictionary<char, int>();
            var symbols = workingText.Distinct().ToArray();
            for (int i = 0; i < symbols.Count(); i++)
            {
                freqDict.Add(symbols[i], 0);
            }
            for (int j = 0; j < workingText.Length; j++)
            {
                freqDict[workingText[j]]++;
            }

            List<CharRange> characterList = new List<CharRange>();

            for (int i = 0; i < freqDict.Count(); i++)
            {
                var key = freqDict.Keys.ElementAt(i);

                characterList.Add(new CharRange()
                {
                    Symbol = key,
                    Probability = (decimal)freqDict[key] / workingText.Length
                });
            }

            characterList = characterList.OrderByDescending(sym => sym.Probability).ToList();

            decimal temp = 0;
            for (int i = 0; i < characterList.Count; i++)
            {
                characterList[i].Low = temp;
                temp = temp + characterList[i].Probability;
                characterList[i].High = temp;
            }

            var characterRangeDict = characterList.ToDictionary(x => x.Symbol);

            /*-----COMPRESSING-----*/
            Range range = new Range()
            {
                Low = 0,
                High = 1
            };

            for (int i = 0; i < workingText.Length; i++)
            {
                decimal High;
                decimal Low;
                High = range.Low + (range.High - range.Low) * characterRangeDict[workingText[i]].High;
                Low = range.Low + (range.High - range.Low) * characterRangeDict[workingText[i]].Low;
                range.High = High;
                range.Low = Low;
            }


            using (FileStream compressFile = new FileStream("C:\\Users\\diana\\OneDrive\\Рабочий стол\\compress.txt", FileMode.Create, FileAccess.Write)) //открывает файл на запись
            using (StreamWriter writer = new StreamWriter(compressFile, Encoding.UTF8))
            {
                writer.Write(range.Low);
            }
            /*-----DECOMPRESSING-----*/
            StringBuilder s = new StringBuilder();
            decimal code = range.Low;

            for (int i = 0; i < workingText.Length; i++)
            {
                var tempRange = characterList.Where(c => c.Low <= code && code < c.High).First();
                code = (code - tempRange.Low) / (tempRange.High - tempRange.Low);
                s.Append(tempRange.Symbol);
            }

            Console.WriteLine(s.ToString());

            using (FileStream decompressFile = new FileStream("C:\\Users\\diana\\OneDrive\\Рабочий стол\\decompress.txt", FileMode.Create, FileAccess.Write)) //открывает файл на запись
            using (StreamWriter writer = new StreamWriter(decompressFile, Encoding.UTF8))
            {
                writer.Write(s.ToString());
            }
        }
    }
}
