using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using vozCompetitionsLibrary.Model;

namespace vozCompetitionsLibrary.DataAccess
{
    public class AccessCompetition
    {
        public static List<Competition> Get()
        {
            using var reader = File.OpenText(ConfigGetter.CompetitionsPath());
            return JsonConvert.DeserializeObject<List<Competition>>(reader.ReadToEnd());
        }

        public static void Create(Competition competition)
        {
            List<Competition> Competitions = Get();
            Competitions.Add(competition);

            using var writer = File.CreateText(ConfigGetter.CompetitionsPath());
            writer.Write(JsonConvert.SerializeObject(Competitions, Formatting.Indented));
        }

        public static bool Close(string hashtag)
        {
            bool exist = false;
            List<Competition> Competitions = Get();
            for (int i = 0; i < Competitions.Count; i++)
                if (Competitions[i].Hashtag == hashtag)
                {
                    Competitions[i].Status = "Closed";
                    exist = true;
                    break;
                }
            using var writer = File.CreateText(ConfigGetter.CompetitionsPath());
            writer.Write(JsonConvert.SerializeObject(Competitions, Formatting.Indented));
            return exist; // return "true" if the competition exists
        }

        public static string GetStatus(string hashtag)
        {
            string status = "";
            foreach (Competition competition in Get())
                if (competition.Hashtag == hashtag)
                {
                    status = competition.Status;
                    break;
                }
            return status;
        }

        public static bool Exists(string hashtag)
        {
            foreach (Competition competition in Get())
                if (competition.Hashtag == hashtag)
                    return true;
            return false;
        }
    }
}
