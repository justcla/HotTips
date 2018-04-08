using System.Collections.Generic;

namespace HotTips
{
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

    public class TipInfo
    {
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
                groupPriority = tipGroup.groupPriority
            };
            return tipInfo;
        }
    }

    public class GroupOfTips
    {
        public string groupId { get; set; }
        public string groupName { get; set; }
        public int groupPriority { get; set; }
        public List<TipInfo>[] tipsPriList { get; set; }
    }

}
