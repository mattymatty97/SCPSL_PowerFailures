using System.Dynamic;
using PowerFailures.Properties;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Config;
using Smod2.Events;

namespace PowerFailures
{
    [PluginDetails(
        name = "Power failures",
        author = "The Matty",
        description = "Creates random blackouts around facility",
        id = "mattymatty.powerfailures",
        SmodMajor = 3,
        SmodMinor = 2,
        SmodRevision = 0,
        version = "0.0.1"
            )]
    public class PowerFailures : Plugin
    {
        public EventHandlers Handlers { get; set; }

        public override void Register()
        {
           
            AddConfig(new ConfigSetting("pf_n_rooms", 7,true, SettingType.NUMERIC, true,"Numeber of rooms that will get blackouts"));
            AddConfig(new ConfigSetting("pf_n_rooms_decont", 4,true, SettingType.NUMERIC, true,"Numeber of rooms that will get blackouts after decontamination"));
            AddConfig(new ConfigSetting("pf_max_time", 120, SettingType.NUMERIC, true,"max seconds betwen one fault and another"));
            AddConfig(new ConfigSetting("pf_min_time", 30, SettingType.NUMERIC, true,"min seconds betwen one fault and another"));
            AddConfig(new ConfigSetting("pf_max_zone_time", 500, SettingType.NUMERIC, true,"max seconds betwen one entire zone fault and another"));
            AddConfig(new ConfigSetting("pf_min_zone_time", 300, SettingType.NUMERIC, true,"max seconds betwen one entire zone fault and another"));
            AddConfig(new ConfigSetting("pf_zone_duration", 30,true,SettingType.NUMERIC, true,"how much time does the blackout persist ( entire zone )"));
            AddConfig(new ConfigSetting("pf_duration", 10,true,SettingType.NUMERIC, true,"how much time does the blackout persist ( single room )"));

            Handlers = new EventHandlers(this);

            AddEventHandlers(Handlers, Priority.Lowest);
            AddCommand("pf_room",new CommandHanlder(this,Handlers,false));
            AddCommand("pf_zone",new CommandHanlder(this,Handlers,true));
        }
        
        public override void OnEnable()
        {
            Info("Power failures enabled!");
        }

        public override void OnDisable()
        {
            Info("Power failures disabled!");
        }

    }
}
