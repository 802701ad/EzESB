using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Xml;
using MySql.Data.MySqlClient;
using System.Collections.Specialized;


public class db : Db_General
{
    public static DataTable EventsToRun()
    {
        var q = new QueryFormatter();
        q.src = @"
            SELECT *
		    FROM t_message m
            JOIN t_consumer c ON m.message_type LIKE c.message_type 
            WHERE
                NOT EXISTS
			    (
				    SELECT *
				    FROM t_message_delivery_log dl
				    WHERE c.consumer_id = dl.consumer_id
                    AND m.message_id = dl.message_id
			    )
            UNION ALL
            SELECT *
		    FROM t_message m
            JOIN t_consumer c ON m.message_type LIKE c.message_type 
            WHERE
                EXISTS
                (
				    SELECT *
				    FROM t_message_delivery_log dl
				    WHERE c.consumer_id = dl.consumer_id
                    AND next_retry < '#sysdate#'
			    )
		    ORDER BY sent_at, message_id, consumer_id
            ";
        q["sysdate"] = DbDate(DateTime.Now);
        return getDataTable(q.Format());
    }

        public static string GetConsumerURL(string consumer_id)
        {
            var q = new QueryFormatter();
            q.src = @"
                SELECT *
			    FROM t_consumer c
                WHERE c.consumer_id = '#consumer_id#'
               ";
            q["consumer_id"] = consumer_id;
            return getRow(q.Format())["dest_url"];
        }

        public static Dictionary<string, string> getValues(string message_id)
        {
            var q = new QueryFormatter();
            q.src = @"
                SELECT *
			    FROM t_message_values
                WHERE message_id = '#message_id#'
               ";
            q["message_id"] = message_id;
            var dt = getDataTable(q.Format());
            var d = new Dictionary<string, string>();
            foreach (DataRow dr in dt.Rows)
                d[Convert.ToString(dr["name"])] = Convert.ToString(dr["value"]);
            return d;
        }

        public static void SaveMessage(string message_id, string message_type, NameValueCollection values)
        {
            var q = new QueryFormatter();
            q["message_id"] = message_id;
            q["message_type"] = message_type;
            q["sysdate"] = DbDate(DateTime.Now);
            q.src = @"
               INSERT INTO t_message
                (message_id, message_type, sent_at)
                VALUES
                ('#message_id#', '#message_type#', '#sysdate#')       
                ";
            Execute(q.Format());

            foreach (string k in values.AllKeys)
            {
                q.src = @"
                INSERT INTO t_message_values
                (message_id, name, value)
                VALUES
                ('#message_id#', '#name#', '#value#')               
                ";
                q["name"] = k;
                q["value"] = values[k];
                Execute(q.Format());
            }


        }

        internal static void AddSucceededResult(string consumer_id, string message_id)
        {
            var q = new QueryFormatter();
            q.src = @"
                SELECT *
			    FROM t_message_delivery_log
                WHERE consumer_id = '#consumer_id#'
                      AND message_id = '#message_id#'
               ";
            q["message_id"] = message_id;
            q["consumer_id"] = consumer_id;
        q["last_delivery_attempt"] = DbDate(System.DateTime.Now);
            var r = getRow(q.Format());
            if (Convert.ToString(r["record_id"]) == "")
            {
                q.src = @"
                INSERT INTO t_message_delivery_log
                (record_id, message_id, consumer_id, status_code, last_delivery_attempt)
                VALUES
                ('#record_id#', '#message_id#', '#consumer_id#', '200', '#last_delivery_attempt#')               
                ";
                q["record_id"] = Guid.NewGuid().ToString();
                Execute(q.Format());
            }
            else
            {
                q.src = @"
                UPDATE t_message_delivery_log
                SET status_code = '200',
                    response_content = '',
                    next_retry = NULL,
                    last_delivery_attempt = '#last_delivery_attempt#'
                WHERE record_id = '#record_id#'
                ";
                q["record_id"] = Convert.ToString(r["record_id"]);
                Execute(q.Format());
            }
           
        }

    internal static void AddFailedResult(string consumer_id, string message_id, string error_message)
    {
        var q = new QueryFormatter();
        q.src = @"
            SELECT *
		    FROM t_message_delivery_log
            WHERE consumer_id = '#consumer_id#'
                    AND message_id = '#message_id#'
            ";
        q["message_id"] = message_id;
        q["consumer_id"] = consumer_id;
        q["response_content"] = error_message;
        var r = getRow(q.Format());
        DateTime next_try = DateTime.Now.AddDays(1.01);
        var retry_count = Convert.ToString(r["retry_count"]);
        switch (retry_count)
        {
            case "":
                case "": next_try = DateTime.Now.AddMinutes(15); break;
                break;
            case "1":
                case "1": next_try = DateTime.Now.AddMinutes(30); break;
                break;
            case "2":
                case "2": next_try = DateTime.Now.AddHours(1); break;
                break;
            case "3":
                case "3": next_try = DateTime.Now.AddHours(2); break;
                break;
            case "4":
                case "4": next_try = DateTime.Now.AddHours(4); break;
                break;
            case "5":
                case "5": next_try = DateTime.Now.AddHours(8); break;
                break;
            case "6":
                case "6": next_try = DateTime.Now.AddHours(12); break;
                break;
            case "7":
                case "7": next_try = DateTime.Now.AddHours(14); break;
                break;
            case "8":
                case "8": next_try = DateTime.Now.AddHours(16); break;
                break;
            case "9":
                case "9": next_try = DateTime.Now.AddHours(18); break;
                break;
            }
            q["next_retry"] = DbDate(next_try);
            q["last_delivery_attempt"] = DbDate(System.DateTime.Now);
            if (Convert.ToString(r["record_id"]) == "")
            {
                q.src = @"
                INSERT INTO t_message_delivery_log
                (record_id, message_id, consumer_id, status_code, response_content, retry_count, next_retry, last_delivery_attempt)
                VALUES
                ('#record_id#', '#message_id#', '#consumer_id#', '500', '#response_content#', 0, '#next_retry#', '#last_delivery_attempt#')               
                ";
                q["record_id"] = Guid.NewGuid().ToString();
                Execute(q.Format());
            }
            else
            {
                q.src = @"
                UPDATE t_message_delivery_log
                SET response_content = '#response_content#',
                    retry_count = '#retry_count#',
                    next_retry = NULL,
                    last_delivery_attempt = '#last_delivery_attempt#'
                WHERE record_id = '#record_id#'
                ";
                q["record_id"] = Convert.ToString(r["record_id"]);
                Execute(q.Format());
            }
           
        }
    }
