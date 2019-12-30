using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using vozCompetitionsLibrary.Model;

namespace vozCompetitionsLibrary.DataAccess
{
    public class AccessVote
    {
        public static List<Vote> Get()
        {
            using var reader = File.OpenText(ConfigGetter.VotesPath());
            return JsonConvert.DeserializeObject<List<Vote>>(reader.ReadToEnd());
        }

        public static void Add(Vote vote)
        {
            List<Vote> Votes = Get();
            Votes.Add(vote);

            using var writer = File.CreateText(ConfigGetter.VotesPath());
            writer.Write(JsonConvert.SerializeObject(Votes, Formatting.Indented));
        }

        public static bool Exists(Vote vote)
        {
            bool exists = false;
            foreach (Vote v in Get())
                if (v.ChatId == vote.ChatId && v.MessageId == vote.MessageId && v.UserId == vote.UserId)
                {
                    exists = true;
                    break;
                }
            return exists;
        }
    }
}
