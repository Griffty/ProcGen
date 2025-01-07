using System;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace City_Gen
{
    public static class Debugger
    {

        private static readonly List<TimeRecord> TimeRecords = new();
        public static  readonly Stopwatch Timer = new();

        public static void AddTimeRecord(String name)
        {
            TimeRecords.Add(new TimeRecord(Timer.ElapsedMilliseconds, name));
            Timer.Restart();
        }
    
        public static void PrintDebugInfo()
        {
            string message = "map info: seed " + Config.Instance.Seed
                                               + " \n size - " + Config.Instance.ActualCitySize
                                               + "; city size - " + Config.Instance.CitySizeFactor
                                               + "\n------------";
            foreach (TimeRecord timeRecord in TimeRecords)
            {
                message += "\n" + timeRecord.Name + " - " + timeRecord.Time;
            }
            Debug.Log(message);
        }

        public static void ClearRecords()
        {       
            TimeRecords.Clear();
        }
    }

    public struct TimeRecord
    {
        public readonly long Time;
        public readonly String Name;

        public TimeRecord(long time, string name)
        {
            Time = time;
            Name = name;
        }
    }
}