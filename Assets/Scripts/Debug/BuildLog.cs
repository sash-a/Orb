using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BuildLog
{

    public static bool isBuild = false;

    public static void writeLog(string log)
    {
        if (!isBuild) return;
        File.AppendAllText("buildLogFile.txt", log + "\t\n");
    }

    public static void flushLogFile()
    {
        if (!isBuild) return;
        File.Delete("buildLogFile.txt");
        File.WriteAllText("buildLogFile.txt", "");
    }

}
