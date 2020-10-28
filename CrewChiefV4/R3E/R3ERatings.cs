using CrewChiefV4.GameState;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
        private static bool initialized = false;
        private static object initLock = new object();
        private static bool ratingDownloadCompleted = false;

        public static bool gotPlayerRating = false;
        public static R3ERatingData playerRating = null;

        public static void getRatingForPlayer(int userId)
        {
            if (!R3ERatings.ratingDownloadCompleted)
            {
                return;
            }

            if (userId > 0)
            {
                playerRating = getRatingForUserId(userId);
                gotPlayerRating = true;
                if (playerRating != null)
                {
                    Console.WriteLine("Got user rating data: " + playerRating.ToString());
                }
            }
        }

        public static R3ERatingData getRatingForUserId(int userId)
        {
            if (!R3ERatings.ratingDownloadCompleted)
            {
                return null;
            }

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

        public static int getAverageRatingForParticipants(Dictionary<string, OpponentData> opponentData)
        {
            if (opponentData == null)
            {
                return -1;
            }
            float average = 0;
            float count = 0;
            foreach (OpponentData opponent in opponentData.Values)
            {
                if (opponent.r3eUserId != -1)
                {
                    R3ERatingData data = getRatingForUserId(opponent.r3eUserId);
                    if (data != null)
                    {
                        average += data.rating;
                        count++;
                    }
                }
            }
            return count == 0 ? -1 : (int) (average / count);
        }
        
        public static void init()
        {
            lock (R3ERatings.initLock)
            {
                // Download ratings only once per CC session.
                if (R3ERatings.initialized)
                {
                    return;
                }

                R3ERatings.initialized = true;
            }

            var ratingDownloadThread = new Thread(() =>
            {
                string url = UserSettings.GetUserSettings().getString(urlPropName);
                if (url != null && url.Trim().Length > 0)
                {
                    try
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
                        string ratingsJson = R3ERatings.download(url);
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
                        Console.WriteLine("Unable to get R3E ranking data from " + url + " error: " + e.Message);
                    }
                    finally
                    {
                        R3ERatings.ratingDownloadCompleted = true;
                    }
                }
            });

            ThreadManager.RegisterTemporaryThread(ratingDownloadThread);
            ratingDownloadThread.Name = "R3ERatings.init";
            ratingDownloadThread.Start();
        }

        private static string download(string url)
        {
            string ratingsJson = null;
            using (WebClient client = new WebClient())
            {
                try
                {
                    ratingsJson = client.DownloadString(url);
                }
                catch (Exception e)
                {
                    // nasty error handling, 404 or SSL / TLS error -> toggle http / https and retry
                    if (e.Message.Contains("404") || (url.Contains("https") && (e.Message.Contains("TLS") || e.Message.Contains("SSL"))))
                    {
                        // try toggling HTTPS
                        string retryUrl = null;
                        if (url.Contains("http:"))
                        {
                            retryUrl = url.Replace("http", "https");
                        }
                        else if (url.Contains("https"))
                        {
                            retryUrl = url.Replace("https", "http");
                        }
                        if (retryUrl != null)
                        {
                            Console.WriteLine("Unable to find ratings at " + url + " trying " + retryUrl);
                            ratingsJson = client.DownloadString(retryUrl);
                        }
                    }
                    else
                    {
                        // not a 404 or TLS / SSL error, rethrow the exception from the first try
                        throw (e);
                    }
                }
            }
            return ratingsJson;
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
