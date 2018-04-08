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
}
