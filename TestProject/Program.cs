using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TestProject
{
    class Program
    {
        #region Fields

        private static int numberOfTotalLines;
        private static int numberOfEmptyOrSpaceLines;
        private static int numberOfCommentLines;
        private static string lineComments = @"//(.*?)\r?\n";
        private static object locker = new object();

        #endregion

        static void Main(string[] args)
        {
            try
            {
                var allSubDirectories = Directory.GetDirectories(args[0], "*", SearchOption.AllDirectories);

                EnumerateFilesAndDirectories(args[0]);

                foreach (var directory in allSubDirectories)
                {
                    EnumerateFilesAndDirectories(directory);
                }

                Console.WriteLine($"Number of lines in files: {numberOfTotalLines}");
                Console.WriteLine($"Number of empty or whitespace lines in files: {numberOfEmptyOrSpaceLines}");
                Console.WriteLine($"Number of comment lines in files: {numberOfCommentLines}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong: {ex.Message}");
            }

            Console.ReadKey();
        }

        public static void GetNumberOfLinesInFile(FileInfo file)
        {
            using (StreamReader stream = file.OpenText())
            {
                string line;
                while ((line = stream.ReadLine()) != null)
                {
                    lock (locker)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            numberOfEmptyOrSpaceLines++;
                        }

                        if(Regex.IsMatch(line, lineComments))
                        {
                            numberOfCommentLines++;
                        }
                        numberOfTotalLines++;
                    }
                }
            }
        }

        public static void EnumerateFilesAndDirectories(string dirName)
        {
            DirectoryInfo directory = new DirectoryInfo(dirName);

            foreach (var file in directory.GetFiles())
            {
                if (file.Extension == ".txt" || file.Extension == ".doc" || file.Extension == ".docx")
                {
                    ThreadPool.QueueUserWorkItem(x => GetNumberOfLinesInFile(file));

                    //Версия с потоками
                    //Thread thread = new Thread(() => GetNumberOfLinesInFile(file)); 
                    //thread.Start();

                    Thread.Sleep(50);
                }
            }
        }
    }
}
