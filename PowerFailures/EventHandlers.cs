
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;

namespace PowerFailures
{
    public class EventHandlers :  IEventHandlerWaitingForPlayers, IEventHandlerRoundStart, IEventHandlerLCZDecontaminate, IEventHandlerFixedUpdate,IEventHandlerWarheadDetonate
    {
        private readonly PowerFailures plugin;

        private DateTime time_blackout = DateTime.MaxValue;
        private DateTime time_zone_blackout = DateTime.MaxValue;
        private DateTime lightCheck = DateTime.MaxValue;
        private Random rnd = new Random();
        private bool decont = false;
        private bool detonate = false;
        private List<Room> rooms;
        private Dictionary<Room,DateTime> blackouts = new Dictionary<Room, DateTime>();
        public EventHandlers(PowerFailures plugin)
        {
            this.plugin = plugin;
        }

        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            time_blackout = DateTime.MaxValue;
            time_zone_blackout = DateTime.MaxValue;
            lightCheck = DateTime.MaxValue;
            decont = false;
            detonate = false;
            rooms = ev.Server.Map.Get079InteractionRooms(Scp079InteractionType.CAMERA).Where(x=>x.ZoneType == ZoneType.HCZ || x.ZoneType == ZoneType.LCZ).ToList();
            lock (blackouts)
            {
                blackouts.Clear();
            }
        }

        public void OnRoundStart(RoundStartEvent ev)
        {
            time_blackout = DateTime.Now.AddSeconds(rnd.Next(plugin.GetConfigInt("pf_min_time"),plugin.GetConfigInt("pf_max_time")));
            time_zone_blackout = DateTime.Now.AddSeconds(rnd.Next(plugin.GetConfigInt("pf_min_zone_time"),plugin.GetConfigInt("pf_max_zone_time")));
            lightCheck = DateTime.Now;
            plugin.Debug($"Next room blakout at: {time_blackout.ToString()}");
            plugin.Debug($"Next zone blakout at: {time_zone_blackout.ToString()}");
        }


        public void OnDecontaminate()
        {
            decont = true;
        }
        
        

        public void OnDetonate()
        {
            detonate = true;
        }
        
        public void OnFixedUpdate(FixedUpdateEvent ev)
        {
            if (!detonate)
            {
                DateTime now = DateTime.Now;
                if (now > time_blackout)
                {
                    time_blackout = DateTime.MaxValue;
                    new Thread(RandomRoomBlackout).Start();
                }

                if (now > time_zone_blackout)
                {
                    time_zone_blackout = DateTime.MaxValue;
                    new Thread(ZoneBlackout).Start();
                }

                if (now > lightCheck)
                {
                    lightCheck = DateTime.MaxValue;
                    new Task(KeepBlackout).Start();
                }
            }
        }

        private void RandomRoomBlackout()
        {
            List<Room> rooms;
            int n_rooms;
            if (decont)
            {
                rooms = this.rooms.Where(x => x.ZoneType == ZoneType.HCZ).ToList();
                n_rooms = plugin.GetConfigInt("pf_n_rooms_decont");
            }
            else{
                rooms = this.rooms.ToList();
                n_rooms = plugin.GetConfigInt("pf_n_rooms");
            }

            Room[] blackout = rooms.OrderBy(x=>rnd.Next(100)).Take(Math.Min(n_rooms,rooms.Count)).ToArray();

            /*foreach (Room room in blackout)
            {
                room.FlickerLights();
                
            }*/
           
            addBlackoutRooms(blackout,plugin.GetConfigInt("pf_duration"));

            time_blackout = DateTime.Now.AddSeconds(rnd.Next(plugin.GetConfigInt("pf_min_time"),plugin.GetConfigInt("pf_max_time")));
            plugin.Debug($"Blacked out {String.Join(",",blackout.Select(x=>x.RoomType.ToString()))}");
            plugin.Debug($"Next room blakout at: {time_blackout.ToString()}");
        }

        private void ZoneBlackout()
        {
            ZoneType zone;
            if (!decont)
            {
                if (rnd.Next(0, 1) > 0)
                {
                    zone = ZoneType.HCZ;
                }
                else{
                    zone = ZoneType.LCZ;
                }
            }
            else
            {
                zone = ZoneType.HCZ;
            }

            Room[] blackout = rooms.Where(x => x.ZoneType == zone).ToArray();

           /*foreach (Room room in blackout)
           {
               room.FlickerLights();
           }*/
            
           addBlackoutRooms(blackout,plugin.GetConfigInt("pf_zone_duration"));

            time_zone_blackout = DateTime.Now.AddSeconds(rnd.Next(plugin.GetConfigInt("pf_min_zone_time"),plugin.GetConfigInt("pf_max_zone_time")));
            plugin.Debug($"Blacked out {zone.ToString()}");
            plugin.Debug($"Next zone blakout at: {time_zone_blackout.ToString()}");
        }
        public void KeepBlackout()
        {
            DateTime now = DateTime.Now;

            lock (blackouts)
            {
                blackouts = blackouts.Where(p => p.Value > now).Select(p =>
                {
                    p.Key.FlickerLights();
                    return p;
                }).ToDictionary(p=>p.Key,p=>p.Value);
            }
            
            lightCheck = DateTime.Now.AddSeconds(8);
        }

        public void addBlackoutRooms(Room[] rooms, int duration)
        {
            lock (blackouts)
            {
                foreach (var room in rooms)
                {
                    if (blackouts.ContainsKey(room))
                    {
                        if (blackouts[room] < DateTime.Now.AddSeconds(duration))
                            blackouts[room] = DateTime.Now.AddSeconds(duration);
                    }
                    else
                    {
                        blackouts.Add(room,DateTime.Now.AddSeconds(duration));
                    }
                }
            }
        }
    }
}
