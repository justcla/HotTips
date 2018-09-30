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
        public TipLevel level { get; set; }
    }

    //---------- Ojbects used internally by Tip Manager ----------------------------------------

    public class TipInfo
    {
        public string globalTipId { get; set; }
        
        // Tip properties
        public string tipId { get; set; }
        public string name { get; set; }
        public int priority { get; set; }
        public string contentUri { get; set; }
        
        // Group properties
        public string groupId { get; set; }
        public string groupName { get; set; }
        public int groupPriority { get; set; }
        public TipLevel Level { get; set; }
        
        // History properties
        public TipLikeEnum TipLikeStatus { get; set; }

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
                globalTipId = GetGlobalTipId(tipGroup.groupId, tip.tipId),
                Level = tip.level,
                TipLikeStatus = TipLikeEnum.NORMAL
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
        public string GlobalTipId { get; set; }

        public TipLikeEnum TipLikeStatus { get; set; }

        public override string ToString()
        {
            return string.Concat(GlobalTipId, ':', (int)TipLikeStatus);
        }

        public TipHistoryInfo()
        {
            // Default to no vote (Vote=Normal)
            this.TipLikeStatus = TipLikeEnum.NORMAL;
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

    public enum TipLevel
    {
        Beginner = 0,
        General = 1,
        Advanced = 2
    }

}
