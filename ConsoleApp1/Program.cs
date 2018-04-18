using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Octokit;

namespace RepoParser
{
    class Program
    {
        private static List<AppData> finalList; // Final List of all Data
        private static List<string> dataAsStringList;


        static void Main(string[] args)
        {
            // Initialize Final List
            finalList = new List<AppData>();
            dataAsStringList = new List<string>();
            
            // Make and Open XML Doc
            XmlDocument doc = new XmlDocument();
            doc.Load("C:/Users/z003vu9m/source/repos/ConsoleApp1/ConsoleApp1/index.xml");

            // Extract List of all applications
            XmlNodeList allApps =  doc.SelectNodes("/fdroid/application");

            // List of all App Data
            List<AppData> appDataList = new List<AppData>();

            // Parse each application node
            foreach (XmlNode node in allApps)
            {
                // Make new Dataset and add ID + Source Url.
                AppData data = new AppData();
                data.ID = node["id"].InnerText;
                data.Source = node["source"].InnerText;

                // Add to List.
                appDataList.Add(data);
            }

            // Extract all usernames and repo names
            foreach(AppData data in appDataList)
            {
                if (data.Source.Contains("github")) // Has Github Source
                {
                    data.Source = data.Source.Replace("https://github.com/", "");
                    string[] urlComponents = data.Source.Split('/');
                    data.Owner = urlComponents[0];
                    data.RepoName = urlComponents.Length > 1 ? urlComponents[1] : "N/A";
                }
                else // NOT GITHUB REPO.
                {
                    data.Owner = "N/A";
                    data.RepoName = "N/A";
                }
            }

            // Parse each individual App
            foreach(AppData data in appDataList)
            {
                getContributorsAsync(data).Wait();
                Console.WriteLine("Now processing App Nr. " + finalList.Count);
                File.WriteAllLines("C:/Users/z003vu9m/source/repos/ConsoleApp1/ConsoleApp1/output.txt", dataAsStringList);
            }

            // Rename .txt file to .csv file when this is finished, it can then be opened in Excel/Other Office applications with proper columns/rows.
            Console.WriteLine("Finished. Press any Key to Exit...");
            Console.ReadKey();
        }

        static async Task getContributorsAsync(AppData data)
        {
            if(data.Owner == "N/A" || data.RepoName == "N/A") // Something went wrong or not Github Repo.
            {
                Console.WriteLine("Not a Github repo.");
                data.Authors = "N/A";
                finalList.Add(data);
                return;
            }

            //  This starts a GitHubClient. Not sure why I have to pass my username like that.. said so on Library Doc.
            var client = new Octokit.GitHubClient(new ProductHeaderValue("ytrehorn"));

            // OAuth Token authentification. Necessary to have more than 60 requests/h. With token 5000 per hour.
            var tokenAuth = new Octokit.Credentials("60a58f7387e72c630f2fc0e073a86a595ce78efb"); // NOTE: Token for "ytrehorn". Please don't abuse :)
            client.Credentials = tokenAuth; // Save token into current client session.

            // Read Only List of all the contributors.
            IReadOnlyList<RepositoryContributor> contributorList;

            try // Tries to read Contributors. If repo doesn't exist or no contributors available, will throw an exception.
            {
                contributorList = await client.Repository.GetAllContributors(data.Owner, data.RepoName);
            }
            catch(Exception e) // Catch exception and log it in data.Authors. This will then show in the output.
            {
                data.Authors = "Error: " + e.Message + ".";
                finalList.Add(data);
                dataAsStringList.Add(data.ID + "; " + data.Owner + "; " + data.RepoName + "; " + data.Authors);
                return;
            }
            

            try // Try making a combined author string. try/catch for safety, not sure if necessary..
            {
                string combinedAuthors = "";
                int counter = 1;
                foreach (RepositoryContributor x in contributorList)
                {
                    if (counter == contributorList.Count)
                        combinedAuthors += x.Login;
                    else
                        combinedAuthors += x.Login + " ,";

                    counter++;
                }

                data.Authors = combinedAuthors;
            }
            catch(Exception e)
            {
                Console.WriteLine("Error in " + data.RepoName + ": " + e.Message);
                data.Authors = "N/A";
            }

            // Add to List<Data>. Isn't really used but maybe useful for other people instead of just a .txt file.
            finalList.Add(data);
            // Add all variables of data to one string. Using ";" as .csv delimiter.
            dataAsStringList.Add(data.ID + "; " + data.Owner + "; " + data.RepoName + "; " + data.Authors);
        }
    }
}
