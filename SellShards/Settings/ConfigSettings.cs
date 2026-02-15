using BepInEx.Configuration;
using TeamCherry.Localization;

namespace SellShards.Settings
{
    public static class ConfigSettings
    {
        /// <summary>
        /// Number of shell shards required for trade
        /// </summary>
        public static ConfigEntry<int> shardCost;

        /// <summary>
        /// Number of rosaries awarded in the trade
        /// </summary>
        public static ConfigEntry<int> rosaries;

        /// <summary>
        /// Initializes the settings
        /// </summary>
        /// <param name="config"></param>
        public static void Initialize(ConfigFile config)
        {
            // Shell Shards
            LocalisedString name = new LocalisedString($"Mods.{SellShards.Id}", "SHARD_NAME");
            LocalisedString description = new LocalisedString($"Mods.{SellShards.Id}", "SHARD_DESC");
            int defaultInt = 500;
            if (name.Exists &&
                description.Exists)
            {
                shardCost = config.Bind<int>("Modifier", name, defaultInt, description);
            }
            else
            {
                shardCost = config.Bind<int>("Modifier", "Shard Cost", defaultInt, "The number of shards required to trade");
            }

            // Rosaries
            name = new LocalisedString($"Mods.{SellShards.Id}", "ROSARY_NAME");
            description = new LocalisedString($"Mods.{SellShards.Id}", "ROSARY_DESC");
            defaultInt = 100;
            if (name.Exists &&
                description.Exists)
            {
                rosaries = config.Bind<int>("Modifier", name, defaultInt, description);
            }
            else
            {
                rosaries = config.Bind<int>("Modifier", "Rosaries", defaultInt, "The number of rosaries given per trade");
            }
        }
    }
}