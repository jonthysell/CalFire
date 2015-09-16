// 
// WebWrapper.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015 Jon Thysell <http://jonthysell.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace CalFire
{
    public class WebWrapper
    {
        public static Incident GetIncidentDetails(int incidentId)
        {
            Task<Incident> task = GetIncidentDetailsAsync(incidentId);
            task.Wait();

            if (task.IsFaulted)
            {
                throw task.Exception;
            }

            return task.Result;
        }

        public static async Task<Incident> GetIncidentDetailsAsync(int incidentId)
        {
            if (incidentId < 0)
            {
                throw new ArgumentOutOfRangeException("incidentId");
            }

            // Download the webpage
            string url = String.Format(GetIncidentDetailsUrl, incidentId);
            HtmlWeb web = new HtmlWeb();

            HtmlDocument doc = await web.LoadFromWebAsync(url);

            string name = "";
            Dictionary<string, string> properties = null;

            // Find incident name
            foreach (HtmlNode headerNode in doc.DocumentNode.Descendants("h3"))
            {
                if (headerNode.HasAttributes && 
                    headerNode.Attributes.Contains("class") &&
                    headerNode.Attributes["class"].Value == "incident_h3")
                {
                    name = headerNode.InnerText;
                    break;
                }
            }

            // Find incident properties
            foreach (HtmlNode tableNode in doc.DocumentNode.Descendants("table"))
            {
                if (tableNode.HasAttributes &&
                    tableNode.Attributes.Contains("class") &&
                    tableNode.Attributes["class"].Value == "incident_table")
                {
                    properties = ParseIncidentTable(tableNode);
                    break;
                }
            }

            return new Incident(incidentId, name, properties);
        }

        // Incident ID
        private static string GetIncidentDetailsUrl = "http://cdfdata.fire.ca.gov/incidents/incidents_details_info?incident_id={0}";

        public static IEnumerable<Incident> GetIncidents(int currentPage = IncidentsDefaultCurrentPage, int itemsPerPage = IncidentsDefaultItemsPerPage, IncidentsSortOrder sortOrder = IncidentsDefaultSortOrder)
        {
            Task<IEnumerable<Incident>> task = GetIncidentsAsync(currentPage, itemsPerPage, sortOrder);
            task.Wait();

            if (task.IsFaulted)
            {
                throw task.Exception;
            }

            return task.Result;
        }

        public static async Task<IEnumerable<Incident>> GetIncidentsAsync(int currentPage = IncidentsDefaultCurrentPage, int itemsPerPage = IncidentsDefaultItemsPerPage, IncidentsSortOrder sortOrder = IncidentsDefaultSortOrder)
        {
            if (currentPage < 0)
            {
                throw new ArgumentOutOfRangeException("currentPage");
            }

            if (itemsPerPage < 0)
            {
                throw new ArgumentOutOfRangeException("itemsPerPage");
            }

            string sortOrderString = "";
            switch (sortOrder)
            {
                case IncidentsSortOrder.Name:
                    sortOrderString = "incident_name";
                    break;
                case IncidentsSortOrder.County:
                    sortOrderString = "incident_county";
                    break;
                case IncidentsSortOrder.AdministrativeUnit:
                    sortOrderString = "incident_administrative_unit";
                    break;
                case IncidentsSortOrder.DateStarted:
                    sortOrderString = "incident_date_created";
                    break;
                case IncidentsSortOrder.DateLastUpdated:
                    sortOrderString = "incident_date_last_update";
                    break;
                case IncidentsSortOrder.Priority:
                default:
                    sortOrderString = "incident_priority";
                    break;
            }

            // Download the webpage
            string url = String.Format(GetIncidentsUrl, currentPage, itemsPerPage, sortOrderString);
            HtmlWeb web = new HtmlWeb();

            HtmlDocument doc = await web.LoadFromWebAsync(url);

            List<Incident> incidents = new List<Incident>();

            // Find incident tables
            foreach (HtmlNode tableNode in doc.DocumentNode.Descendants("table"))
            {
                if (tableNode.HasAttributes &&
                    tableNode.Attributes.Contains("class") &&
                    tableNode.Attributes["class"].Value == "incident_table")
                {
                    string name = tableNode.Attributes["title"].Value;
                    if (name != "Search for a fire")
                    {
                        int id = Incident.InvalidId;
                        Dictionary<string, string> details = ParseIncidentTable(tableNode);

                        // Parse out the hidden id
                        if (details.ContainsKey(IncidentIDKey))
                        {
                            id = Int32.Parse(details[IncidentIDKey]);
                        }

                        Incident incident = (id == Incident.InvalidId) ? new Incident(name, details) : new Incident(id, name, details);
                        incidents.Add(incident);
                    }
                }
            }

            return incidents;
        }

        // CurrentPage, ItemsPerPage, SortOrder
        private static string GetIncidentsUrl = "http://cdfdata.fire.ca.gov/incidents/incidents_current?cp={0}&pc={1}&sort={2}";

        private static Dictionary<string, string> ParseIncidentTable(HtmlNode tableNode)
        {
            if (null == tableNode)
            {
                throw new ArgumentNullException("tableNode");
            }

            Dictionary<string, string> details = new Dictionary<string, string>();

            foreach (HtmlNode rowNode in tableNode.Descendants("tr"))
            {
                if (rowNode.HasAttributes &&
                    rowNode.Attributes.Contains("class") &&
                    rowNode.Attributes["class"].Value == "header_tr")
                {
                    // Try to get ID for details later
                    foreach (HtmlNode aNode in rowNode.Descendants("a"))
                    {
                        if (aNode.HasAttributes &&
                            aNode.Attributes.Contains("href") &&
                            aNode.Attributes["href"].Value.Contains("incident_id=") &&
                            !details.ContainsKey(IncidentIDKey))
                        {
                            int id_index = aNode.Attributes["href"].Value.IndexOf("incident_id=");

                            string id = aNode.Attributes["href"].Value.Substring(id_index + "incident_id=".Length);
                            details.Add(IncidentIDKey, id);
                            break;
                        }
                    }
                }
                else if (rowNode.HasAttributes &&
                         rowNode.Attributes.Contains("class") &&
                         (rowNode.Attributes["class"].Value == "odd" || rowNode.Attributes["class"].Value == "even"))
                {
                    string propertyKey = null;
                    string propertyValue = null;
                    foreach (HtmlNode cellNode in rowNode.Descendants("td"))
                    {
                        if (cellNode.HasAttributes &&
                            cellNode.Attributes.Contains("class") &&
                            cellNode.Attributes["class"].Value == "emphasized" &&
                            null == propertyKey)
                        {
                            propertyKey = cellNode.InnerText.Replace("&nbsp;", " ").Trim(':', ' ');
                        }
                        else if (null != propertyKey && null == propertyValue)
                        {
                            propertyValue = cellNode.InnerText.Replace("&nbsp;", " ").Trim();
                            if (details.ContainsKey(propertyKey))
                            {
                                details[propertyKey] += Environment.NewLine + propertyValue;
                            }
                            else
                            {
                                details.Add(propertyKey, propertyValue);
                            }
                            break;
                        }
                    }
                }
            }

            return details;
        }

        private const string IncidentIDKey = "_IncidentID";

        public const int IncidentsDefaultCurrentPage = 0;
        public const int IncidentsDefaultItemsPerPage = 5;
        public const IncidentsSortOrder IncidentsDefaultSortOrder = IncidentsSortOrder.Priority;
    }

    public enum IncidentsSortOrder
    {
        Priority,
        Name,
        County,
        AdministrativeUnit,
        DateStarted,
        DateLastUpdated
    }
}
