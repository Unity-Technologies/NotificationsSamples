using System;
using System.Collections;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

namespace NotificationSamples.Demo
{
    /// <summary>
    /// Read a news feed (RSS).
    /// </summary>
    public class NewsFeedReader : MonoBehaviour
    {
        /// <summary>
        /// A single news item.
        /// </summary>
        public class NewsItem
        {
            public string Title;
            public string Description;
        }

        /// <summary>
        /// Get the first news item from the RSS URL.
        /// </summary>
        /// <param name="url">RSS URL.</param>
        /// <param name="doneAction">Action to fire when the process is done. The item will be null if it failed.</param>
        public void GetFirstItem(string url, Action<NewsItem> doneAction)
        {
            StartCoroutine(GetFirstItemInternal(url, doneAction));
        }

        // Get the first news item from the RSS URL.
        private IEnumerator GetFirstItemInternal(string url, Action<NewsItem> doneAction)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogErrorFormat("Failed to get the feed from the url. ERROR: {0}", www.error);
                doneAction?.Invoke(null);
                yield break;
            }

            XmlDocument document = new XmlDocument();
            try
            {
                document.LoadXml(www.downloadHandler.text);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Failed to extract the XML. ERROR: {0}", e);
                doneAction?.Invoke(null);
                yield break;
            }

            XmlNode channel = FindNode(document.FirstChild, "channel");
            if (channel == null || !channel.HasChildNodes)
            {
                Debug.LogError("XML does not have a channel node.");
                doneAction?.Invoke(null);
                yield break;
            }

            XmlNode channelTitle = FindNode(channel.FirstChild, "title");
            XmlNode item = FindNode(channel.FirstChild, "item");
            if (item == null || !item.HasChildNodes)
            {
                Debug.LogError("First item is null or has no children.");
                doneAction?.Invoke(null);
                yield break;
            }
            XmlNode title = FindNode(item.FirstChild, "title");
            XmlNode description = FindNode(item.FirstChild, "description");
            if (title == null || description == null)
            {
                Debug.LogErrorFormat("Item ({0}) does not have a title or description.", item.Name);
                doneAction?.Invoke(null);
                yield break;
            }

            NewsItem newsItem = new NewsItem
            {
                Title = channelTitle != null ? channelTitle.InnerText : title.InnerText,
                Description = title.InnerText
            };
            doneAction?.Invoke(newsItem);
        }

        // Find the node with the specified name. The search starts at the first node and checks its children,
        // siblings and children's siblings.
        private XmlNode FindNode(XmlNode firstNode, string nodeName)
        {
            XmlNode result = firstNode;
            while (result != null)
            {
                if (result.Name == nodeName)
                {
                    return result;
                }

                // Check children
                if (result.HasChildNodes)
                {
                    XmlNode childNode = FindNode(result.FirstChild, nodeName);
                    if (childNode != null)
                    {
                        return childNode;
                    }
                }

                result = result.NextSibling;
            }
            return null;
        }
    }
}
