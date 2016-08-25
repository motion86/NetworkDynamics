using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkDynamics
{
    public class FileIO : IDisposable

    {
        private System.IO.FileInfo fileInfo;
        private System.IO.StreamWriter file;

        private char[] delimiterChars = { ' ', ',', ':', '\t' };
        private System.IO.StreamReader myReader;

        public FileIO(string path, bool writer)
            // writer - true if opening a file to write, false if opening a file to read.
        {
            if (writer)
            {
                file = new System.IO.StreamWriter(path);

                fileInfo = new System.IO.FileInfo(path);
                fileInfo.Attributes &= ~System.IO.FileAttributes.ReadOnly;
            }
            else
            {
                try { myReader = new System.IO.StreamReader(path); } catch { } 
            }
        }

        public static void HideUnhideFile(string path, bool hide)
            // HideUnhideFile - hides or unhides a file.
        {
            System.IO.FileInfo fileInfo = null;
            if (path != null)
            {
                fileInfo = new System.IO.FileInfo(path);
                if (!hide)
                {
                    try
                    {
                        fileInfo = new System.IO.FileInfo(path);
                        fileInfo.Attributes &= ~System.IO.FileAttributes.Hidden;
                        fileInfo.Attributes &= ~System.IO.FileAttributes.ReadOnly;
                    }
                    catch { }
                }
                else
                {
                    try { fileInfo.Attributes |= System.IO.FileAttributes.Hidden; } catch { }
                }
            }
        }

        public void WriteLine(string line)
        {
            file.WriteLine(line);
        }

        public void WriteLine<T>(List<T> data)
            // WriteLine<T> - writes the vector data to file as comma separated values.
        {
            string myString = "";
            int i = 0;
            foreach(T n in data)
            {
                if (i > 0)
                    myString += "," + n;
                else
                    myString += n;
                i++;
            }
            this.WriteLine(myString);
        }

        public string[] ReadLine()
            // ReadLine - reads a line from a CSV file and splits the values into an array of strings.
            //            null - if end of file is reached.
        {
            if (myReader != null)
            {
                if (myReader.Peek() != -1)
                {
                    string temp = myReader.ReadLine();
                    return temp.Split(delimiterChars);
                }
                else
                {
                    myReader.Close();
                    return null;
                }
            }
            return null;
        }

        public double ReadLineDouble(out bool endFile)
            // ReadLineDouble - reads an entry of a column vector and returns it in a double format.
            // throws exception if data file is not a single column vector.
        {
            endFile = false;
            string[] temp = ReadLine();

            if (temp == null)
            {
                endFile = true;
                return 0;
            }
            else
            {
                if (temp.Length > 1)
                    if(temp[1] != "")
                        throw new Exception("Read Line Has More Than One Elements!");
                return Double.Parse(temp[0]);
            }
        }
        

        public List<double> ReadLineListDouble()
            // ReadLineDouble - reads a line of a CSV  file and returns a list of doubles.
            //                  null - if end of file is reached.
        {
            return FileIO.getDoubles(this.ReadLine());
        }

        public void CloseFile()
        {
            if (file != null)
                file.Close();
            if (myReader != null)
                myReader.Close();
        }

        // STATIC METHODS >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

        private static List<double> getDoubles(string[] data)
            // getDoubles - returns a list of doubles from a list of strings representing floats
            //              null - if end of file is reached.
        {
            List<double> dataOut = new List<double>();
            if (data == null) return null;
            foreach (string s in data)
                    if(s.Length > 0) dataOut.Add(Convert.ToDouble(s));
            return dataOut;
        }

        public static string getFileNameFromPath(string path)
            // getFileNameFromPath - returns the name and extension of the file.
        {
            return path.Slice(path.LastIndexOf("\\") + 1, path.Length);
        }
        
        public static string setupPathForFileWrite(string folderName, string fileName)
        // returns a path to the specified file in specified folder. Creates directory if it doesn't exist. Root - current working dir.
        {
            string path = System.IO.Directory.GetCurrentDirectory() + @"\" + folderName + @"\";
            if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);

            path += fileName;
            return path;
        }

        public static void saveMatrixToFile(string path, Matrix mat)
            // writes a Matrix file to specified file.
        {
            System.IO.FileInfo fileInfo = null;

            using (var file = new System.IO.StreamWriter(path))
            {
                fileInfo = new System.IO.FileInfo(path);
                fileInfo.Attributes &= ~System.IO.FileAttributes.ReadOnly;

                foreach (int i in range(mat.rows))
                {
                    foreach (int j in range(mat.cols))
                    {
                        file.Write(mat.getEntry(i, j).ToString("F5") + ",");
                    }
                    file.Write("\n");
                }
            }
        }

        public static List<string> GetFiles(string relativePath, string fileExtension, bool searchSubDir)
            // GetFiles - returns the names of the files mathching the specified extension.
            // @param relativePath - null for curr dir OR rel path: @"/someDir/anotherDir/"
            // @param fileExtension - the extension of the files to be looked up. ex: "csv"
            // @param searchSubDir - true to search all sub dir, false to search only in path.
        {
            fileExtension = @"*." + fileExtension;
            string path = System.IO.Directory.GetCurrentDirectory();
            if (relativePath != null) path += relativePath;
            var searchOption = System.IO.SearchOption.TopDirectoryOnly;
            if (searchSubDir) searchOption = System.IO.SearchOption.AllDirectories;
            return (System.IO.Directory.GetFiles(path , fileExtension, searchOption)).ToList();
        }

        public static List<string> GetDirectories(string relativePath)
            // GetDirectories - returns a list of all directory paths in the specified path.
        {
            return System.IO.Directory.GetDirectories(System.IO.Directory.GetCurrentDirectory() + relativePath).ToList();
        }

        public static List<string[]> ExtractTokens(string inString, char delimTags, char delimVals)
            // ExtractTokens - extracts the tokens and values from the given string based on tag delimiter 
            // and value delimiter. Removes the file extension from result.
        {         
            inString = inString.Substring(0, inString.LastIndexOf("."));
            var tokensValues = new List<string[]>();
            string[] temp = inString.Split(delimTags);
            foreach (string t in temp)
                tokensValues.Add(t.Split(delimVals));

            return tokensValues;
        }

        public static string GetValue(List<string[]> pars, string tag)
            // look for the tag and return the value, if not found returns null.
            // expects data in the form of {tag, val}
        {
            if (pars == null) return null;

            foreach(string[] p in pars)
            {
                try
                {
                    if (p[0] == tag)
                        return p[1];
                }
                catch { }
            }
            return null;
        }

        public static void RemoveCharFromFileName(string sourcePath, int pathIndexToRemove)
            // RemoveCharFromFileName - this methord renames the file in the source path by removing the a 
            //                          character at a specified index from the begining of the path.
        {
            StringBuilder sb = new StringBuilder(sourcePath);
            sb.Remove(pathIndexToRemove, 1);
            string theString = sb.ToString();
            System.IO.File.Move(sourcePath, theString);
        }

        public static string GetTagsVals(string path, char delimTags, char delimVals, int token)
            // GetTagsVals - returns a string of tab separated values [1] or tags [0]
        {
            var temp = FileIO.ExtractTokens(FileIO.getFileNameFromPath(path), delimTags, delimVals);
            string tags = "";
            foreach (string[] t in temp)
                tags += t[token] + "\t";

            return tags;
        }

        public static string GetNewFilePath(string name, string ext, string DirName)
        {
            string dir = System.IO.Directory.GetCurrentDirectory() + "\\" + DirName + "\\" + DateTime.Today.ToString("D") + "\\";
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            name = name.Replace(':', '#').Replace(' ', '_');

            return dir + name + "__" + DateTime.Now.ToString("HH-mm-ss") + "." + ext;

        }

        private static void GetDataFromFile(string path, out List<double> ydata)
        // Read whole file and return values (single column data)
        {
            ydata = new List<double>();
            using (var file = new FileIO(path, false))
            {
                bool eof = false;
                while (true)
                {
                    double y = file.ReadLineDouble(out eof);
                    if (eof) break;
                    ydata.Add(y);
                }
            }
        }

        private static void GetDataFromFile(string path, out List<double> xdata, out List<double> ydata)
        // y-values read from file, x-values 0-N
        {
            xdata = new List<double>();
            ydata = new List<double>();
            using (var file = new FileIO(path, false))
            {
                double i = 0;
                bool eof = false;
                while (true)
                {
                    double y = file.ReadLineDouble(out eof);
                    if (eof) break;
                    ydata.Add(y);
                    xdata.Add(i++);
                }
            }
        }

        public static double GetDataFileAvgValue(string path)
        // reads data from a file with a single column of data entries and returns the mean.
        {
            var data = new List<double>();

            return data.Sum() / data.Count();
        }

        private static System.Collections.IEnumerable range(int range)
        {
            for (int i = 0; i < range; i++) yield return i;
        }

        public void Dispose()
        {
            CloseFile();
        }
    }

    public static class Extensions
    {
        // add string slice method to the string class.
        public static string Slice(this string inString, int start, int end)
        {

            if (end < 0) end = inString.Length + end; // neg end support
            int len = end - start;
            return inString.Substring(start, len);
            
        }

    }
}
