using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// simple utility class to read and parse r3e multiplayer ratings, allowing drivers' rank and reputation rating to be retrieved
/// </summary>
namespace CrewChiefV4.R3E
{
    public class R3ERatings
    {
        private static String urlPropName = "r3e_ratings_url";
        private static Dictionary<int, R3ERatingData> ratingDataForUserId = new Dictionary<int, R3ERatingData>();
        public static bool gotPlayerRating = false;
        public static R3ERatingData playerRating = null;

        public static void getRatingForPlayer(int userId)
        {
            playerRating = getRatingForUserId(userId);
            gotPlayerRating = true;
            if (playerRating != null)
            {
                Console.WriteLine("Got user rating data: " + playerRating.ToString());
            }
        }

        public static R3ERatingData getRatingForUserId(int userId)
        {
            // don't attempt a lookup for AI drivers, who have user ID -1
            if (userId != -1)
            {
                R3ERatingData r3ERatingData = null;
                if (ratingDataForUserId.TryGetValue(userId, out r3ERatingData))
                {
                    return r3ERatingData;
                }
            }
            return null;
        }
        
        public static void init()
        {
            string url = UserSettings.GetUserSettings().getString(urlPropName);
            if (url != null && url.Trim().Length > 0)
            {
                try
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    WebClient client = new WebClient();
                    string ratingsJson = client.DownloadString(url);
                    stopwatch.Stop();
                    Console.WriteLine("Downloaded driver rating profiles from " + url + " in " + stopwatch.ElapsedMilliseconds + "ms");
                    stopwatch.Reset();
                    stopwatch.Start();
                    R3ERatingData[] rawData = Newtonsoft.Json.JsonConvert.DeserializeObject<R3ERatingData[]>(ratingsJson);
                    // use the ordering of the list to get the ranking
                    int rank = 1;
                    foreach (R3ERatingData data in rawData)
                    {
                        data.rank = rank;
                        ratingDataForUserId[data.userId] = data;
                        rank++;
                    }
                    stopwatch.Stop();
                    Console.WriteLine("Processed " + rawData.Length + " driver rating profiles in " + stopwatch.ElapsedMilliseconds + "ms");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to get R3E ranking data from " + url + " error: " + e.StackTrace);
                }
            }
        }
    }
    
    public class R3ERatingData
    {
        public string username;
        public string fullName;
        public int userId;
        public int racesCompleted;
        public float reputation;
        public float rating;
        public int rank;

        public override string ToString()
        {
            return "username " + username + ", full name " + fullName + ", userId = " + userId + ", racesCompleted = " +
                racesCompleted + ", reputation = " + reputation + ", rating = " + rating + ", rank = " + rank;
        }
    }
}
