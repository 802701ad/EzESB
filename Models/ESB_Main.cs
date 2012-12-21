using System.Collections.Generic;
using System.Threading;
using System.Data;
using System;
using System.Text;
using System.Collections.Specialized;
using System.Xml;
using System.Web;

public class ESB_Main
{
    #region invoke as a separate thread
    //we invoke this separately so as not to hold up the caller of the page calling this method
    public static List<int> pendingInvocations = new System.Collections.Generic.List<int>();
    public static int InvocationID = 0;
    public static void Queue()
    {
        int i;
        lock (pendingInvocations)
        {
            i = ++InvocationID;
            pendingInvocations.Add(i);
        }
        Thread.Sleep(500);
        lock (pendingInvocations)
        {
            if (pendingInvocations[pendingInvocations.Count - 1].Equals(i))
            {
                Thread oThread = new Thread(RunEvents);
                oThread.Start();
            }
            else
            {
                pendingInvocations.Remove(i);
            }
        }
    }
    #endregion

    public static object lck=new object();
    public static Dictionary<string, string> ConsumerUrls = new Dictionary<string, string>();
    public static void RunEvents()
    {
        lock (lck)//we do not want more than one thread to be querying the database at a time
        {
			ThreadPool.Clean();
            //figure out what needs to be run
            var events_to_run = db.EventsToRun();

            var current_message_id = "";
            Dictionary<string,string> values=null;

            //send to consumers
            foreach (DataRow dr in events_to_run.Rows)
            {
                string consumer_id = Convert.ToString(dr["consumer_id"]);
                string message_id = Convert.ToString(dr["message_id"]);
                if (current_message_id != message_id)
                {
                    values = db.getValues(message_id);
                    current_message_id = message_id;
                }
                var t = new ThreadItem();
                t.tag = consumer_id;
                
                //if the current consumer isn't busy, process all the events in this group
                if (ThreadPool.GetByTag(consumer_id) == null)
                {

                    t.thread = new Thread(() => TryCallConsumer(consumer_id, message_id, values));
                            ThreadPool.Items.Add(t);
                            t.thread.Start();
                }
                else
                {
                    //if thread for this consumer is busy end; it will be handled later
                }


            }
            Thread.Sleep(1000);//wait a second before querying the database again
        }
    }

    private static Dictionary<string,object> consumerlocks=new Dictionary<string,object>();
    private static object locklock=new object();
    public static void TryCallConsumer(string consumer_id, string message_id, Dictionary<string,string> values)
    {
        lock(locklock)
        {
            if(!consumerlocks.ContainsKey(consumer_id))
                consumerlocks.Add(consumer_id,new object());
        }

        try
        {
            lock(consumerlocks[consumer_id])//if we have a bunch of events we are sending to the same consumer in this round, they need to be processed one at a time.
            {
                RunConsumer(consumer_id, message_id, values);
            }
        }
        catch (Exception e)
        {
            HandleError(e.Message + "<br>" + e.StackTrace, consumer_id);
        }
    }

    private static void HandleError(String errorInfo, String event_id)
    {
        try
        {
            MemoryLog.Add("ERROR:" + errorInfo, event_id);
        }
        catch (Exception ex)
        {
            MemoryLog.Add("ERROR IN ERROR HANDLER:" + ex.Message, event_id);
        }
    }

    public static void RunConsumer(string consumer_id, string message_id, Dictionary<string, string> values)
    {
        if (!ConsumerUrls.ContainsKey(consumer_id))
            ConsumerUrls[consumer_id] = db.GetConsumerURL(consumer_id);

        var postdata = new NameValueCollection();
        foreach (string key in values.Keys)
            postdata[key] = values[key];
       
        try
        {
            var w = new Utility.WebAddress(ConsumerUrls[consumer_id]);
            w.PostParams = postdata;
            string result = w.Post();
            db.AddSucceededResult(consumer_id, message_id);
        }
        catch(Exception e)
        {
            db.AddFailedResult(consumer_id, message_id, e.Message);
        }
    }

   

}