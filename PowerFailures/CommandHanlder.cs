using System;
using System.Linq;
using Smod2.API;
using Smod2.Commands;
using Smod2.EventHandlers;
using Smod2.Events;

namespace PowerFailures.Properties
{
    public class CommandHanlder : ICommandHandler
    {
        private bool zoneSwitch;
        private PowerFailures plugin;
        private EventHandlers handler;
        
        public string[] OnCall(ICommandSender sender, string[] args)
        {
            if (zoneSwitch)
                return zoneBlackout(sender, args);
            return roomBlackout(sender, args);
        }

        private string[] zoneBlackout(ICommandSender sender, string[] args)
        {
            if (args.Length == 0)
                return new[] {"Error Wrong format"};

            if (args[0].ToUpper() != "HCZ" && args[0].ToUpper() != "LCZ")
                return new[] {$"Error unknown zone \"{args[0]}\""};
            
            
            int duration;
            if (args.Length > 1)
            {
                if (!Int32.TryParse(args[1], out duration))
                    return new[] {$"Error unknow value \"{args[1]}\""};
            }
            else
            {
                duration = plugin.GetConfigInt("pf_zone_duration");
            }

            Room[] rooms = null;
            switch (args[0].ToUpper())
            {
                case "HCZ":
                {
                    rooms = plugin.Server.Map.Get079InteractionRooms(Scp079InteractionType.CAMERA)
                        .Where(x => x.ZoneType == ZoneType.HCZ).ToArray();
                }
                 break;
                case "LCZ":
                {
                    rooms = plugin.Server.Map.Get079InteractionRooms(Scp079InteractionType.CAMERA)
                        .Where(x => x.ZoneType == ZoneType.LCZ).ToArray();
                }
                break;
            }

            if (rooms != null)
            {
                handler.addBlackoutRooms(rooms, duration);
                foreach (var room in rooms)
                {
                    room.FlickerLights();
                }
            }

            return new []{$"{args[0]} correctly blackout for {duration} seconds"};
        }



        private string[] roomBlackout(ICommandSender sender,string[] args)
        {
            if (args.Length == 0)
                return new[] {"Error Wrong format"};
            
            int duration;
            if (args.Length > 1)
            {
                if (!Int32.TryParse(args[1], out duration))
                    return new[] {$"Error unknow value \"{args[1]}\""};
            }
            else
            {
                duration = plugin.GetConfigInt(zoneSwitch?"pf_zone_duration":"pf_duration");
            }

            Room room = plugin.Server.Map.Get079InteractionRooms(Scp079InteractionType.CAMERA)
                .Where(x => x.RoomType.ToString() == args[0].ToUpper()).DefaultIfEmpty(null).First();
            
            if(room==null)
                return new []{$"Unknown room {args[0]}"};
            
            room.FlickerLights();
            
            handler.addBlackoutRooms(new[]{room},duration);
            
            return new []{$"{args[0]} correctly blackout for {duration} seconds"};;
        }
        
        
        

        public string GetUsage()
        {
            if (zoneSwitch)
            {
                return "pf_zone [ HCZ/LCZ ] ( duration )";
            }

            return "pf_room [ room name ] ( duration ) ";
        }

        public string GetCommandDescription()
        {
            if (zoneSwitch)
            {
                return "generates a zone blackout in the specified zone";
            }

            return "generates a room blackout in the specified room";
        }


        public CommandHanlder(PowerFailures plugin,EventHandlers handler,bool zoneSwitch)
        {
            this.zoneSwitch = zoneSwitch;
            this.plugin = plugin;
            this.handler = handler;
        }
    }
}