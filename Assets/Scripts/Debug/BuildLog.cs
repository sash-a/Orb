using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BuildLog
{

    public static void writeLog(string log)
    {
        File.AppendAllText("buildLogFile.txt", log + "\t\n");
    }

    public static void flushLogFile()
    {
        File.Delete("buildLogFile.txt");
        File.WriteAllText("buildLogFile.txt", "");
    }

}
