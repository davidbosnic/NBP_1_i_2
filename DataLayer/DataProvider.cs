using Cassandra;
using DataLayer.QueryEntities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLayer
{
    public static class DataProvider
    {
        public static List<Poruka> GetPoruka(string senderid, string receiverid)
        {
            ISession session = SessionManager.GetSession();
            if (session == null)
                return null;

            var messages = session.Execute("select * from \"Poruka\" where senderid='"+senderid+"' and receiverid='"+receiverid+"'");

            List<Poruka> poruke = new List<Poruka>();

            if (messages!=null)
            {
                foreach (var item in messages)
                {
                    Poruka poruka = new Poruka();
                    poruka.senderid = item["senderid"] != null ? item["senderid"].ToString() : string.Empty;
                    poruka.receiverid = item["receiverid"] != null ? item["receiverid"].ToString() : string.Empty;
                    poruka.senttime = item["senttime"] != null ? item["senttime"].ToString() : string.Empty;
                    poruka.id = item["id"] != null ? item["id"].ToString() : string.Empty;
                    poruka.message = item["message"] != null ? item["message"].ToString() : string.Empty;
                    poruke.Add(poruka);
                }
            }
            return poruke;

        }

        public static List<Notifikacija> GetNotifikacija( string subscriberid)
        {
            ISession session = SessionManager.GetSession();
            if (session == null)
                return null;

            var messages = session.Execute("select * from \"Notifikacija\" where subscriberid='" + subscriberid+"'");

            List<Notifikacija> notifikacije = new List<Notifikacija>();

            if (messages != null)
            {
                foreach (var item in messages)
                {
                    Notifikacija poruka = new Notifikacija();
                    poruka.publisherid = item["publisherid"] != null ? item["publisherid"].ToString() : string.Empty;
                    poruka.subscriberid = item["subscriberid"] != null ? item["subscriberid"].ToString() : string.Empty;
                    poruka.senttime = item["senttime"] != null ? item["senttime"].ToString() : string.Empty;
                    poruka.id = item["id"] != null ? item["id"].ToString() : string.Empty;
                    
                    notifikacije.Add(poruka);
                }
            }
            return notifikacije;

        }


        public static void AddPoruka(string senderid, string receiverid, string id, string senttime, string message)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return;

            RowSet messageData = session.Execute("insert into \"Poruka\" (senderid, receiverid, id, senttime, message)  values ('" + senderid + "', '"+receiverid+"', '"+ GetCounter("poruka_id") + "', '"+senttime+"','"+message+"')");

            UpdateCounter("poruka_id");
        }

        public static void AddNotifikacija(string publisherid, string subscriberid, string id, string senttime)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return;

            RowSet notificationData = session.Execute("insert into \"Notifikacija\" (publisherid, subscriberid, id, senttime)  values ('" + publisherid + "', '" + subscriberid + "', '" + GetCounter("notifikacija_id") + "', '" + senttime + "')");

            UpdateCounter("notifikacija_id");
        }

        public static void DeleteNotifikacija(string id, string sid)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return;

            RowSet hotelData = session.Execute("delete from \"Notifikacija\" where \"subscriberid\" = '"+sid+"' and \"id\" = '" + id + "'");

        }


        public static string GetCounter(string x)
        {
            ISession session = SessionManager.GetSession();
            if (session == null)
                return null;

            Row counter = session.Execute("select * from \"ids\" where id_name = '" + x + "'").FirstOrDefault();

            return counter["next_id"].ToString();
        }

        public static void UpdateCounter(string x)
        {
            ISession session = SessionManager.GetSession();
            if (session == null)
                return;
            Row counter = session.Execute("select * from \"ids\" where id_name = '" + x + "'").FirstOrDefault();
            if(counter!=null)
            {
                var br = Convert.ToInt32(counter["next_id"].ToString());
                br++;
                var counterupdate = session.Execute("update \"ids\" set next_id = " + br+" where id_name = '" + x + "';");
            }
        }




    }
}
