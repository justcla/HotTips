using System;
using System.Collections.Generic;

namespace HotTips
{

    //------ Raw objects for parsing from groups json --------------
    public class TipGroup
    {
        public string groupId { get; set; }
        public string groupName { get; set; }
        public int groupPriority { get; set; }
        public List<Tip> tips { get; set; }
    }

    public class Tip
    {
        public string tipId { get; set; }
        public string name { get; set; }
        public string content { get; set; }
        public int priority { get; set; }
    }

    //---------- Ojbects used internally by Tip Manager ----------------------------------------

    public class TipInfo
    {
        public string globalTipId { get; set; }
        public string tipId { get; set; }
        public string name { get; set; }
        public int priority { get; set; }
        public string contentUri { get; set; }
        public string groupId { get; set; }
        public string groupName { get; set; }
        public int groupPriority { get; set; }

        public static TipInfo Create(TipGroup tipGroup, Tip tip, string tipContentUri)
        {
            TipInfo tipInfo = new TipInfo
            {
                tipId = tip.tipId,
                name = tip.name,
                priority = tip.priority,
                contentUri = tipContentUri,
                groupId = tipGroup.groupId,
                groupName = tipGroup.groupName,
                groupPriority = tipGroup.groupPriority,
                globalTipId = GetGlobalTipId(tipGroup.groupId, tip.tipId)
            };
            return tipInfo;
        }

        public static string GetGlobalTipId(string groupId, string tipId)
        {
            return groupId + "-" + tipId;
        }

        public static string GetGroupId(string globalTipId)
        {
            // TODO: Error Check
            return globalTipId.Split('-')[0];
        }

        public static string GetTipId(string globalTipId)
        {
            // TODO: Error Check
            return globalTipId.Split('-')[1];
        }
    }

    public class TipHistoryInfo
    {
        public string globalTipId { get; set; }

        public TipLikeEnum tipLikeStatus { get; set; }

        public override string ToString()
        {
            return string.Concat(globalTipId, ':', (int)tipLikeStatus);
        }

        public TipHistoryInfo()
        {

        }
        public TipHistoryInfo(string tipHistoryItem)
        {
            if (tipHistoryItem.Contains(":"))
            {
                this.globalTipId = tipHistoryItem.Split(':')[0];
                this.tipLikeStatus = (TipLikeEnum)Convert.ToInt32(tipHistoryItem.Split(':')[1]);
            }
        }
    }

    public class GroupOfTips
    {
        public string groupId { get; set; }
        public string groupName { get; set; }
        public int groupPriority { get; set; }
        public List<TipInfo>[] TipsPriList { get; set; }
        public List<string> TipsSorted { get; set; }

        public static GroupOfTips Create(TipGroup tipGroup)
        {
            return new GroupOfTips
            {
                groupId = tipGroup.groupId,
                groupName = tipGroup.groupName,
                groupPriority = tipGroup.groupPriority,
                TipsPriList = new List<TipInfo>[3]
            };
        }
    }

    public enum TipLikeEnum
    {
        DISLIKE=0,
        NORMAL=1,
        LIKE=2
    }

}
