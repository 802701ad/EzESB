using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Threading;

public class MemoryLog
{
    public static List<MemoryLogEntry> Entries = new List<MemoryLogEntry>();
    public static void Add(string message, string tag)
    {
        var m = new MemoryLogEntry();
        m.Stamp = DateTime.Now;
        m.Message = message;
        m.tag = tag;
        Entries.Add(m);
    }

    public static void PurgeOld(int DaysOld)
    {
        for (int i = Entries.Count - 1; i >= 0; i--)
        {
            if (Entries[i].Stamp < DateTime.Now.AddDays(-DaysOld)) Entries.RemoveAt(i);
        }
    }

    public static void PurgeByTag(string startDate, string tag)
    {
        DateTime d;
        if (!DateTime.TryParse(startDate, out d))
            d = DateTime.Now;


        for (int i = Entries.Count - 1; i >= 0; i--)
        {
            if (Entries[i].Stamp < d && Entries[i].tag == tag)
                Entries.RemoveAt(i);
        }
    }

    public static DataTable getAsDataTable()
    {
        var d = new DataTable();
        d.Columns.Add("tag");
        d.Columns.Add("datestamp");
        d.Columns.Add("message");
        foreach (MemoryLogEntry i in Entries)
        {
            d.Rows.Add(i.tag, i.Stamp, i.Message);
        }
        return d;
    }
}

public class MemoryLogEntry
{
    public DateTime Stamp;
    public string Message;
    public string tag;
}

public class ThreadPool
{
    public static List<ThreadItem> Items = new List<ThreadItem>();
    public static ThreadItem GetByTag(string tag)
    {
        foreach (ThreadItem i in Items)
        {
            if (i.tag == tag) return i;
        }
        return null;
    }

    public static void Clean()
    {
        for (int i = Items.Count - 1; i >= 0; i--)
        {
            if (!Items[i].thread.IsAlive) Items.RemoveAt(i);
        }
    }

    public static DataTable getListAsDataTable()
    {
        var d = new DataTable();
        d.Columns.Add("tag");
        d.Columns.Add("guid");
        d.Columns.Add("status");
        foreach (ThreadItem i in Items)
        {
            if (i.thread.IsAlive)
                d.Rows.Add(i.tag, i.guid, i.thread.IsAlive ? "Running" : "Completed");
        }
        return d;
    }
}

public class ThreadItem
{
    public Thread thread;
    public string guid;
    public string tag = "";

    public ThreadItem()
    {
        guid = new Guid().ToString();
    }
    public ThreadItem(string tag)
    {
        this.tag = tag;
    }
}