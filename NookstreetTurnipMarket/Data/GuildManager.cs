using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace NookstreetTurnipMarket.Data
{
    class GuildManager
    {
        private static List<GuildInfo> m_Guilds = new List<GuildInfo>();
        public static List<GuildInfo> Guilds { get { return m_Guilds; } set { m_Guilds = value; } }

        public static DiscordMember GetMember(ulong aUserID)
        {
            foreach (GuildInfo guild in m_Guilds)
            {
                if (guild.Guild.Members.ContainsKey(aUserID))
                    return guild.Guild.Members[aUserID];
            }

            return null;
        }

        public static List<DiscordGuild> GetGuildsUserBelongsTo(ulong aUserID)
        {
            List<DiscordGuild> guilds = new List<DiscordGuild>();

            foreach (GuildInfo guild in m_Guilds)
            {
                if (guild.Guild.Members.ContainsKey(aUserID))
                    guilds.Add(guild.Guild);
            }
            
            return guilds;
        }

        public static GuildInfo GetGuild(ulong aID)
        {
            foreach (GuildInfo guild in m_Guilds)
            {
                if (guild.ID == aID)
                    return guild;
            }

            return null;
        }

        public static void AddGuild(DiscordGuild aGuild)
        {
            m_Guilds.Add(new GuildInfo(aGuild));

            if (!DatabaseManager.TableExists(aGuild.Id))
                DatabaseManager.CreateTable(aGuild.Id);
        }

        public static void GuildTableUpdated(ulong aGuild)
        {
            for (int i = 0; i < m_Guilds.Count; i++)
            {
                if (m_Guilds[i].ID == aGuild)
                {
                    m_Guilds[i].Update();
                    break;
                }
            }
        }

        public static DateTime LastUpdate(ulong aGuildID)
        {
            for (int i = 0; i < m_Guilds.Count; i++)
            {
                if (m_Guilds[i].ID == aGuildID)
                {
                    return m_Guilds[i].LastDatabaseUpdate;
                }
            }

            return DateTime.Now;
        }
    }
}