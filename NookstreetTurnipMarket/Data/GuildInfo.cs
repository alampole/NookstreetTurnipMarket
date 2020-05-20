using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Entities;

namespace NookstreetTurnipMarket.Data
{
    public class GuildInfo
    {
        private DiscordGuild m_Guild;
        private DateTime m_LastDatabaseUpdate;

        public DiscordGuild Guild { get { return m_Guild; } }
        public ulong ID { get { return m_Guild.Id; } }
        public DateTime LastDatabaseUpdate { get { return m_LastDatabaseUpdate; } }

        public GuildInfo(DiscordGuild aGuild)
        {
            m_Guild = aGuild;
        }

        public void Update()
        {
            m_LastDatabaseUpdate = DateTime.Now;
        }
    }
}
